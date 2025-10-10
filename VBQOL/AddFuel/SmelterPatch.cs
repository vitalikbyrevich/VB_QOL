namespace VBQOL.AddFuel
{
    [HarmonyPatch]
    static class SmelterPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Smelter), nameof(Smelter.OnAddOre))]
        static bool SmelterOnAddOrePrefix(ref Smelter __instance, ref Switch sw, ref Humanoid user, ItemDrop.ItemData item, ref bool __result)
        {
            if (!AddFuelUtil.AFEnable.Value) return true;

            bool isAddOne = !Input.GetKey(AddFuelUtil.AFModifierKeyConfig.Value);
            int queueSizeNow = AddFuelUtil.GetQueueSizeSafe(__instance);
            
            if (queueSizeNow >= __instance.m_maxOre)
            {
                user.Message(MessageHud.MessageType.Center, "$msg_itsfull");
                return false;
            }

            if (item == null) item = AddFuelUtil.FindCookableItem(__instance, user.GetInventory());

            if (item == null)
            {
                if (isAddOne) return true;
                user.Message(MessageHud.MessageType.Center, "$msg_noprocessableitems");
                return false;
            }

            if (!Traverse.Create(__instance).Method("IsItemAllowed", item.m_dropPrefab.name).GetValue<bool>())
            {
                user.Message(MessageHud.MessageType.Center, "$msg_wontwork");
                return false;
            }

            user.Message(MessageHud.MessageType.Center, "$msg_added " + item.m_shared.m_name);

            int queueSizeLeft = __instance.m_maxOre - queueSizeNow;
            int stackToAdd = AddFuelUtil.CalculateStackToAdd(isAddOne, item.m_stack, queueSizeLeft);
            
            user.GetInventory().RemoveItem(item, stackToAdd);
            AddFuelUtil.AddOreBulk(__instance, item.m_dropPrefab.name, stackToAdd);
            
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Smelter), nameof(Smelter.OnAddFuel))]
        static bool Prefix(ref Smelter __instance, ref bool __result, Switch sw, Humanoid user, ItemDrop.ItemData item)
        {
            if (!AddFuelUtil.AFEnable.Value) return true;

            bool isAddOne = !Input.GetKey(AddFuelUtil.AFModifierKeyConfig.Value);
            string fuelName = __instance.m_fuelItem.m_itemData.m_shared.m_name;

            if (item != null && item.m_shared.m_name != fuelName)
            {
                user.Message(MessageHud.MessageType.Center, "$msg_wrongitem");
                return false;
            }

            float fuelNow = AddFuelUtil.GetFuelSafe(__instance);
            if (fuelNow > __instance.m_maxFuel - 1)
            {
                user.Message(MessageHud.MessageType.Center, "$msg_itsfull");
                return false;
            }

            item = user.GetInventory().GetItem(fuelName);

            if (item == null)
            {
                if (isAddOne) return true;
                user.Message(MessageHud.MessageType.Center, $"$msg_donthaveany {fuelName}");
                return false;
            }

            user.Message(MessageHud.MessageType.Center, $"$msg_added {fuelName}");

            int fuelLeft = (int)(__instance.m_maxFuel - fuelNow);
            int stackToAdd = AddFuelUtil.CalculateStackToAdd(isAddOne, item.m_stack, fuelLeft);

            user.GetInventory().RemoveItem(item, stackToAdd);
            AddFuelUtil.AddFuelBulk(__instance, stackToAdd);
            
            __result = true;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Smelter), nameof(Smelter.OnHoverAddFuel))]
        static string AddFuel_OnHoverAddFuel_Patch(string __result, Smelter __instance)
        {
            if (!AddFuelUtil.AFEnable.Value || !__instance) return __result;
            
            string modifierKey = AddFuelUtil.AFModifierKeyConfig.Value.ToString();
            string useKey = AddFuelUtil.AFModifierKeyUseConfig.ToString();
            return $"{__result}\n[<color=yellow><b>{modifierKey}+{useKey}</b></color>] {AddFuelUtil.AFTextConfig.Value}";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Smelter), nameof(Smelter.OnHoverAddOre))]
        static string AddFuel_OnHoverAddOre_Patch(string __result, Smelter __instance)
        {
            if (!AddFuelUtil.AFEnable.Value || !__instance) return __result;

            string modifierKey = AddFuelUtil.AFModifierKeyConfig.Value.ToString();
            string useKey = AddFuelUtil.AFModifierKeyUseConfig.ToString();
            return $"{__result}\n[<color=yellow><b>{modifierKey}+{useKey}</b></color>] {AddFuelUtil.AFTextConfig.Value}";
        }
    }
}