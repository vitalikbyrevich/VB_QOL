namespace VBQOL
{
    [HarmonyPatch]
    public class VB_WardPatch
    {
        [HarmonyPatch(typeof(Pickable), nameof(Pickable.Interact))]
        internal static class PickableInteract_Patch
        {
            private static bool Prefix(Pickable __instance, ref bool repeat)
            {
                if (PrivateArea.m_allAreas.Any(x => x.IsEnabled() && x.IsInside(__instance.transform.position, 0) && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false)))
                {
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Pickable), nameof(Pickable.GetHoverText))]
        internal static class PickableHover_Patch
        {
            private static string Postfix(string __result, Pickable __instance)
            {
                if (PrivateArea.m_allAreas.Any(x => x.IsEnabled() && x.IsInside(__instance.transform.position, 0) && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false)))
                {
                    return Localization.instance.Localize(__instance.GetHoverName() + "\n$piece_noaccess");
                }
                return __result;
            }
        }
        
        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.Interact))]
        internal static class ItemDropInteract_Patch
        {
            private static bool Prefix(ItemDrop __instance, ref bool repeat)
            {
                if (PrivateArea.m_allAreas.Any(x => x.IsEnabled() && x.IsInside(__instance.transform.position, 0) && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false)))
                {
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(ItemDrop), nameof(ItemDrop.GetHoverText))]
        internal static class ItemDropHover_Patch
        {
            private static string Postfix(string __result, ItemDrop __instance)
            {
                if (PrivateArea.m_allAreas.Any(x => x.IsEnabled() && x.IsInside(__instance.transform.position, 0) && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false)))
                {
                    return Localization.instance.Localize(__instance.GetHoverName() + "\n$piece_noaccess");
                }
                return __result;
            }
        }
        
        [HarmonyPatch(typeof(Beehive), nameof(Beehive.Interact))]
        internal static class BeehiveInteract_Patch
        {
            private static bool Prefix(Beehive __instance, ref bool repeat)
            {
                if (PrivateArea.m_allAreas.Any(x => x.IsEnabled() && x.IsInside(__instance.transform.position, 0) && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false)))
                {
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Beehive), nameof(Beehive.GetHoverText))]
        internal static class BeehiveHover_Patch
        {
            private static string Postfix(string __result, Beehive __instance)
            {
                if (PrivateArea.m_allAreas.Any(x => x.IsEnabled() && x.IsInside(__instance.transform.position, 0) && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false)))
                {
                    return Localization.instance.Localize(__instance.GetHoverName() + "\n$piece_noaccess");
                }
                return __result;
            }
        }
        
        [HarmonyPatch(typeof(ShipControlls), nameof(ShipControlls.Interact))]
        internal static class ShipControllsInteract_Patch
        {
            private static bool Prefix(ShipControlls __instance, ref bool repeat)
            {
                if (PrivateArea.m_allAreas.Any(x => x.IsEnabled() && x.IsInside(__instance.transform.position, 0) && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false)))
                {
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(ShipControlls), nameof(ShipControlls.GetHoverText))]
        internal static class ShipControllsHover_Patch
        {
            private static string Postfix(string __result, ShipControlls __instance)
            {
                if (PrivateArea.m_allAreas.Any(x => x.IsEnabled() && x.IsInside(__instance.transform.position, 0) && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false)))
                {
                    return Localization.instance.Localize(__instance.GetHoverName() + "\n$piece_noaccess");
                }
                return __result;
            }
        }
        
        [HarmonyPatch(typeof(Sign), nameof(Sign.Interact))]
        internal static class SignInteract_Patch
        {
            private static bool Prefix(Sign __instance)
            {
                if (PrivateArea.m_allAreas.Any(x => x.IsEnabled() && x.IsInside(__instance.transform.position, 0) && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false)))
                {
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Sign), nameof(Sign.GetHoverText))]
        internal static class SignHover_Patch
        {
            private static string Postfix(string __result, Sign __instance)
            {
                if (PrivateArea.m_allAreas.Any(x => x.IsEnabled() && x.IsInside(__instance.transform.position, 0) && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false)))
                {
                   // return "\"" + __instance.GetText() + "\"";
                    return Localization.instance.Localize(__instance.GetHoverName() + "\n$piece_noaccess");
                }
                return "\"" + __instance.GetText() + "\"\n" + Localization.instance.Localize(__instance.m_name + "\n[<color=#FFFF00><b>$KEY_Use</b></color>] $piece_use");
            }
        }
        
        [HarmonyPatch(typeof(Container), nameof(Container.Interact))]
        internal static class ContainerInteract_Patch
        {
            private static bool Prefix(Container __instance, bool hold)
            {
                if (PrivateArea.m_allAreas.Any(x => x.IsEnabled() && x.IsInside(__instance.transform.position, 0) && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false)))
                {
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Container), nameof(Container.GetHoverText))]
        internal static class ContainerHover_Patch
        {
            private static string Postfix(string __result, Container __instance)
            {
                if (PrivateArea.m_allAreas.Any(x => x.IsEnabled() && x.IsInside(__instance.transform.position, 0) && !PrivateArea.CheckAccess(__instance.transform.position, 0f, false)))
                {
                    return Localization.instance.Localize(__instance.GetHoverName() + "\n$piece_noaccess");
                }
                return __result;
            }
        }
    }
}