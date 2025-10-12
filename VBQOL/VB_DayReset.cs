namespace VBQOL
{
    [HarmonyPatch]
    public class VB_DayReset
    {
        private static int customDay = -1;
        
        [HarmonyPatch(typeof(Terminal), "TryRunCommand")]
        [HarmonyPrefix]
        private static bool TerminalCommandPatch(Terminal __instance)
        {
            string text = __instance.m_input.text;
            if (text.StartsWith("vb_set_day"))
            {
                // Проверяем, включены ли читы
                if (!Helper.AreCheatsEnabled(__instance))
                {
                    __instance.AddString("Эта командля только для админов.");
                    return false;
                }

                string[] parts = text.Split(' ');
                if (parts.Length == 2 && int.TryParse(parts[1], out int newDay))
                {
                    newDay = Math.Max(0, newDay);
                    customDay = newDay;
                
                    double dayLength = EnvMan.instance.m_dayLengthSec;
                    double newTime = newDay * dayLength + 3600;
                
                    Traverse.Create(ZNet.instance).Field("m_netTime").SetValue(newTime);
                
                    __instance.AddString($"День установлен на: {newDay}");
                    return false;
                }
                __instance.AddString("Использование: vb_set_day <число>");
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(EnvMan), "Update")]
        [HarmonyPostfix]
        private static void EnvManUpdatePatch(EnvMan __instance)
        {
            if (customDay >= 0) Traverse.Create(__instance).Field("m_day").SetValue(customDay);
        }
    }
}