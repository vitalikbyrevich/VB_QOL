namespace VBQOL
{
    [HarmonyPatch, HarmonyWrapSafe]
    public class VB_FontChange
    {
        private static TMP_FontAsset? GoodFont;
        public static ConfigEntry<string> fontname;
        private static bool isInitialized;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake), [])]
        private static void InitializeFont()
        {
            if (isInitialized) return;
            
            var allFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            GoodFont = allFonts.FirstOrDefault(x => x.name == fontname.Value);
            
            if (!GoodFont)
            {
                UnityEngine.Debug.LogError($"Шрифт {fontname.Value} не найден!");
                foreach (var font in allFonts) UnityEngine.Debug.LogWarning($" - {font.name}");
                return;
            }
            
            UnityEngine.Debug.Log($"Шрифт {fontname.Value} успешно загружен");
            isInitialized = true;
            
            // Однократное применение ко всем существующим объектам при загрузке
            ApplyFontToAllExistingObjects();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Menu), nameof(Menu.Start), [])]
        private static void Menu_Patch(Menu __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hud), nameof(Hud.Awake), [])]
        private static void Hud_Patch(Hud __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake), [])]
        private static void Inventory_Patch(InventoryGui __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake), [])]
        private static void Minimap_Patch(Minimap __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake), [])]
        private static void Terminal_Patch(Terminal __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tutorial), nameof(Tutorial.Awake), [])]
        private static void Tutorial_Patch(Tutorial __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TextViewer), nameof(TextViewer.Awake), [])]
        private static void TextViewer_Patch(TextViewer __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.m_text.gameObject);
        }

        private static void ApplyFontToAllExistingObjects()
        {
            if (!GoodFont) return;
            
            var allTextsUI = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            var allTexts3D = Resources.FindObjectsOfTypeAll<TextMeshPro>();
            int fixedCount = 0;
            
            foreach (var text in allTextsUI)
            {
                if (text && text.font != GoodFont)
                {
                    text.font = GoodFont;
                    fixedCount++;
                }
            }
            
            foreach (var text in allTexts3D)
            {
                if (text && text.font != GoodFont)
                {
                    text.font = GoodFont;
                    fixedCount++;
                }
            }
            UnityEngine.Debug.Log($"Применен шрифт к {fixedCount} текстовым элементам при загрузке");
        }

        private static void ApplyFontToAllTextsInObject(GameObject obj)
        {
            if (!GoodFont || !obj) return;
            
            var textsUI = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
            var texts3D = obj.GetComponentsInChildren<TextMeshPro>(true);
            
            foreach (var text in textsUI) if (text && text.font != GoodFont) text.font = GoodFont;
            foreach (var text in texts3D) if (text && text.font != GoodFont) text.font = GoodFont;
        }
    }
}