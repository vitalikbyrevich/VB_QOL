namespace VBQOL
{
    [HarmonyPatch]
    public class VB_DayCycle
    {
        public static long vanillaDayLengthSec;

        [HarmonyPatch(typeof(EnvMan), "Awake")]
        static class EnvMan_Awake_Patch
        {
            public static void Postfix(ref long ___m_dayLengthSec)
            {
                if (!VBQOL.seasons)
                {
                    vanillaDayLengthSec = ___m_dayLengthSec;
                    ___m_dayLengthSec = 5400;
                }
            }
        }
    }
}