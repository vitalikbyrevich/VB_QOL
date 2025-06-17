namespace VBQOL.AddFuel
{
    [HarmonyPatch(typeof(Fireplace))]
    static class FireplacePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Interact")]
        public static bool FireplaceInteractPrefix(ref Fireplace __instance, Humanoid user, bool hold, bool alt, ref bool __result)
        {
            if (!VBQOL.AFEnable.Value) return true;
            bool useAllFuel = !Input.GetKey(VBQOL.AFModifierKeyConfig.Value);
            __result = false;
            if (hold) return false;
            if (!__instance.m_nview.HasOwner()) __instance.m_nview.ClaimOwnership();
            string fuelName = __instance.m_fuelItem.m_itemData.m_shared.m_name;
            ZLog.Log("Found fuel " + fuelName);
            float currentFuel = Mathf.CeilToInt(__instance.m_nview.GetZDO().GetFloat("fuel"));
            if (currentFuel > __instance.m_maxFuel - 1f)
            {
                user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", fuelName));
                return false;
            }
            ZLog.Log("Checking Inventory for fuel " + fuelName);
            var itemData = user.GetInventory()?.GetItem(fuelName);
            if (itemData == null)
            {
                if (useAllFuel) return true;
                user.Message(MessageHud.MessageType.Center, "$msg_outof " + fuelName);
                return false;
            }
            user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", fuelName));
            int addAmount = useAllFuel ? Math.Min(itemData.m_stack, (int)(__instance.m_maxFuel - currentFuel)) : 1;
            user.GetInventory().RemoveItem(itemData, addAmount);
            for (int i = 0; i < addAmount; i++) __instance.m_nview.InvokeRPC("RPC_AddFuel");
            __result = true;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fireplace), "GetHoverText")]
        public static string AddFuel_FirePlaceGetHoverText_Patch(string __result, Fireplace __instance)
        {
            if (!VBQOL.AFEnable.Value) return __result;
            string modifierKey = VBQOL.AFModifierKeyConfig.Value.ToString();
            string actionKey = VBQOL.AFModifierKeyUseConfig.ToString();
            if (!__instance || !__instance.IsBurning() || __instance.m_wet) return __result;
            return $"{__result}\n[<color=yellow><b>{modifierKey}+{actionKey}</b></color>] {VBQOL.AFTextConfig.Value}";
        }

        public static void RPC_AddFuelAmount(Fireplace instance, ZNetView m_nview, float count)
        {
            if (VBQOL.AFEnable.Value && m_nview.IsOwner())
            {
                float newFuel = Mathf.Clamp(m_nview.GetZDO().GetFloat("fuel") + count, 0f, instance.m_maxFuel);
                m_nview.GetZDO().Set("fuel", newFuel);
                instance.m_fuelAddedEffects.Create(instance.transform.position, instance.transform.rotation);
                ZLog.Log($"Added fuel * {count}");
                Traverse.Create(instance).Method("UpdateState").GetValue();
            }
        }
    }
}
