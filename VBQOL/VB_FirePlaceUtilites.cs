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
        [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.UpdateFireplace))]
        public static void FireplaceUpdateFireplace_Patch(Fireplace __instance)
        {
            if (!__instance.m_canRefill) return;
            
            ZDO zdo = __instance.m_nview.GetZDO();
            float currentFuel = zdo.GetFloat("fuel");
            
            if (!zdo.GetBool("enabledFire") && currentFuel > 0f)
            {
                zdo.Set("enabledFire", true);
                zdo.Set("fuel", zdo.GetFloat("hiddenFuelAmount") + currentFuel);
            }
            
            if (!Mathf.Approximately(zdo.GetFloat("hiddenFuelAmount"), currentFuel)) zdo.Set("hiddenFuelAmount", currentFuel);
            if (zdo.GetFloat("fuel") > __instance.m_maxFuel) zdo.Set("fuel", __instance.m_maxFuel);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.GetHoverText))]
        public static string FireplaceGetHoverText_Patch(string __result, Fireplace __instance)
        {
            if (!__instance || !__instance.m_canRefill) return __result;

            string keyText = keyPOCodeStringConfig.Value.ToString();
            ZDO zdo = __instance.m_nview.GetZDO();
            float hiddenFuel = zdo.GetFloat("hiddenFuelAmount");
            int maxFuel = (int)__instance.m_maxFuel;

            if (extinguishItemsConfig.Value)
            {
                // Показываем "зажечь" если костер потушен И есть сохраненное топливо
                if (!__instance.IsBurning() && hiddenFuel > 0f)
                {
                    string fuelText = $"{(int)Mathf.Ceil(hiddenFuel)}/{maxFuel}";
                    string resultWithFuel = __result.Replace($"0/{maxFuel}", fuelText);
                    return $"{resultWithFuel}\n[<color=yellow><b>{keyText}</b></color>] {igniteStringConfig.Value}";
                }
                
                // Показываем "потушить" если костер горит и не мокрый
                if (__instance.IsBurning() && !__instance.m_wet)
                {
                    return $"{__result}\n[<color=yellow><b>{keyText}</b></color>] {extinguishStringConfig.Value}";
                }
            }
            
            return __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        public static void PlayerUpdate_Patch(Player __instance)
        {
            if (!__instance || !Input.GetKeyUp(configPOKey) || !extinguishItemsConfig.Value) return;

            Fireplace fireplace = GetAndCheckFireplace(__instance, false);
            if (!fireplace || !fireplace.m_canRefill) return;

            ZDO zdo = fireplace.m_nview.GetZDO();
            bool isCurrentlyEnabled = zdo.GetBool("enabledFire");
            bool newState = !isCurrentlyEnabled;
            
            zdo.Set("enabledFire", newState);
            fireplace.m_fuelAddedEffects.Create(fireplace.transform.position, fireplace.transform.rotation);
            
            if (newState) // Разжигаем - восстанавливаем сохраненное топливо
            {
                float hiddenFuel = zdo.GetFloat("hiddenFuelAmount");
                zdo.Set("fuel", hiddenFuel);
            }
            else // Тушим - сохраняем текущее топливо и устанавливаем 0
            {
                float currentFuel = zdo.GetFloat("fuel");
                zdo.Set("hiddenFuelAmount", currentFuel);
                zdo.Set("fuel", 0f);
            }
        }
    }
}