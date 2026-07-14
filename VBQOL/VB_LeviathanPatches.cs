using Random = UnityEngine.Random;

namespace VBQOL
{
    [HarmonyPatch]
    public static class VB_LeviathanPatch
    {
        private const string SurfaceKey = "VBQOL_Surface";
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
        
        [HarmonyPatch(typeof(Leviathan), nameof(Leviathan.Awake))]
        [HarmonyPostfix]
        static void AwakePostfix(Leviathan __instance)
        {
            if (!__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner()) return;

            bool isLava = __instance.gameObject.name.Contains("LeviathanLava");
            var zdo = __instance.m_nview.GetZDO();
            zdo.Set(SurfaceKey, true);

            if (isLava && zdo.GetFloat(OriginalHeightKey) == 0f)
            {
                zdo.Set(OriginalHeightKey, __instance.transform.position.y);
                Debug.Log($"[LeviathanPatch] Сохранена высота при спавне: {__instance.transform.position.y}");
            }
        }

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
                float targetDepth = isLava ? ground + m_diveOffsetLava : ground + m_diveOffset;

                __instance.StartCoroutine(DiveRoutine(__instance, targetDepth));
                Debug.Log($"[LeviathanPatch] Начато погружение с {__instance.transform.position.y:F2} до {targetDepth:F2}");
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
            bool rising = zdo.GetBool(RisingKey, true);
            long startTicks = zdo.GetLong(DiveStartKey);
            bool surface = zdo.GetBool(SurfaceKey, true);
            
            if (surface && IsMineRockEmpty(__instance.m_mineRock))
            {
                StartDiving(__instance, isLava);
                return;
            }

            if (startTicks > 0 && !rising)
            {
                double elapsed = (ZNet.instance.GetTime() - new DateTime(startTicks)).TotalSeconds;
                if (elapsed >= m_riseDelay.Value)
                {
                    StartRising(__instance, isLava);
                }
            }
        }

        private static void StartDiving(Leviathan leviathan, bool isLava)
        {
            var zdo = leviathan.m_nview.GetZDO();

            if (isLava && zdo.GetFloat(OriginalHeightKey) == 0f)
            {
                zdo.Set(OriginalHeightKey, leviathan.transform.position.y);
                Debug.Log($"[LeviathanPatch] Сохранена высота: {leviathan.transform.position.y:F2}");
            }

            zdo.Set(SurfaceKey, false);
            zdo.Set(DiveTriggeredKey, true);
            leviathan.m_alignToWaterLevel = false;

            float ground = ZoneSystem.instance.GetGroundHeight(leviathan.transform.position);
            float targetDepth = isLava ? ground + m_diveOffsetLava : ground + m_diveOffset;

            Debug.Log("[LeviathanPatch] Начинаем погружение");
            leviathan.StartCoroutine(DiveRoutine(leviathan, targetDepth, isLava));
        }

        private static IEnumerator DiveRoutine(Leviathan leviathan, float targetDepth, bool isLava = false)
        {
            long ticks = ZNet.instance.GetTime().Ticks;
            leviathan.m_nview.GetZDO().Set(DiveStartKey, ticks);
            leviathan.m_nview.GetZDO().Set(RisingKey, false);
            
            Debug.Log($"[LeviathanPatch] Таймер {m_riseDelay.Value} сек стартовал");

            // Для лавового левиафана - спавним платформы при старте погружения
            if (isLava)
            {
                SpawnLavaPlatforms(leviathan);
            }

            while (leviathan.transform.position.y > targetDepth + 0.1f)
            {
                leviathan.m_body.MovePosition(leviathan.transform.position + Vector3.down * (Time.deltaTime * m_diveSpeed));
                yield return null;
            }
        }

        private static void SpawnLavaPlatforms(Leviathan leviathan)
        {
            GameObject platformPrefab = ZNetScene.instance.GetPrefab("lavabomb_rock1");
            if (!platformPrefab)
            {
                Debug.LogWarning("[LeviathanPatch] Не удалось загрузить lavabomb_rock1");
                return;
            }

            Vector3 centerPos = leviathan.transform.position;
            
            // Исходная позиция + платформы на расстоянии 3м по сторонам
            SpawnPlatform(platformPrefab, centerPos);
            SpawnPlatform(platformPrefab, centerPos + new Vector3(3.3f, 0f, 0f));
            SpawnPlatform(platformPrefab, centerPos + new Vector3(-3.3f, 0f, 0f));
            SpawnPlatform(platformPrefab, centerPos + new Vector3(0f, 0f, 3.3f));
            SpawnPlatform(platformPrefab, centerPos + new Vector3(0f, 0f, -3.3f));
            
            // Добавим ещё 4 диагональных для лучшего покрытия
            SpawnPlatform(platformPrefab, centerPos + new Vector3(3.3f, 0f, 3.3f));
            SpawnPlatform(platformPrefab, centerPos + new Vector3(-3.3f, 0f, 3.3f));
            SpawnPlatform(platformPrefab, centerPos + new Vector3(3.3f, 0f, -3.3f));
            SpawnPlatform(platformPrefab, centerPos + new Vector3(-3.3f, 0f, -3.3f));
            
            Debug.Log("[LeviathanPatch] Создано 9 платформ из lavabomb_rock1");
        }

        private static void SpawnPlatform(GameObject prefab, Vector3 position)
        {
            // Немного приподнимаем над поверхностью
            position.y += 0.5f;
            
            // Случайное вращение для естественности
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Object.Instantiate(prefab, position, rotation);
        }

        private static void StartRising(Leviathan leviathan, bool isLava)
        {
            var zdo = leviathan.m_nview.GetZDO();

            // Сбрасываем таймер
            zdo.Set(DiveStartKey, 0);
            zdo.Set(RisingKey, true);

            Debug.Log("[LeviathanPatch] Таймер завершён, восстанавливаем MineRock");

            // Восстанавливаем руду
            if (leviathan.m_mineRock)
            {
                RestoreMineRock(leviathan.m_mineRock);
            }

            // Начинаем всплытие
            leviathan.StartCoroutine(RiseRoutine(leviathan, isLava));

            if (!isLava) leviathan.m_alignToWaterLevel = true;
        }

        private static IEnumerator RiseRoutine(Leviathan leviathan, bool isLava)
        {
            float targetHeight = isLava 
                ? leviathan.m_nview.GetZDO().GetFloat(OriginalHeightKey, leviathan.transform.position.y)
                : Floating.GetLiquidLevel(leviathan.transform.position, leviathan.m_waveScale);

            Debug.Log($"[LeviathanPatch] Всплываем до {(isLava ? "сохранённой" : "уровня воды")} высоты: {targetHeight:F2}");
            
            var zdo = leviathan.m_nview.GetZDO();
            zdo.Set(SurfaceKey, true);

            // Загружаем префаб эффекта для лавового левиафана
            GameObject lavaSplashPrefab = null;
            if (isLava)
            {
                lavaSplashPrefab = ZNetScene.instance.GetPrefab("vfx_BombBlob_explode_lava");
                if (!lavaSplashPrefab)
                {
                    Debug.LogWarning("[LeviathanPatch] Не удалось загрузить vfx_BombBlob_explode_lava");
                }
            }

            // Сохраняем начальную позицию для эффектов
            Vector3 startPos = leviathan.transform.position;

            while (leviathan.transform.position.y < targetHeight - 0.1f)
            {
                leviathan.m_body.MovePosition(leviathan.transform.position + Vector3.up * (Time.deltaTime * m_diveSpeed));
                
                // Эффекты в процессе подъема (каждые 2 метра)
                if (isLava && lavaSplashPrefab && leviathan.transform.position.y % 2f < 0.1f)
                {
                    Vector3 splashPos = new Vector3(startPos.x, targetHeight, startPos.z);
                    Object.Instantiate(lavaSplashPrefab, splashPos, Quaternion.identity);
                }
                
                yield return null;
            }

            // Финальный эффект на исходной высоте (поверхность лавы)
            if (isLava && lavaSplashPrefab)
            {
                // Спавним эффекты прямо на исходной позиции
                Vector3 finalPos = new Vector3(startPos.x, targetHeight, startPos.z);
                
                // Несколько эффектов для красоты
                Object.Instantiate(lavaSplashPrefab, finalPos, Quaternion.identity);
                Object.Instantiate(lavaSplashPrefab, finalPos + new Vector3(2.5f, 0f, 2.5f), Quaternion.identity);
                Object.Instantiate(lavaSplashPrefab, finalPos + new Vector3(-2.5f, 0f, -2.5f), Quaternion.identity);
                Object.Instantiate(lavaSplashPrefab, finalPos + new Vector3(2.5f, 0f, -2.5f), Quaternion.identity);
                Object.Instantiate(lavaSplashPrefab, finalPos + new Vector3(-2.5f, 0f, 2.5f), Quaternion.identity);
                
                Debug.Log($"[LeviathanPatch] Создан эффект vfx_BombBlob_explode_lava на высоте {targetHeight:F2}");
            }

            Debug.Log($"[LeviathanPatch] Всплытие завершено на высоте {leviathan.transform.position.y:F2}");
        }

        private static void RestoreMineRock(MineRock mineRock)
        {
            if (mineRock?.m_nview?.GetZDO() == null) return;

            var zdo = mineRock.m_nview.GetZDO();
            float maxHealth = mineRock.GetHealth();

            // Восстанавливаем здоровье
            for (int i = 0; i < mineRock.m_hitAreas.Length; i++) zdo.Set("Health" + i, maxHealth);

            // Активируем зоны
            foreach (var area in mineRock.m_hitAreas) 
                if (area) area.gameObject.SetActive(true);

            if (mineRock.m_baseModel) mineRock.m_baseModel.SetActive(true);

            // И только теперь обновляем визуализацию
            mineRock.UpdateVisability();

            Debug.Log("[LeviathanPatch] MineRock восстановлен корректно");
        }


        private static bool IsMineRockEmpty(MineRock mineRock)
        {
            if (!mineRock) return true;

            var zdo = mineRock.m_nview?.GetZDO();
            if (zdo == null) return true;

            // Проверяем здоровье в ZDO
            for (int i = 0; i < 20; i++) 
                if (zdo.GetFloat("Health" + i) > 0f) return false;

            // Если в ZDO ничего нет, проверяем активные зоны
            if (mineRock.m_hitAreas != null)
            {
                foreach (var area in mineRock.m_hitAreas)
                    if (area && area.gameObject.activeSelf) return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(ZNetView), nameof(ZNetView.Destroy))]
        [HarmonyPrefix]
        static bool PreventDestroy(ZNetView __instance)
        {
            if (__instance.GetComponent<Leviathan>()) return false;
            return true;
        }

        // Патч для предотвращения удаления MineRock у Левиафанов
        [HarmonyPatch(typeof(MineRock), nameof(MineRock.AllDestroyed))]
        [HarmonyPrefix]
        static bool AllDestroyedPrefix(MineRock __instance, ref bool __result)
        {
            if (__instance.GetComponentInParent<Leviathan>())
            {
                __result = false;
                return false;
            }
            return true;
        }
    }
}