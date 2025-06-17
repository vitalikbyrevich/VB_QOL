namespace VBQOL
{
	[HarmonyPatch]
	public class VB_CraftingStationLevelRange
	{
		[HarmonyPatch(typeof(CraftingStation), "UpdateKnownStationsInRange")]
		private static class UpdateKnownStationsInRange_Patch
		{
			private static void Postfix(Player player)
			{
				foreach (CraftingStation item in (List<CraftingStation>)AccessTools.Field(typeof(CraftingStation), "m_allStations").GetValue(null))
				{
					/*if (item.m_name == "$piece_cauldron") continue;
					int level = item.GetLevel();
					if (level > 1)
					{
						int num = stationDefaultRange + stationAmountIncrease * (level - 1);
						ChangeStationRange(item, num);
						continue;
					}*/

					if (item.m_name == "$piece_stonecutter") ChangeChildStationRange(item);
				//	if (item.m_name == "$piece_workbench" || item.m_name == "$piece_forge") ChangeStationRange(item, stationDefaultRange);
				}
			}
		}

		private static int stationAmountIncrease = 4;

		private static int stationDefaultRange = 20;
		private static double stationSearchRange = 50f;
	//	private static bool parentInheritance = true;
	//	private static double inheritanceAmount = 0.5f;

		public static (bool, CraftingStation) IsParentWorkbenchInRange(CraftingStation station, string workbenchType, float searchRange)
		{
			CraftingStation craftingStation = CraftingStation.FindClosestStationInRange(workbenchType, station.transform.position, searchRange);
			if (!craftingStation) return (false, craftingStation);
			Vector2 a = new Vector2(craftingStation.transform.position.x, craftingStation.transform.position.z);
			Vector2 b = new Vector2(station.transform.position.x, station.transform.position.z);
			if (Vector2.Distance(a, b) <= craftingStation.m_rangeBuild) return (true, craftingStation);
			return (false, craftingStation);
		}

		public static void ChangeStationRange(CraftingStation station, float newRange)
		{
			if (!Mathf.Approximately(station.m_rangeBuild, newRange))
			{
				CraftingStation component = station.GetComponent<ZNetView>().GetComponent<CraftingStation>();
				component.m_rangeBuild = newRange;
				component.m_areaMarker.GetComponent<CircleProjector>().m_radius = newRange;
				component.m_areaMarker.GetComponent<CircleProjector>().m_nrOfSegments = (int)Math.Ceiling(Math.Max(5f, 4f * newRange));
				VBQOL.Logger.LogInfo($"{station.m_name} ({station.GetInstanceID()}) новый радиус {newRange} ");
			}
		}

		public static void ChangeChildStationRange(CraftingStation station)
		{
			var (flag, craftingStation) = IsParentWorkbenchInRange(station, "$piece_workbench", (float)stationSearchRange);
			if (!flag) ChangeStationRange(station, stationDefaultRange);
			if (flag && craftingStation.GetLevel() > 1)
			{
				float newRange = stationDefaultRange + stationAmountIncrease * (float)(craftingStation.GetLevel() - 1) /* (float)inheritanceAmount*/;
				ChangeStationRange(station, newRange);
			}
			else if (flag && craftingStation.GetLevel() == 1) ChangeStationRange(station, stationDefaultRange);
		}
	}
}