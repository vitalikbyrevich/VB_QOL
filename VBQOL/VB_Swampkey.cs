using static UnityEngine.Random;

namespace VBQOL
{
	[HarmonyPatch]
    public class VB_Swampkey
	{
		[HarmonyPatch(typeof(Door), "Interact")]
		public static class SingleDoorPatch
		{
			private static void Postfix(Door __instance, ref bool __result, Humanoid character)
			{
				Player localPlayer = Player.m_localPlayer;
				if (__result)
				{
					if (character == localPlayer)
					{
						ItemDrop keyItem = __instance.m_keyItem;
						if (keyItem)
						{
							if (keyItem.m_itemData.m_shared.m_name == "$item_cryptkey")
							{
								Inventory inventory = character.GetInventory();
								if (inventory != null)
								{
									if (inventory.ContainsItemByName("$item_cryptkey"))
									{
										if (value * 100 <= 33)
										{
											inventory.RemoveItem("$item_cryptkey", 1);
											character.Message(MessageHud.MessageType.TopLeft, Localization.instance.Localize("<color=yellow>Болотный ключ сломался</color>"));
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}