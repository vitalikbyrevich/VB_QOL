using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace FireplaceUtilities
{
	// Token: 0x02000002 RID: 2
	[BepInPlugin("smallo.mods.fireplaceutilities", "Fireplace Utilities", "2.1.0")]
	[HarmonyPatch]
	internal class FireplaceUtilitiesPlugin : BaseUnityPlugin
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		private void Awake()
		{
			FireplaceUtilitiesPlugin.enableMod = base.Config.Bind<bool>("1 - Global", "Enable Mod", true, "Enable or disable this mod");
			if (!FireplaceUtilitiesPlugin.enableMod.Value)
			{
				return;
			}
			FireplaceUtilitiesPlugin.burnItems = base.Config.Bind<bool>("2 - Toggles", "Burn Items In Fire", true, "Allows you to burn items in fires");
			FireplaceUtilitiesPlugin.extinguishItems = base.Config.Bind<bool>("2 - Toggles", "Extinguish Fires", true, "Allows you to turn fires off/on");
			FireplaceUtilitiesPlugin.disableTorches = base.Config.Bind<bool>("2 - Toggles", "Disable Fires During The Day", false, "Allows you to make fires turn off during the day, you must press a key on each item to let it toggle");
			FireplaceUtilitiesPlugin.returnFuel = base.Config.Bind<bool>("2 - Toggles", "Return Fuel", true, "Allows you to press a key to return the fuel left in a fire");
			FireplaceUtilitiesPlugin.torchUseCoal = base.Config.Bind<bool>("2 - Toggles", "Torch and Sconce Use Coal", false, "Makes the Wood/Iron Torch and Sconce use Coal as fuel instead of resin");
			FireplaceUtilitiesPlugin.customBurnTimes = base.Config.Bind<bool>("2 - Toggles", "Custom Burn Times", false, "Enable custom burn times for all fireplaces, the default values are the games vanilla values");
			FireplaceUtilitiesPlugin.torchBurn = base.Config.Bind<bool>("3 - Burn Items In Fire", "Burn In Torches", false, "Allows items to be burnt in ground torches, wall torches or braziers");
			FireplaceUtilitiesPlugin.giveCoal = base.Config.Bind<bool>("3 - Burn Items In Fire", "Give Coal", true, "Returns coal when burning an item");
			FireplaceUtilitiesPlugin.blacklistBurn = base.Config.Bind<string>("3 - Burn Items In Fire", "Blacklist Items", "$item_wood", "Items that aren't allowed to be burned. Seperate items by a comma. Wood should remain as a default so that way it doesn't take your wood twice when lighting a fire, if you have a mod that allows other wood types to burn, put them on this list.");
			FireplaceUtilitiesPlugin.burnItemString = base.Config.Bind<string>("3 - Burn Items In Fire", "Burn Item Text", "Burn item", "The text to show when hovering over the fire");
			FireplaceUtilitiesPlugin.coalAmount = base.Config.Bind<int>("3 - Burn Items In Fire", "Coal Amount", 1, "Amount of coal to give when burning an item");
			FireplaceUtilitiesPlugin.keyBurnCodeString = base.Config.Bind<string>("3 - Burn Items In Fire", "Burn Key", "LeftShift", "The key to use in combination with the hotkeys. KeyCodes can be found here https://docs.unity3d.com/ScriptReference/KeyCode.html");
			FireplaceUtilitiesPlugin.keyBurnTextString = base.Config.Bind<string>("3 - Burn Items In Fire", "Burn Key Text", "LShift", "The custom text to show for the string, if you set it to \"none\" then it'll use what you have in the 'Key' config option.");
			FireplaceUtilitiesPlugin.extinguishString = base.Config.Bind<string>("4 - Extinguish Fires", "Extinguish Fire Text", "Extinguish fire", "The text to show when hovering over the fire");
			FireplaceUtilitiesPlugin.igniteString = base.Config.Bind<string>("4 - Extinguish Fires", "Ignite Fire Text", "Ignite fire", "The text to show when hovering over the fire if the fire is extinguished");
			FireplaceUtilitiesPlugin.keyPOCodeString = base.Config.Bind<string>("4 - Extinguish Fires", "Put Out Fire Key", "LeftAlt", "The key to use to put out a fire. KeyCodes can be found here https://docs.unity3d.com/ScriptReference/KeyCode.html");
			FireplaceUtilitiesPlugin.keyPOTextString = base.Config.Bind<string>("4 - Extinguish Fires", "Put Out Fire Key Text", "LAlt", "The custom text to show for the string, if you set it to \"none\" then it'll use what you have in the 'Key' config option.");
			FireplaceUtilitiesPlugin.returnString = base.Config.Bind<string>("5 - Return Fuel", "Return Fuel Text", "Return fuel", "The text to show when hovering over the fire");
			FireplaceUtilitiesPlugin.returnCodeString = base.Config.Bind<string>("5 - Return Fuel", "Return Fuel Key", "LeftControl", "The key to use to return the fuel. KeyCodes can be found here https://docs.unity3d.com/ScriptReference/KeyCode.html");
			FireplaceUtilitiesPlugin.returnTextString = base.Config.Bind<string>("5 - Return Fuel", "Return Fuel Key Text", "LCtrl", "The custom text to show for the string, if you set it to \"none\" then it'll use what you have in the 'Key' config option.");
			FireplaceUtilitiesPlugin.timeToggleString = base.Config.Bind<string>("6 - Disable Fires During The Day", "Time Toggle On Text", "Enable Timer", "The text to show when hovering over the fire to enable the timer");
			FireplaceUtilitiesPlugin.timeToggleOffString = base.Config.Bind<string>("6 - Disable Fires During The Day", "Time Toggle Off Text", "Disable Timer", "The text to show when hovering over the fire to disable the timer");
			FireplaceUtilitiesPlugin.timeToggleCodeString = base.Config.Bind<string>("6 - Disable Fires During The Day", "Time Toggle Key", "Equals", "The key to use to return the fuel. KeyCodes can be found here https://docs.unity3d.com/ScriptReference/KeyCode.html");
			FireplaceUtilitiesPlugin.timeToggleTextString = base.Config.Bind<string>("6 - Disable Fires During The Day", "Time Toggle Key Text", "=", "The custom text to show for the string, if you set it to \"none\" then it'll use what you have in the 'Key' config option.");
			FireplaceUtilitiesPlugin.firepitBurnTime = base.Config.Bind<int>("7 - Custom Burn Times", "Firepit", 5000, "Custom burntime for the standard firepit");
			FireplaceUtilitiesPlugin.groundtorchwoodBurnTime = base.Config.Bind<int>("7 - Custom Burn Times", "Wood Ground Torch", 10000, "Custom burntime for the wooden ground torch");
			FireplaceUtilitiesPlugin.bonfireBurnTime = base.Config.Bind<int>("7 - Custom Burn Times", "Bonfire", 5000, "Custom burntime for the bonfire");
			FireplaceUtilitiesPlugin.hearthBurnTime = base.Config.Bind<int>("7 - Custom Burn Times", "Hearth", 5000, "Custom burntime for the hearth");
			FireplaceUtilitiesPlugin.walltorchBurnTime = base.Config.Bind<int>("7 - Custom Burn Times", "Sconce", 20000, "Custom burntime for the sconce");
			FireplaceUtilitiesPlugin.groundtorchironBurnTime = base.Config.Bind<int>("7 - Custom Burn Times", "Iron Ground Torch", 20000, "Custom burntime for the iron ground torch");
			FireplaceUtilitiesPlugin.groundtorchgreenBurnTime = base.Config.Bind<int>("7 - Custom Burn Times", "Green Ground Torch", 20000, "Custom burntime for the green ground torch");
			FireplaceUtilitiesPlugin.braziercBurnTime = base.Config.Bind<int>("7 - Custom Burn Times", "Brazier", 20000, "Custom burntime for the brazier");
			if (FireplaceUtilitiesPlugin.customBurnTimes.Value)
			{
				FireplaceUtilitiesPlugin.customBurnDict.Add("fire_pit", FireplaceUtilitiesPlugin.firepitBurnTime.Value);
				FireplaceUtilitiesPlugin.customBurnDict.Add("piece_groundtorch_wood", FireplaceUtilitiesPlugin.groundtorchwoodBurnTime.Value);
				FireplaceUtilitiesPlugin.customBurnDict.Add("bonfire", FireplaceUtilitiesPlugin.bonfireBurnTime.Value);
				FireplaceUtilitiesPlugin.customBurnDict.Add("hearth", FireplaceUtilitiesPlugin.hearthBurnTime.Value);
				FireplaceUtilitiesPlugin.customBurnDict.Add("piece_walltorch", FireplaceUtilitiesPlugin.walltorchBurnTime.Value);
				FireplaceUtilitiesPlugin.customBurnDict.Add("piece_groundtorch", FireplaceUtilitiesPlugin.groundtorchironBurnTime.Value);
				FireplaceUtilitiesPlugin.customBurnDict.Add("piece_groundtorch_green", FireplaceUtilitiesPlugin.groundtorchgreenBurnTime.Value);
				FireplaceUtilitiesPlugin.customBurnDict.Add("piece_brazierceiling01", FireplaceUtilitiesPlugin.braziercBurnTime.Value);
			}
			FireplaceUtilitiesPlugin.notAllowed = FireplaceUtilitiesPlugin.blacklistBurn.Value.Replace(" ", "").Split(new char[]
			{
				','
			}).ToList<string>();
			FireplaceUtilitiesPlugin.configBurnKey = (KeyCode)Enum.Parse(typeof(KeyCode), FireplaceUtilitiesPlugin.keyBurnCodeString.Value);
			FireplaceUtilitiesPlugin.configPOKey = (KeyCode)Enum.Parse(typeof(KeyCode), FireplaceUtilitiesPlugin.keyPOCodeString.Value);
			FireplaceUtilitiesPlugin.returnKey = (KeyCode)Enum.Parse(typeof(KeyCode), FireplaceUtilitiesPlugin.returnCodeString.Value);
			FireplaceUtilitiesPlugin.timeToggleKey = (KeyCode)Enum.Parse(typeof(KeyCode), FireplaceUtilitiesPlugin.timeToggleCodeString.Value);
			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002688 File Offset: 0x00000888
		public static Fireplace GetAndCheckFireplace(Player player, bool checkIfBurning)
		{
			GameObject hoverObject = player.GetHoverObject();
			Fireplace fireplace = (hoverObject != null) ? hoverObject.GetComponentInParent<Fireplace>() : null;
			if (fireplace == null)
			{
				return null;
			}
			Fireplace component = fireplace.GetComponent<ZNetView>().GetComponent<Fireplace>();
			if (component == null)
			{
				return null;
			}
			if (checkIfBurning)
			{
				if (!component.IsBurning())
				{
					return null;
				}
				if (component.m_wet)
				{
					return null;
				}
			}
			return component;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x000026E4 File Offset: 0x000008E4
		[HarmonyPostfix]
		[HarmonyPatch(typeof(Player), "OnSpawned")]
		public static void PlayerOnSpawned_Patch()
		{
			if (FireplaceUtilitiesPlugin.disableTorches.Value)
			{
				float num = 0f;
				if (EnvMan.instance)
				{
					num = (float)typeof(EnvMan).GetField("m_smoothDayFraction", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(EnvMan.instance);
				}
				int num2 = (int)(num * 24f);
				FireplaceUtilitiesPlugin.timeOfDay = ((num2 < 18 && num2 > 6) ? (FireplaceUtilitiesPlugin.timeOfDay = "Day") : (FireplaceUtilitiesPlugin.timeOfDay = "Night"));
			}
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002764 File Offset: 0x00000964
		[HarmonyPostfix]
		[HarmonyPatch(typeof(EnvMan), "OnEvening")]
		public static void EvnManOnEvening_Patch(Heightmap.Biome biome, EnvSetup currentEnv)
		{
			if (FireplaceUtilitiesPlugin.disableTorches.Value)
			{
				Fireplace[] array = UnityEngine.Object.FindObjectsOfType<Fireplace>();
				for (int i = 0; i < array.Length; i++)
				{
					ZDO zdo = array[i].m_nview.GetZDO();
					if (zdo == null)
					{
						return;
					}
					if (zdo.GetBool("turnOffBetweenTime", false) && !zdo.GetBool("enabledFire", false))
					{
						zdo.Set("enabledFire", true);
						zdo.Set("fuel", zdo.GetFloat("hiddenFuelAmount", 0f));
					}
				}
				FireplaceUtilitiesPlugin.timeOfDay = "Night";
			}
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000027F0 File Offset: 0x000009F0
		[HarmonyPostfix]
		[HarmonyPatch(typeof(EnvMan), "OnMorning")]
		public static void EvnManOnMorning_Patch(Heightmap.Biome biome, EnvSetup currentEnv)
		{
			if (FireplaceUtilitiesPlugin.disableTorches.Value)
			{
				Fireplace[] array = UnityEngine.Object.FindObjectsOfType<Fireplace>();
				for (int i = 0; i < array.Length; i++)
				{
					ZDO zdo = array[i].m_nview.GetZDO();
					if (zdo == null)
					{
						return;
					}
					if (zdo.GetBool("turnOffBetweenTime", false) && zdo.GetBool("enabledFire", false))
					{
						zdo.Set("enabledFire", false);
						zdo.Set("fuel", 0f);
					}
				}
				FireplaceUtilitiesPlugin.timeOfDay = "Day";
			}
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00002874 File Offset: 0x00000A74
		[HarmonyPostfix]
		[HarmonyPatch(typeof(Fireplace), "UpdateFireplace")]
		public static void FireplaceUpdateFireplace_Patch(Fireplace __instance)
		{
			ZDO zdo = __instance.m_nview.GetZDO();
			bool @bool = zdo.GetBool("enabledFire", false);
			float @float = zdo.GetFloat("fuel", 0f);
			if (FireplaceUtilitiesPlugin.disableTorches.Value)
			{
				if (!zdo.GetBool("turnOffBetweenTime", false))
				{
					return;
				}
				if (FireplaceUtilitiesPlugin.timeOfDay == "Night")
				{
					if (!@bool)
					{
						zdo.Set("enabledFire", true);
						zdo.Set("fuel", zdo.GetFloat("hiddenFuelAmount", 0f));
					}
				}
				else if (@bool)
				{
					zdo.Set("enabledFire", false);
					zdo.Set("fuel", 0f);
				}
			}
			if (!@bool)
			{
				if (@float <= 0f)
				{
					return;
				}
				zdo.Set("enabledFire", true);
				zdo.Set("fuel", zdo.GetFloat("hiddenFuelAmount", 0f) + @float);
			}
			if (zdo.GetFloat("hiddenFuelAmount", 0f) != @float)
			{
				float value = @float;
				zdo.Set("hiddenFuelAmount", value);
			}
			if (zdo.GetFloat("fuel", 0f) > __instance.m_maxFuel)
			{
				zdo.Set("fuel", __instance.m_maxFuel);
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000029A4 File Offset: 0x00000BA4
		[HarmonyPostfix]
		[HarmonyPatch(typeof(Fireplace), "RPC_AddFuel")]
		public static void RPC_AddFuel_Patch(long sender, Fireplace __instance)
		{
			ZDO zdo = __instance.m_nview.GetZDO();
			if (FireplaceUtilitiesPlugin.timeOfDay == "Day" && zdo.GetBool("turnOffBetweenTime", false))
			{
				zdo.Set("turnOffBetweenTime", false);
			}
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000029E8 File Offset: 0x00000BE8
		[HarmonyPostfix]
		[HarmonyPatch(typeof(Fireplace), "Awake")]
		public static void FireplaceAwake_Patch(Fireplace __instance)
		{
			string name = __instance.name;
			if (FireplaceUtilitiesPlugin.customBurnTimes.Value)
			{
				foreach (KeyValuePair<string, int> keyValuePair in FireplaceUtilitiesPlugin.customBurnDict)
				{
					if (name.Contains(keyValuePair.Key) && __instance.m_secPerFuel != (float)keyValuePair.Value)
					{
						__instance.m_secPerFuel = (float)keyValuePair.Value;
					}
				}
			}
			if (!FireplaceUtilitiesPlugin.torchUseCoal.Value)
			{
				return;
			}
			GameObject prefab = ZNetScene.instance.GetPrefab("Coal");
			if ((name.Contains("groundtorch") && !name.Contains("green")) || name.Contains("walltorch"))
			{
				__instance.m_fuelItem = prefab.GetComponent<ItemDrop>();
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002AC4 File Offset: 0x00000CC4
		[HarmonyPostfix]
		[HarmonyPatch(typeof(Fireplace), "GetHoverText")]
		public static string FireplaceGetHoverText_Patch(string __result, Fireplace __instance)
		{
			string text = __result;
			if (__instance == null)
			{
				return text;
			}
			ZDO zdo = __instance.m_nview.GetZDO();
			float @float = zdo.GetFloat("hiddenFuelAmount", 0f);
			if (FireplaceUtilitiesPlugin.extinguishItems.Value && !__instance.IsBurning() && @float > 0f)
			{
				string text2 = (FireplaceUtilitiesPlugin.keyPOTextString.Value != "none") ? FireplaceUtilitiesPlugin.keyPOTextString.Value : FireplaceUtilitiesPlugin.keyPOCodeString.Value;
				int num = (int)__instance.m_maxFuel;
				text = string.Concat(new string[]
				{
					text.Replace(string.Format("0/{0}", num), string.Format("{0}/{1}", (int)Mathf.Ceil(@float), num)),
					"\n[<color=yellow><b>",
					text2,
					"</b></color>] ",
					FireplaceUtilitiesPlugin.igniteString.Value
				});
			}
			if (FireplaceUtilitiesPlugin.disableTorches.Value)
			{
				string text3 = (FireplaceUtilitiesPlugin.timeToggleTextString.Value != "none") ? FireplaceUtilitiesPlugin.timeToggleTextString.Value : FireplaceUtilitiesPlugin.timeToggleCodeString.Value;
				string text4 = zdo.GetBool("turnOffBetweenTime", false) ? FireplaceUtilitiesPlugin.timeToggleOffString.Value : FireplaceUtilitiesPlugin.timeToggleString.Value;
				text = string.Concat(new string[]
				{
					text,
					"\n[<color=yellow><b>",
					text3,
					"</b></color>] ",
					text4
				});
			}
			if (!__instance.IsBurning())
			{
				return text;
			}
			if (__instance.m_wet)
			{
				return text;
			}
			if (FireplaceUtilitiesPlugin.extinguishItems.Value)
			{
				string text5 = (FireplaceUtilitiesPlugin.keyPOTextString.Value != "none") ? FireplaceUtilitiesPlugin.keyPOTextString.Value : FireplaceUtilitiesPlugin.keyPOCodeString.Value;
				text = string.Concat(new string[]
				{
					text,
					"\n[<color=yellow><b>",
					text5,
					"</b></color>] ",
					FireplaceUtilitiesPlugin.extinguishString.Value
				});
			}
			if (FireplaceUtilitiesPlugin.returnFuel.Value)
			{
				string text6 = (FireplaceUtilitiesPlugin.returnTextString.Value != "none") ? FireplaceUtilitiesPlugin.returnTextString.Value : FireplaceUtilitiesPlugin.returnCodeString.Value;
				text = string.Concat(new string[]
				{
					text,
					"\n[<color=yellow><b>",
					text6,
					"</b></color>] ",
					FireplaceUtilitiesPlugin.returnString.Value
				});
			}
			if (FireplaceUtilitiesPlugin.burnItems.Value)
			{
				if (!FireplaceUtilitiesPlugin.torchBurn.Value)
				{
					string name = __instance.name;
					if (name.Contains("groundtorch") || name.Contains("walltorch") || name.Contains("brazier"))
					{
						return text;
					}
				}
				string text7 = (FireplaceUtilitiesPlugin.keyBurnTextString.Value != "none") ? FireplaceUtilitiesPlugin.keyBurnTextString.Value : FireplaceUtilitiesPlugin.keyBurnCodeString.Value;
				text = string.Concat(new string[]
				{
					text,
					"\n[<color=yellow><b>",
					text7,
					" + 1-8</b></color>] ",
					FireplaceUtilitiesPlugin.burnItemString.Value
				});
			}
			return text;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x00002DE0 File Offset: 0x00000FE0
		[HarmonyPostfix]
		[HarmonyPatch(typeof(Player), "Update")]
		public static void PlayerUpdate_Patch(Player __instance)
		{
			if (!__instance)
			{
				return;
			}
			bool key = Input.GetKey(FireplaceUtilitiesPlugin.configBurnKey);
			bool keyUp = Input.GetKeyUp(FireplaceUtilitiesPlugin.configPOKey);
			bool keyUp2 = Input.GetKeyUp(FireplaceUtilitiesPlugin.returnKey);
			if (Input.GetKeyUp(FireplaceUtilitiesPlugin.timeToggleKey) && FireplaceUtilitiesPlugin.disableTorches.Value)
			{
				Fireplace andCheckFireplace = FireplaceUtilitiesPlugin.GetAndCheckFireplace(__instance, false);
				if (andCheckFireplace == null)
				{
					return;
				}
				ZDO zdo = andCheckFireplace.m_nview.GetZDO();
				zdo.Set("turnOffBetweenTime", !zdo.GetBool("turnOffBetweenTime", false));
			}
			if (keyUp2 && FireplaceUtilitiesPlugin.returnFuel.Value)
			{
				Fireplace andCheckFireplace2 = FireplaceUtilitiesPlugin.GetAndCheckFireplace(__instance, true);
				if (andCheckFireplace2 == null)
				{
					return;
				}
				float num = Mathf.Floor(andCheckFireplace2.m_nview.GetZDO().GetFloat("fuel", 0f));
				GameObject prefab = ZNetScene.instance.GetPrefab(andCheckFireplace2.m_fuelItem.name);
				andCheckFireplace2.m_fuelAddedEffects.Create(andCheckFireplace2.transform.position, andCheckFireplace2.transform.rotation, null, 1f);
				andCheckFireplace2.m_nview.GetZDO().Set("fuel", 0f);
				for (int i = 0; i < (int)num; i++)
				{
					UnityEngine.Object.Instantiate<GameObject>(prefab, andCheckFireplace2.transform.position + Vector3.up, Quaternion.identity).GetComponent<Character>();
				}
			}
			if (keyUp && FireplaceUtilitiesPlugin.extinguishItems.Value)
			{
				Fireplace andCheckFireplace3 = FireplaceUtilitiesPlugin.GetAndCheckFireplace(__instance, false);
				if (andCheckFireplace3 == null)
				{
					return;
				}
				ZDO zdo2 = andCheckFireplace3.m_nview.GetZDO();
				bool flag = !zdo2.GetBool("enabledFire", false);
				zdo2.Set("enabledFire", flag);
				if (!flag)
				{
					if (FireplaceUtilitiesPlugin.timeOfDay == "Night" && zdo2.GetBool("turnOffBetweenTime", false))
					{
						zdo2.Set("turnOffBetweenTime", false);
					}
					andCheckFireplace3.m_fuelAddedEffects.Create(andCheckFireplace3.transform.position, andCheckFireplace3.transform.rotation, null, 1f);
					zdo2.Set("fuel", 0f);
				}
				if (flag)
				{
					if (FireplaceUtilitiesPlugin.timeOfDay == "Day" && zdo2.GetBool("turnOffBetweenTime", false))
					{
						zdo2.Set("turnOffBetweenTime", false);
					}
					andCheckFireplace3.m_fuelAddedEffects.Create(andCheckFireplace3.transform.position, andCheckFireplace3.transform.rotation, null, 1f);
					zdo2.Set("fuel", zdo2.GetFloat("hiddenFuelAmount", 0f));
				}
			}
			for (int j = 1; j < 9; j++)
			{
				if (key && Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), "Alpha" + j.ToString())) && FireplaceUtilitiesPlugin.burnItems.Value)
				{
					Fireplace andCheckFireplace4 = FireplaceUtilitiesPlugin.GetAndCheckFireplace(__instance, true);
					if (andCheckFireplace4 == null)
					{
						return;
					}
					if (!FireplaceUtilitiesPlugin.torchBurn.Value)
					{
						string name = andCheckFireplace4.name;
						if (name.Contains("groundtorch") || name.Contains("walltorch") || name.Contains("brazier"))
						{
							return;
						}
					}
					Inventory inventory = __instance.GetInventory();
					ItemDrop.ItemData itemAt = inventory.GetItemAt(j - 1, 0);
					if (itemAt == null)
					{
						return;
					}
					if (!FireplaceUtilitiesPlugin.notAllowed.Contains(itemAt.m_shared.m_name))
					{
						inventory.RemoveOneItem(itemAt);
						andCheckFireplace4.m_fuelAddedEffects.Create(andCheckFireplace4.transform.position, andCheckFireplace4.transform.rotation, null, 1f);
						MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "", 0, null);
						if (itemAt.IsEquipable())
						{
							__instance.ToggleEquiped(itemAt);
						}
						if (!FireplaceUtilitiesPlugin.giveCoal.Value)
						{
							return;
						}
						GameObject prefab2 = ZNetScene.instance.GetPrefab("Coal");
						for (int k = 0; k < FireplaceUtilitiesPlugin.coalAmount.Value; k++)
						{
							UnityEngine.Object.Instantiate<GameObject>(prefab2, andCheckFireplace4.transform.position + Vector3.up, Quaternion.identity).GetComponent<Character>();
						}
					}
				}
			}
		}

		// Token: 0x04000001 RID: 1
		private static ConfigEntry<bool> enableMod;

		// Token: 0x04000002 RID: 2
		private static ConfigEntry<string> blacklistBurn;

		// Token: 0x04000003 RID: 3
		private static ConfigEntry<bool> torchBurn;

		// Token: 0x04000004 RID: 4
		private static ConfigEntry<bool> giveCoal;

		// Token: 0x04000005 RID: 5
		private static ConfigEntry<bool> torchUseCoal;

		// Token: 0x04000006 RID: 6
		private static ConfigEntry<bool> burnItems;

		// Token: 0x04000007 RID: 7
		private static ConfigEntry<bool> extinguishItems;

		// Token: 0x04000008 RID: 8
		private static ConfigEntry<bool> returnFuel;

		// Token: 0x04000009 RID: 9
		private static ConfigEntry<bool> disableTorches;

		// Token: 0x0400000A RID: 10
		private static ConfigEntry<bool> customBurnTimes;

		// Token: 0x0400000B RID: 11
		private static ConfigEntry<int> coalAmount;

		// Token: 0x0400000C RID: 12
		private static ConfigEntry<int> firepitBurnTime;

		// Token: 0x0400000D RID: 13
		private static ConfigEntry<int> groundtorchwoodBurnTime;

		// Token: 0x0400000E RID: 14
		private static ConfigEntry<int> bonfireBurnTime;

		// Token: 0x0400000F RID: 15
		private static ConfigEntry<int> hearthBurnTime;

		// Token: 0x04000010 RID: 16
		private static ConfigEntry<int> walltorchBurnTime;

		// Token: 0x04000011 RID: 17
		private static ConfigEntry<int> groundtorchironBurnTime;

		// Token: 0x04000012 RID: 18
		private static ConfigEntry<int> groundtorchgreenBurnTime;

		// Token: 0x04000013 RID: 19
		private static ConfigEntry<int> braziercBurnTime;

		// Token: 0x04000014 RID: 20
		public static ConfigEntry<string> extinguishString;

		// Token: 0x04000015 RID: 21
		public static ConfigEntry<string> burnItemString;

		// Token: 0x04000016 RID: 22
		public static ConfigEntry<string> keyBurnCodeString;

		// Token: 0x04000017 RID: 23
		public static ConfigEntry<string> keyBurnTextString;

		// Token: 0x04000018 RID: 24
		public static ConfigEntry<string> keyPOCodeString;

		// Token: 0x04000019 RID: 25
		public static ConfigEntry<string> keyPOTextString;

		// Token: 0x0400001A RID: 26
		public static ConfigEntry<string> returnString;

		// Token: 0x0400001B RID: 27
		public static ConfigEntry<string> returnCodeString;

		// Token: 0x0400001C RID: 28
		public static ConfigEntry<string> returnTextString;

		// Token: 0x0400001D RID: 29
		public static ConfigEntry<string> igniteString;

		// Token: 0x0400001E RID: 30
		public static ConfigEntry<string> timeToggleString;

		// Token: 0x0400001F RID: 31
		public static ConfigEntry<string> timeToggleCodeString;

		// Token: 0x04000020 RID: 32
		public static ConfigEntry<string> timeToggleTextString;

		// Token: 0x04000021 RID: 33
		public static ConfigEntry<string> timeToggleOffString;

		// Token: 0x04000022 RID: 34
		public static Dictionary<string, int> customBurnDict = new Dictionary<string, int>();

		// Token: 0x04000023 RID: 35
		public static string timeOfDay = "Day";

		// Token: 0x04000024 RID: 36
		public static List<string> notAllowed;

		// Token: 0x04000025 RID: 37
		public static KeyCode configBurnKey;

		// Token: 0x04000026 RID: 38
		public static KeyCode configPOKey;

		// Token: 0x04000027 RID: 39
		public static KeyCode returnKey;

		// Token: 0x04000028 RID: 40
		public static KeyCode timeToggleKey;
	}
}
