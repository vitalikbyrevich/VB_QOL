namespace VBQOL.Recycle
{
    [HarmonyPatch(typeof(Humanoid))]
    internal class HumanoidPatch
    {   
        [HarmonyPatch(nameof(Humanoid.EquipItem))]
        [HarmonyPostfix]
        private static void EquipItem(Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects = true)
        {
            if (__instance.IsPlayer()) InventoryGui.instance?.UpdateCraftingPanel();
        }
            
        [HarmonyPatch(nameof(Humanoid.UnequipItem))]
        [HarmonyPostfix]
        private static void UnequipItem(Humanoid __instance, ItemDrop.ItemData item, bool triggerEquipEffects = true)
        {
            if (__instance.IsPlayer()) InventoryGui.instance?.UpdateCraftingPanel();
        }
    }
}