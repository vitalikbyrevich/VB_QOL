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
            Fireplace fireplace = (hoverObject) ? hoverObject.GetComponentInParent<Fireplace>() : null;
            if (!fireplace) return null;
            Fireplace component = fireplace.GetComponent<ZNetView>().GetComponent<Fireplace>();
            if (!component) return null;
            if (checkIfBurning)
            {
                if (!component.IsBurning()) return null;
                if (component.m_wet) return null;
            }
            return component;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fireplace), "UpdateFireplace")]
        public static void FireplaceUpdateFireplace_Patch(Fireplace __instance)
        {
            ZDO zdo = __instance.m_nview.GetZDO();
            float @float = zdo.GetFloat("fuel");
            if (!zdo.GetBool("enabledFire"))
            {
                if (@float <= 0f) return;
                zdo.Set("enabledFire", true);
                zdo.Set("fuel", zdo.GetFloat("hiddenFuelAmount") + @float);
            } 
            if (!Mathf.Approximately(zdo.GetFloat("hiddenFuelAmount"), @float)) 
            {
                float value = @float;
                zdo.Set("hiddenFuelAmount", value);
            }
            if (zdo.GetFloat("fuel") > __instance.m_maxFuel) zdo.Set("fuel", __instance.m_maxFuel);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fireplace), "GetHoverText")]
        public static string FireplaceGetHoverText_Patch(string __result, Fireplace __instance)
        {
            string text = __result;
            string text5 = keyPOCodeStringConfig.Value.ToString();
            if (!__instance) return text;
            ZDO zdo = __instance.m_nview.GetZDO();
            float @float = zdo.GetFloat("hiddenFuelAmount");
            if (extinguishItemsConfig.Value && !__instance.IsBurning() && @float > 0f)
            {
                int num = (int)__instance.m_maxFuel;
                text = string.Concat(text.Replace(string.Format("0/{0}", num), string.Format("{0}/{1}", (int)Mathf.Ceil(@float), num)), "\n[<color=yellow><b>", text5, "</b></color>] ", igniteStringConfig.Value);
            }
            if (!__instance.IsBurning()) return text;
            if (__instance.m_wet) return text;
            if (extinguishItemsConfig.Value) text = string.Concat(text, "\n[<color=yellow><b>", text5, "</b></color>] ", extinguishStringConfig.Value);
            return text;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "Update")]
        public static void PlayerUpdate_Patch(Player __instance)
        {
            if (!__instance) return; 
            bool keyUp = Input.GetKeyUp(configPOKey);
            if (keyUp && extinguishItemsConfig.Value)
            {
                Fireplace andCheckFireplace3 = GetAndCheckFireplace(__instance, false);
                if (!andCheckFireplace3) return;
                ZDO zdo2 = andCheckFireplace3.m_nview.GetZDO();
                bool flag = !zdo2.GetBool("enabledFire");
                zdo2.Set("enabledFire", flag);
                if (!flag)
                {
                    andCheckFireplace3.m_fuelAddedEffects.Create(andCheckFireplace3.transform.position, andCheckFireplace3.transform.rotation);
                    zdo2.Set("fuel", 0f);
                }
                if (flag)
                {
                    andCheckFireplace3.m_fuelAddedEffects.Create(andCheckFireplace3.transform.position, andCheckFireplace3.transform.rotation);
                    zdo2.Set("fuel", zdo2.GetFloat("hiddenFuelAmount"));
                }
            }
        }
    }
}