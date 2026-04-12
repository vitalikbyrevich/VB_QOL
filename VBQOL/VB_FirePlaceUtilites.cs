namespace VBQOL
{
    [HarmonyPatch]
    public class VB_FirePlaceUtilites
    {
        public static KeyCode configPOKey;
        public static ConfigEntry<bool> extinguishItemsConfig;
        public static ConfigEntry<string> extinguishStringConfig;
        public static ConfigEntry<string> igniteStringConfig;
        public static ConfigEntry<KeyCode> keyPOCodeStringConfig;

        public static Fireplace GetAndCheckFireplace(Player player, bool checkIfBurning)
        {
            GameObject hoverObject = player.GetHoverObject();
            if (!hoverObject) return null;

            Fireplace fireplace = hoverObject.GetComponentInParent<Fireplace>();
            if (!fireplace) return null;

            ZNetView netView = fireplace.GetComponent<ZNetView>();
            if (!netView || !netView.IsValid()) return null;

            if (checkIfBurning && (!fireplace.IsBurning() || fireplace.m_wet)) return null;

            return fireplace;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.GetHoverText))]
        public static string FireplaceGetHoverText_Patch(string __result, Fireplace __instance)
        {
            if (!__instance || !__instance.m_canRefill || !extinguishItemsConfig.Value) return __result;

            if (__instance.IsBurning() && !__instance.m_wet)
            {
                string keyText = keyPOCodeStringConfig.Value.ToString();
                return $"{__result}\n[<color=yellow><b>{keyText}</b></color>] {extinguishStringConfig.Value}";
            }
            
            return __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        public static void PlayerUpdate_Patch(Player __instance)
        {
            if (!__instance || !__instance.IsOwner()) return;
            if (!Input.GetKeyUp(configPOKey) || !extinguishItemsConfig.Value) return;

            Fireplace fireplace = GetAndCheckFireplace(__instance, false);
            if (!fireplace || !fireplace.m_canRefill) return;
    
            if (!fireplace.IsBurning() || fireplace.m_wet) return;
    
            ZNetView netView = fireplace.GetComponent<ZNetView>();
            if (!netView.IsValid()) return;
    
            if (!netView.HasOwner()) netView.ClaimOwnership();
    
            netView.InvokeRPC("RPC_SetFuelAmount", 0f);
    
            __instance.Message(MessageHud.MessageType.Center, igniteStringConfig.Value);
            fireplace.m_fuelAddedEffects.Create(fireplace.transform.position, fireplace.transform.rotation);
        }
    }
}