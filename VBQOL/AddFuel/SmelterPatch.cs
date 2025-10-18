namespace VBQOL.AddFuel
{
    [HarmonyPatch]
    static class SmelterPatch
    {
        // Добавляем защиту от спама
        private static readonly Dictionary<Smelter, float> _lastOreInteraction = new Dictionary<Smelter, float>();
        private const float INTERACTION_COOLDOWN = 0.3f;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Smelter), nameof(Smelter.OnAddOre))]
        static bool SmelterOnAddOrePrefix(ref Smelter __instance, ref Switch sw, ref Humanoid user, ItemDrop.ItemData item, ref bool __result)
        {
            if (!AddFuelUtil.AFEnable.Value) return true;

            // Защита от спама
            if (_lastOreInteraction.TryGetValue(__instance, out float lastTime) && Time.time - lastTime < INTERACTION_COOLDOWN)
            {
                __result = false;
                return false;
            }
            _lastOreInteraction[__instance] = Time.time;

            bool isAddOne = !Input.GetKey(AddFuelUtil.AFModifierKeyConfig.Value);
            int queueSizeNow = __instance.GetQueueSize(); // Используем нативный метод

            if (queueSizeNow >= __instance.m_maxOre)
            {
                user.Message(MessageHud.MessageType.Center, "$msg_itsfull");
                return false;
            }

            if (item == null) item = __instance.FindCookableItem(user.GetInventory());

            if (item == null)
            {
                if (isAddOne) return true;
                user.Message(MessageHud.MessageType.Center, "$msg_noprocessableitems");
                return false;
            }

            if (!__instance.IsItemAllowed(item.m_dropPrefab.name))
            {
                user.Message(MessageHud.MessageType.Center, "$msg_wontwork");
                return false;
            }

            user.Message(MessageHud.MessageType.Center, "$msg_added " + item.m_shared.m_name);

            int queueSizeLeft = __instance.m_maxOre - queueSizeNow;
            int stackToAdd = AddFuelUtil.CalculateStackToAdd(isAddOne, item.m_stack, queueSizeLeft);

            // ВАЖНО: Удаляем предметы на клиенте
            user.GetInventory().RemoveItem(item, stackToAdd);

            // Отправляем отдельные RPC вызовы для каждого предмета
            // Это гарантирует синхронизацию как в ванильной игре
            for (int i = 0; i < stackToAdd; i++) __instance.m_nview.InvokeRPC("RPC_AddOre", item.m_dropPrefab.name);

            __instance.m_addedOreTime = Time.time;
            if (__instance.m_addOreAnimationDuration > 0f) __instance.SetAnimation(active: true);

            __result = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Smelter), nameof(Smelter.RPC_AddOre))]
        static bool RPC_AddOrePrefix(Smelter __instance, long sender, string name)
        {
            // Только сервер должен обрабатывать логику добавления
            if (!__instance.m_nview.IsOwner()) 
            {
                // Клиенты только воспроизводят эффекты
                __instance.m_oreAddedEffects.Create(__instance.transform.position, __instance.transform.rotation);
                return false; // Блокируем оригинальный метод на клиентах
            }

            // СЕРВЕР: проверяем лимиты перед добавлением
            if (__instance.GetQueueSize() >= __instance.m_maxOre)
            {
                ZLog.Log("Smelter is full, ignoring RPC_AddOre");
                return false;
            }

            if (!__instance.IsItemAllowed(name))
            {
                ZLog.Log("Item not allowed: " + name);
                return false;
            }

            // Добавляем руду на сервере
            __instance.QueueOre(name);
            __instance.m_oreAddedEffects.Create(__instance.transform.position, __instance.transform.rotation);
            
            return false; // Блокируем оригинальный метод на сервере
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

            float fuelNow = __instance.GetFuel();
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
            
            // Используем нативные методы для топлива
            for (int i = 0; i < stackToAdd; i++) __instance.m_nview.InvokeRPC("RPC_AddFuel");

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

        // Очистка словаря при уничтожении плавильни
        [HarmonyPatch(typeof(Smelter), nameof(Smelter.OnDestroyed))]
        [HarmonyPostfix]
        static void SmelterOnDestroyPostfix(Smelter __instance) => _lastOreInteraction.Remove(__instance);
    }
}