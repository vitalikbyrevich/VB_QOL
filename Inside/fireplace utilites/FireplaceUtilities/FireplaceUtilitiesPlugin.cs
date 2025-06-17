using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using ServerSync;
using BepInEx.Logging;
using System.Threading.Tasks;

namespace FireplaceUtilities
{
	[BepInPlugin(ModGUID, ModName, ModVersion)]
	[HarmonyPatch]
	public class FireplaceUtilitiesPlugin : BaseUnityPlugin
	{
		private const string ModName = "Fireplace Utilities";
		private const string ModVersion = "2.1.1";
		private const string ModGUID = "smallo.mods.fireplaceutilities";

		public static Dictionary<string, int> customBurnDict = new Dictionary<string, int>();
		public static string timeOfDay = "Day";
		public static List<string> notAllowed;
		public static KeyCode configBurnKey;
		public static KeyCode configPOKey;
		public static KeyCode returnKey;
		public static KeyCode timeToggleKey;

		private readonly Harmony _harmony = new(ModGUID);
		internal static FireplaceUtilitiesPlugin _self;
		DateTime LastConfigChange;
        private static ConfigEntry<bool> _serverConfigLocked = null!;

		public static ConfigEntry<bool> enableModConfig;
		public static ConfigEntry<bool> burnItemsConfig;
		public static ConfigEntry<bool> extinguishItemsConfig;
		public static ConfigEntry<bool> disableTorchesConfig;
		public static ConfigEntry<bool> returnFuelConfig;
		public static ConfigEntry<bool> torchUseCoalConfig;
		public static ConfigEntry<bool> customBurnTimesConfig;
		public static ConfigEntry<bool> torchBurnConfig;
		public static ConfigEntry<bool> giveCoalConfig;
		public static ConfigEntry<string> blacklistBurnConfig;
		public static ConfigEntry<string> burnItemStringConfig;
		public static ConfigEntry<float> coalAmountConfig;
		public static ConfigEntry<string> keyBurnCodeStringConfig;
		public static ConfigEntry<string> keyBurnTextStringConfig;
		public static ConfigEntry<string> extinguishStringConfig;
		public static ConfigEntry<string> igniteStringConfig;
		public static ConfigEntry<string> keyPOCodeStringConfig;
		public static ConfigEntry<string> keyPOTextStringConfig;
		public static ConfigEntry<string> returnStringConfig;
		public static ConfigEntry<string> returnCodeStringConfig;
		public static ConfigEntry<string> returnTextStringConfig;
		public static ConfigEntry<string> timeToggleStringConfig;
		public static ConfigEntry<string> timeToggleOffStringConfig;
		public static ConfigEntry<string> timeToggleCodeStringConfig;
		public static ConfigEntry<string> timeToggleTextStringConfig;
		public static ConfigEntry<int> firepitBurnTimeConfig;
		public static ConfigEntry<int> groundtorchwoodBurnTimeConfig;
		public static ConfigEntry<int> bonfireBurnTimeConfig;
		public static ConfigEntry<int> hearthBurnTimeConfig;
		public static ConfigEntry<int> walltorchBurnTimeConfig;
		public static ConfigEntry<int> groundtorchironBurnTimeConfig;
		public static ConfigEntry<int> groundtorchgreenBurnTimeConfig;
		public static ConfigEntry<int> braziercBurnTimeConfig;

		internal static bool enableMod;
		internal static bool burnItems;
		internal static bool extinguishItems;
		internal static bool disableTorches;
		internal static bool returnFuel;
		internal static bool torchUseCoal;
		internal static bool customBurnTimes;
		internal static bool torchBurn;
		internal static bool giveCoal;
		internal static string blacklistBurn;
		internal static string burnItemString;
		internal static float coalAmount;
		internal static string keyBurnCodeString;
		internal static string keyBurnTextString;
		internal static string extinguishString;
		internal static string igniteString;
		internal static string keyPOCodeString;
		internal static string keyPOTextString;
		internal static string returnString;
		internal static string returnCodeString;
		internal static string returnTextString;
		internal static string timeToggleString;
		internal static string timeToggleOffString;
		internal static string timeToggleCodeString;
		internal static string timeToggleTextString;
		internal static int firepitBurnTime;
		internal static int groundtorchwoodBurnTime;
		internal static int bonfireBurnTime;
		internal static int hearthBurnTime;
		internal static int walltorchBurnTime;
		internal static int groundtorchironBurnTime;
		internal static int groundtorchgreenBurnTime;
		internal static int braziercBurnTime;

		private void Awake()
		{
			_self = this;
			_serverConfigLocked = config("1 - Global", "Lock Configuration", true,
				"Если включено, конфигурация заблокирована и может быть изменена только администраторами сервера.");

			enableModConfig = config("1 - Global", "Enable Mod", true, new ConfigDescription("Enable or disable this mod"), true);
            if (!enableModConfig.Value)
			{
				return;
			}
            burnItemsConfig = config("2 - Toggles", "Burn Items In Fire", true, new ConfigDescription("Allows you to burn items in fires"), true);
			extinguishItemsConfig = config("2 - Toggles", "Extinguish Fires", true, new ConfigDescription("Allows you to turn fires off/on"), true);
			disableTorchesConfig = config("2 - Toggles", "Disable Fires During The Day", false, new ConfigDescription("Allows you to make fires turn off during the day, you must press a key on each item to let it toggle"), true);
			returnFuelConfig = config("2 - Toggles", "Return Fuel", true, new ConfigDescription("Allows you to press a key to return the fuel left in a fire"), true);
			torchUseCoalConfig = config("2 - Toggles", "Torch and Sconce Use Coal", false, new ConfigDescription("Makes the Wood/Iron Torch and Sconce use Coal as fuel instead of resin"), true);
			customBurnTimesConfig = config("2 - Toggles", "Custom Burn Times", false, new ConfigDescription("Enable custom burn times for all fireplaces, the default values are the games vanilla values"), true);

			torchBurnConfig = config("3 - Burn Items In Fire", "Burn In Torches", false, new ConfigDescription("Allows items to be burnt in ground torches, wall torches or braziers"), true);
			giveCoalConfig = config("3 - Burn Items In Fire", "Give Coal", true, new ConfigDescription("Returns coal when burning an item"), true);
			blacklistBurnConfig = config("3 - Burn Items In Fire", "Blacklist Items", "$item_wood", new ConfigDescription("Items that aren't allowed to be burned. Seperate items by a comma. Wood should remain as a default so that way it doesn't take your wood twice when lighting a fire, if you have a mod that allows other wood types to burn, put them on this list."), true);
			burnItemStringConfig = config("3 - Burn Items In Fire", "Burn Item Text", "Burn item", new ConfigDescription("The text to show when hovering over the fire"), false);
			coalAmountConfig = config("3 - Burn Items In Fire", "Coal Amount", 1f, new ConfigDescription("Amount of coal to give when burning an item"), true);
			keyBurnCodeStringConfig = config("3 - Burn Items In Fire", "Burn Key", "LeftShift", new ConfigDescription("The key to use in combination with the hotkeys. KeyCodes can be found here https://docs.unity3d.com/ScriptReference/KeyCode.html"), false);
			keyBurnTextStringConfig = config("3 - Burn Items In Fire", "Burn Key Text", "LShift", new ConfigDescription("The custom text to show for the string, if you set it to \"none\" then it'll use what you have in the 'Key' config option."), false);

			extinguishStringConfig = config("4 - Extinguish Fires", "Extinguish Fire Text", "Extinguish fire", new ConfigDescription("The text to show when hovering over the fire"), false);
			igniteStringConfig = config("4 - Extinguish Fires", "Ignite Fire Text", "Ignite fire", new ConfigDescription("The text to show when hovering over the fire if the fire is extinguished"), false);
			keyPOCodeStringConfig = config("4 - Extinguish Fires", "Put Out Fire Key", "LeftAlt", new ConfigDescription("The key to use to put out a fire. KeyCodes can be found here https://docs.unity3d.com/ScriptReference/KeyCode.html"), false);
			keyPOTextStringConfig = config("4 - Extinguish Fires", "Put Out Fire Key Text", "LAlt", new ConfigDescription("The custom text to show for the string, if you set it to \"none\" then it'll use what you have in the 'Key' config option."), false);

			returnStringConfig = config("5 - Return Fuel", "Return Fuel Text", "Return fuel", new ConfigDescription("The text to show when hovering over the fire"), false);
			returnCodeStringConfig = config("5 - Return Fuel", "Return Fuel Key", "LeftControl", new ConfigDescription("The key to use to return the fuel. KeyCodes can be found here https://docs.unity3d.com/ScriptReference/KeyCode.html"), false);
			returnTextStringConfig = config("5 - Return Fuel", "Return Fuel Key Text", "LCtrl", new ConfigDescription("The custom text to show for the string, if you set it to \"none\" then it'll use what you have in the 'Key' config option."), false);

			timeToggleStringConfig = config("6 - Disable Fires During The Day", "Time Toggle On Text", "Enable Timer", new ConfigDescription("The text to show when hovering over the fire to enable the timer"), false);
			timeToggleOffStringConfig = config("6 - Disable Fires During The Day", "Time Toggle Off Text", "Disable Timer", new ConfigDescription("The text to show when hovering over the fire to disable the timer"), false);
			timeToggleCodeStringConfig = config("6 - Disable Fires During The Day", "Time Toggle Key", "Equals", new ConfigDescription("The key to use to return the fuel. KeyCodes can be found here https://docs.unity3d.com/ScriptReference/KeyCode.html"), false);
			timeToggleTextStringConfig = config("6 - Disable Fires During The Day", "Time Toggle Key Text", "=", new ConfigDescription("The custom text to show for the string, if you set it to \"none\" then it'll use what you have in the 'Key' config option."), false);

			firepitBurnTimeConfig = config("7 - Custom Burn Times", "Firepit", 5000, new ConfigDescription("Custom burntime for the standard firepit"), true);
			groundtorchwoodBurnTimeConfig = config("7 - Custom Burn Times", "Wood Ground Torch", 10000, new ConfigDescription("Custom burntime for the wooden ground torch"), true);
			bonfireBurnTimeConfig = config("7 - Custom Burn Times", "Bonfire", 5000, new ConfigDescription("Custom burntime for the bonfire"), true);
			hearthBurnTimeConfig = config("7 - Custom Burn Times", "Hearth", 5000, new ConfigDescription("Custom burntime for the hearth"), true);
			walltorchBurnTimeConfig = config("7 - Custom Burn Times", "Sconce", 20000, new ConfigDescription("Custom burntime for the sconce"), true);
			groundtorchironBurnTimeConfig = config("7 - Custom Burn Times", "Iron Ground Torch", 20000, new ConfigDescription("Custom burntime for the iron ground torch"), true);
			groundtorchgreenBurnTimeConfig = config("7 - Custom Burn Times", "Green Ground Torch", 20000, new ConfigDescription("Custom burntime for the green ground torch"), true);
			braziercBurnTimeConfig = config("7 - Custom Burn Times", "Brazier", 20000, new ConfigDescription("Custom burntime for the brazier"), true);
			if (customBurnTimesConfig.Value)
			{
                customBurnDict.Add("fire_pit", firepitBurnTimeConfig.Value);
                customBurnDict.Add("piece_groundtorch_wood", groundtorchwoodBurnTimeConfig.Value);
                customBurnDict.Add("bonfire", bonfireBurnTimeConfig.Value);
                customBurnDict.Add("hearth", hearthBurnTimeConfig.Value);
                customBurnDict.Add("piece_walltorch", walltorchBurnTimeConfig.Value);
                customBurnDict.Add("piece_groundtorch", groundtorchironBurnTimeConfig.Value);
                customBurnDict.Add("piece_groundtorch_green", groundtorchgreenBurnTimeConfig.Value);
                customBurnDict.Add("piece_brazierceiling01", braziercBurnTimeConfig.Value);
			}
            notAllowed = blacklistBurnConfig.Value.Replace(" ", "").Split(new char[]
			{
				','
			}).ToList<string>();
            configBurnKey = (KeyCode)Enum.Parse(typeof(KeyCode), keyBurnCodeStringConfig.Value);
            configPOKey = (KeyCode)Enum.Parse(typeof(KeyCode), keyPOCodeStringConfig.Value);
            returnKey = (KeyCode)Enum.Parse(typeof(KeyCode), returnCodeStringConfig.Value);
            timeToggleKey = (KeyCode)Enum.Parse(typeof(KeyCode), timeToggleCodeStringConfig.Value);

			_ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

			SetupWatcherOnConfigFile();
			Config.ConfigReloaded += (_, _) => { UpdateConfiguration(); };
			Config.SaveOnConfigSet = true;

			Assembly assembly = Assembly.GetExecutingAssembly();
			_harmony.PatchAll(assembly);
			SetupWatcher();
		}

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

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Player), "OnSpawned")]
		public static void PlayerOnSpawned_Patch()
		{
			if (disableTorchesConfig.Value)
			{
				float num = 0f;
				if (EnvMan.instance)
				{
					num = (float)typeof(EnvMan).GetField("m_smoothDayFraction", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(EnvMan.instance);
				}
				int num2 = (int)(num * 24f);
                timeOfDay = ((num2 < 18 && num2 > 6) ? (timeOfDay = "Day") : (timeOfDay = "Night"));
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(EnvMan), "OnEvening")]
		public static void EvnManOnEvening_Patch(Heightmap.Biome biome, EnvSetup currentEnv)
		{
			if (disableTorchesConfig.Value)
			{
				Fireplace[] array = FindObjectsOfType<Fireplace>();
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
                timeOfDay = "Night";
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(EnvMan), "OnMorning")]
		public static void EvnManOnMorning_Patch(Heightmap.Biome biome, EnvSetup currentEnv)
		{
			if (disableTorchesConfig.Value)
			{
				Fireplace[] array = FindObjectsOfType<Fireplace>();
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
                timeOfDay = "Day";
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Fireplace), "UpdateFireplace")]
		public static void FireplaceUpdateFireplace_Patch(Fireplace __instance)
		{
			ZDO zdo = __instance.m_nview.GetZDO();
			bool @bool = zdo.GetBool("enabledFire", false);
			float @float = zdo.GetFloat("fuel", 0f);
			if (disableTorchesConfig.Value)
			{
				if (!zdo.GetBool("turnOffBetweenTime", false))
				{
					return;
				}
				if (timeOfDay == "Night")
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

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Fireplace), "RPC_AddFuel")]
		public static void RPC_AddFuel_Patch(long sender, Fireplace __instance)
		{
			ZDO zdo = __instance.m_nview.GetZDO();
			if (timeOfDay == "Day" && zdo.GetBool("turnOffBetweenTime", false))
			{
				zdo.Set("turnOffBetweenTime", false);
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Fireplace), "Awake")]
		public static void FireplaceAwake_Patch(Fireplace __instance)
		{
			string name = __instance.name;
			if (customBurnTimesConfig.Value)
			{
				foreach (KeyValuePair<string, int> keyValuePair in customBurnDict)
				{
					if (name.Contains(keyValuePair.Key) && __instance.m_secPerFuel != (float)keyValuePair.Value)
					{
						__instance.m_secPerFuel = (float)keyValuePair.Value;
					}
				}
			}
			if (!torchUseCoalConfig.Value)
			{
				return;
			}
			GameObject prefab = ZNetScene.instance.GetPrefab("Coal");
			if ((name.Contains("groundtorch") && !name.Contains("green")) || name.Contains("walltorch"))
			{
				__instance.m_fuelItem = prefab.GetComponent<ItemDrop>();
			}
		}

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
			if (extinguishItemsConfig.Value && !__instance.IsBurning() && @float > 0f)
			{
				string text2 = (keyPOTextStringConfig.Value != "none") ? keyPOTextStringConfig.Value : keyPOCodeStringConfig.Value;
				int num = (int)__instance.m_maxFuel;
				text = string.Concat(new string[]
				{
					text.Replace(string.Format("0/{0}", num), string.Format("{0}/{1}", (int)Mathf.Ceil(@float), num)),
					"\n[<color=yellow><b>",
					text2,
					"</b></color>] ",
					igniteStringConfig.Value
				});
			}
			if (disableTorchesConfig.Value)
			{
				string text3 = (timeToggleTextStringConfig.Value != "none") ? timeToggleTextStringConfig.Value : timeToggleCodeStringConfig.Value;
				string text4 = zdo.GetBool("turnOffBetweenTime", false) ? timeToggleOffStringConfig.Value : timeToggleStringConfig.Value;
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
			if (extinguishItemsConfig.Value)
			{
				string text5 = (keyPOTextStringConfig.Value != "none") ? keyPOTextStringConfig.Value : keyPOCodeStringConfig.Value;
				text = string.Concat(new string[]
				{
					text,
					"\n[<color=yellow><b>",
					text5,
					"</b></color>] ",
					extinguishStringConfig.Value
				});
			}
			if (returnFuelConfig.Value)
			{
				string text6 = (returnTextStringConfig.Value != "none") ? returnTextStringConfig.Value : returnCodeStringConfig.Value;
				text = string.Concat(new string[]
				{
					text,
					"\n[<color=yellow><b>",
					text6,
					"</b></color>] ",
					returnStringConfig.Value
				});
			}
			if (burnItemsConfig.Value)
			{
				if (!torchBurnConfig.Value)
				{
					string name = __instance.name;
					if (name.Contains("groundtorch") || name.Contains("walltorch") || name.Contains("brazier"))
					{
						return text;
					}
				}
				string text7 = (keyBurnTextStringConfig.Value != "none") ? keyBurnTextStringConfig.Value : keyBurnCodeStringConfig.Value;
				text = string.Concat(new string[]
				{
					text,
					"\n[<color=yellow><b>",
					text7,
					" + 1-8</b></color>] ",
					burnItemStringConfig.Value
				});
			}
			return text;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Player), "Update")]
		public static void PlayerUpdate_Patch(Player __instance)
		{
			if (!__instance)
			{
				return;
			}
			bool key = Input.GetKey(configBurnKey);
			bool keyUp = Input.GetKeyUp(configPOKey);
			bool keyUp2 = Input.GetKeyUp(returnKey);
			if (Input.GetKeyUp(timeToggleKey) && disableTorchesConfig.Value)
			{
				Fireplace andCheckFireplace = GetAndCheckFireplace(__instance, false);
				if (andCheckFireplace == null)
				{
					return;
				}
				ZDO zdo = andCheckFireplace.m_nview.GetZDO();
				zdo.Set("turnOffBetweenTime", !zdo.GetBool("turnOffBetweenTime", false));
			}
			if (keyUp2 && returnFuelConfig.Value)
			{
				Fireplace andCheckFireplace2 = GetAndCheckFireplace(__instance, true);
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
                    Instantiate(prefab, andCheckFireplace2.transform.position + Vector3.up, Quaternion.identity).GetComponent<Character>();
				}
			}
			if (keyUp && extinguishItemsConfig.Value)
			{
				Fireplace andCheckFireplace3 = GetAndCheckFireplace(__instance, false);
				if (andCheckFireplace3 == null)
				{
					return;
				}
				ZDO zdo2 = andCheckFireplace3.m_nview.GetZDO();
				bool flag = !zdo2.GetBool("enabledFire", false);
				zdo2.Set("enabledFire", flag);
				if (!flag)
				{
					if (timeOfDay == "Night" && zdo2.GetBool("turnOffBetweenTime", false))
					{
						zdo2.Set("turnOffBetweenTime", false);
					}
					andCheckFireplace3.m_fuelAddedEffects.Create(andCheckFireplace3.transform.position, andCheckFireplace3.transform.rotation, null, 1f);
					zdo2.Set("fuel", 0f);
				}
				if (flag)
				{
					if (timeOfDay == "Day" && zdo2.GetBool("turnOffBetweenTime", false))
					{
						zdo2.Set("turnOffBetweenTime", false);
					}
					andCheckFireplace3.m_fuelAddedEffects.Create(andCheckFireplace3.transform.position, andCheckFireplace3.transform.rotation, null, 1f);
					zdo2.Set("fuel", zdo2.GetFloat("hiddenFuelAmount", 0f));
				}
			}
			for (int j = 1; j < 9; j++)
			{
				if (key && Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), "Alpha" + j.ToString())) && burnItemsConfig.Value)
				{
					Fireplace andCheckFireplace4 = GetAndCheckFireplace(__instance, true);
					if (andCheckFireplace4 == null)
					{
						return;
					}
					if (!torchBurnConfig.Value)
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
					if (!notAllowed.Contains(itemAt.m_shared.m_name))
					{
						inventory.RemoveOneItem(itemAt);
						andCheckFireplace4.m_fuelAddedEffects.Create(andCheckFireplace4.transform.position, andCheckFireplace4.transform.rotation, null, 1f);
						MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "", 0, null);
						if (itemAt.IsEquipable())
						{
							__instance.ToggleEquipped(itemAt);
						}
						if (!giveCoalConfig.Value)
						{
							return;
						}
						GameObject prefab2 = ZNetScene.instance.GetPrefab("Coal");
						for (int k = 0; k < coalAmountConfig.Value; k++)
						{
                            Instantiate(prefab2, andCheckFireplace4.transform.position + Vector3.up, Quaternion.identity).GetComponent<Character>();
						}
					}
				}
			}
		}


		private static readonly string ConfigFileName = ModGUID + ".cfg";

		private static readonly string ConfigFileFullPath =
			Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;


		public static readonly ManualLogSource CreatureManagerModTemplateLogger =
			BepInEx.Logging.Logger.CreateLogSource(ModName);

		private static readonly ConfigSync ConfigSync = new(ModGUID)
		{ DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

		private void SetupWatcher()
		{
			FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
			watcher.Changed += ReadConfigValues;
			watcher.Created += ReadConfigValues;
			watcher.Renamed += ReadConfigValues;
			watcher.IncludeSubdirectories = true;
			watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
			watcher.EnableRaisingEvents = true;
		}

		private void ReadConfigValues(object sender, FileSystemEventArgs e)
		{
			if (!File.Exists(ConfigFileFullPath)) return;
			try
			{
				CreatureManagerModTemplateLogger.LogDebug("ReadConfigValues called");
				Config.Reload();
			}
			catch
			{
				CreatureManagerModTemplateLogger.LogError($"There was an issue loading your {ConfigFileName}");
				CreatureManagerModTemplateLogger.LogError("Please check your config entries for spelling and format!");
			}
		}

		private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
			bool synchronizedSetting = true)
		{
			ConfigDescription extendedDescription =
				new(
					description.Description +
					(synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
					description.AcceptableValues, description.Tags);
			ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
			// var configEntry = Config.Bind(group, name, value, description);

			SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
			syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

			return configEntry;
		}

		private ConfigEntry<T> config<T>(string group, string name, T value, string description,
			bool synchronizedSetting = true)
		{
			return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
		}

		private class ConfigurationManagerAttributes
		{
			public bool? Browsable = false;
		}

		public void SetupWatcherOnConfigFile()
		{
            FileSystemWatcher fileSystemWatcherOnConfig = new(Paths.ConfigPath, ConfigFileName);
			fileSystemWatcherOnConfig.Changed += ConfigChanged;
			fileSystemWatcherOnConfig.IncludeSubdirectories = true;
			fileSystemWatcherOnConfig.SynchronizingObject = ThreadingHelper.SynchronizingObject;
			fileSystemWatcherOnConfig.EnableRaisingEvents = true;
		}

		private void ConfigChanged(object sender, FileSystemEventArgs e)
		{
			if ((DateTime.Now - LastConfigChange).TotalSeconds <= 5.0)
			{
				return;
			}

			LastConfigChange = DateTime.Now;
			try
			{
				Config.Reload();
				// Debug("Reloading Config...");
			}
			catch
			{
				//  DebugError("Can't reload Config");
			}
		}

		private void UpdateConfiguration()
		{
			Task task = null;
			task = Task.Run(() =>
			{
				enableMod = enableModConfig.Value;
				burnItems = burnItemsConfig.Value;
				extinguishItems = extinguishItemsConfig.Value;
				disableTorches = disableTorchesConfig.Value;
				returnFuel = returnFuelConfig.Value;
				torchUseCoal = torchUseCoalConfig.Value;
				customBurnTimes = customBurnTimesConfig.Value;
				torchBurn = torchBurnConfig.Value;
				giveCoal = giveCoalConfig.Value;
				blacklistBurn = blacklistBurnConfig.Value;
				burnItemString = burnItemStringConfig.Value;
				coalAmount = coalAmountConfig.Value;
				keyBurnCodeString = keyBurnCodeStringConfig.Value;
				keyBurnTextString = keyBurnTextStringConfig.Value;
				extinguishString = extinguishStringConfig.Value;
				igniteString = igniteStringConfig.Value;
				keyPOCodeString = keyPOCodeStringConfig.Value;
				keyPOTextString = keyPOTextStringConfig.Value;
				returnString = returnStringConfig.Value;
				returnCodeString = returnCodeStringConfig.Value;
				returnTextString = returnTextStringConfig.Value;
				timeToggleString = timeToggleStringConfig.Value;
				timeToggleOffString = timeToggleOffStringConfig.Value;
				timeToggleCodeString = timeToggleCodeStringConfig.Value;
				timeToggleTextString = timeToggleTextStringConfig.Value;
				firepitBurnTime = firepitBurnTimeConfig.Value;
				groundtorchwoodBurnTime = groundtorchwoodBurnTimeConfig.Value;
				bonfireBurnTime = bonfireBurnTimeConfig.Value;
				hearthBurnTime = hearthBurnTimeConfig.Value;
				walltorchBurnTime = walltorchBurnTimeConfig.Value;
				groundtorchironBurnTime = groundtorchironBurnTimeConfig.Value;
				groundtorchgreenBurnTime = groundtorchgreenBurnTimeConfig.Value;
				braziercBurnTime = braziercBurnTimeConfig.Value;
			});

			Task.WaitAll();
			//   Debug("Configuration Received");
		}

	}
}
