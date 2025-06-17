namespace VBQOL.Recycle
{
    public class RecycleUtil
    { 
        public static int GetModifiedAmount(int quality, Piece.Requirement requirement) => (int)Math.Round(VBQOL.self.resourceMultiplier.Value * requirement.GetAmount(quality), 0);

        public static bool HaveEmptySlotsForRecipe(Inventory inventory, Recipe recipe, int quality)
        {
            int emptySlots = inventory.GetEmptySlots();
            int requiredSlots = 0;

            foreach (Piece.Requirement req in recipe.m_resources) if (GetModifiedAmount(quality, req) > 0) requiredSlots++;

            if (emptySlots >= requiredSlots) return true;
            return false;
        }

        public static void AddResources(Inventory inventory, Piece.Requirement[] requirements, int qualityLevel)
        {
            foreach (Piece.Requirement requirement in requirements)
            {
                if (requirement.m_resItem)
                {
                    int amount = GetModifiedAmount(qualityLevel + 1, requirement);
                    if (amount > 0) inventory.AddItem(requirement.m_resItem.name, amount, requirement.m_resItem.m_itemData.m_quality, requirement.m_resItem.m_itemData.m_variant, 0L, "");
                }
            }
        }

        internal static void DoRecycle(Player player, InventoryGui __instance)
        {
            if (!__instance.m_craftRecipe) return;
            int num = ((__instance.m_craftUpgradeItem != null) ? (__instance.m_craftUpgradeItem.m_quality - 1) : 0);
            if ((__instance.m_craftUpgradeItem != null && !player.GetInventory().ContainsItem(__instance.m_craftUpgradeItem)) || (__instance.m_craftUpgradeItem == null && HaveEmptySlotsForRecipe(player.GetInventory(), __instance.m_craftRecipe, num + 1)))
                return;
          /*  int variant = __instance.m_craftUpgradeItem.m_variant;
            long playerID = player.GetPlayerID();
            string playerName = player.GetPlayerName();*/
            if (__instance.m_craftUpgradeItem != null)
            {
             /*   if (num >= 1)
                {
                    player.UnequipItem(__instance.m_craftUpgradeItem);
                    if (VBQOL.self.preserveOriginalItem.Value) __instance.m_craftUpgradeItem.m_quality = num;
                    else
                    {
                        player.GetInventory().RemoveItem(__instance.m_craftUpgradeItem);
                        player.GetInventory().AddItem(__instance.m_craftRecipe.m_item.gameObject.name, __instance.m_craftRecipe.m_amount, num, variant, playerID, playerName);
                    }
                }
                else
                {*/
                    player.UnequipItem(__instance.m_craftUpgradeItem);
                    player.GetInventory().RemoveItem(__instance.m_craftUpgradeItem, __instance.m_craftRecipe.m_amount);
              //  }
            }
            AddResources(player.GetInventory(), __instance.m_craftRecipe.m_resources,  num);
            __instance.UpdateCraftingPanel(focusView: true);
            CraftingStation currentCraftingStation = Player.m_localPlayer.GetCurrentCraftingStation();
            if ((bool)currentCraftingStation) currentCraftingStation.m_craftItemDoneEffects.Create(player.transform.position, Quaternion.identity);
            else __instance.m_craftItemDoneEffects.Create(player.transform.position, Quaternion.identity);
        }
    }
}