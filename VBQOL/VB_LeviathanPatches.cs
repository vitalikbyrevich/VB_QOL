namespace VBQOL
{
    [HarmonyPatch]
    public static class VB_LeviathanPatch
    {
        private const string RisingKey = "VBQOL_Rising";
        private const string DiveStartKey = "VBQOL_DiveStart";
        private const string DiveTriggeredKey = "VBQOL_DiveTriggered";
        private const string OriginalHeightKey = "VBQOL_OriginalHeight";

        public static ConfigEntry<bool> m_resetLeviathanOn;
        public static ConfigEntry<bool> m_resetLeviathanLavaOn;
        public static ConfigEntry<float> m_riseDelay;
        public static float m_diveSpeed = 2.5f;
        public static float m_diveOffset = -5f;
        public static float m_diveOffsetLava = -25f;

        // Запрещаем уничтожение Leviathan
        [HarmonyPatch(typeof(ZNetView), nameof(ZNetView.Destroy))]
        [HarmonyPrefix]
        static bool PreventDestroy(ZNetView __instance)
        {
            if (__instance.GetComponent<Leviathan>()) return false;
            return true;
        }

        // Перехватываем Leave: запускаем погружение
        [HarmonyPatch(typeof(Leviathan), nameof(Leviathan.Leave))]
        [HarmonyPrefix]
        static bool LeavePrefix(Leviathan __instance)
        {
            if (__instance.m_nview.IsValid() && __instance.m_nview.IsOwner())
            {
                bool isLava = __instance.gameObject.name.Contains("LeviathanLava");
                if (isLava && !m_resetLeviathanLavaOn.Value) return true;
                if (!isLava && !m_resetLeviathanOn.Value) return true;

                __instance.m_alignToWaterLevel = false;

                float ground = ZoneSystem.instance.GetGroundHeight(__instance.transform.position);
                float targetDepth;
                if (isLava) targetDepth = ground + m_diveOffsetLava;
                else targetDepth = ground + m_diveOffset;

                __instance.StartCoroutine(DiveRoutine(__instance, targetDepth));
                Debug.Log($"[LeviathanPatch] Начато погружение с {__instance.transform.position.y} до {targetDepth}");
            }
            return false;
        }

        [HarmonyPatch(typeof(Leviathan), nameof(Leviathan.FixedUpdate))]
        [HarmonyPostfix]
        static void FixedUpdatePostfix(Leviathan __instance)
        {
            if (!__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner()) return;
            bool isLava = __instance.gameObject.name.Contains("LeviathanLava");
            if (isLava && !m_resetLeviathanLavaOn.Value) return;
            if (!isLava && !m_resetLeviathanOn.Value) return;

            var zdo = __instance.m_nview.GetZDO();
            bool rising = zdo.GetBool(RisingKey, false);
            long startTicks = zdo.GetLong(DiveStartKey, 0);
            float originalHeight = __instance.transform.position.y;

            // Проверка на пустышку
            bool diveTriggered = zdo.GetBool(DiveTriggeredKey, false);

            if (startTicks == 0 && !rising && !diveTriggered && IsMineRockEmpty(__instance.m_mineRock))
            {
                if (isLava)
                {
                    zdo.Set(OriginalHeightKey, originalHeight);
                    Debug.Log($"[LeviathanPatch] Сохранена высота: {originalHeight}, ZDO записано: {zdo.GetFloat(OriginalHeightKey)}");
                }

                zdo.Set("VBQOL_MineRockActive", false);
                zdo.Set(DiveTriggeredKey, true); // помечаем, что уже обработали
                __instance.m_alignToWaterLevel = false;

                float ground = ZoneSystem.instance.GetGroundHeight(__instance.transform.position);
                float targetDepth;
                if (isLava) targetDepth = ground + m_diveOffsetLava;
                else targetDepth = ground + m_diveOffset;
                __instance.StartCoroutine(DiveRoutine(__instance, targetDepth));

                Debug.Log("[LeviathanPatch] Обнаружен пустой Leviathan → запускаем погружение");
                return;
            }

            // Проверка таймера
            if (startTicks > 0 && !rising)
            {
                double elapsed = (ZNet.instance.GetTime() - new DateTime(startTicks)).TotalSeconds;
                if (elapsed >= m_riseDelay.Value)
                {
                    // СРАЗУ сбрасываем таймер, чтобы не вызывать снова
                    zdo.Set(DiveStartKey, 0);
                    zdo.Set(RisingKey, true);

                    Debug.Log("[LeviathanPatch] Таймер завершён, восстанавливаем MineRock");
                    RestoreMineRock(__instance.m_mineRock);

                    __instance.StartCoroutine(RiseRoutine(__instance, isLava));

                    if (!isLava) __instance.m_alignToWaterLevel = true;

                    zdo.Set(DiveTriggeredKey, false);
                }
            }
        }

        // Плавное погружение
        private static IEnumerator DiveRoutine(Leviathan leviathan, float targetDepth)
        {
            long ticks = ZNet.instance.GetTime().Ticks;
            leviathan.m_nview.GetZDO().Set(DiveStartKey, ticks);
            leviathan.m_nview.GetZDO().Set(RisingKey, false);
            while (leviathan.transform.position.y > targetDepth)
            {
                leviathan.m_body.MovePosition(leviathan.transform.position + Vector3.down * (Time.deltaTime * m_diveSpeed));
                yield return null;
            }

            Debug.Log($"[LeviathanPatch] Погружение завершено, таймер {m_riseDelay.Value} сек стартовал");
        }

        // Обновляем RiseRoutine
        private static IEnumerator RiseRoutine(Leviathan leviathan, bool isLava = false)
        {
            float targetHeight;

            if (isLava)
            {
                // Получаем сохраненную высоту из ZDO
                var zdo = leviathan.m_nview.GetZDO();
                float originalHeight = zdo.GetFloat(OriginalHeightKey);
                targetHeight = originalHeight;
                Debug.Log($"[LeviathanPatch] Восстанавливаем на сохраненную высоту: {targetHeight}");
            }
            else
            {
                // Обычный Левиафан - уровень воды
                targetHeight = Floating.GetLiquidLevel(leviathan.transform.position, leviathan.m_waveScale);
                Debug.Log($"[LeviathanPatch] Обычный Левиафан всплывает до уровня воды: {targetHeight}");
            }

            // Поднимаем пока не достигнем цели
            while (leviathan.transform.position.y < targetHeight) // -0.1f для допуска
            {
                leviathan.m_body.MovePosition(leviathan.transform.position + Vector3.up * (Time.deltaTime * 1.5f)); // Быстрее поднимаем
                yield return null;
            }

            Debug.Log($"[LeviathanPatch] Всплытие завершено. Финальная высота: {leviathan.transform.position.y}, целевая была: {targetHeight}");
        }

        // Восстановление хитина
        private static void RestoreMineRock(MineRock mineRock)
        {
            if (mineRock?.m_nview?.GetZDO() == null) return;

            for (int i = 0; i < mineRock.m_hitAreas.Length; i++)
            {
                string key = "Health" + i;
                mineRock.m_nview.GetZDO().Set(key, mineRock.GetHealth());
                if (mineRock.m_hitAreas[i]) mineRock.m_hitAreas[i].gameObject.SetActive(true);
            }

            if (mineRock.m_baseModel) mineRock.m_baseModel.SetActive(true);

            // Сохраняем состояние
            mineRock.m_nview.GetZDO().Set("VBQOL_MineRockActive", true);

            Debug.Log("[LeviathanPatch] MineRock восстановлен и состояние сохранено");
        }

        private static bool IsMineRockEmpty(MineRock mineRock)
        {
            if (!mineRock || mineRock.m_hitAreas == null || mineRock.m_hitAreas.Length == 0) return true;

            // Если ни одна зона удара не активна — считаем пустышкой
            int active = 0;
            for (int i = 0; i < mineRock.m_hitAreas.Length; i++)
            {
                var ha = mineRock.m_hitAreas[i];
                if (ha && ha.gameObject.activeSelf) active++;
            }

            return active == 0;
        }
    }
}