namespace VBQOL.Network
{
	[HarmonyPatch]
    public class ClutterSystemPatch
    {
	    [HarmonyPatch(typeof(ClutterSystem), nameof(ClutterSystem.Awake))]
		private static class ClutterSystem_Awake_Patch
		{
			private static void Prefix(ClutterSystem __instance)
			{
				__instance.m_grassPatchSize = VBQOL.grass_patch_size.Value;
				__instance.m_distance = 100f;
				__instance.m_playerPushFade = 0.05f;
				__instance.m_amountScale = 2f;
			}
		}
    }
}