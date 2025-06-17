namespace VBQOL
{
	[HarmonyPatch]
    public class VB_FeedFromHandPatch
	{
		[HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UseItem))]
		public static class Humanoid_UseItem_Prefix_Patch
		{
			public static bool Prefix(Humanoid __instance, ItemDrop.ItemData item)
			{
				GameObject hoverObject = __instance.GetHoverObject();
                bool result;
                if (!hoverObject) result = true;
                else
                {
	                if (!hoverObject.GetComponent<MonsterAI>() && !hoverObject.GetComponent<Character>() && !hoverObject.GetComponent<Tameable>() && !hoverObject.GetComponent<Humanoid>()) result = true;
	                else
	                {
						MonsterAI component = hoverObject.GetComponent<MonsterAI>();
						Character component2 = hoverObject.GetComponent<Character>();
						Tameable component3 = hoverObject.GetComponent<Tameable>();
						Humanoid component4 = hoverObject.GetComponent<Humanoid>();
                        if (component2.IsTamed() && component.CanConsume(item))
						{
							string hoverName = component2.GetHoverName();
                            if (component2.GetHealth() < component2.GetMaxHealth())
							{
								component2.Heal(50f);
								component3.ResetFeedingTimer();
								component3.Interact(component4, false, false);
								__instance.DoInteractAnimation(hoverObject);
								__instance.Message(MessageHud.MessageType.Center, hoverName + " $hud_tamelove");
								Inventory inventory = __instance.GetInventory();
								inventory.RemoveOneItem(item);
								result = false;
							}
							if (component3.IsHungry())
							{
								component2.Heal(50f);
								component3.ResetFeedingTimer();
								component3.Interact(component4, false, false);
								__instance.DoInteractAnimation(hoverObject);
								__instance.Message(MessageHud.MessageType.Center, hoverName + " $hud_tamelove");
								Inventory inventory = __instance.GetInventory();
								inventory.RemoveOneItem(item);
								result = false;
							}
							else
							{
								__instance.Message(MessageHud.MessageType.Center, hoverName + " $msg_nomore");
								result = false;
							}
						}
						else result = true;
	                }
				}
				return result;
			}
		}
	}
}