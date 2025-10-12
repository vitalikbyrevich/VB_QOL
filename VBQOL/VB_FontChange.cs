namespace VBQOL
{
    [HarmonyPatch, HarmonyWrapSafe]
    public class VB_FontChange
    {
        private static TMP_FontAsset? GoodFont;
        public static ConfigEntry<string> fontname;
        private static bool isInitialized;
        private static List<TMP_FontAsset> cyrillicFonts = new List<TMP_FontAsset>();

        // Словарь для быстрого доступа к шрифтам по коротким алиасам
        private static Dictionary<string, string> fontAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"v_norse", "Valheim-Norse"},
            {"v_norsebold", "Valheim-Norsebold"},
            {"v_averiasans", "Valheim-AveriaSansLibre"},
            {"v_averiaserif", "Valheim-AveriaSerifLibre"},
            {"f_notosans", "Fallback-NotoSansNormal"},
            {"f_notosansthin", "Fallback-NotoSansThin"},
            {"f_notoserif", "Fallback-NotoSerifNormal"},
            {"liberation", "LiberationSans SDF"},
            {"liberationfallback", "LiberationSans SDF - Fallback"}
        };

        // Белый список шрифтов, поддерживающих кириллицу
        private static string[] knownCyrillicFonts = new string[]
        {
            "Valheim-Norse", "Valheim-Norsebold", "Valheim-AveriaSansLibre", "Valheim-AveriaSerifLibre", "Fallback-NotoSansNormal", "Fallback-NotoSansThin",
            "Fallback-NotoSerifNormal", "LiberationSans SDF", "LiberationSans SDF - Fallback", "NotoSansJP-Regular SDF", "NotoSansJP-Thin SDF", "NotoSansKR-Regular SDF",
            "NotoSansKR-Thin SDF", "NotoSansSC-Regular SDF", "NotoSansSC-Thin SDF", "NotoSerifJP-Regular SDF", "NotoSerifKR-Regular SDF", "NotoSerifSC-Regular SDF"
        };

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake), [])]
        private static void InitializeFont()
        {
            if (isInitialized) return;
            
            var allFonts = GetAllFontsInGame();
            cyrillicFonts = FilterCyrillicFonts(allFonts);
            
            GoodFont = cyrillicFonts.FirstOrDefault(x => x.name == fontname.Value);
            
            if (!GoodFont && cyrillicFonts.Count > 0)
            {
                GoodFont = cyrillicFonts[0];
                fontname.Value = GoodFont.name;
                UnityEngine.Debug.Log($"Шрифт из конфига не найден, установлен первый поддерживающий кириллицу: {GoodFont.name}");
            }
            
            if (!GoodFont)
            {
                UnityEngine.Debug.LogError($"Не найдено ни одного шрифта, поддерживающего кириллицу!");
                return;
            }
            
            UnityEngine.Debug.Log($"Шрифт {fontname.Value} успешно загружен");
            isInitialized = true;
            
            ApplyFontToAllExistingObjects();
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal), [])]
        private static void AddConsoleCommand()
        {
            new Terminal.ConsoleCommand("vb_set_font", "изменить шрифт: vb_set_font [fontname] или font list", 
            (args) =>
            {
                if (args.Length < 2)
                {
                    args.Context.AddString("Используй: vb_set_font [fontname] или font list");
                    args.Context.AddString($"Текущий шрифт: {fontname.Value}");
                    args.Context.AddString("Тип 'font list' чтобы увидеть доступные шрифты");
                    args.Context.AddString("Тип 'font aliases' чтобы увидеть короткие команды");
                    return;
                }

                if (args[1] == "list")
                {
                    args.Context.AddString($"Доступные кириллические шрифты ({cyrillicFonts.Count}):");
                    foreach (var font in cyrillicFonts)
                    {
                        bool isCurrent = font.name == fontname.Value;
                        args.Context.AddString($" - {font.name} {(isCurrent ? "[CURRENT]" : "")}");
                    }
                    args.Context.AddString("Используй 'font aliases' чтобы просмотреть короткие имена для быстрого ввода");
                    return;
                }

                if (args[1] == "aliases")
                {
                    args.Context.AddString("Короткие псевдонимы шрифтов для быстрого набора текста:");
                    foreach (var alias in fontAliases) args.Context.AddString($" - {alias.Key} => {alias.Value}");
                    args.Context.AddString("Например: 'vb_set_font_v_norse' чтобы установить Valheim-Norse");
                    return;
                }

                if (args[1] == "current")
                {
                    args.Context.AddString($"Доступные шрифты: {fontname.Value}");
                    return;
                }

                if (args[1] == "help")
                {
                    args.Context.AddString("Доступные команды:");
                    args.Context.AddString(" - vb_set_font list: Показывает доступные кириллически шрифты");
                    args.Context.AddString(" - vb_set_font aliases: показывает короткие имена-команды");
                    args.Context.AddString(" - vb_set_font current: показывает текущий шрифт");
                    args.Context.AddString(" - vb_set_font [name]: изменить шрифт (поддерживаемые шрифты)");
                    args.Context.AddString(" - vb_set_font help: показывает список команд");
                    return;
                }

                string newFontName = string.Join(" ", args.Args.Skip(1).Take(args.Length - 1));
                newFontName = newFontName.Trim('"', '\'');
                
                if (ChangeFont(newFontName, args.Context)) args.Context.AddString($"Шрифт изменен на: {fontname.Value}");
            });

            // Быстрые команды для популярных шрифтов
            AddQuickFontCommands();
        }

        // Добавляем быстрые команды для каждого шрифта
        private static void AddQuickFontCommands()
        {
            foreach (var alias in fontAliases)
            {
                string commandName = $"vb_set_font_{alias.Key}";
                string fontName = alias.Value;
                
                new Terminal.ConsoleCommand(commandName, $"изменит на шрифт {fontName}", 
                (args) =>
                {
                    if (ChangeFont(fontName, args.Context)) args.Context.AddString($"Шрифт изменен на: {fontName}");
                });
            }
        }

        private static bool ChangeFont(string newFontName, Terminal context = null)
        {
            // Сначала проверяем алиасы
            if (fontAliases.ContainsKey(newFontName)) newFontName = fontAliases[newFontName];

            // Обновляем список шрифтов
            var allFonts = GetAllFontsInGame();
            cyrillicFonts = FilterCyrillicFonts(allFonts);
            
            // Ищем шрифт (регистронезависимо)
            var newFont = cyrillicFonts.FirstOrDefault(x => x.name.Equals(newFontName, StringComparison.OrdinalIgnoreCase));
            
            // Если не нашли точное совпадение, ищем частичное
            if (!newFont) newFont = cyrillicFonts.FirstOrDefault(x => x.name.IndexOf(newFontName, StringComparison.OrdinalIgnoreCase) >= 0);
            
            if (!newFont)
            {
                string errorMsg = $"Шрифт '{newFontName}' не найден! Используй 'font list' чтобы увидеть доступный список.";
                UnityEngine.Debug.LogError(errorMsg);
                context?.AddString(errorMsg);
                
                // Показываем похожие варианты
                var similarFonts = cyrillicFonts.Where(f => f.name.IndexOf(newFontName, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                
                if (similarFonts.Count > 0)
                {
                    context?.AddString("Вы имели в виду что-то из этого?");
                    foreach (var font in similarFonts) context?.AddString($" - {font.name}");
                }
                return false;
            }

            GoodFont = newFont;
            fontname.Value = newFont.name;
            
            UnityEngine.Debug.Log($"Шрифт изменен на: {newFont.name}");
            
            ApplyFontToAllExistingObjects();
            RefreshAllUIElements();
            return true;
        }

        // Остальные методы остаются без изменений...
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Menu), nameof(Menu.Start), [])]
        private static void Menu_Patch(Menu __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject, 15, 15);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hud), nameof(Hud.Awake), [])]
        private static void Hud_Patch(Hud __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject, 15, 15);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake), [])]
        private static void Inventory_Patch(InventoryGui __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject, 15, 15);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.Awake), [])]
        private static void Minimap_Patch(Minimap __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject, 15, 15);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake), [])]
        private static void Terminal_Patch(Terminal __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject, 15, 15);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ConnectPanel), nameof(ConnectPanel.Start), [])]
        private static void ConnectPanel_Patch(ConnectPanel __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject, 15, 15);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ConnectPanel), nameof(ConnectPanel.Update), [])]
        private static void ConnectPanel_Update_Patch(ConnectPanel __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject, 15, 15);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tutorial), nameof(Tutorial.Awake), [])]
        private static void Tutorial_Patch(Tutorial __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.gameObject, 15, 15);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TextViewer), nameof(TextViewer.Awake), [])]
        private static void TextViewer_Patch(TextViewer __instance)
        {
            if (!GoodFont) return;
            ApplyFontToAllTextsInObject(__instance.m_text.gameObject, 15, 15);
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
            UnityEngine.Debug.Log($"Применен шрифт к {fixedCount} текстовым элементам");
        }

        private static void RefreshAllUIElements()
        {
            var menu = Object.FindObjectOfType<Menu>();
            if (menu) ApplyFontToAllTextsInObject(menu.gameObject, 15, 15);
            
            var hud = Object.FindObjectOfType<Hud>();
            if (hud) ApplyFontToAllTextsInObject(hud.gameObject, 15, 15);
            
            var inventory = Object.FindObjectOfType<InventoryGui>();
            if (inventory) ApplyFontToAllTextsInObject(inventory.gameObject, 15, 15);
            
            var minimap = Object.FindObjectOfType<Minimap>();
            if (minimap) ApplyFontToAllTextsInObject(minimap.gameObject, 15, 15);
            
            var terminal = Object.FindObjectOfType<Terminal>();
            if (terminal) ApplyFontToAllTextsInObject(terminal.gameObject, 15, 15);
            
            var connectPanel = Object.FindObjectOfType<ConnectPanel>();
            if (connectPanel) ApplyFontToAllTextsInObject(connectPanel.gameObject, 15, 15);
            
            var tutorial = Object.FindObjectOfType<Tutorial>();
            if (tutorial) ApplyFontToAllTextsInObject(tutorial.gameObject, 15, 15);
            
            var textViewer = Object.FindObjectOfType<TextViewer>();
            if (textViewer) ApplyFontToAllTextsInObject(textViewer.m_text.gameObject, 15, 15);
        }

        private static void ApplyFontToAllTextsInObject(GameObject obj, int f_size, int f_sizeMin)
        {
            if (!GoodFont || !obj) return;
            
            var textsUI = obj.GetComponentsInChildren<TextMeshProUGUI>(true);
            var texts3D = obj.GetComponentsInChildren<TextMeshPro>(true);
            
            foreach (var text in textsUI) if (text && text.font != GoodFont)
            {
                text.font = GoodFont;
                text.fontSize = f_size;
                text.fontSizeMin = f_sizeMin;
            }
            foreach (var text in texts3D) if (text && text.font != GoodFont)
            {
                text.font = GoodFont;
                text.fontSize = f_size;
                text.fontSizeMin = f_sizeMin;
            }
        }
    }
}