namespace VBQOL
{
    [HarmonyPatch]
    public class VB_ToolTierPatch
    {
        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake)), HarmonyPostfix]
        public static void Patch(ZNetScene __instance)
        {
            // Деревья
            __instance.GetPrefab("Beech1").GetComponent<TreeBase>().m_minToolTier = 0; // VBQOL.BeechTier.Value;
            __instance.GetPrefab("beech_log").GetComponent<TreeLog>().m_minToolTier = 0; // VBQOL.BeechTier.Value;
            __instance.GetPrefab("beech_log_half").GetComponent<TreeLog>().m_minToolTier = 0; // VBQOL.BeechTier.Value;
            __instance.GetPrefab("Beech_Stub").GetComponent<Destructible>().m_minToolTier = 0; //VBQOL.BeechTier.Value;

            __instance.GetPrefab("FirTree").GetComponent<TreeBase>().m_minToolTier = 0; //VBQOL.FirTreeTier.Value;
            __instance.GetPrefab("FirTree_log").GetComponent<TreeLog>().m_minToolTier = 0; //VBQOL.FirTreeTier.Value;
            __instance.GetPrefab("FirTree_log_half").GetComponent<TreeLog>().m_minToolTier =
                0; //VBQOL.FirTreeTier.Value;
            __instance.GetPrefab("FirTree_Stub").GetComponent<Destructible>().m_minToolTier =
                0; //VBQOL.FirTreeTier.Value;

            __instance.GetPrefab("Pinetree_01").GetComponent<TreeBase>().m_minToolTier = 1; //VBQOL.PinetreeTier.Value;
            __instance.GetPrefab("PineTree_log").GetComponent<TreeLog>().m_minToolTier = 1; //VBQOL.PinetreeTier.Value;
            __instance.GetPrefab("PineTree_log_half").GetComponent<TreeLog>().m_minToolTier =
                1; //VBQOL.PinetreeTier.Value;
            __instance.GetPrefab("Pinetree_01_Stub").GetComponent<Destructible>().m_minToolTier =
                1; //VBQOL.PinetreeTier.Value;

            __instance.GetPrefab("SwampTree1").GetComponent<TreeBase>().m_minToolTier = 2; //VBQOL.SwampTreeTier.Value;
            __instance.GetPrefab("SwampTree1_log").GetComponent<TreeLog>().m_minToolTier =
                2; //VBQOL.SwampTreeTier.Value;
            __instance.GetPrefab("SwampTree1_Stub").GetComponent<Destructible>().m_minToolTier =
                2; //VBQOL.SwampTreeTier.Value;

            __instance.GetPrefab("Birch1").GetComponent<TreeBase>().m_minToolTier = 2; //VBQOL.BirchTier.Value;
            __instance.GetPrefab("Birch2").GetComponent<TreeBase>().m_minToolTier = 2; //VBQOL.BirchTier.Value;
            __instance.GetPrefab("Birch_log").GetComponent<TreeLog>().m_minToolTier = 2; //VBQOL.BirchTier.Value;
            __instance.GetPrefab("Birch_log_half").GetComponent<TreeLog>().m_minToolTier = 2; //VBQOL.BirchTier.Value;
            __instance.GetPrefab("BirchStub").GetComponent<Destructible>().m_minToolTier = 2; //VBQOL.BirchTier.Value;

            __instance.GetPrefab("Birch1_aut").GetComponent<TreeBase>().m_minToolTier = 2; //VBQOL.BirchPlainTier.Value;
            __instance.GetPrefab("Birch2_aut").GetComponent<TreeBase>().m_minToolTier = 2; //VBQOL.BirchPlainTier.Value;

            __instance.GetPrefab("caverock_ice_stalagtite").GetComponent<Destructible>().m_minToolTier =
                2; //VBQOL.IceStalagtiteTier.Value;
            __instance.GetPrefab("caverock_ice_stalagtite_falling").GetComponent<TreeLog>().m_minToolTier =
                2; //VBQOL.IceStalagtiteTier.Value;
            __instance.GetPrefab("caverock_ice_stalagmite").GetComponent<Destructible>().m_minToolTier =
                2; //VBQOL.IceStalagtiteTier.Value;
            __instance.GetPrefab("caverock_ice_stalagmite_broken").GetComponent<Destructible>().m_minToolTier =
                2; //VBQOL.IceStalagtiteTier.Value;
            __instance.GetPrefab("caverock_ice_pillar_wall").GetComponent<Destructible>().m_minToolTier =
                2; //VBQOL.IceStalagtiteTier.Value;

            __instance.GetPrefab("Oak1").GetComponent<TreeBase>().m_minToolTier = 3; //VBQOL.OakTier.Value;
            __instance.GetPrefab("Oak_log").GetComponent<TreeLog>().m_minToolTier = 3; //VBQOL.OakTier.Value;
            __instance.GetPrefab("Oak_log_half").GetComponent<TreeLog>().m_minToolTier = 3; //VBQOL.OakTier.Value;
            __instance.GetPrefab("OakStub").GetComponent<Destructible>().m_minToolTier = 3; //VBQOL.OakTier.Value;

            __instance.GetPrefab("YggaShoot1").GetComponent<TreeBase>().m_minToolTier = 4; //VBQOL.YggaShootTier.Value;
            __instance.GetPrefab("YggaShoot2").GetComponent<TreeBase>().m_minToolTier = 4; //VBQOL.YggaShootTier.Value;
            __instance.GetPrefab("YggaShoot3").GetComponent<TreeBase>().m_minToolTier = 4; //VBQOL.YggaShootTier.Value;
            __instance.GetPrefab("yggashoot_log").GetComponent<TreeLog>().m_minToolTier =
                4; //VBQOL.YggaShootTier.Value;
            __instance.GetPrefab("yggashoot_log_half").GetComponent<TreeLog>().m_minToolTier =
                4; //VBQOL.YggaShootTier.Value;
            __instance.GetPrefab("ShootStump").GetComponent<Destructible>().m_minToolTier =
                4; //VBQOL.YggaShootTier.Value;

            // Камни и руды
            __instance.GetPrefab("Leviathan").GetComponent<MineRock>().m_minToolTier = 1; //VBQOL.LeviathanTier.Value;

            __instance.GetPrefab("mudpile").GetComponent<Destructible>().m_minToolTier = 1; //VBQOL.MudpileTier.Value;
            __instance.GetPrefab("mudpile2").GetComponent<Destructible>().m_minToolTier = 1; //VBQOL.MudpileTier.Value;
            __instance.GetPrefab("mudpile_beacon").GetComponent<Destructible>().m_minToolTier =
                1; //VBQOL.MudpileTier.Value;
            __instance.GetPrefab("mudpile_old").GetComponent<MineRock>().m_minToolTier = 1; //VBQOL.MudpileTier.Value;
            __instance.GetPrefab("mudpile_frac").GetComponent<MineRock5>().m_minToolTier = 1; //VBQOL.MudpileTier.Value;
            __instance.GetPrefab("mudpile2_frac").GetComponent<MineRock5>().m_minToolTier =
                1; //VBQOL.MudpileTier.Value;

            __instance.GetPrefab("MineRock_Meteorite").GetComponent<MineRock>().m_minToolTier =
                3; //VBQOL.MeteoriteTier.Value;

            __instance.GetPrefab("Rock_3").GetComponent<Destructible>().m_minToolTier = 0; //VBQOL.RockTier.Value;
            __instance.GetPrefab("Rock_3_frac").GetComponent<MineRock5>().m_minToolTier = 0; //VBQOL.RockTier.Value;
            __instance.GetPrefab("rock4_coast").GetComponent<Destructible>().m_minToolTier = 0; //VBQOL.RockTier.Value;
            __instance.GetPrefab("rock4_coast_frac").GetComponent<MineRock5>().m_minToolTier =
                0; //VBQOL.RockTier.Value;
            __instance.GetPrefab("rock4_forest").GetComponent<Destructible>().m_minToolTier = 0; //VBQOL.RockTier.Value;
            __instance.GetPrefab("rock4_forest_frac").GetComponent<MineRock5>().m_minToolTier =
                0; //VBQOL.RockTier.Value;

            __instance.GetPrefab("rock4_copper").GetComponent<Destructible>().m_minToolTier =
                0; //VBQOL.CopperTier.Value;
            __instance.GetPrefab("rock4_copper_frac").GetComponent<MineRock5>().m_minToolTier =
                0; //VBQOL.CopperTier.Value;

            __instance.GetPrefab("rock1_mountain").GetComponent<Destructible>().m_minToolTier =
                1; //VBQOL.RockMountainTier.Value;
            __instance.GetPrefab("rock1_mountain_frac").GetComponent<MineRock5>().m_minToolTier =
                1; //VBQOL.RockMountainTier.Value;
            __instance.GetPrefab("rock2_mountain").GetComponent<Destructible>().m_minToolTier =
                1; //VBQOL.RockMountainTier.Value;
            __instance.GetPrefab("rock2_mountain_frac").GetComponent<MineRock5>().m_minToolTier =
                1; //VBQOL.RockMountainTier.Value;
            __instance.GetPrefab("rock3_mountain").GetComponent<Destructible>().m_minToolTier =
                1; //VBQOL.RockMountainTier.Value;
            __instance.GetPrefab("rock3_mountain_frac").GetComponent<MineRock5>().m_minToolTier =
                1; //VBQOL.RockMountainTier.Value;

            __instance.GetPrefab("rock2_heath").GetComponent<Destructible>().m_minToolTier =
                1; //VBQOL.RockPlainTier.Value;
            __instance.GetPrefab("rock2_heath_frac").GetComponent<MineRock5>().m_minToolTier =
                1; //VBQOL.RockPlainTier.Value;
            __instance.GetPrefab("rock4_heath").GetComponent<Destructible>().m_minToolTier =
                1; //VBQOL.RockPlainTier.Value;
            __instance.GetPrefab("rock4_heath_frac").GetComponent<MineRock5>().m_minToolTier =
                1; //VBQOL.RockPlainTier.Value;
            __instance.GetPrefab("HeathRockPillar").GetComponent<Destructible>().m_minToolTier =
                1; //VBQOL.RockPlainTier.Value;
            __instance.GetPrefab("HeathRockPillar_frac").GetComponent<MineRock5>().m_minToolTier =
                1; //VBQOL.RockPlainTier.Value;
            __instance.GetPrefab("RockThumb").GetComponent<Destructible>().m_minToolTier =
                1; //VBQOL.RockPlainTier.Value;
            __instance.GetPrefab("RockThumb_frac").GetComponent<MineRock5>().m_minToolTier =
                1; //VBQOL.RockPlainTier.Value;
            __instance.GetPrefab("RockFinger").GetComponent<Destructible>().m_minToolTier =
                1; //VBQOL.RockPlainTier.Value;
            __instance.GetPrefab("RockFinger_frac").GetComponent<MineRock5>().m_minToolTier =
                1; //VBQOL.RockPlainTier.Value;
            __instance.GetPrefab("RockFingerBroken").GetComponent<Destructible>().m_minToolTier =
                1; //VBQOL.RockPlainTier.Value;
            __instance.GetPrefab("RockFingerBroken_frac").GetComponent<MineRock5>().m_minToolTier =
                1; //VBQOL.RockPlainTier.Value;

            __instance.GetPrefab("rock_mistlands1").GetComponent<Destructible>().m_minToolTier =
                2; //VBQOL.RockMistlandTier.Value;
            __instance.GetPrefab("rock_mistlands1_frac").GetComponent<MineRock5>().m_minToolTier =
                2; //VBQOL.RockMistlandTier.Value;
            __instance.GetPrefab("cliff_mistlands1").GetComponent<Destructible>().m_minToolTier =
                2; //VBQOL.RockMistlandTier.Value;
            __instance.GetPrefab("cliff_mistlands1_frac").GetComponent<MineRock5>().m_minToolTier =
                2; //VBQOL.RockMistlandTier.Value;
            __instance.GetPrefab("cliff_mistlands2").GetComponent<Destructible>().m_minToolTier =
                2; //VBQOL.RockMistlandTier.Value;
            __instance.GetPrefab("cliff_mistlands2_frac").GetComponent<MineRock5>().m_minToolTier =
                2; //VBQOL.RockMistlandTier.Value;
            __instance.GetPrefab("cliff_mistlands1_creep").GetComponent<Destructible>().m_minToolTier =
                2; //VBQOL.RockMistlandTier.Value;
            __instance.GetPrefab("cliff_mistlands1_creep_frac").GetComponent<MineRock5>().m_minToolTier =
                2; //VBQOL.RockMistlandTier.Value;

            __instance.GetPrefab("silvervein").GetComponent<Destructible>().m_minToolTier = 2; //VBQOL.SilverTier.Value;
            __instance.GetPrefab("silvervein_frac").GetComponent<MineRock5>().m_minToolTier =
                2; //VBQOL.SilverTier.Value;
            __instance.GetPrefab("rock3_silver").GetComponent<Destructible>().m_minToolTier =
                2; //VBQOL.SilverTier.Value;
            __instance.GetPrefab("rock3_silver_frac").GetComponent<MineRock5>().m_minToolTier =
                2; //VBQOL.SilverTier.Value;

            __instance.GetPrefab("giant_brain").GetComponent<Destructible>().m_minToolTier = 3; //VBQOL.BrainTier.Value;
            __instance.GetPrefab("giant_brain_frac").GetComponent<MineRock5>().m_minToolTier =
                3; //VBQOL.BrainTier.Value;
            __instance.GetPrefab("giant_ribs").GetComponent<Destructible>().m_minToolTier = 3; //VBQOL.GiantTier.Value;
            __instance.GetPrefab("giant_ribs_frac").GetComponent<MineRock5>().m_minToolTier =
                3; //VBQOL.GiantTier.Value;
            __instance.GetPrefab("giant_skull").GetComponent<Destructible>().m_minToolTier = 3; //VBQOL.GiantTier.Value;
            __instance.GetPrefab("giant_skull_frac").GetComponent<MineRock5>().m_minToolTier =
                3; //VBQOL.GiantTier.Value;
            __instance.GetPrefab("giant_helmet1").GetComponent<Destructible>().m_minToolTier =
                3; //VBQOL.GiantArmorTier.Value;
            __instance.GetPrefab("giant_helmet1_destruction").GetComponent<MineRock5>().m_minToolTier =
                3; //VBQOL.GiantArmorTier.Value;
            __instance.GetPrefab("giant_helmet2").GetComponent<Destructible>().m_minToolTier = 3;
            __instance.GetPrefab("giant_helmet2_destruction").GetComponent<MineRock5>().m_minToolTier = 3;
            __instance.GetPrefab("giant_sword1").GetComponent<Destructible>().m_minToolTier = 3;
            __instance.GetPrefab("giant_sword1_destruction").GetComponent<MineRock5>().m_minToolTier = 3;
            __instance.GetPrefab("giant_sword2").GetComponent<Destructible>().m_minToolTier = 3;
            __instance.GetPrefab("giant_sword2_destruction").GetComponent<MineRock5>().m_minToolTier = 3;

            __instance.GetPrefab("ice_rock1").GetComponent<Destructible>().m_minToolTier = 3;
            __instance.GetPrefab("ice_rock1_frac").GetComponent<MineRock5>().m_minToolTier = 3;
            __instance.GetPrefab("rock3_ice").GetComponent<Destructible>().m_minToolTier = 3;
            __instance.GetPrefab("rock3_ice_frac").GetComponent<MineRock5>().m_minToolTier = 3;
            __instance.GetPrefab("ice1").GetComponent<Destructible>().m_minToolTier = 3;
        }
    }
}