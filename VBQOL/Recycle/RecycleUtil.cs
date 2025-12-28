namespace VBQOL.Recycle
{
    public static class RecycleUtil
    {
        internal static GameObject recycleObject;
        internal static Button recycleButton;

        internal static ConfigEntry<TabPositions> tabPosition;

        public enum TabPositions
        {
            Left,
            Middle,
            Right,
        }

        internal static ConfigEntry<float> resourceMultiplier;
        internal static ConfigEntry<bool> preserveOriginalItem;
        internal static ConfigEntry<string> recyclebuttontext;

        internal static bool InTabDeconstruct() => !recycleButton.interactable;

        public static void ForceRebuildRecycleTab()
        {
            if (recycleObject)
            {
                Object.Destroy(recycleObject);
                recycleObject = null;
                recycleButton = null;
            }
        
            RebuildRecycleTab();
            Debug.LogWarning("Кнопка 'Разбор' пересоздана с новой позицией: " + tabPosition.Value);
        }
        
        public static void RebuildRecycleTab()
        {
            if (recycleObject) return;

            Debug.LogWarning("Создана кнопка 'Разобрать'");

            recycleObject = Object.Instantiate(InventoryGui.instance.m_tabUpgrade.gameObject, InventoryGui.instance.m_tabUpgrade.transform.parent);
            if (!recycleObject)
            {
                Debug.LogWarning("Не удалось создать кнопку 'Разобрать'.");
                return;
            }

            recycleObject.name = "Recycle";
            recycleObject.GetComponentInChildren<TMP_Text>().text = "Разбор";

            recycleButton = recycleObject.GetComponent<Button>();
            recycleButton.transform.localPosition = new Vector3(
                recycleObject.transform.localPosition.x + ((recycleObject.GetComponent<RectTransform>().rect.width + 10f) * ((int)tabPosition.Value + 1)),
                recycleObject.transform.localPosition.y, recycleObject.transform.localPosition.z
            );
            recycleButton.name = "RecycleButton";
            recycleButton.onClick.RemoveAllListeners();
            recycleButton.onClick.AddListener(() =>
            {
                Debug.LogWarning("Selected recycle");
                recycleButton.interactable = false;
                InventoryGui.m_instance.m_tabCraft.interactable = true;
                InventoryGui.m_instance.m_tabUpgrade.interactable = true;
                InventoryGui.m_instance.UpdateCraftingPanel();
            });

            recycleObject.SetActive(false);
        }

        public static int GetModifiedAmount(int quality, Piece.Requirement requirement) => (int)Math.Round(resourceMultiplier.Value * requirement.GetAmount(quality), 0);

        public static bool HaveEmptySlotsForRecipe(Inventory inventory, Recipe recipe, int quality)
        {
            int requiredSlots = recipe.m_resources.Count(req => GetModifiedAmount(quality, req) > 0);
            return inventory.GetEmptySlots() >= requiredSlots;
        }

        public static void AddResources(Inventory inventory, Piece.Requirement[] requirements, int qualityLevel)
        {
            foreach (var req in requirements.Where(r => r.m_resItem is not null))
            {
                int amount = GetModifiedAmount(qualityLevel + 1, req);
                if (amount > 0) inventory.AddItem(req.m_resItem.name, amount, req.m_resItem.m_itemData.m_quality, req.m_resItem.m_itemData.m_variant, 0L, "");
            }
        }

        internal static void DoRecycle(Player player, InventoryGui gui)
        {
            if (gui.m_craftRecipe is null) return;

            int quality = gui.m_craftUpgradeItem?.m_quality - 1 ?? 0;
            bool isUpgrade = gui.m_craftUpgradeItem is not null;
            bool canRecycle = isUpgrade ? player.GetInventory().ContainsItem(gui.m_craftUpgradeItem) : HaveEmptySlotsForRecipe(player.GetInventory(), gui.m_craftRecipe, quality + 1);

            if (!canRecycle) return;

            if (isUpgrade)
            {
                player.UnequipItem(gui.m_craftUpgradeItem);
                player.GetInventory().RemoveItem(gui.m_craftUpgradeItem, gui.m_craftRecipe.m_amount);
            }

            AddResources(player.GetInventory(), gui.m_craftRecipe.m_resources, quality);
            gui.UpdateCraftingPanel(focusView: true);

            var station = Player.m_localPlayer.GetCurrentCraftingStation();
            var effects = station?.m_craftItemDoneEffects ?? gui.m_craftItemDoneEffects;
            effects.Create(player.transform.position, Quaternion.identity);
        }
    }
}