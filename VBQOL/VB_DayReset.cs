using Debug = UnityEngine.Debug;

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
            if (text.StartsWith("setday"))
            {
                string[] parts = text.Split(' ');
                if (parts.Length == 2 && int.TryParse(parts[1], out int newDay))
                {
                    newDay = Math.Max(0, newDay);
                    customDay = newDay;
                
                    double dayLength = EnvMan.instance.m_dayLengthSec;
                    double newTime = newDay * dayLength + 3600; // +1 час чтобы было утро
                
                    // Альтернативный метод изменения времени
                    Traverse.Create(ZNet.instance).Field("m_netTime").SetValue(newTime);
                
                    __instance.AddString($"День установлен на: {newDay} (время: {newTime})");
                    return false;
                }
                __instance.AddString("Использование: setday <число>");
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(EnvMan), "Update")]
        [HarmonyPostfix]
        private static void EnvManUpdatePatch(EnvMan __instance)
        {
            if (customDay >= 0)
            {
                // Принудительно обновляем день
                Traverse.Create(__instance).Field("m_day").SetValue(customDay);
            }
        }
    }
}