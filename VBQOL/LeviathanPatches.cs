namespace VBQOL
{
    [HarmonyPatch]
    public static class LeviathanPatch
    {
        private const string RisingKey = "VBQOL_Rising";
        private const string DiveStartKey = "VBQOL_DiveStart";

        public static ConfigEntry<float> m_riseDelay; // задержка до всплытия
        public static float m_diveSpeed = 2.5f; // скорость погружения
        public static float m_diveOffset = -5f; // глубина ниже земли

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
                __instance.m_alignToWaterLevel = false;

                float ground = ZoneSystem.instance.GetGroundHeight(__instance.transform.position);
                float targetDepth = ground + m_diveOffset;

                __instance.StartCoroutine(DiveRoutine(__instance, targetDepth));
                Debug.Log($"[LeviathanPatch] Начато погружение до {targetDepth}, скорость {m_diveSpeed}");
            }

            return false;
        }

        // Проверяем таймер в FixedUpdate
        // Проверка состояния в FixedUpdate
        [HarmonyPatch(typeof(Leviathan), nameof(Leviathan.FixedUpdate))]
        [HarmonyPostfix]
        static void FixedUpdatePostfix(Leviathan __instance)
        {
            if (!__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner()) return;

            // Проверка на пустышку
            bool rockActive = __instance.m_nview.GetZDO().GetBool("VBQOL_MineRockActive", true);
            if (!rockActive)
            {
                Debug.Log("[LeviathanPatch] Обнаружен пустой Leviathan, запускаем погружение");
                LeavePrefix(__instance); // триггерим погружение
                return;
            }

            // Проверка таймера
            long startTicks = __instance.m_nview.GetZDO().GetLong(DiveStartKey, 0);
            bool rising = __instance.m_nview.GetZDO().GetBool(RisingKey, false);

            if (startTicks > 0 && !rising)
            {
                double elapsed = (ZNet.instance.GetTime() - new DateTime(startTicks)).TotalSeconds;
                if (elapsed >= m_riseDelay.Value)
                {
                    Debug.Log("[LeviathanPatch] Таймер завершён, восстанавливаем MineRock и начинаем всплытие");

                    RestoreMineRock(__instance.m_mineRock);

                    float waterLevel = Floating.GetLiquidLevel(__instance.transform.position, __instance.m_waveScale);
                    __instance.StartCoroutine(RiseRoutine(__instance, waterLevel));

                    __instance.m_alignToWaterLevel = true;
                    __instance.m_nview.GetZDO().Set(DiveStartKey, 0);
                    __instance.m_nview.GetZDO().Set(RisingKey, true);
                }
            }
        }
        
        // Плавное погружение
        private static IEnumerator DiveRoutine(Leviathan leviathan, float targetDepth)
        {
            while (leviathan.transform.position.y > targetDepth)
            {
                leviathan.m_body.MovePosition(leviathan.transform.position + Vector3.down * (Time.deltaTime * m_diveSpeed));
                yield return null;
            }

            // достигли глубины — запускаем таймер
            long ticks = ZNet.instance.GetTime().Ticks;
            leviathan.m_nview.GetZDO().Set(DiveStartKey, ticks);
            leviathan.m_nview.GetZDO().Set(RisingKey, false);

            Debug.Log($"[LeviathanPatch] Погружение завершено, таймер {m_riseDelay.Value} сек стартовал");
        }

        // Плавное всплытие
        private static IEnumerator RiseRoutine(Leviathan leviathan, float waterLevel)
        {
            while (leviathan.transform.position.y < waterLevel)
            {
                leviathan.m_body.MovePosition(leviathan.transform.position + Vector3.up * Time.deltaTime);
                yield return null;
            }
            Debug.Log("[LeviathanPatch] Всплытие завершено");
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
    }
}