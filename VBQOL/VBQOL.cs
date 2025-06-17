namespace VBQOL
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("VitByr.ParadoxBuild", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("shudnal.Seasons", BepInDependency.DependencyFlags.SoftDependency)]
    class VBQOL : BaseUnityPlugin
    {
        private const string ModName = "VBQOL";
        private const string ModVersion = "0.3.1";
        private const string ModGUID = "VitByr.VBQOL";
        private Harmony _harmony = new(ModGUID);
        internal static VBQOL self;

        internal static ConfigEntry<string> EiW_Custom;
        internal static List<string> EiW_CustomStrings = [];
        
        internal static ConfigEntry<bool> soft_particles;
        internal static ConfigEntry<bool> soft_vegetation;
        internal static ConfigEntry<int> particle_raycast_budget;
        internal static ConfigEntry<float> streaming_mipmaps_memory_budget;
        internal static ConfigEntry<float> grass_patch_size;
        internal static ConfigEntry<float> lod_bias;
        internal static ConfigEntry<int> anti_aliasing;
        internal static ConfigEntry<AnisotropicFiltering> anisotropic_filtering;

        #region Add fuel
        public static ConfigEntry<KeyCode> AFModifierKeyConfig;
        public static KeyCode AFModifierKeyUseConfig=KeyCode.E;
        public static ConfigEntry<string> AFTextConfig;
        public static ConfigEntry<bool> AFEnable;
        
        public static ItemDrop.ItemData FindCookableItem(Smelter __instance, Inventory inventory, bool isAddOne)
        {
            IEnumerable<string> names;
            names = __instance.m_conversion.Select(n => n.m_from.m_itemData.m_shared.m_name);

            foreach (string name in names)
            {
                ItemDrop.ItemData item = inventory?.GetItem(name);
                if (item != null) return item;
            }
            return null;
        }
        #endregion
        
        #region Recycle

        internal GameObject recycleObject;
        internal Button recycleButton;
        internal float width;
        Vector3 craftingPos;

        internal ConfigEntry<TabPositions> tabPosition;
        public enum TabPositions
        {
            Left,
            Middle,
            Right,
        }
        internal ConfigEntry<float> resourceMultiplier;
        internal ConfigEntry<bool> preserveOriginalItem;
        internal ConfigEntry<string> recyclebuttontext;

        internal bool InTabDeconstruct() => !recycleButton.interactable;

        #endregion
        
        internal static bool paradoxbuild;
        internal static bool seasons;

        private void Awake()
        {
            self = this;
          //  Harmony.CreateAndPatchAll(typeof(VB_ClearLogPatch)); 
            paradoxbuild = CheckIfModIsLoaded("VitByr.ParadoxBuild");
            seasons = CheckIfModIsLoaded("shudnal.Seasons");

            _serverConfigLocked = config("01 - Awake", "Lock Configuration", true, "Включает синхронизацию конфигурации с сервером.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

         //   Timeout = config("01 - General", "Timeout", 90f, "Время ожидания в секундах");
            
            AFEnable = config("02 - AddAllFuel", "AF_Enable", true, "Вкл/Выкл секцию");
            AFModifierKeyConfig = config("02 - AddAllFuel", "AF_ModifierKey", KeyCode.LeftShift, "Клавиша для добавления сразу стака в печь/ плавильню.", false);
            AFTextConfig = config("02 - AddAllFuel", "AF_Extinguish_Text", "Добавить стак", "Текст отображаемый при наведении печь/костер", false);

            #region BuildDamage

            VB_BuildDamage.enableModBDConfig = config("03 - BuildDamage", "BD_Enable_Section", true, "Включите или отключите этот раздел"); 
            VB_BuildDamage.creatorDamageMultConfig = config("03 - BuildDamage", "BD_CreatorDamageMult", 0.75f, "Множитель урона от создателя постройки");
            VB_BuildDamage.nonCreatorDamageMultConfig = config("03 - BuildDamage", "BD_NonCreatorDamageMult", 0.05f, "Множитель урона от не создателя постройки");
            VB_BuildDamage.uncreatedDamageMultConfig = config("03 - BuildDamage", "BD_UncreatedDamageMult", 0.75f, "Множитель урона постройкам не созданным игроком");
            VB_BuildDamage.naturalDamageMultConfig = config("03 - BuildDamage", "BD_NaturalDamageMult", 0.75f, "Множитель урона от погоды и монстров.");

            #endregion

            #region BetterPickupNotifications

            VB_BetterPickupNotifications.MessageLifetime = config("04 - BetterPickupNotifications", "BPN_MessageLifetime", 4f, "Как долго уведомление отображается в HUD, прежде чем исчезнуть", false);
            VB_BetterPickupNotifications.MessageFadeTime = config("04 - BetterPickupNotifications", "BPN_MessageFadeTime", 2f, "Как долго исчезают уведомления", false);
            VB_BetterPickupNotifications.MessageBumpTime = config("04 - BetterPickupNotifications", "BPN_MessageBumpTime", 2f, "Сколько времени добавлять к сроку действия уведомления при получении дублирующегося элемента", false);
            VB_BetterPickupNotifications.ResetMessageTimerOnDupePickup = config("04 - BetterPickupNotifications", "BPN_ResetMessageTimerOnDupePickup", false, "Сбрасывает таймер уведомления на максимальное время жизни при получении дублирующегося предмета", false);
            VB_BetterPickupNotifications.MessageVerticalSpacingModifier = config("04 - BetterPickupNotifications", "BPN_MessageVerticalSpacingModifier", 1.25f, "Вертикальное разделение между сообщениями", false);
            VB_BetterPickupNotifications.MessageTextHorizontalSpacingModifier = config("04 - BetterPickupNotifications", "BPN_MessageTextHorizontalSpacingModifier", 2f, "Горизонтальный интервал между иконкой и текста уведомлений", false);
            VB_BetterPickupNotifications.MessageTextVerticalModifier = config("04 - BetterPickupNotifications", "BPN_MessageTextVerticalModifier", 1f, "Вертикальное выравнивание текста уведомлений", false);

            #endregion
            
            EiW_Custom = config("05 - EquipInWater", "EiW_Custom", "KnifeFlint,KnifeCopper,KnifeChitin,KnifeSilver,KnifeBlackMetal,KnifeButcher,KnifeSkollAndHati,SpearFlint,SpearBronze,SpearElderbark,SpearWolfFang,SpearChitin,SpearCarapace,PickaxeAntler,PickaxeBronze,PickaxeIron,PickaxeBlackMetal,Hammer,Hoe,FistFenrirClaw",
                "Разрешить использовать оружие/инструмент в воде");
            EiW_Custom.SettingChanged += (_, _) => EiW_CustomStrings = [.. EiW_Custom.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)];
            EiW_CustomStrings = [.. EiW_Custom.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)];
            
            #region FirePlaceUtilites

            VB_FirePlaceUtilites.extinguishItemsConfig = config("06 - EnableFire", "EF_Enable", true, "Вкл/Выкл секцию");
            VB_FirePlaceUtilites.extinguishStringConfig = config("06 - EnableFire", "EF_Extinguish_Fire_Text", "Тушить огонь", "Текст отображаемый при наведении курсора на огонь", false);
            VB_FirePlaceUtilites.igniteStringConfig = config("06 - EnableFire", "EF_Ignite_Fire_Text", "Разжечь огонь", "Текст, отображаемый при наведении курсора на огонь, если тот потушен", false);
            VB_FirePlaceUtilites.keyPOCodeStringConfig = config("06 - EnableFire", "EF_Put_Out_Fire_Key", KeyCode.LeftAlt, "Клавиша чтобы потушить огонь.", false);
            VB_FirePlaceUtilites.configPOKey = (KeyCode)Enum.Parse(typeof(KeyCode), VB_FirePlaceUtilites.keyPOCodeStringConfig.Value.ToString());

            #endregion

            #region Recycle

            tabPosition = config("07 - Recycle", "R_TabPosition", TabPositions.Left, "Положение вкладки Разобрать в меню крафта. (Требуется перезапуск)", false);
            resourceMultiplier = config("07 - Recycle", "R_ResourceMultiplier", 0.35f, "Количество ресурсов, возвращаемых в результате разбора (от 0 до 1, где 1 возвращает 100% ресурсов, а 0 - 0%)");
            preserveOriginalItem = config("07 - Recycle", "R_PreserveOriginalItem", true, "Сохранять ли данные оригинального предмета при понижении уровня. Полезно для модов, добавляющих дополнительные свойства к предметам, например EpicLoot.\nОтключите, если возникли проблемы.");
            recyclebuttontext = config("07 - Recycle", "R_RecycleButtonText", "Разобрать", "Текст кнопки в меню крафта", false);

            #endregion

            #region fps
            soft_particles = Config.Bind("08 - QualitySettings", "soft_particles", true, "Следует ли использовать мягкое перемешивание для частиц?");
            particle_raycast_budget = Config.Bind("08 - QualitySettings", "particle_raycast_budget", 2048, "сколько отражений лучей может быть выполнено на один кадр для приблизительного тестирования на столкновение.");
            soft_vegetation = Config.Bind("08 - QualitySettings", "soft_vegetation", true, "Используйте двухпроходной шейдер для растительности в движке terrain engine.");
            streaming_mipmaps_memory_budget = Config.Bind("08 - QualitySettings", "streaming_mipmaps_memory_budget", 4096f, "Общий объем памяти, который будет использоваться потоковыми и непотоковыми текстурами.");
            grass_patch_size = Config.Bind("08 - QualitySettings", "grass_patch_size", 10f, "Плотность травы");
            lod_bias = Config.Bind("08 - QualitySettings", "lod_bias", 5f, "Глобальный множитель для расстояния переключения LOD.");
            anisotropic_filtering = Config.Bind("08 - QualitySettings", "anisotropic_filtering",  AnisotropicFiltering.ForceEnable, "Режим глобальной анизотропной фильтрации.");
            anti_aliasing = Config.Bind("08 - QualitySettings", "anti_aliasing", 4, "Установите опцию Фильтрации AA.");
            
            SetGraphicsSettings();
        /*    foreach (KeyValuePair<ConfigDefinition, ConfigEntryBase> item in Config)
            {
                if (item.Key.Section == "QualitySettings")
                {
                    if (item.Value is ConfigEntry<bool>)
                    {
                        ((ConfigEntry<bool>)item.Value).SettingChanged += BepInExPlugin_SettingChanged;
                    }
                    else if (item.Value is ConfigEntry<int>)
                    {
                        ((ConfigEntry<int>)item.Value).SettingChanged += BepInExPlugin_SettingChanged;
                    }
                    else if (item.Value is ConfigEntry<float>)
                    {
                        ((ConfigEntry<float>)item.Value).SettingChanged += BepInExPlugin_SettingChanged;
                    }
                    else if (item.Value is ConfigEntry<AnisotropicFiltering>)
                    {
                        ((ConfigEntry<AnisotropicFiltering>)item.Value).SettingChanged += BepInExPlugin_SettingChanged;
                    }
                    else if (item.Value is ConfigEntry<ShadowmaskMode>)
                    {
                        ((ConfigEntry<ShadowmaskMode>)item.Value).SettingChanged += BepInExPlugin_SettingChanged;
                    }
                    else if (item.Value is ConfigEntry<ShadowProjection>)
                    {
                        ((ConfigEntry<ShadowProjection>)item.Value).SettingChanged += BepInExPlugin_SettingChanged;
                    }
                    else if (item.Value is ConfigEntry<ShadowResolution>)
                    {
                        ((ConfigEntry<ShadowResolution>)item.Value).SettingChanged += BepInExPlugin_SettingChanged;
                    }
                    else if (item.Value is ConfigEntry<ShadowQuality>)
                    {
                        ((ConfigEntry<ShadowQuality>)item.Value).SettingChanged += BepInExPlugin_SettingChanged;
                    }
                    else if (item.Value is ConfigEntry<SkinWeights>)
                    {
                        ((ConfigEntry<SkinWeights>)item.Value).SettingChanged += BepInExPlugin_SettingChanged;
                    }
                }
            }*/
            #endregion
            
            VB_CustomSlotItem.ItemSlotPairs = config("08 - CustomSlot", "ItemSlotPairs",
                "Demister,wisplight;Wishbone,wishbone;par_item_ring_25,par_item_ring;par_item_ring_50,par_item_ring;par_item_ring_75,par_item_ring;par_item_ring_100,par_item_ring",
                "\"ItemName1,SlotName;...;ItemNameN,SlotName\"\nНесколько предметов могут быть помещены в один и тот же слот (не все сразу), но один и тот же предмет не может быть помещен в несколько слотов.\nЧтобы изменения вступили в силу, игру необходимо перезапустить." );
           

            VB_BossDespawn.radiusConfig = config("09 - BossDespawn", "Despawn radius", 100f, "Радиус обнаружения игроков"); 
            VB_BossDespawn.despawnDelayConfig = config("09 - BossDespawn", "Despawn delay", 1f, "Через сколько минут босс деспавнится");
            
           _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), "VitByr.VBQOL");
            SetupWatcher();
        }

        /*    [HarmonyPatch, HarmonyWrapSafe]
        static class VitInventory
        {
            private static bool inChange = false;

            [HarmonyPatch(typeof(Player), nameof(Player.OnInventoryChanged))]
            [HarmonyPostfix]
            private static void OnInventoryChanged(Player __instance)
            {
                if (inChange) return;
                var player = Player.m_localPlayer;
                if (player == null || __instance != player) return;
                if (player.m_isLoading) return;

                if (player.m_legItem is not null)
                {
                    player.m_inventory.m_height = 2;
                    var tombstone = player.m_tombstone?.GetComponent<Container>();
                    if (tombstone) tombstone.m_height = 2;
                    inChange = true;
                    player.m_inventory.Changed();
                    inChange = false;
                } else
                {
                    player.m_inventory.m_height = 1;
                    var tombstone = player.m_tombstone?.GetComponent<Container>();
                    if (tombstone) tombstone.m_height = 1;
                    inChange = true;
                    player.m_inventory.Changed();
                    inChange = false;
                }
            }
        }*/
/*    [HarmonyPatch, HarmonyWrapSafe]
    static class VitInventory
    {
        private static bool inChange = false;
        
        [HarmonyPatch(typeof(Player), "Awake")]
        [HarmonyPrefix]
        private static void PlayerAwakePatch(Player __instance)
        { 
            __instance.m_inventory.m_height = 1; 
            UpdateInventorySize();
        }

        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Update))]
        [HarmonyPostfix]
        private static void InventoryGuiUpdatePatch()
        { 
            Player localPlayer = Player.m_localPlayer;
            if (!localPlayer) return; 
            localPlayer.m_inventory.m_height = 1;
            UpdateInventorySize();
        }
        [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateInventory))]
        [HarmonyPostfix]
        private static void InventoryGuiUpdateInventoryPatch()
        { 
            Player localPlayer = Player.m_localPlayer;
            if (!localPlayer) return; 
            localPlayer.m_inventory.m_height = 1;
            UpdateInventorySize();
        }

        public static void UpdateInventorySize()
        {
            Player localPlayer = Player.m_localPlayer;
            if (!localPlayer) return;
            if (!InventoryGui.instance) return;

            if (localPlayer.m_legItem.m_equipped == true)
            {
                localPlayer.m_inventory.m_height = 2;
                localPlayer.m_tombstone.GetComponent<Container>().m_height = 2;
                inChange = true;
                localPlayer.m_inventory.Changed();
                inChange = false;
            }
        }
    }*/
        
        #region equipinwater
  
        public static bool HS_CheckWaterItem(ItemDrop.ItemData item)
        {
            if (item == null)
            {
                var player = Player.m_localPlayer;
                if (player.m_leftItem != null && !EiW_CustomStrings.Contains(player.m_leftItem.m_dropPrefab.name)) player.UnequipItem(player.m_leftItem);
                if (player.m_rightItem != null && !EiW_CustomStrings.Contains(player.m_rightItem.m_dropPrefab.name)) player.UnequipItem(player.m_rightItem);
                return false;
            }
            return EiW_CustomStrings.Contains(item.m_shared.m_name);
        }

        [HarmonyPatch]
        private static class EquipInWaterPatches
        {
            // Target Patch Methods
            private static IEnumerable<MethodBase> TargetMethods() =>
            [
                AccessTools.Method(typeof(Player), nameof(Player.Update)),
                AccessTools.Method(typeof(Humanoid), nameof(Humanoid.EquipItem)),
                AccessTools.Method(typeof(Humanoid), nameof(Humanoid.UpdateEquipment))
            ];

            // IL Filter
            static readonly CodeMatch[] Matches =
            [
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Character), nameof(Character.IsSwimming))),
                new CodeMatch(OpCodes.Brfalse),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Character), nameof(Character.IsOnGround)))
            ];

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
            {
                // Search Instructions for Call to IsSwimming() and IsOnGround() using the filter
                var codeMatcher = new CodeMatcher(instructions).MatchStartForward(Matches);
                try
                {
                    switch ((original.DeclaringType?.FullName, original.Name))
                    {
                        case (nameof(Player), nameof(Player.Update)):
                            // Remove Calls to IsSwimming() and IsOnGround() from Player->Update to allow the ShowHandItems() Call to run.
                            codeMatcher.Advance(1).RemoveInstructions(6);
                            break;
                        case (nameof(Humanoid), nameof(Humanoid.EquipItem)):
                            // Inject to Call HS_CheckWaterItem() from Humanoid->EquipItem with ItemDrop.ItemData as argument
                            codeMatcher.Advance(6).Insert(new List<CodeInstruction>
                            {
                                new(OpCodes.Ldarg_1), // Stack Var with ItemDrop.ItemData
                                new(OpCodes.Call, typeof(VBQOL).GetMethod(nameof(HS_CheckWaterItem))),
                                new(OpCodes.Brfalse, codeMatcher.InstructionAt(-1).operand) // Borrow Index from previous instruction
                            });
                            break;
                        case (nameof(Humanoid), nameof(Humanoid.UpdateEquipment)):
                            // Inject to Call HS_CheckWaterItem() from Humanoid->UpdateEquipment with Null as argument
                            codeMatcher.Advance(6).Insert(new List<CodeInstruction>
                            {
                                new(OpCodes.Ldnull),
                                new(OpCodes.Call, typeof(VBQOL).GetMethod(nameof(HS_CheckWaterItem))),
                                new(OpCodes.Brfalse, codeMatcher.InstructionAt(-1).operand)
                            });
                            break;
                    }
                }
                catch (ArgumentException)
                {
                    Logger.LogError("ERROR: Startup Failure, IL Transpiler Errors Detected. Please Update or Remove the Plugin, or Notify the Author of the Issue. Valheim Shutdown Initiated.");
                    Environment.Exit(1);
                }
                return codeMatcher.InstructionEnumeration();
            }
        }
        #endregion

        #region Recycle

        internal GameObject GetOrCreateRecycleTab()
        {
            if (self.recycleObject) return self.recycleObject;

            Logger.LogInfo("Создана кнопка 'Разобрать'");

            recycleObject = Instantiate(InventoryGui.instance.m_tabUpgrade.gameObject, InventoryGui.instance.m_tabUpgrade.gameObject.transform.parent);
            if (recycleObject is null)
            {
                Logger.LogError("Не удалось создать кнопку 'Разобрать'.");
                return null;
            }

            recycleObject.name = "Recycle";
            recycleObject.GetComponentInChildren<TMP_Text>().text = "Разбор";
            width = recycleObject.GetComponent<RectTransform>().rect.width;
            craftingPos = new Vector3(recycleObject.transform.localPosition.x + ((width + 10f) * ((int)tabPosition.Value + 1)), recycleObject.transform.localPosition.y, recycleObject.transform.localPosition.z);
            recycleButton = recycleObject.GetComponent<Button>();
            recycleButton.transform.localPosition = craftingPos;
            recycleButton.interactable = true;
            recycleButton.name = "RecycleButton";
            recycleButton.onClick.RemoveAllListeners();
            recycleButton.onClick.AddListener(SelectRecycleTab);
            recycleObject.SetActive(false);
            return recycleObject;
        }

        internal void SelectRecycleTab()
        {
            Logger.LogDebug("Selected recycle");
            recycleButton.interactable = false;
            InventoryGui.m_instance.m_tabCraft.interactable = true;
            InventoryGui.m_instance.m_tabUpgrade.interactable = true;
            InventoryGui.m_instance.UpdateCraftingPanel();
        }

        internal void RebuildRecycleTab() => GetOrCreateRecycleTab();

        #endregion

    /*    #region ZRpcPatch
        
        public static ConfigEntry<float> Timeout { get; set; }
        
        [HarmonyPatch(typeof(ZRpc), nameof(ZRpc.SetLongTimeout)), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SetLongTimeoutTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatch[] loadFloat =
            {
                new(OpCodes.Ldc_R4),
                new(OpCodes.Stsfld)
            };

            CodeInstruction[] loadTimeout =
            {
                new(OpCodes.Call, AccessTools.PropertyGetter(typeof(VBQOL), nameof(Timeout))),
                new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(ConfigEntry<float>), nameof(ConfigEntry<float>.Value)))
            };

            return new CodeMatcher(instructions)
                // match 30s timeout
                .MatchForward(false, loadFloat)
                .RemoveInstructions(1)
                .InsertAndAdvance(loadTimeout)
                // match 90s timeout and preserve labels
                .MatchForward(false, loadFloat)
                .GetLabels(out List<Label> labels)
                .RemoveInstructions(1)
                .Insert(loadTimeout)
                .AddLabels(labels)
                .InstructionEnumeration();
        }
        #endregion*/
/*
        #region ZSteamSocketPatch
        
        [HarmonyPatch(typeof(ZNet), "Start")]
        private static class FejdStartup_Start_Patch
        {
            [UsedImplicitly]
            private static void Postfix()
            {
                if (IsServer) Application.targetFrameRate = VBQOL.self.Config.Bind("01 - General", "Target_FPS", Application.targetFrameRate, "Целевой FPS").Value;
            }
        }

        [HarmonyPatch]
        private static class PatchMethods
        {
            [HarmonyTargetMethods]
            private static IEnumerable<MethodBase> Target()
            {
                yield return AccessTools.Method(typeof(ZSteamSocket), "Send", new Type[1] { typeof(ZPackage) });
                yield return AccessTools.Method(typeof(ZSteamSocket), "Flush");
                yield return AccessTools.Method(typeof(ZSteamSocket), "Update");
            }

            [HarmonyTranspiler]
            private static IEnumerable<CodeInstruction> FixingIssue(IEnumerable<CodeInstruction> code)
            {
                List<CodeInstruction> list = new List<CodeInstruction>(code);
                MethodInfo operand = AccessTools.Method(typeof(VBQOL), IsServer ? "Replacement_Server" : "Replacement_Client");
                MethodInfo targetMethod = AccessTools.Method(typeof(ZSteamSocket), "SendQueuedPackages");
              //  int num = list.FindIndex((ins) => ins.opcode == OpCodes.Call && ins.operand == targetMethod);
                int num = list.FindIndex((ins) => ins.opcode == OpCodes.Call && (MethodInfo)ins.operand == targetMethod);
                if (num == -1)
                {
                    Debug.LogError("NoMoreCrashes: Failed to find callvirt to ZSteamSocket.SendQueuedPackages");
                    return list;
                }
                list[num] = new CodeInstruction(OpCodes.Call, operand);
                return list;
            }
        }
        
        private static bool IsServer => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;

        private unsafe static void Replacement_Client(ZSteamSocket socket)
        {
            if (!socket.IsConnected() || socket.m_sendQueue.Count <= 0) return;
            byte[] array = socket.m_sendQueue.Dequeue();
            while (true)
            {
                fixed (byte* pData = &array[0])
                {
                    SteamNetworkingSockets.SendMessageToConnection(socket.m_con, (nint)pData, (uint)array.Length, 8, out var _);
                    socket.m_totalSent += array.Length;
                    if (socket.m_sendQueue.Count > 0)
                    {
                        array = socket.m_sendQueue.Dequeue();
                        continue;
                    }
                    break;
                }
            }
        }

        private unsafe static void Replacement_Server(ZSteamSocket socket)
        { 
            if (!socket.IsConnected() || socket.m_sendQueue.Count <= 0) return;
            byte[] array = socket.m_sendQueue.Dequeue();
            while (true)
            {
                fixed (byte* pData = &array[0])
                {
                    SteamGameServerNetworkingSockets.SendMessageToConnection(socket.m_con, (nint)pData, (uint)array.Length, 8, out var _);
                    socket.m_totalSent += array.Length;
                    if (socket.m_sendQueue.Count > 0)
                    {
                        array = socket.m_sendQueue.Dequeue();
                        continue;
                    }
                    break;
                }
            }
        }

        #endregion
   */     
        #region config

        private static ConfigEntry <bool> _serverConfigLocked;
        public new static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource(ModName);
        private static readonly ConfigSync ConfigSync = new(ModGUID) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };
        private static string ConfigFileName = $"{ModGUID}.cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;
      //  internal static string ConnectionError = "";
        
        private void OnDestroy()
        {
            Config.Save();
            Logger.LogInfo("DESTROY");
            Destroy(recycleObject);
            _harmony.UnpatchSelf();
        }
        
        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                Logger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                Logger.LogError($"There was an issue loading your {ConfigFileName}");
                Logger.LogError("Please check your config entries for spelling and format!");
            }
        }
        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription = new(description.Description + (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"), description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);

        private class ConfigurationManagerAttributes
        {
            [UsedImplicitly] public int Order;
            [UsedImplicitly] public bool Browsable;
            [UsedImplicitly] public string Category;
            [UsedImplicitly] public Action<ConfigEntryBase> CustomDrawer;
        }
        #endregion
        
     /*   private void Update()
        {
            SetGraphicsSettings();
        }
        public static bool reloadOnChange = true;
        
        private void BepInExPlugin_SettingChanged(object sender, EventArgs e)
        {
            if (reloadOnChange)
            {
                SetGraphicsSettings();
            }
        }*/

        private static void SetGraphicsSettings()
        {
            QualitySettings.softParticles = soft_particles.Value;
            QualitySettings.particleRaycastBudget = particle_raycast_budget.Value;
            QualitySettings.softVegetation = soft_vegetation.Value;
            QualitySettings.streamingMipmapsMemoryBudget = streaming_mipmaps_memory_budget.Value;
            QualitySettings.lodBias = lod_bias.Value;
            QualitySettings.anisotropicFiltering = anisotropic_filtering.Value;
            QualitySettings.antiAliasing = anti_aliasing.Value;
        }
        
        private bool CheckIfModIsLoaded(string modGUID)
        {
            foreach (KeyValuePair<string, PluginInfo> keyValuePair in Chainloader.PluginInfos) if (keyValuePair.Value.Metadata.GUID.Equals(modGUID)) return true;
            return false;
        }
    }
}
