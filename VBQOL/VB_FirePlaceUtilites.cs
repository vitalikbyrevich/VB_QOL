namespace VBQOL
{
    [HarmonyPatch]
    public static class VB_FirePlaceUtilites
    {
        // 🔥 Конфигурация тушения
        public static ConfigEntry<bool> extinguishItemsConfig;
        public static ConfigEntry<string> extinguishStringConfig;
        public static ConfigEntry<KeyCode> keyPOCodeStringConfig;

       private static float s_lastExtinguishTime;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.GetHoverText))]
        public static void Patch_GetHoverText(Fireplace __instance, ref string __result)
        {
            if (!__instance?.m_nview?.IsValid() ?? true) return;
            if (__instance.m_infiniteFuel || !__instance.m_canRefill) return;

            float fuel = __instance.m_nview.GetZDO().GetFloat(ZDOVars.s_fuel);
            int state = __instance.m_nview.GetZDO().GetInt(ZDOVars.s_state, 1);
            bool isBurning = state == 1 && fuel > 0f && !__instance.m_wet;

            // Подсказка тушения
            if (extinguishItemsConfig?.Value == true && isBurning)
            {
                __result += $"\n[<color=yellow><b>{keyPOCodeStringConfig.Value}</b></color>] {extinguishStringConfig.Value}";
            }

            // Подсказка массового добавления
            if (AddFuelUtil.AFEnable?.Value == true && __instance.IsBurning() && !__instance.m_wet)
            {
                string modKey = AddFuelUtil.AFModifierKeyConfig.Value.ToString();
                string useKey = AddFuelUtil.AFModifierKeyUseConfig.ToString();
                __result += $"\n[<color=yellow><b>{modKey}+{useKey}</b></color>] {AddFuelUtil.AFTextConfig.Value}";
            }
        }

        // ⛽ Патч на взаимодействие (добавление топлива стаком)
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Fireplace), nameof(Fireplace.Interact))]
        public static bool Patch_Interact(Fireplace __instance, Humanoid user, bool hold, bool alt, ref bool __result)
        {
            if (!__instance?.m_nview?.IsValid() ?? true) return true;
            if (__instance.m_infiniteFuel || !__instance.m_canRefill || !AddFuelUtil.AFEnable.Value) return true;

            ZNetView nview = __instance.m_nview;
            float fuel = nview.GetZDO().GetFloat(ZDOVars.s_fuel);
            bool isBurning = nview.GetZDO().GetInt(ZDOVars.s_state, 1) == 1 && fuel > 0f && !__instance.m_wet;

            // Проверяем: нажата клавиша Use + удерживается модификатор
            if (!hold && isBurning && Input.GetKey(AddFuelUtil.AFModifierKeyConfig.Value))
            {
                string fuelName = __instance.m_fuelItem?.m_itemData?.m_shared?.m_name;
                if (string.IsNullOrEmpty(fuelName)) return true;

                if (fuel >= __instance.m_maxFuel - 1f)
                {
                    user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", fuelName));
                    __result = false;
                    return false;
                }

                ItemDrop.ItemData fuelItem = user.GetInventory()?.GetItem(fuelName);
                if (fuelItem == null)
                {
                    user.Message(MessageHud.MessageType.Center, "$msg_outof " + fuelName);
                    __result = false;
                    return false;
                }

                user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", fuelName));

                int fuelSpaceLeft = (int)(__instance.m_maxFuel - fuel);
                int stackToAdd = Mathf.Min(fuelItem.m_stack, fuelSpaceLeft);

                // Удаляем предмет у игрока (валидация инвентаря клиентская, как в ваниле)
                user.GetInventory().RemoveItem(fuelItem, stackToAdd);

                // 🔑 СЕТЕВАЯ БЕЗОПАСНОСТЬ: вызываем ванильный RPC.
                // Он автоматически маршрутизируется к владельцу костра, обновляет ZDO и рассылает всем клиентам.
                nview.InvokeRPC("RPC_AddFuelAmount", (float)stackToAdd);

                __result = true;
                return false; // Блокируем ванильный Interact, т.к. уже обработали
            }

            return true; // Для остальных случаев выполняем ванильную логику
        }

        // 🔥 Патч на тушение (отдельная клавиша)
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        public static void Patch_PlayerUpdate(Player __instance)
        {
            if (__instance != Player.m_localPlayer) return;
            if (!extinguishItemsConfig.Value) return;

            // Кулдаун 0.2с от спама
            if (Time.time - s_lastExtinguishTime < 0.2f) return;

            if (!Input.GetKeyDown(keyPOCodeStringConfig.Value)) return;

            GameObject hover = __instance.GetHoverObject();
            if (!hover) return;

            Fireplace fireplace = hover.GetComponentInParent<Fireplace>();
            if (!fireplace || !fireplace.m_canRefill || fireplace.m_infiniteFuel) return;

            ZNetView nview = fireplace.m_nview;
            if (!nview?.IsValid() ?? true) return;

            float fuel = nview.GetZDO().GetFloat(ZDOVars.s_fuel);
            int state = nview.GetZDO().GetInt(ZDOVars.s_state, 1);

            if (state == 1 && fuel > 0f && !fireplace.m_wet)
            {
                // 🔑 СЕТЕВАЯ БЕЗОПАСНОСТЬ: ванильный RPC сам доставит команду владельцу
                nview.InvokeRPC("RPC_SetFuelAmount", 0f);
                fireplace.m_toggleOnEffects?.Create(fireplace.transform.position, Quaternion.identity);
                
                s_lastExtinguishTime = Time.time;
            }
        }
    }
}