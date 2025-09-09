namespace VBQOL.Recycle
{
    public static class RecycleUtil
    {
        public static int GetModifiedAmount(int quality, Piece.Requirement requirement) => (int)Math.Round(VBQOL.self.resourceMultiplier.Value * requirement.GetAmount(quality), 0);

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