namespace VBQOL
{
	[HarmonyPatch]
    public class VB_ClearLogPatch
	{
		[HarmonyPatch(typeof(ConsoleLogListener), nameof(ConsoleLogListener.LogEvent))]
		[HarmonyPrefix]
		private static bool ConsoleLogListenerLog(object sender, LogEventArgs eventArgs)
		{
			string text = eventArgs.Data.ToString();
			return !text.StartsWith("Failed to find expected binary shader data")
                && !text.Contains("Fetching PlatformPrefs 'GuiScale' before loading defaults")
                && !text.Contains("Missing audio clip in music respawn")
				&& !text.Contains("Set button")
				&& !text.Contains("Only custom filters can be played. Please add a custom filter or an audioclip to the audiosource (Amb_MainMenu).")
				&& !text.Contains("The character with Unicode value");
		}
	}
}