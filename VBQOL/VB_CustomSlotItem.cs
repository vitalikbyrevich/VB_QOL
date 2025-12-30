namespace VBQOL
{
	internal class VB_CustomSlotItem : MonoBehaviour
	{
		public string m_slotName;
		public static ConfigEntry<string> ItemSlotPairs;
		
		public static class HumanoidExtensions
		{
			private static readonly Dictionary<string, Traverse> traverseCache = new Dictionary<string, Traverse>();

			public static void SetupEquipment(Humanoid humanoid)
			{
				var key = "SetupEquipment";
				if (!traverseCache.TryGetValue(key, out var traverse))
				{
					traverse = Traverse.Create(typeof(Humanoid)).Method("SetupEquipment");
					traverseCache[key] = traverse;
				}
				traverse.GetValue(humanoid);
			}

			public static bool HaveSetEffect(Humanoid humanoid, ItemDrop.ItemData item)
			{
				var key = "HaveSetEffect";
				if (!traverseCache.TryGetValue(key, out var traverse))
				{
					traverse = Traverse.Create(typeof(Humanoid)).Method("HaveSetEffect", new[] { typeof(ItemDrop.ItemData) });
					traverseCache[key] = traverse;
				}
				return traverse.GetValue<bool>(humanoid, item);
			}

			public static void TriggerEquipEffect(Humanoid humanoid, ItemDrop.ItemData item)
			{
				var key = "TriggerEquipEffect";
				if (!traverseCache.TryGetValue(key, out var traverse))
				{
					traverse = Traverse.Create(typeof(Humanoid)).Method("TriggerEquipEffect", new[] { typeof(ItemDrop.ItemData) });
					traverseCache[key] = traverse;
				}
				traverse.GetValue(humanoid, item);
			}

			public static void UpdateEquipmentStatusEffects(Humanoid humanoid)
			{
				var key = "UpdateEquipmentStatusEffects";
				if (!traverseCache.TryGetValue(key, out var traverse))
				{
					traverse = Traverse.Create(typeof(Humanoid)).Method("UpdateEquipmentStatusEffects");
					traverseCache[key] = traverse;
				}
				traverse.GetValue(humanoid);
			}
		}

		public static IEnumerable<(string itemName, string slotName)> ParseItemSlotPairs(string configValue)
		{
			if (string.IsNullOrWhiteSpace(configValue)) return Enumerable.Empty<(string, string)>();

			var results = new List<(string, string)>();
			var pairs = configValue.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
    
			foreach (var pair in pairs)
			{
				try
				{
					var keyValue = ValidateItemSlotPair(pair.Trim());
					results.Add((keyValue[0], keyValue[1]));
				}
				catch (Exception e)
				{
					Debug.LogWarning($"[CustomSlotItem] Failed to parse item-slot pair '{pair}': {e.Message}");
				}
			}
			return results;
		}

		[HarmonyPatch(typeof(Humanoid))]
		[HarmonyPriority(Priority.VeryHigh)]
		public class HumanoidPatch
		{
			private static bool ShouldProcess(Humanoid humanoid) => humanoid && humanoid;

			public static HashSet<StatusEffect> GetStatusEffectsFromCustomSlotItems(Humanoid __instance)
			{
				if (!ShouldProcess(__instance)) return new HashSet<StatusEffect>();

				HashSet<StatusEffect> statuses = new HashSet<StatusEffect>();

				foreach (ItemDrop.ItemData itemData in VB_CustomSlotManager.AllSlotItems(__instance))
				{
					if (itemData?.m_shared?.m_equipStatusEffect) statuses.Add(itemData.m_shared.m_equipStatusEffect);

					if (itemData != null && __instance.HaveSetEffect(itemData)) statuses.Add(itemData.m_shared.m_setStatusEffect);
				}
				return statuses;
			}

			[HarmonyPatch(nameof(Humanoid.Awake))]
			[HarmonyPostfix]
			static void AwakePostfix(ref Humanoid __instance)
			{
				if (ShouldProcess(__instance)) VB_CustomSlotManager.Register(__instance);
			}

			[HarmonyPatch(nameof(Humanoid.OnDestroy))]
			[HarmonyPostfix]
			static void OnDestroyPostfix(ref Humanoid __instance) => VB_CustomSlotManager.Unregister(__instance);

			[HarmonyPatch(nameof(Humanoid.EquipItem))]
			[HarmonyPostfix]
			static void EquipItemPostfix(ref bool __result, ref Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects = true)
			{
				if (!ShouldProcess(__instance) || !__result || !VB_CustomSlotManager.IsCustomSlotItem(item)) return;

				string slotName = VB_CustomSlotManager.GetCustomSlotName(item);
				ItemDrop.ItemData existingSlotItem = VB_CustomSlotManager.GetSlotItem(__instance, slotName);
				if (existingSlotItem != null) __instance.UnequipItem(existingSlotItem, triggerEquipEffects);
				VB_CustomSlotManager.SetSlotItem(__instance, slotName, item);

				item.m_equipped = __instance.IsItemEquiped(item);
				__instance.SetupEquipment();

				if (item.m_equipped && triggerEquipEffects) __instance.TriggerEquipEffect(item);
				__result = true;
			}

			[HarmonyPatch(nameof(Humanoid.GetEquipmentWeight))]
			[HarmonyPostfix]
			static void GetEquipmentWeightPostfix(ref float __result, ref Humanoid __instance)
			{
				if (!ShouldProcess(__instance)) return;

				foreach (ItemDrop.ItemData itemData in VB_CustomSlotManager.AllSlotItems(__instance))
				{
					if (itemData?.m_shared != null) __result += itemData.m_shared.m_weight;
				}
			}

			[HarmonyPatch(nameof(Humanoid.GetSetCount))]
			[HarmonyPostfix]
			static void GetSetCountPostfix(ref int __result, ref Humanoid __instance, string setName)
			{
				if (!ShouldProcess(__instance) || string.IsNullOrEmpty(setName)) return;

				foreach (ItemDrop.ItemData itemData in VB_CustomSlotManager.AllSlotItems(__instance))
				{
					if (itemData?.m_shared?.m_setName == setName) __result++;
				}
			}

			[HarmonyPatch(nameof(Humanoid.IsItemEquiped))]
			[HarmonyPostfix]
			static void IsItemEquipedPostfix(ref bool __result, ref Humanoid __instance, ItemDrop.ItemData item)
			{
				if (!ShouldProcess(__instance) || !VB_CustomSlotManager.IsCustomSlotItem(item)) return;

				string slotName = VB_CustomSlotManager.GetCustomSlotName(item);
				__result |= VB_CustomSlotManager.GetSlotItem(__instance, slotName) == item;
			}

			[HarmonyPatch(nameof(Humanoid.UnequipAllItems))]
			[HarmonyPostfix]
			static void UnequipAllItemsPostfix(ref Humanoid __instance)
			{
				if (!ShouldProcess(__instance)) return;

				foreach (ItemDrop.ItemData itemData in VB_CustomSlotManager.AllSlotItems(__instance))
				{
					if (itemData != null) __instance.UnequipItem(itemData, false);
				}
			}

			[HarmonyPatch(nameof(Humanoid.UnequipItem))]
			[HarmonyPostfix]
			static void UnequipItemPostfix(ref Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects = true)
			{
				if (!ShouldProcess(__instance) || item == null) return;

				string slotName = VB_CustomSlotManager.GetCustomSlotName(item);
				if (item == VB_CustomSlotManager.GetSlotItem(__instance, slotName))
				{
					VB_CustomSlotManager.SetSlotItem(__instance, slotName, null);
					item.m_equipped = __instance.IsItemEquiped(item);
					__instance.UpdateEquipmentStatusEffects();
				}
			}

			[HarmonyPatch(nameof(Humanoid.UpdateEquipmentStatusEffects))]
			[HarmonyTranspiler]
			static IEnumerable<CodeInstruction> UpdateEquipmentStatusEffectsTranspiler(IEnumerable<CodeInstruction> instructionsIn)
			{
				List<CodeInstruction> instructions = instructionsIn.ToList();
				if (instructions[0].opcode != OpCodes.Newobj || instructions[1].opcode != OpCodes.Stloc_0) throw new Exception("CustomSlotItemLib transpiler injection point not found!");

				yield return instructions[0];
				yield return instructions[1];

				yield return new CodeInstruction(OpCodes.Ldloc_0);
				yield return new CodeInstruction(OpCodes.Ldarg_0);
				yield return CodeInstruction.Call(typeof(HumanoidPatch), nameof(HumanoidPatch.GetStatusEffectsFromCustomSlotItems));
				yield return CodeInstruction.Call(typeof(HashSet<StatusEffect>), nameof(HashSet<StatusEffect>.UnionWith));

				for (int index = 2; index < instructions.Count; index++)
				{
					CodeInstruction instruction = instructions[index];
					yield return instruction;
				}
			}
		}

		[HarmonyPatch(typeof(ItemDrop.ItemData))]
		[HarmonyPriority(Priority.VeryHigh)]
		private class ItemDropItemDataPatch
		{
			[HarmonyPatch(nameof(ItemDrop.ItemData.IsEquipable))]
			[HarmonyPostfix]
			static void IsEquipablePostfix(ref bool __result, ref ItemDrop.ItemData __instance) => __result |= VB_CustomSlotManager.IsCustomSlotItem(__instance);
		}

		[HarmonyPatch(typeof(Player))]
		[HarmonyPriority(Priority.VeryHigh)]
		internal class PlayerPatch
		{
			[HarmonyPatch(nameof(Player.Load))]
			[HarmonyPostfix]
			static void LoadPostfix(ref Player __instance)
			{
				if (!__instance) return;

				foreach (ItemDrop.ItemData itemData in __instance.GetInventory().GetEquippedItems())
				{
					string slotName = VB_CustomSlotManager.GetCustomSlotName(itemData);
					if (slotName != null) VB_CustomSlotManager.SetSlotItem(__instance, slotName, itemData);
				}
			}
		}

		private static string[] ValidateItemSlotPair(string rawPair)
		{
			if (rawPair == null) throw new ArgumentNullException("rawPair");

			string[] keyValue = rawPair.Split(',');
			if (keyValue.Length < 2) throw new ArgumentException("Item slot pair does not name a slot!");
			else if (keyValue.Length > 2) throw new ArgumentException("Item slot pair lists more than a Item and a slot!");
			else if (keyValue[0].IsNullOrWhiteSpace()) throw new ArgumentException("Item name is null or whitespace!");
			else if (keyValue[1].IsNullOrWhiteSpace()) throw new ArgumentException("Slot name is null or whitespace!");
			else if (!ZNetScene.instance.GetPrefab(keyValue[0])) throw new NullReferenceException($"Item \"{keyValue[0]}\" is NULL!");

			return keyValue;
		}

		[HarmonyPatch(typeof(ZNetScene))]
		[HarmonyPriority(Priority.VeryHigh)]
		public class ZNetScenePatch
		{
			[HarmonyPatch(nameof(ZNetScene.Awake))]
			[HarmonyPostfix]
			static void AwakePostfix(ref ZNetScene __instance)
			{
				if (ItemSlotPairs.Value.IsNullOrWhiteSpace()) return;

				try
				{
					foreach (var (itemName, slotName) in ParseItemSlotPairs(ItemSlotPairs.Value))
					{
						GameObject gameObject = __instance.GetPrefab(itemName);
						if (gameObject) VB_CustomSlotManager.ApplyCustomSlotItem(gameObject, slotName);
						else Debug.LogWarning($"[CustomSlotItem] Prefab not found for item: {itemName}");
					}
				}
				catch (Exception e)
				{
					Debug.LogWarning($"[CustomSlotItem] Error applying custom slot items: {e}");
				}
			}
		}

		[HarmonyPatch(typeof(FejdStartup))]
		public class FejdStartupPatch
		{
			[HarmonyPatch(nameof(FejdStartup.Update))]
			[HarmonyPostfix]
			static void UpdatePostfix()
			{
				if (Time.frameCount % 3600 == 0) VB_CustomSlotManager.CleanupDestroyed();
			}
		}
	}
}