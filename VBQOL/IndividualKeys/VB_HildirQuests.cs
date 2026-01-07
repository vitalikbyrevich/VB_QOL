namespace VBQOL.IndividualKeys;

public class VB_HildirQuests
{
    [HarmonyPatch]
	private static class StorePlayerKey
	{
		private static IEnumerable<MethodInfo> TargetMethods() => new[]
		{
			AccessTools.DeclaredMethod(typeof(Trader), nameof(Trader.UseItem)),
			AccessTools.DeclaredMethod(typeof(Trader), nameof(Trader.GetAvailableItems)),
		};

		private static readonly MethodInfo getKey = AccessTools.DeclaredMethod(typeof(ZoneSystem), nameof(ZoneSystem.GetGlobalKey), new[] { typeof(string) });
		private static readonly MethodInfo setKey = AccessTools.DeclaredMethod(typeof(ZoneSystem), nameof(ZoneSystem.SetGlobalKey), new[] { typeof(string) });

		[UsedImplicitly]
		private static void setPlayerKey(ZoneSystem _, string key) => Player.m_localPlayer.m_customData[key] = "";

		private static bool getPlayerKey(ZoneSystem zoneSystem, string key, Trader trader)
		{
			if (Utils.GetPrefabName(trader.gameObject) == "Hildir")
			{
				return Player.m_localPlayer.m_customData.ContainsKey(key);
			}
			return zoneSystem.GetGlobalKey(key);
		}

		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.Calls(getKey))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(StorePlayerKey), nameof(getPlayerKey)));
				}
				else if (instruction.Calls(setKey))
				{
					yield return new CodeInstruction(OpCodes.Call, AccessTools.DeclaredMethod(typeof(StorePlayerKey), nameof(setPlayerKey)));
				}
				else
				{
					yield return instruction;
				}
			}
		}
	}

	[HarmonyPatch(typeof(ConditionalObject), nameof(ConditionalObject.ShouldBeVisible))]
	private static class HideHildirChest
	{
		private static void Postfix(ConditionalObject __instance, ref bool __result)
		{
			if (__instance.name.CustomStartsWith("hildir_chest"))
			{
				__result = Player.m_localPlayer && Player.m_localPlayer.m_customData.ContainsKey(__instance.m_globalKeyCondition);
			}
		}
	}
}