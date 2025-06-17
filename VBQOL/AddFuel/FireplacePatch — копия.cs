namespace AddAllFuel
{
    using HarmonyLib;
    using UnityEngine;

    using static PluginConfig;

    [HarmonyPatch(typeof(Fireplace))]
    static class FireplacePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(Fireplace.Interact))]
        public static bool InteractPrefix(Fireplace __instance, Humanoid user, bool hold, bool alt, ref bool __result)
        {
            if (!IsModEnabled.Value) return true;
            if (hold) return true;

            bool useAllFuel = ZInput.GetKey(AddAllModifier.Value);
            string fuelItemName = __instance.GetFuelItemName();
            var item = user.GetInventory()?.GetItem(fuelItemName);

            if (item == null || item.m_shared.m_name != fuelItemName)
            {
                user.Message(MessageHud.MessageType.Center, "$msg_outof " + fuelItemName);
                __result = false;
                return false;
            }

            int requiredFuel = Mathf.CeilToInt(__instance.m_maxFuel - __instance.GetFuel());
            int fuelToAdd = useAllFuel ? Mathf.Min(requiredFuel, item.m_stack) : 1;

            if (requiredFuel <= 0 || !user.GetInventory().RemoveItem(item, fuelToAdd))
            {
                __result = false;
                return false;
            }

            for (int i = 0; i < fuelToAdd; i++)
            {
                __instance.m_nview.InvokeRPC("RPC_AddFuel");
            }

            user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", fuelItemName));
            __result = true;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(Fireplace.GetHoverText))]
        public static void GetHoverTextPostfix(ref string __result, Fireplace __instance)
        {
            if (!IsModEnabled.Value) return;
            if (!__instance || !__instance.IsBurning() || __instance.m_wet) return;

            string modifierKey = AddAllModifier.Value.ToString();
            string actionKey = "your action key here"; // Replace with your actual action key config
            __result += $"\n[<color=yellow><b>{modifierKey}+{actionKey}</b></color>] Add All Fuel";
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Fireplace.UseItem))]
        public static bool UseItemPrefix(Fireplace __instance, Humanoid user, ItemDrop.ItemData item)
        {
            if (IsModEnabled.Value && ZInput.GetKey(AddAllModifier.Value))
            {
                string fuelItemName = __instance.GetFuelItemName();
                item ??= user.GetInventory().GetItem(fuelItemName);

                if (item == null || item.m_shared.m_name != fuelItemName)
                {
                    return false;
                }

                int requiredFuel = Mathf.CeilToInt(__instance.m_maxFuel - __instance.GetFuel());
                requiredFuel = Mathf.Max(Mathf.Min(requiredFuel, item.m_stack), 0);

                if (requiredFuel <= 0 || !user.GetInventory().RemoveItem(item, requiredFuel))
                {
                    return false;
                }

                __instance.AddFuel(requiredFuel);
                return true;
            }

            return false;
        }
    }
}
