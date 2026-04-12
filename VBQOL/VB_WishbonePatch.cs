namespace VBQOL
{
	[HarmonyPatch]
	public static class VB_WishbonePatch
	{
		[HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake)), HarmonyPostfix]
		public static void Patch(ZNetScene __instance)
		{
			var wishbone = __instance.GetPrefab("Wishbone").GetComponent<ItemDrop>().m_itemData.m_shared;
			wishbone.m_useDurability = true;
			wishbone.m_durabilityDrain = 0.05f;
		}
	}
}