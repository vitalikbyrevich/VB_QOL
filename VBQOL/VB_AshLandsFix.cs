namespace VBQOL
{
    [HarmonyPatch]
    public class VB_AshLandsFix
    {
        public static float maxTtl = 30f;

        [HarmonyPatch(typeof(SEMan), nameof(SEMan.Update))]
        [HarmonyPostfix]
        public static void PatchUpdate(SEMan __instance)
        {
            var wet = __instance.GetStatusEffects().Find(x => x.name == "Wet");
            if (!wet) return;
            if (WorldGenerator.instance.GetBiome(__instance.m_character.transform.position) != Heightmap.Biome.AshLands) return;
            if (wet.m_ttl > maxTtl)  wet.m_ttl -= maxTtl/5f;
        }
    }
}