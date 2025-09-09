using static UnityEngine.Random;

namespace VBQOL
{
	[HarmonyPatch(typeof(Door), "Interact")]
	public static class VB_Swampkey
	{
		private static void Postfix(Door __instance, ref bool __result, Humanoid character)
		{
			if (!__result || character != Player.m_localPlayer || !__instance.m_keyItem) return;

			var keyItem = __instance.m_keyItem.m_itemData;
			if (keyItem.m_shared.m_name != "$item_cryptkey") return;

			var inventory = character.GetInventory();
			if (inventory == null || !inventory.ContainsItemByName("$item_cryptkey")) return;

			if (value * 100 <= 33)
			{
				inventory.RemoveItem("$item_cryptkey", 1);
				character.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("<color=yellow>Болотный ключ сломался</color>"));
			}
		}
	}
}