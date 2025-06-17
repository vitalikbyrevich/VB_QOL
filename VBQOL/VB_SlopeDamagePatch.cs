namespace VBQOL
{
	[HarmonyPatch]
    public class VB_SlopeDamagePatch
	{
		[HarmonyPatch(typeof(Attack), "DoMeleeAttack")]
		public class PatchAttackDoMeleeAttack
		{
			public static void Prefix(ref Attack __instance, ref float ___m_maxYAngle, ref float ___m_attackOffset, ref float ___m_attackHeight)
			{
				Player player = __instance.m_character as Player;
				if (!player || player != Player.m_localPlayer) return;
				___m_maxYAngle = 75f;
				___m_attackOffset = 0f;
				___m_attackHeight = 1f;
			}
		}
	}
}