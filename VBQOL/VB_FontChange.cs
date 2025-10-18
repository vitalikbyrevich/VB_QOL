namespace VBQOL
{
    [HarmonyPatch, HarmonyWrapSafe]
    public class VB_FontChange
    {
        private static TMP_FontAsset MainFont; // Основной шрифт (дефолтный)
        private static TMP_FontAsset SecondaryFont; // Второстепенный шрифт для специфичных интерфейсов
        public static ConfigEntry<string> mainFontName;
        public static ConfigEntry<string> secondaryFontName;
        private static bool isInitialized;

        // Белый список шрифтов, поддерживающих кириллицу
        private static string[] knownCyrillicFonts = new string[]
        {
            "Valheim-Norse", "Valheim-Norsebold", "Valheim-AveriaSansLibre", "Valheim-AveriaSerifLibre", "LiberationSans SDF", "LiberationSans SDF - Fallback"
        };

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake), [])]
        private static void InitializeFont()
        {
            if (isInitialized) return;
            
            var allFonts = GetAllFontsInGame();
            var cyrillicFonts = FilterCyrillicFonts(allFonts);
            
            MainFont = cyrillicFonts.FirstOrDefault(x => x.name == mainFontName.Value);
            SecondaryFont = cyrillicFonts.FirstOrDefault(x => x.name == secondaryFontName.Value);
            
            if (!MainFont && cyrillicFonts.Count > 0)
            {
                MainFont = cyrillicFonts[0];
                mainFontName.Value = MainFont.name;
                Debug.Log($"Основной шрифт из конфига не найден, установлен первый поддерживающий кириллицу: {MainFont.name}");
            }
            
            if (!MainFont)
            {
                Debug.LogError($"Не найдено ни одного шрифта, поддерживающего кириллицу!");
                return;
            }
            
            Debug.Log($"Основной шрифт: {mainFontName.Value}, Второстепенный: {secondaryFontName.Value}");
            isInitialized = true;

            ApplyMainFontToAllExistingObjects();
        }

        private static List<TMP_FontAsset> GetAllFontsInGame()
        {
            var fonts = new List<TMP_FontAsset>();
            fonts.AddRange(Resources.FindObjectsOfTypeAll<TMP_FontAsset>());
            return fonts.Where(f => f).GroupBy(f => f.name).Select(g => g.First()).OrderBy(f => f.name).ToList();
        }

        private static List<TMP_FontAsset> FilterCyrillicFonts(List<TMP_FontAsset> allFonts)
        {
            var cyrillicFonts = new List<TMP_FontAsset>();
            foreach (var font in allFonts) if (knownCyrillicFonts.Contains(font.name)) cyrillicFonts.Add(font);
            return cyrillicFonts.GroupBy(f => f.name).Select(g => g.First()).OrderBy(f => f.name).ToList();
        }

        // Основные UI элементы используют MainFont (дефолтный)
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Menu), nameof(Menu.Start), [])]
        private static void Menu_Patch(Menu __instance)
        {
            if (!MainFont) return;
            ApplyMainFontToAllTextsInObject(__instance.gameObject, 30, 10);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Settings), nameof(Settings.Awake), [])]
        private static void Settings_Patch(Settings __instance)
        {
            if (!MainFont) return;
            ApplyMainFontToAllTextsInObject(__instance.gameObject, 25, 10);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hud), nameof(Hud.Awake), [])]
        private static void Hud_Patch(Hud __instance)
        {
            if (!MainFont) return;
            ApplyMainFontToAllTextsInObject(__instance.gameObject, 20, 10);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake), [])]
        private static void Minimap_Patch(Minimap __instance)
        {
            if (!MainFont) return;
            ApplyMainFontToAllTextsInObject(__instance.gameObject, 20, 10);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ConnectPanel), nameof(ConnectPanel.Start), [])]
        private static void ConnectPanel_Patch(ConnectPanel __instance)
        {
            if (!MainFont) return;
            ApplyMainFontToAllTextsInObject(__instance.gameObject, 17, 10);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ConnectPanel), nameof(ConnectPanel.Update), [])]
        private static void ConnectPanel_Update_Patch(ConnectPanel __instance)
        {
            if (!MainFont) return;
            ApplyMainFontToAllTextsInObject(__instance.gameObject, 17, 10);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tutorial), nameof(Tutorial.Awake), [])]
        private static void Tutorial_Patch(Tutorial __instance)
        {
            if (!MainFont) return;
            ApplyMainFontToAllTextsInObject(__instance.gameObject, 20, 10);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TextViewer), nameof(TextViewer.Awake), [])]
        private static void TextViewer_Patch(TextViewer __instance)
        {
            if (!MainFont) return;
            ApplyMainFontToAllTextsInObject(__instance.m_text.gameObject, 25, 25);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MessageHud), nameof(MessageHud.Awake), [])]
        private static void MessageHud_Patch(MessageHud __instance)
        {
            if (!MainFont) return;
            ApplyMainFontToAllTextsInObject(__instance.gameObject, 55, 35);
            ApplyMainFontToAllTextsInObject(__instance.m_unlockMsgPrefab.gameObject, 25, 15);
            ApplyMainFontToAllTextsInObject(__instance.m_messageText.gameObject, 25, 15);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MessageHud), nameof(MessageHud.Update), [])]
        private static void MessageHud_Update_Patch(MessageHud __instance)
        {
            if (!MainFont) return;
            ApplyMainFontToAllTextsInObject(__instance.gameObject, 55, 35);
            ApplyMainFontToAllTextsInObject(__instance.m_unlockMsgPrefab.gameObject, 25, 15);
            ApplyMainFontToAllTextsInObject(__instance.m_messageText.gameObject, 25, 15);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake), [])]
        private static void Inventory_Patch(InventoryGui __instance)
        {
            if (!SecondaryFont) return;
            ApplySecondaryFontToAllTextsInObject(__instance.gameObject);
            ApplyMainFontToAllTextsInObject(__instance.m_craftButton.gameObject, 20, 10);
            ApplyMainFontToAllTextsInObject(__instance.m_skillsDialog.gameObject, 20, 10);
            ApplyMainFontToAllTextsInObject(__instance.m_tabUpgrade.gameObject, 20, 10);
            ApplyMainFontToAllTextsInObject(__instance.m_tabCraft.gameObject, 20, 10);
            ApplyMainFontToAllTextsInObject(__instance.m_craftingStationName.gameObject, 40, 20);
            ApplyMainFontToAllTextsInObject(__instance.m_qualityPanel.gameObject, 25, 10);
            ApplyMainFontToAllTextsInObject(__instance.m_infoPanel.gameObject, 20, 10);
            ApplyMainFontToAllTextsInObject(__instance.m_textsDialog.gameObject, 25, 10);
            ApplyMainFontToAllTextsInObject(__instance.m_trophiesPanel.gameObject, 25, 10);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake), [])]
        private static void Terminal_Patch(Terminal __instance)
        {
            if (!SecondaryFont) return;
            ApplySecondaryFontToAllTextsInObject(__instance.gameObject);
        }

        public static void ApplyMainFontToAllExistingObjects()
        {
            if (!MainFont) return;
            
            var allTextsUI = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            var allTexts3D = Resources.FindObjectsOfTypeAll<TextMeshPro>();
            int fixedCount = 0;
            
            foreach (var text in allTextsUI)
            {
                if (text && text.font != MainFont)
                {
                    text.font = MainFont;
                    fixedCount++;
                }
            }
            
            foreach (var text in allTexts3D)
            {
                if (text && text.font != MainFont)
                {
                    text.font = MainFont;
                    fixedCount++;
                }
            }
            UnityEngine.Debug.Log($"Применен основной шрифт к {fixedCount} текстовым элементам");
        }

        private static void ApplyMainFontToAllTextsInObject(GameObject obj, int fontSize, int fontSizeMin)
        {
            if (!MainFont || !obj) return;
            
            var textsUI = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
            var texts3D = obj.GetComponentsInChildren<TextMeshPro>(true);
            
            foreach (var text in textsUI) 
            {
                if (text && text.font != MainFont)
                {
                    text.font = MainFont;
                    text.fontSize = fontSize;
                    text.fontSizeMin = fontSizeMin;
                }
            }
            
            foreach (var text in texts3D) 
            {
                if (text && text.font != MainFont)
                {
                    text.font = MainFont;
                    text.fontSize = fontSize;
                    text.fontSizeMin = fontSizeMin;
                }
            }
        }

        private static void ApplySecondaryFontToAllTextsInObject(GameObject obj)
        {
            if (!SecondaryFont || !obj) return;
            
            var textsUI = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
            var texts3D = obj.GetComponentsInChildren<TextMeshPro>(true);
            
            foreach (var text in textsUI) if (text && text.font != SecondaryFont) text.font = SecondaryFont;
            foreach (var text in texts3D) if (text && text.font != SecondaryFont) text.font = SecondaryFont;
        }
        
        public static void ReloadFonts()
        {
            var allFonts = GetAllFontsInGame();
            var cyrillicFonts = FilterCyrillicFonts(allFonts);
    
            MainFont = cyrillicFonts.FirstOrDefault(x => x.name == mainFontName.Value);
            SecondaryFont = cyrillicFonts.FirstOrDefault(x => x.name == secondaryFontName.Value);
    
            if (!MainFont && cyrillicFonts.Count > 0)
            {
                MainFont = cyrillicFonts[0];
                mainFontName.Value = MainFont.name;
                UnityEngine.Debug.Log($"Основной шрифт из конфига не найден, установлен первый поддерживающий кириллицу: {MainFont.name}");
            }
    
            if (!MainFont)
            {
                UnityEngine.Debug.LogError($"Не найдено ни одного шрифта, поддерживающего кириллицу!");
                return;
            }
    
            UnityEngine.Debug.Log($"Основной шрифт: {mainFontName.Value}, Второстепенный: {secondaryFontName.Value}");
        }
        
        public static void RefreshAllUIElements()
        {
            ReloadFonts();
            
            var menu = Object.FindObjectOfType<Menu>();
            if (menu) ApplyMainFontToAllTextsInObject(menu.gameObject, 30, 10);
            
            var Settings = Object.FindObjectOfType<Settings>();
            if (Settings) ApplyMainFontToAllTextsInObject(Settings.gameObject, 25, 10);
            
            var hud = Object.FindObjectOfType<Hud>();
            if (hud) ApplyMainFontToAllTextsInObject(hud.gameObject, 20, 10);
            
            var inventory = Object.FindObjectOfType<InventoryGui>();
            if (inventory) ApplySecondaryFontToAllTextsInObject(inventory.gameObject);
            if (inventory) ApplyMainFontToAllTextsInObject(inventory.m_craftButton.gameObject, 20, 10);
            if (inventory) ApplyMainFontToAllTextsInObject(inventory.m_skillsDialog.gameObject, 20, 10);
            if (inventory) ApplyMainFontToAllTextsInObject(inventory.m_tabUpgrade.gameObject, 20, 10);
            if (inventory) ApplyMainFontToAllTextsInObject(inventory.m_tabCraft.gameObject, 20, 10);
            if (inventory) ApplyMainFontToAllTextsInObject(inventory.m_craftingStationName.gameObject, 40, 20);
            if (inventory) ApplyMainFontToAllTextsInObject(inventory.m_qualityPanel.gameObject, 25, 10);
            if (inventory) ApplyMainFontToAllTextsInObject(inventory.m_infoPanel.gameObject, 20, 10);
            if (inventory) ApplyMainFontToAllTextsInObject(inventory.m_textsDialog.gameObject, 25, 10);
            if (inventory) ApplyMainFontToAllTextsInObject(inventory.m_trophiesPanel.gameObject, 25, 10);
            
            var minimap = Object.FindObjectOfType<Minimap>();
            if (minimap) ApplyMainFontToAllTextsInObject(minimap.gameObject, 20, 10);
            
            var terminal = Object.FindObjectOfType<Terminal>();
            if (terminal) ApplySecondaryFontToAllTextsInObject(terminal.gameObject);
            
            var connectPanel = Object.FindObjectOfType<ConnectPanel>();
            if (connectPanel) ApplyMainFontToAllTextsInObject(connectPanel.gameObject, 17, 10);
            
            var tutorial = Object.FindObjectOfType<Tutorial>();
            if (tutorial) ApplyMainFontToAllTextsInObject(tutorial.gameObject, 20, 10);
            
            var textViewer = Object.FindObjectOfType<TextViewer>();
            if (textViewer) ApplyMainFontToAllTextsInObject(textViewer.m_text.gameObject, 25, 25);
            
            var messageHud = Object.FindObjectOfType<MessageHud>();
            if (messageHud) ApplyMainFontToAllTextsInObject(messageHud.gameObject, 55, 35);
            if (messageHud) ApplyMainFontToAllTextsInObject(messageHud.m_unlockMsgPrefab.gameObject, 25, 15);
            if (messageHud) ApplyMainFontToAllTextsInObject(messageHud.m_messageText.gameObject, 25, 15);
        }
    }
}