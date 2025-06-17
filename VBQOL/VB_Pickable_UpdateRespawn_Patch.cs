namespace VBQOL
{
	[HarmonyPatch]
    public class VB_Pickable_UpdateRespawn_Patch
    {
		[HarmonyPatch(typeof(Pickable), nameof(Pickable.UpdateRespawn))]
		public static class Patch_Pickable_UpdateRespawn
		{
			private static Exception Finalizer(Pickable __instance, Exception __exception)
			{
				if (__instance && __exception != null)
				{
					VBQOL.Logger.LogDebug(__exception);
					var timeNow = ZNet.instance.GetTime();
					__instance.m_nview.GetZDO().Set(ZDOVars.s_pickedTime, timeNow.Ticks);
					VBQOL.Logger.LogDebug("Исправлен таймер расстения:");
				}
				return null;
			}
		}
	}
}