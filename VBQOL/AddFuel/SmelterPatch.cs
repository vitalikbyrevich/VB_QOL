namespace VBQOL.AddFuel
{
    [HarmonyPatch]
    static class SmelterPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Smelter),nameof(Smelter.OnAddOre))]
        static bool SmelterOnAddOrePrefix(ref Smelter __instance, ref Switch sw, ref Humanoid user, ItemDrop.ItemData item, ref bool __result)
        {
            if (!VBQOL.AFEnable.Value) return true;

            bool isAddOne = !Input.GetKey(VBQOL.AFModifierKeyConfig.Value);
            int queueSizeNow = Traverse.Create(__instance).Method("GetQueueSize").GetValue<int>();
            if (queueSizeNow >= __instance.m_maxOre)
            {
                user.Message(MessageHud.MessageType.Center, "$msg_itsfull");
                return false;
            }

            if (item == null) item = VBQOL.FindCookableItem(__instance, user.GetInventory(), isAddOne);

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
            int queueSize = 1;
            if (!isAddOne) queueSize = Math.Min(item.m_stack, queueSizeLeft);
            user.GetInventory().RemoveItem(item, queueSize);
            for (int i = 0; i < queueSize; i++) __instance.m_nview.InvokeRPC("RPC_AddOre", item.m_dropPrefab.name);
            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Smelter),nameof(Smelter.OnAddFuel))]
        static bool Prefix(ref Smelter __instance, ref bool __result, Switch sw, Humanoid user, ItemDrop.ItemData item)
        {
            if (!VBQOL.AFEnable.Value) return true;

            bool isAddOne = !Input.GetKey(VBQOL.AFModifierKeyConfig.Value);
            string fuelName = __instance.m_fuelItem.m_itemData.m_shared.m_name;

            if (item != null && item.m_shared.m_name != fuelName)
            {
                user.Message(MessageHud.MessageType.Center, "$msg_wrongitem");
                return false;
            }

            float fuelNow = Traverse.Create(__instance).Method("GetFuel").GetValue<float>();
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
            int fuelSize = 1;
            if (!isAddOne) fuelSize = Math.Min(item.m_stack, fuelLeft);

            user.GetInventory().RemoveItem(item, fuelSize);
            for (int i = 0; i < fuelSize; i++) __instance.m_nview.InvokeRPC("RPC_AddFuel", Array.Empty<object>());
            __result = true;
            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Smelter), nameof(Smelter.OnHoverAddFuel))]
        static string AddFuel_OnHoverAddFuel_Patch(string __result, Smelter __instance)
        {

            if (!VBQOL.AFEnable.Value) return __result;
            string text = __result;
            string text2 = VBQOL.AFModifierKeyConfig.Value.ToString();
            string text3 = VBQOL.AFModifierKeyUseConfig.ToString();
            if (!__instance) return text;
            text = string.Concat(text, "\n[<color=yellow><b>", text2, "+", text3, "</b></color>] ", VBQOL.AFTextConfig.Value);
            return text;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Smelter), nameof(Smelter.OnHoverAddOre))]
        static string AddFuel_OnHoverAddOre_Patch(string __result, Smelter __instance)
        {
            if (!VBQOL.AFEnable.Value) return __result;

            string text = __result;
            string text2 = VBQOL.AFModifierKeyConfig.Value.ToString();
            string text3 = VBQOL.AFModifierKeyUseConfig.ToString();
            if (!__instance) return text;
            text = string.Concat(text, "\n[<color=yellow><b>", text2, "+", text3, "</b></color>] ", VBQOL.AFTextConfig.Value);
            return text;
        }

        public static void RPC_AddOre(Smelter instance, ZNetView m_nview, string name, int count)
        {
            if (!VBQOL.AFEnable.Value) return;
            if (!m_nview.IsOwner()) return;
            if (!Traverse.Create(instance).Method("IsItemAllowed", name).GetValue<bool>()) return;

            int start = Traverse.Create(instance).Method("GetQueueSize").GetValue<int>();
            for (int i = 0; i < count; i++) m_nview.GetZDO().Set($"item{start + i}", name);
            m_nview.GetZDO().Set("queued", start + count);

            instance.m_oreAddedEffects.Create(instance.transform.position, instance.transform.rotation);
            ZLog.Log($"Added ore {name} * {count}");
        }

        public static void RPC_AddFuel(Smelter instance, ZNetView m_nview, float count)
        {
            if (!VBQOL.AFEnable.Value) return;
            if (!m_nview.IsOwner()) return;

            float now = Traverse.Create(instance).Method("GetFuel").GetValue<float>();
            m_nview.GetZDO().Set("fuel", now + count);
            instance.m_fuelAddedEffects.Create(instance.transform.position, instance.transform.rotation, instance.transform);
            ZLog.Log($"Added fuel * {count}");
        }
    }
}