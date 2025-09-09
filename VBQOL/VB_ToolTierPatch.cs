namespace VBQOL
{
    [HarmonyPatch]
    public class VB_ToolTierPatch
    {
        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake)), HarmonyPostfix]
        public static void Patch(ZNetScene __instance)
        {
            Tree_Patch(__instance);
            Roock_Patch(__instance);
        }

        private static void Tree_Patch(ZNetScene __instance)
        {
            TierPatch(__instance, 0, "Beech1", "beech_log", "beech_log_half", "Beech_Stub", "FirTree", "FirTree_log", "FirTree_log_half", "FirTree_Stub");
            TierPatch(__instance, 1, "Pinetree_01", "PineTree_log", "PineTree_log_half", "Pinetree_01_Stub");
            TierPatch(__instance, 2, "SwampTree1", "SwampTree1_log", "SwampTree1_Stub", "Birch1", "Birch2", "Birch_log", "Birch_log_half", "BirchStub", "Birch1_aut", "Birch2_aut");
            TierPatch(__instance, 3, "Oak1", "Oak_log", "Oak_log_half", "OakStub");
            TierPatch(__instance, 4, "YggaShoot1", "YggaShoot2", "YggaShoot3", "yggashoot_log", "yggashoot_log_half", "ShootStump");
        }

        private static void Roock_Patch(ZNetScene __instance)
        {
            TierPatch(__instance, 1,
                "rock1_mountain", "rock1_mountain_frac", "rock2_mountain", "rock2_mountain_frac", "rock3_mountain",
                "rock3_mountain_frac", "rock2_heath", "rock2_heath_frac", "rock4_heath", "rock4_heath_frac", "HeathRockPillar", "HeathRockPillar_frac", "RockThumb", "RockThumb_frac", "RockFinger",
                "RockFinger_frac", "RockFingerBroken", "RockFingerBroken_frac", "Leviathan", "mudpile", "mudpile2", "mudpile_beacon", "mudpile_old", "mudpile_frac", "mudpile2_frac"
            );
            TierPatch(__instance, 2,
                "caverock_ice_stalagtite", "caverock_ice_stalagtite_falling", "caverock_ice_stalagmite", "caverock_ice_stalagmite_broken", "caverock_ice_pillar_wall", "rock_mistlands1",
                "rock_mistlands1_frac", "cliff_mistlands1", "cliff_mistlands1_frac", "cliff_mistlands2", "cliff_mistlands2_frac", "cliff_mistlands1_creep", "cliff_mistlands1_creep_frac",
                "silvervein", "silvervein_frac", "rock3_silver", "rock3_silver_frac"
            );
            TierPatch(__instance, 3,
                "MineRock_Meteorite", "giant_brain", "giant_brain_frac", "giant_ribs", "giant_ribs_frac", "giant_skull", "giant_skull_frac", "giant_helmet1", "giant_helmet1_destruction",
                "giant_helmet2", "giant_helmet2_destruction", "giant_sword1", "giant_sword1_destruction", "giant_sword2", "giant_sword2_destruction", "ice_rock1", "ice_rock1_frac", "rock3_ice",
                "rock3_ice_frac", "ice1"
            );
        }

        private static void TierPatch(ZNetScene scene, int tier, params string[] prefabNames)
        {
            foreach (var name in prefabNames)
            {
                var prefab = scene.GetPrefab(name);
                if (!prefab) continue;

                if (prefab.GetComponent<Destructible>() is { } destructible) destructible.m_minToolTier = tier;
                if (prefab.GetComponent<TreeBase>() is { } treeBase) treeBase.m_minToolTier = tier;
                if (prefab.GetComponent<TreeLog>() is { } treeLog) treeLog.m_minToolTier = tier;
                if (prefab.GetComponent<MineRock>() is { } mineRock) mineRock.m_minToolTier = tier;
                if (prefab.GetComponent<MineRock5>() is { } mineRock5) mineRock5.m_minToolTier = tier;
            }
        }
    }
}