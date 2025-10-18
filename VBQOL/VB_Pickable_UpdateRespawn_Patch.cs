using Debug = UnityEngine.Debug;

namespace VBQOL
{
	[HarmonyPatch]
	[HarmonyPatch(typeof(Pickable), nameof(Pickable.UpdateRespawn))]
	public static class VB_Pickable_UpdateRespawn_Patch
	{
		private static Exception Finalizer(Pickable __instance, Exception __exception)
		{
			if (__instance && __exception != null)
			{
				Debug.LogWarning(__exception);
				var timeNow = ZNet.instance.GetTime();
				__instance.m_nview.GetZDO().Set(ZDOVars.s_pickedTime, timeNow.Ticks);
				Debug.LogWarning("Исправлен таймер расстения:");
			}
			return null;
		}
	}
}