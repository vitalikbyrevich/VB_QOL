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
			bool flag = !Input.GetKey(VBQOL.AFModifierKeyConfig.Value);
			__result = false;
			if (hold) return false;
			if (!__instance.m_nview.HasOwner()) __instance.m_nview.ClaimOwnership();
			string name = __instance.m_fuelItem.m_itemData.m_shared.m_name;
			ZLog.Log("Found fuel " + name);
			float num = Mathf.CeilToInt(__instance.m_nview.GetZDO().GetFloat("fuel"));
			if (num > __instance.m_maxFuel - 1f)
			{
				user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_cantaddmore", name));
				return false;
			}
			ZLog.Log("Checking Inventory for fuel " + name);
			ItemDrop.ItemData itemData = user.GetInventory()?.GetItem(name);
			if (itemData == null)
			{
				if (flag) return true;
				user.Message(MessageHud.MessageType.Center, "$msg_outof " + name);
				return false;
			}
			user.Message(MessageHud.MessageType.Center, Localization.instance.Localize("$msg_fireadding", name));
			int val = (int)(__instance.m_maxFuel - num);
			int num2 = 1;
			if (!flag) num2 = Math.Min(itemData.m_stack, val);
			user.GetInventory().RemoveItem(itemData, num2);
			for (int i = 0; i < num2; i++) __instance.m_nview.InvokeRPC("RPC_AddFuel");
			__result = true;
			return false;
		}

		[HarmonyPatch(typeof(Fireplace), "GetHoverText")]
		[HarmonyPostfix]
		public static string AddFuel_FirePlaceGetHoverText_Patch(string __result, Fireplace __instance)
		{
			if (!VBQOL.AFEnable.Value) return __result;
			string text = VBQOL.AFModifierKeyConfig.Value.ToString();
			string text2 = VBQOL.AFModifierKeyUseConfig.ToString();
			if (!__instance) return __result;
			if (!__instance.IsBurning()) return __result;
			if (__instance.m_wet) return __result;
			return __result + "\n[<color=yellow><b>" + text + "+" + text2 + "</b></color>] " + VBQOL.AFTextConfig.Value;
		}

		public static void RPC_AddFuelAmount(Fireplace instance, ZNetView m_nview, float count)
		{
			if (VBQOL.AFEnable.Value && m_nview.IsOwner())
			{
				float value = Mathf.Clamp(m_nview.GetZDO().GetFloat("fuel") + count, 0f, instance.m_maxFuel);
				m_nview.GetZDO().Set("fuel", value);
				instance.m_fuelAddedEffects.Create(instance.transform.position, instance.transform.rotation);
				ZLog.Log($"Added fuel * {count}");
				Traverse.Create(instance).Method("UpdateState").GetValue();
			}
		}
    }
}