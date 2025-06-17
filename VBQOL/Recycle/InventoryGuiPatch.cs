namespace VBQOL.Recycle
{

	[HarmonyPatch(typeof(InventoryGui))]
	public class InventoryGuiPatch
	{
		[HarmonyPostfix]
		[HarmonyPatch("Update")]
		internal static void PostfixUpdate(InventoryGui __instance) => VBQOL.self?.RebuildRecycleTab();

		[HarmonyPrefix]
		[HarmonyPatch("OnTabCraftPressed")]
		internal static bool PrefixOnTabCraftPressed(InventoryGui __instance)
		{
			VBQOL.self.recycleButton.interactable = true;
			return true;
		}

		[HarmonyPrefix]
		[HarmonyPatch("OnTabUpgradePressed")]
		internal static bool PrefixOnTabUpgradePressed(InventoryGui __instance)
		{
			VBQOL.self.recycleButton.interactable = true;
			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch("SetupRequirement")]
		internal static void PostfixSetupRequirement(Transform elementRoot, Piece.Requirement req, int quality)
		{
			if (VBQOL.self.InTabDeconstruct())
			{
				TMP_Text component = elementRoot.transform.Find("res_amount").GetComponent<TMP_Text>();
				component.text = RecycleUtil.GetModifiedAmount(quality, req).ToString();
				component.color = Color.green;
			}
		}

		[HarmonyPrefix]
		[HarmonyPatch("UpdateCraftingPanel")]
		internal static bool PrefixUpdateCraftingPanel(InventoryGui __instance, bool focusView = false)
		{
			if (VBQOL.self)
			{
				Player localPlayer = Player.m_localPlayer;
				if ((bool)localPlayer.GetCurrentCraftingStation() && (localPlayer.GetCurrentCraftingStation().gameObject.name.Contains("cauldron") ||
				                                                      localPlayer.GetCurrentCraftingStation().gameObject.name.Contains("artisanstation")))
				{
					VBQOL.self.recycleObject.SetActive(value: false);
					VBQOL.self.recycleButton.interactable = true;
					return true;
				}

				if (!localPlayer.GetCurrentCraftingStation() && !localPlayer.NoCostCheat())
				{
					__instance.m_tabCraft.interactable = false;
					__instance.m_tabUpgrade.interactable = true;
					__instance.m_tabUpgrade.gameObject.SetActive(value: false);
					VBQOL.self.recycleObject.SetActive(value: false);
					VBQOL.self.recycleButton.interactable = true;
				}
				else
				{
					__instance.m_tabUpgrade.gameObject.SetActive(value: true);
					VBQOL.self.recycleObject.SetActive(value: true);
				}

				List<Recipe> available = new List<Recipe>();
				localPlayer.GetAvailableRecipes(ref available);
				__instance.UpdateRecipeList(available);
				if (__instance.m_availableRecipes.Count <= 0)
				{
					__instance.SetRecipe(-1, focusView);
					return false;
				}

				if (__instance.m_selectedRecipe.Recipe)
				{
					int selectedRecipeIndex = __instance.GetSelectedRecipeIndex(acceptOneLevelHigher: false);
					__instance.SetRecipe(selectedRecipeIndex, focusView);
					return false;
				}
				__instance.SetRecipe(0, focusView);
				return false;
			}

			return true;
		}

		[HarmonyPostfix]
		[HarmonyPatch("UpdateRecipeList")]
		internal static void PostfixUpdateRecipeList(InventoryGui __instance, List<Recipe> recipes)
		{
			if (!VBQOL.self.InTabDeconstruct()) return;

			Player localPlayer = Player.m_localPlayer;
			Inventory inventory = localPlayer.GetInventory();
			foreach (InventoryGui.RecipeDataPair availableRecipe in __instance.m_availableRecipes) Object.Destroy(availableRecipe.InterfaceElement);

			__instance.m_availableRecipes.Clear();
			List<KeyValuePair<Recipe, ItemDrop.ItemData>> list = new List<KeyValuePair<Recipe, ItemDrop.ItemData>>();
			for (int i = 0; i < recipes.Count; i++)
			{
				Recipe recipe = recipes[i];
				if (recipe.m_item.m_itemData.m_shared.m_maxQuality < 1) continue;

				__instance.m_tempItemList.Clear();
				if (recipe.m_item.m_itemData.m_shared.m_maxStackSize == 1) inventory.GetAllItems(recipe.m_item.m_itemData.m_shared.m_name, __instance.m_tempItemList);
				else
				{
					for (int j = 0; j < inventory.m_inventory.Count; j++)
					{
						if (inventory.m_inventory[j].m_shared.m_name.Equals(recipe.m_item.m_itemData.m_shared.m_name) && inventory.m_inventory[j].m_stack >= recipe.m_amount)
						{
							__instance.m_tempItemList.Add(inventory.m_inventory[j]);
							break;
						}
					}
				}

				foreach (ItemDrop.ItemData tempItem in __instance.m_tempItemList) if (tempItem.m_quality >= 1) list.Add(new KeyValuePair<Recipe, ItemDrop.ItemData>(recipe, tempItem));
			}

			IEnumerable<int> equipped = from item in inventory.GetEquippedItems() select item.GetHashCode();
			list.RemoveAll(m => equipped.Contains(m.Value.GetHashCode()));
			List<ItemDrop.ItemData> list2 = new List<ItemDrop.ItemData>();
			inventory.GetBoundItems(list2);
			IEnumerable<int> hotbarItemsHashes = list2.Select(item => item.GetHashCode());
			list.RemoveAll(m => hotbarItemsHashes.Contains(m.Value.GetHashCode()));
			foreach (KeyValuePair<Recipe, ItemDrop.ItemData> item in list) __instance.AddRecipeToList(localPlayer, item.Key, item.Value, canCraft: true);

			float b = __instance.m_availableRecipes.Count * __instance.m_recipeListSpace;
			b = Mathf.Max(__instance.m_recipeListBaseSize, b);
			__instance.m_recipeListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, b);
		}

		[HarmonyPrefix]
		[HarmonyPatch("UpdateRecipe")]
		internal static bool PrefixUpdateRecipe(InventoryGui __instance, Player player, float dt)
		{
			if (VBQOL.self.InTabDeconstruct())
			{
				CraftingStation currentCraftingStation = player.GetCurrentCraftingStation();
				if ((bool)currentCraftingStation)
				{
					__instance.m_craftingStationName.text = Localization.instance.Localize(currentCraftingStation.m_name);
					__instance.m_craftingStationIcon.gameObject.SetActive(value: true);
					__instance.m_craftingStationIcon.sprite = currentCraftingStation.m_icon;
					int level = currentCraftingStation.GetLevel();
					__instance.m_craftingStationLevel.text = level.ToString();
					__instance.m_craftingStationLevelRoot.gameObject.SetActive(value: true);
				}
				else
				{
					__instance.m_craftingStationName.text = Localization.instance.Localize("$hud_crafting");
					__instance.m_craftingStationIcon.gameObject.SetActive(value: false);
					__instance.m_craftingStationLevelRoot.gameObject.SetActive(value: false);
				}

				if ((bool)__instance.m_selectedRecipe.Recipe)
				{
					__instance.m_recipeIcon.enabled = true;
					__instance.m_recipeName.enabled = true;
					ItemDrop.ItemData itemData = __instance.m_selectedRecipe.ItemData;
					if (itemData.m_quality == 1) __instance.m_recipeDecription.enabled = false;
					else __instance.m_recipeDecription.enabled = true;

					_ = __instance.m_selectedRecipe.ItemData;
				    int num = ((itemData == null) ? 1 : ((itemData.m_quality >= 1) ? (itemData.m_quality - 1) : 0));
					bool flag = num <= __instance.m_selectedRecipe.Recipe.m_item.m_itemData.m_shared.m_maxQuality;
					int num2 = (int)itemData?.m_variant;
					int amount = __instance.m_multiCraftAmount;
					__instance.m_recipeIcon.sprite = __instance.m_selectedRecipe.Recipe.m_item.m_itemData.m_shared.m_icons[num2];
					string text = Localization.instance.Localize(__instance.m_selectedRecipe.Recipe.m_item.m_itemData.m_shared.m_name);
					if (__instance.m_selectedRecipe.Recipe.m_amount > 1) text = text + " x" + __instance.m_selectedRecipe.Recipe.m_amount;

					__instance.m_recipeName.text = text;
					__instance.m_recipeDecription.text = Localization.instance.Localize(ItemDrop.ItemData.GetTooltip(__instance.m_selectedRecipe.Recipe.m_item.m_itemData, num, crafting: true, Game.m_worldLevel));
				/*	if (itemData != null)
					{*/
						__instance.m_itemCraftType.gameObject.SetActive(value: true);
					/*	if (itemData.m_quality <= 1)
						{*/
							__instance.m_itemCraftType.text = "Предмет будет разобран";
					/*	}
						else
						{
							string text2 = Localization.instance.Localize(itemData.m_shared.m_name);
							__instance.m_itemCraftType.text = "Downgrade " + text2 + " quality to " + (itemData.m_quality - 1);
						}*/
				/*	}
					else
					{
						__instance.m_itemCraftType.gameObject.SetActive(value: false);
					}*/

					__instance.m_variantButton.gameObject.SetActive(__instance.m_selectedRecipe.Recipe.m_item.m_itemData.m_shared.m_variants > 1 && __instance.m_selectedRecipe.ItemData == null);
					__instance.SetupRequirementList(num + 1, player, flag, amount);
					int num3 = 0;
					CraftingStation requiredStation = __instance.m_selectedRecipe.Recipe.GetRequiredStation(num);
					if (requiredStation && flag)
					{
						__instance.m_minStationLevelIcon.gameObject.SetActive(value: true);
						__instance.m_minStationLevelText.text = num3.ToString();
						if (!currentCraftingStation || currentCraftingStation.GetLevel() < num3) __instance.m_minStationLevelText.color = ((Mathf.Sin(Time.time * 10f) > 0f) ? Color.red : __instance.m_minStationLevelBasecolor);
						else __instance.m_minStationLevelText.color = __instance.m_minStationLevelBasecolor;
					}
					else __instance.m_minStationLevelIcon.gameObject.SetActive(value: false);

					bool flag3 = RecycleUtil.HaveEmptySlotsForRecipe(player.GetInventory(), __instance.m_selectedRecipe.Recipe, num + 1);
					bool flag4 = !requiredStation || ((bool)currentCraftingStation && currentCraftingStation.CheckUsable(player, showMessage: false));
					__instance.m_craftButton.interactable = ((flag4) || player.NoCostCheat()) && flag3 && flag;
					__instance.m_craftButton.GetComponentInChildren<TMP_Text>().text = "Разобрать";
					UITooltip component = __instance.m_craftButton.GetComponent<UITooltip>();
					if (!flag3) component.m_text = Localization.instance.Localize("$inventory_full");
					else if (!flag4) component.m_text = Localization.instance.Localize("$msg_missingstation");
					else component.m_text = "";
				}
				else
				{
					__instance.m_recipeIcon.enabled = false;
					__instance.m_recipeName.enabled = false;
					__instance.m_recipeDecription.enabled = false;
					__instance.m_qualityPanel.gameObject.SetActive(value: false);
					__instance.m_minStationLevelIcon.gameObject.SetActive(value: false);
					__instance.m_craftButton.GetComponent<UITooltip>().m_text = "";
					__instance.m_variantButton.gameObject.SetActive(value: false);
					__instance.m_itemCraftType.gameObject.SetActive(value: false);
					for (int i = 0; i < __instance.m_recipeRequirementList.Length; i++) InventoryGui.HideRequirement(__instance.m_recipeRequirementList[i].transform);

					__instance.m_craftButton.interactable = false;
				}

				if (__instance.m_craftTimer < 0f)
				{
					__instance.m_craftProgressPanel.gameObject.SetActive(value: false);
					__instance.m_craftButton.gameObject.SetActive(value: true);
					return false;
				}

				__instance.m_craftButton.gameObject.SetActive(value: false);
				__instance.m_craftProgressPanel.gameObject.SetActive(value: true);
				__instance.m_craftProgressBar.SetMaxValue(__instance.m_craftDuration);
				__instance.m_craftProgressBar.SetValue(__instance.m_craftTimer);
				__instance.m_craftTimer += dt;
				if (__instance.m_craftTimer >= __instance.m_craftDuration)
				{
					if (VBQOL.self.InTabDeconstruct()) RecycleUtil.DoRecycle(player, __instance);
					else __instance.DoCrafting(player);

					__instance.m_craftTimer = -1f;
				}
				return false;
			}
			return true;
		}
	}
}