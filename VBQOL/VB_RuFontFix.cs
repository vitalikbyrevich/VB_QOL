namespace VBQOL
{
    [HarmonyPatch, HarmonyWrapSafe]
    public class VB_RuFontFix
    {
        private static TMP_FontAsset? GoodFont;
        public static ConfigEntry<string> fontname;
    
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ConnectPanel), nameof(ConnectPanel.Start), [])]
        private static void FindGoodFond()
        {
            var allFonts = Resources.FindObjectsOfTypeAll(typeof(TMP_FontAsset)).Cast<TMP_FontAsset>().ToList();
            GoodFont = allFonts.FirstOrDefault(x => x.name == fontname.Value);
            if (!GoodFont) UnityEngine.Debug.LogWarning("LiberationSans font not found");

            UnityEngine.Debug.LogWarning("--- All Fonts ---");
            foreach (var font in allFonts)
            {
                UnityEngine.Debug.LogWarning($" - {font.name}");
            }
    
            UnityEngine.Debug.LogWarning("");
        }
    

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ConnectPanel), nameof(ConnectPanel.Update), [])]
        private static void FixFonts(ConnectPanel __instance)
        {      
            foreach(var obj in __instance.m_playerListElements)
            {
                var playerName = obj.transform.Find("name")?.gameObject.GetComponent<TextMeshProUGUI>();
                if(!playerName) continue;

                playerName.font = GoodFont;
                playerName.fontSize = Mathf.Max(playerName.fontSize, 15);
                playerName.fontSizeMin = Mathf.Max(playerName.fontSize, 15);
            }
        }
    }
}