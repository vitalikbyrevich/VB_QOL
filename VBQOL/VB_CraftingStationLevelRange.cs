namespace VBQOL
{
    [HarmonyPatch(typeof(CraftingStation), nameof(CraftingStation.GetStationBuildRange))]
    public static class StationRangePatch
    {
        private static readonly HashSet<string> craftingStationChangeList = new HashSet<string>
        {
            "$piece_stonecutter"
            // сюда можно добавить ещё: "$piece_forge", "$piece_cauldron" и т.п.
        };

        private static void Postfix(CraftingStation __instance, ref float __result)
        {
            // Проверяем, что станция в списке
            if (!craftingStationChangeList.Contains(__instance.m_name)) return;

            // Собираем все верстаки в радиусе 50 м
            List<CraftingStation> candidates = new List<CraftingStation>();
            CraftingStation.FindStationsInRange("$piece_workbench", __instance.transform.position, 50f, candidates);

            if (candidates.Count == 0) return;

            // Выбираем лучший верстак: сначала по уровню, потом по дистанции
            CraftingStation best = candidates
                .OrderByDescending(ws => ws.GetLevel())
                .ThenBy(ws => Vector3.Distance(ws.transform.position, __instance.transform.position))
                .FirstOrDefault();

            if (!best) return;

            int level = best.GetLevel();
            if (level <= 1) return;

            float baseRange = 20f;
            float extraPerLevel = 4f;

            __result = baseRange + (level - 1) * extraPerLevel;

            // Обновляем визуальный маркер
            if (__instance.m_areaMarker)
            {
                var projector = __instance.m_areaMarker.GetComponent<CircleProjector>();
                if (projector)
                {
                    projector.m_radius = __result;
                    projector.m_nrOfSegments = (int)Mathf.Ceil(Mathf.Max(5f, 4f * __result));
                }
            }
        }
    }
}
