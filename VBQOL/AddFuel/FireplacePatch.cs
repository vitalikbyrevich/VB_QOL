namespace VBQOL.AddFuel
{
    [HarmonyPatch(typeof(Fireplace))]
    static class FireplacePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Interact")]
        public static bool FireplaceInteractPrefix(ref Fireplace __instance, Humanoid user, bool hold, bool alt, ref bool __result)
        {
            if (!AddFuelUtil.AFEnable.Value) return true;
            if (!__instance.m_canRefill) return true;
            
            bool isAddOne = !Input.GetKey(AddFuelUtil.AFModifierKeyConfig.Value);
            __result = false;
            
            if (hold) return false;
            if (!__instance.m_nview.HasOwner()) __instance.m_nview.ClaimOwnership();
            
            string fuelName = __instance.m_fuelItem.m_itemData.m_shared.m_name;
            float currentFuel = __instance.m_nview.GetZDO().GetFloat("fuel");
            
            if (currentFuel > __instance.m_maxFuel - 1f)
            {
                user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", fuelName));
                return false;
            }
            
            ItemDrop.ItemData fuelItem = user.GetInventory()?.GetItem(fuelName);
            if (fuelItem == null)
            {
                if (isAddOne) return true;
                user.Message(MessageHud.MessageType.Center, "$msg_outof " + fuelName);
                return false;
            }
            
            user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", fuelName));
            
            int fuelSpaceLeft = (int)(__instance.m_maxFuel - currentFuel);
            int stackToAdd = AddFuelUtil.CalculateStackToAdd(isAddOne, fuelItem.m_stack, fuelSpaceLeft);
            
            user.GetInventory().RemoveItem(fuelItem, stackToAdd);
            AddFuelBulk(__instance, stackToAdd);
            
            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(Fireplace), "GetHoverText")]
        [HarmonyPostfix]
        public static string AddFuel_FirePlaceGetHoverText_Patch(string __result, Fireplace __instance)
        {
            if (!__instance) return __result;
            
            string finalText = __result;
            
            // Добавляем функциональность AddFuel (только для объектов с дозаправкой)
            if (AddFuelUtil.AFEnable.Value && __instance.IsBurning() && !__instance.m_wet && __instance.m_canRefill)
            {
                string modifierKey = AddFuelUtil.AFModifierKeyConfig?.Value.ToString() ?? "LeftShift";
                string useKey = AddFuelUtil.AFModifierKeyUseConfig.ToString();
                finalText = $"{finalText}\n[<color=yellow><b>{modifierKey}+{useKey}</b></color>] {AddFuelUtil.AFTextConfig?.Value ?? "Добавить стак"}";
            }
            
            return finalText;
        }

        private static void AddFuelBulk(Fireplace fireplace, int count)
        {
            if (!fireplace.m_nview.IsOwner() || count <= 0) return;
            
            float currentFuel = fireplace.m_nview.GetZDO().GetFloat("fuel");
            float newFuel = Mathf.Clamp(currentFuel + count, 0f, fireplace.m_maxFuel);
            
            fireplace.m_nview.GetZDO().Set("fuel", newFuel);
            fireplace.m_fuelAddedEffects.Create(fireplace.transform.position, fireplace.transform.rotation);
            
            // Обновляем состояние один раз для всех добавленных единиц
            Traverse.Create(fireplace).Method("UpdateState").GetValue();
        }
    }
}