namespace VBQOL.Recycle
{
    [HarmonyPatch(typeof(InventoryGui))]
    public static class InventoryGuiPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryGui.Update))]
        private static void PostfixUpdate(InventoryGui __instance) 
        {
            VBQOL.self?.RebuildRecycleTab();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(InventoryGui.OnTabCraftPressed))]
        [HarmonyPatch(nameof(InventoryGui.OnTabUpgradePressed))]
        private static bool Prefix_EnableRecycleButton(InventoryGui __instance)
        {
            VBQOL.self.recycleButton.interactable = true;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryGui.SetupRequirement))]
        private static void PostfixSetupRequirement(Transform elementRoot, Piece.Requirement req, int quality)
        {
            if (!VBQOL.self.InTabDeconstruct()) return;

            var amountText = elementRoot.Find("res_amount").GetComponent<TMP_Text>();
            amountText.text = RecycleUtil.GetModifiedAmount(quality, req).ToString();
            amountText.color = Color.green;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(InventoryGui.UpdateCraftingPanel))]
        private static bool PrefixUpdateCraftingPanel(InventoryGui __instance, bool focusView)
        {
            if (!VBQOL.self) return true;

            Player localPlayer = Player.m_localPlayer;
            CraftingStation currentCraftingStation = localPlayer.GetCurrentCraftingStation();
            
            bool isSpecialStation = false;
            if (currentCraftingStation)
            {
                string stationName = currentCraftingStation.gameObject.name;
                isSpecialStation = stationName.Contains("cauldron") || stationName.Contains("artisanstation");
            }

            if (currentCraftingStation && isSpecialStation)
            {
                VBQOL.self.recycleObject.SetActive(false);
                VBQOL.self.recycleButton.interactable = true;
                return true;
            }

            if (!currentCraftingStation && !localPlayer.NoCostCheat())
            {
                __instance.m_tabCraft.interactable = false;
                __instance.m_tabUpgrade.interactable = true;
                __instance.m_tabUpgrade.gameObject.SetActive(false);
                VBQOL.self.recycleObject.SetActive(false);
                VBQOL.self.recycleButton.interactable = true;
                return true;
            }

            __instance.m_tabUpgrade.gameObject.SetActive(true);
            VBQOL.self.recycleObject.SetActive(true);

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
                int selectedRecipeIndex = __instance.GetSelectedRecipeIndex(false);
                __instance.SetRecipe(selectedRecipeIndex, focusView);
                return false;
            }

            __instance.SetRecipe(0, focusView);
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(InventoryGui.UpdateRecipeList))]
        private static void PostfixUpdateRecipeList(InventoryGui __instance, List<Recipe> recipes)
        {
            if (!VBQOL.self.InTabDeconstruct()) return;

            Player localPlayer = Player.m_localPlayer;
            Inventory inventory = localPlayer.GetInventory();

            foreach (var recipeData in __instance.m_availableRecipes) Object.Destroy(recipeData.InterfaceElement);
            __instance.m_availableRecipes.Clear();

            List<KeyValuePair<Recipe, ItemDrop.ItemData>> recyclableItems = new List<KeyValuePair<Recipe, ItemDrop.ItemData>>();

            foreach (Recipe recipe in recipes)
            {
                if (recipe.m_item.m_itemData.m_shared.m_maxQuality < 1) continue;

                __instance.m_tempItemList.Clear();
                if (recipe.m_item.m_itemData.m_shared.m_maxStackSize == 1) inventory.GetAllItems(recipe.m_item.m_itemData.m_shared.m_name, __instance.m_tempItemList);
                else
                {
                    foreach (var item in inventory.m_inventory)
                    {
                        if (item.m_shared.m_name == recipe.m_item.m_itemData.m_shared.m_name && item.m_stack >= recipe.m_amount)
                        {
                            __instance.m_tempItemList.Add(item);
                            break;
                        }
                    }
                }

                foreach (var item in __instance.m_tempItemList)
                {
                    if (item.m_quality >= 1) recyclableItems.Add(new KeyValuePair<Recipe, ItemDrop.ItemData>(recipe, item));
                }
            }

            recyclableItems.RemoveAll(pair => inventory.GetEquippedItems().Any(x => x.GetHashCode() == pair.Value.GetHashCode()));

            List<ItemDrop.ItemData> boundItems = new List<ItemDrop.ItemData>();
            inventory.GetBoundItems(boundItems);
            recyclableItems.RemoveAll(pair => boundItems.Any(x => x.GetHashCode() == pair.Value.GetHashCode()));

            foreach (var item in recyclableItems) __instance.AddRecipeToList(localPlayer, item.Key, item.Value, true);

            float height = Mathf.Max(__instance.m_recipeListBaseSize, __instance.m_availableRecipes.Count * __instance.m_recipeListSpace);
            __instance.m_recipeListRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(InventoryGui.UpdateRecipe))]
        private static bool PrefixUpdateRecipe(InventoryGui __instance, Player player, float dt)
        {
            if (!VBQOL.self.InTabDeconstruct()) return true;

            CraftingStation currentCraftingStation = player.GetCurrentCraftingStation();
            if (currentCraftingStation)
            {
                __instance.m_craftingStationName.text = Localization.instance.Localize(currentCraftingStation.m_name);
                __instance.m_craftingStationIcon.gameObject.SetActive(true);
                __instance.m_craftingStationIcon.sprite = currentCraftingStation.m_icon;
                int level = currentCraftingStation.GetLevel();
                __instance.m_craftingStationLevel.text = level.ToString();
                __instance.m_craftingStationLevelRoot.gameObject.SetActive(true);
            }
            else
            {
                __instance.m_craftingStationName.text = Localization.instance.Localize("$hud_crafting");
                __instance.m_craftingStationIcon.gameObject.SetActive(false);
                __instance.m_craftingStationLevelRoot.gameObject.SetActive(false);
            }

            if (__instance.m_selectedRecipe.Recipe)
            {
                ItemDrop.ItemData itemData = __instance.m_selectedRecipe.ItemData;
                int quality = (itemData == null) ? 1 : (itemData.m_quality >= 1 ? itemData.m_quality - 1 : 0);
                bool canRecycle = quality <= __instance.m_selectedRecipe.Recipe.m_item.m_itemData.m_shared.m_maxQuality;
                
                __instance.m_recipeIcon.enabled = true;
                __instance.m_recipeName.enabled = true;
                __instance.m_recipeDecription.enabled = itemData?.m_quality != 1;

                int variant = itemData?.m_variant ?? 0;
                __instance.m_recipeIcon.sprite = __instance.m_selectedRecipe.Recipe.m_item.m_itemData.m_shared.m_icons[variant];
                
                string itemName = Localization.instance.Localize(__instance.m_selectedRecipe.Recipe.m_item.m_itemData.m_shared.m_name);
                if (__instance.m_selectedRecipe.Recipe.m_amount > 1) itemName += " x" + __instance.m_selectedRecipe.Recipe.m_amount;
                __instance.m_recipeName.text = itemName;
                
                __instance.m_recipeDecription.text = Localization.instance.Localize(ItemDrop.ItemData.GetTooltip(__instance.m_selectedRecipe.Recipe.m_item.m_itemData, quality, true, Game.m_worldLevel));

                __instance.m_itemCraftType.gameObject.SetActive(true);
                __instance.m_itemCraftType.text = "Предмет будет разобран";
                __instance.m_variantButton.gameObject.SetActive(__instance.m_selectedRecipe.Recipe.m_item.m_itemData.m_shared.m_variants > 1 && __instance.m_selectedRecipe.ItemData == null);

                __instance.SetupRequirementList(quality + 1, player, canRecycle, __instance.m_multiCraftAmount);

                CraftingStation requiredStation = __instance.m_selectedRecipe.Recipe.GetRequiredStation(quality);
                if (requiredStation && canRecycle)
                {
                    int minStationLevel = 0;
                    __instance.m_minStationLevelIcon.gameObject.SetActive(true);
                    __instance.m_minStationLevelText.text = minStationLevel.ToString();
                    if (!currentCraftingStation || currentCraftingStation.GetLevel() < minStationLevel)
                    {
                        __instance.m_minStationLevelText.color = (Mathf.Sin(Time.time * 10f) > 0f) ? Color.red : __instance.m_minStationLevelBasecolor;
                    }
                    else __instance.m_minStationLevelText.color = __instance.m_minStationLevelBasecolor;
                }
                else __instance.m_minStationLevelIcon.gameObject.SetActive(false);

                bool hasEmptySlots = RecycleUtil.HaveEmptySlotsForRecipe(
                    player.GetInventory(), __instance.m_selectedRecipe.Recipe, quality + 1);
                
                bool hasStation = !requiredStation || (currentCraftingStation && currentCraftingStation.CheckUsable(player, false));

                __instance.m_craftButton.interactable = (hasStation || player.NoCostCheat()) && hasEmptySlots && canRecycle;
                __instance.m_craftButton.GetComponentInChildren<TMP_Text>().text = "Разобрать";
                
                UITooltip tooltip = __instance.m_craftButton.GetComponent<UITooltip>();
                if (!hasEmptySlots) tooltip.m_text = Localization.instance.Localize("$inventory_full");
                else if (!hasStation) tooltip.m_text = Localization.instance.Localize("$msg_missingstation");
                else tooltip.m_text = "";
            }
            else
            {
                __instance.m_recipeIcon.enabled = false;
                __instance.m_recipeName.enabled = false;
                __instance.m_recipeDecription.enabled = false;
                __instance.m_qualityPanel.gameObject.SetActive(false);
                __instance.m_minStationLevelIcon.gameObject.SetActive(false);
                __instance.m_craftButton.GetComponent<UITooltip>().m_text = "";
                __instance.m_variantButton.gameObject.SetActive(false);
                __instance.m_itemCraftType.gameObject.SetActive(false);
                
                for (int i = 0; i < __instance.m_recipeRequirementList.Length; i++) InventoryGui.HideRequirement(__instance.m_recipeRequirementList[i].transform);
                __instance.m_craftButton.interactable = false;
            }

            if (__instance.m_craftTimer < 0f)
            {
                __instance.m_craftProgressPanel.gameObject.SetActive(false);
                __instance.m_craftButton.gameObject.SetActive(true);
                return false;
            }

            __instance.m_craftButton.gameObject.SetActive(false);
            __instance.m_craftProgressPanel.gameObject.SetActive(true);
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
    }
}