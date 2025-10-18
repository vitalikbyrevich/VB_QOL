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
				bool result = true;

				if (hoverObject)
				{
					MonsterAI component = hoverObject.GetComponent<MonsterAI>();
					Character component2 = hoverObject.GetComponent<Character>();
					Tameable component3 = hoverObject.GetComponent<Tameable>();
					Humanoid component4 = hoverObject.GetComponent<Humanoid>();

					if (component || component2 || component3 || component4)
					{
						if (component2.IsTamed() && component.CanConsume(item))
						{
							string hoverName = component2.GetHoverName();
							if (component2.GetHealth() < component2.GetMaxHealth())
							{
								FeedCreature(__instance, component2, component3, component4, item);
								__instance.Message(MessageHud.MessageType.Center, $"{hoverName} подлечился и перекусил!");
								result = false;
							}
							else if (component3.IsHungry())
							{
								FeedCreature(__instance, component2, component3, component4, item);
								__instance.Message(MessageHud.MessageType.Center, $"{hoverName} с удовольствием поел!");
								result = false;
							}
							else
							{
								__instance.Message(MessageHud.MessageType.Center, $"{hoverName} не голоден.");
								result = false;
							}
						}
					}
				}
				return result;
			}

			private static void FeedCreature(Humanoid feeder, Character creature, Tameable tameable, Humanoid targetHumanoid, ItemDrop.ItemData item)
			{
				creature.Heal(25f);
				tameable.ResetFeedingTimer();
				tameable.Interact(targetHumanoid, false, false);
				feeder.DoInteractAnimation(creature.gameObject);
				feeder.Message(MessageHud.MessageType.Center, creature.GetHoverName() + " $hud_tamelove");
				feeder.GetInventory().RemoveOneItem(item);
			}
		}
	}
}