namespace VBQOL
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("VitByr.ParadoxBuild", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("shudnal.Seasons", BepInDependency.DependencyFlags.SoftDependency)]
    class VBQOL : BaseUnityPlugin
    {
        private const string ModName = "VBQOL";
        private const string ModVersion = "0.3.4";
        private const string ModGUID = "VitByr.VBQOL";
     //   private Harmony _harmony = new(ModGUID);
        internal static VBQOL self;

        
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
            paradoxbuild = CheckIfModIsLoaded("VitByr.ParadoxBuild");
            seasons = CheckIfModIsLoaded("shudnal.Seasons");
            ConfigurationManagerAttributes isAdminOnly = new ConfigurationManagerAttributes { IsAdminOnly = true };
            
            VB_BossDespawn.radiusConfig = Config.Bind("01 - BossDespawn", "Despawn radius", 150f, "Радиус обнаружения игроков"); 
            VB_BossDespawn.despawnDelayConfig = Config.Bind("01 - BossDespawn", "Despawn delay", 5f, "Через сколько минут босс деспавнится");
            
            AFEnable = Config.Bind("02 - AddAllFuel", "AF_Enable", true, "Вкл/Выкл секцию");
            AFModifierKeyConfig = Config.Bind("02 - AddAllFuel", "AF_ModifierKey", KeyCode.LeftShift, new ConfigDescription("Клавиша для добавления сразу стака в печь/ плавильню."));
            AFTextConfig = Config.Bind("02 - AddAllFuel", "AF_Extinguish_Text", "Добавить стак", new ConfigDescription("Текст отображаемый при наведении печь/костер", null, isAdminOnly));

            #region BuildDamage

            VB_BuildDamage.enableModBDConfig = Config.Bind("03 - BuildDamage", "BD_Enable_Section", true, "Включите или отключите этот раздел"); 
            VB_BuildDamage.creatorDamageMultConfig = Config.Bind("03 - BuildDamage", "BD_CreatorDamageMult", 0.75f, "Множитель урона от создателя постройки");
            VB_BuildDamage.nonCreatorDamageMultConfig = Config.Bind("03 - BuildDamage", "BD_NonCreatorDamageMult", 0.05f, "Множитель урона от не создателя постройки");
            VB_BuildDamage.uncreatedDamageMultConfig = Config.Bind("03 - BuildDamage", "BD_UncreatedDamageMult", 0.75f, "Множитель урона постройкам не созданным игроком");
            VB_BuildDamage.naturalDamageMultConfig = Config.Bind("03 - BuildDamage", "BD_NaturalDamageMult", 0.75f, "Множитель урона от погоды и монстров.");

            #endregion

            #region BetterPickupNotifications

            VB_BetterPickupNotifications.MessageLifetime = Config.Bind("04 - BetterPickupNotifications", "BPN_MessageLifetime", 4f, "Как долго уведомление отображается в HUD, прежде чем исчезнуть");
            VB_BetterPickupNotifications.MessageFadeTime = Config.Bind("04 - BetterPickupNotifications", "BPN_MessageFadeTime", 2f, "Как долго исчезают уведомления");
            VB_BetterPickupNotifications.MessageBumpTime = Config.Bind("04 - BetterPickupNotifications", "BPN_MessageBumpTime", 2f, "Сколько времени добавлять к сроку действия уведомления при получении дублирующегося элемента");
            VB_BetterPickupNotifications.ResetMessageTimerOnDupePickup = Config.Bind("04 - BetterPickupNotifications", "BPN_ResetMessageTimerOnDupePickup", false, "Сбрасывает таймер уведомления на максимальное время жизни при получении дублирующегося предмета");
            VB_BetterPickupNotifications.MessageVerticalSpacingModifier = Config.Bind("04 - BetterPickupNotifications", "BPN_MessageVerticalSpacingModifier", 1.25f, "Вертикальное разделение между сообщениями");
            VB_BetterPickupNotifications.MessageTextHorizontalSpacingModifier = Config.Bind("04 - BetterPickupNotifications", "BPN_MessageTextHorizontalSpacingModifier", 2f, "Горизонтальный интервал между иконкой и текста уведомлений");
            VB_BetterPickupNotifications.MessageTextVerticalModifier = Config.Bind("04 - BetterPickupNotifications", "BPN_MessageTextVerticalModifier", 1f, "Вертикальное выравнивание текста уведомлений");

            #endregion
            
            VB_EquipInWater.EiW_Custom = Config.Bind("05 - EquipInWater", "EiW_Custom",
                "KnifeFlint,KnifeCopper,KnifeChitin,KnifeSilver,KnifeBlackMetal,KnifeButcher,KnifeSkollAndHati,SpearFlint,SpearBronze,SpearElderbark,SpearWolfFang,SpearChitin,SpearCarapace,PickaxeAntler,PickaxeBronze,PickaxeIron,PickaxeBlackMetal,Hammer,Hoe,FistFenrirClaw",
                "Разрешить использовать оружие/инструмент в воде");
            VB_EquipInWater.EiW_Custom.SettingChanged += (_, _) =>  VB_EquipInWater.EiW_CustomStrings = [..  VB_EquipInWater.EiW_Custom.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)];
            VB_EquipInWater.EiW_CustomStrings = [..  VB_EquipInWater.EiW_Custom.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)];
            
            #region FirePlaceUtilites

            VB_FirePlaceUtilites.extinguishItemsConfig = Config.Bind("06 - EnableFire", "EF_Enable", true, "Вкл/Выкл секцию");
            VB_FirePlaceUtilites.extinguishStringConfig = Config.Bind("06 - EnableFire", "EF_Extinguish_Fire_Text", "Тушить огонь", "Текст отображаемый при наведении курсора на огонь");
            VB_FirePlaceUtilites.igniteStringConfig = Config.Bind("06 - EnableFire", "EF_Ignite_Fire_Text", "Разжечь огонь", "Текст, отображаемый при наведении курсора на огонь, если тот потушен");
            VB_FirePlaceUtilites.keyPOCodeStringConfig = Config.Bind("06 - EnableFire", "EF_Put_Out_Fire_Key", KeyCode.LeftAlt, "Клавиша чтобы потушить огонь.");
            VB_FirePlaceUtilites.configPOKey = (KeyCode)Enum.Parse(typeof(KeyCode), VB_FirePlaceUtilites.keyPOCodeStringConfig.Value.ToString());

            #endregion

            #region Recycle

            tabPosition = Config.Bind("07 - Recycle", "R_TabPosition", TabPositions.Left, "Положение вкладки Разобрать в меню крафта. (Требуется перезапуск)");
            resourceMultiplier = Config.Bind("07 - Recycle", "R_ResourceMultiplier", 0.35f, "Количество ресурсов, возвращаемых в результате разбора (от 0 до 1, где 1 возвращает 100% ресурсов, а 0 - 0%)");
            preserveOriginalItem = Config.Bind("07 - Recycle", "R_PreserveOriginalItem", true, "Сохранять ли данные оригинального предмета при понижении уровня. Полезно для модов, добавляющих дополнительные свойства к предметам, например EpicLoot.\nОтключите, если возникли проблемы.");
            recyclebuttontext = Config.Bind("07 - Recycle", "R_RecycleButtonText", "Разобрать", "Текст кнопки в меню крафта");

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
            #endregion
            
            VB_CustomSlotItem.ItemSlotPairs = Config.Bind("09 - CustomSlot", "ItemSlotPairs",
                "Demister,wisplight;Wishbone,wishbone;par_item_ring_25,par_item_ring;par_item_ring_50,par_item_ring;par_item_ring_75,par_item_ring;par_item_ring_100,par_item_ring",
                "\"ItemName1,SlotName;...;ItemNameN,SlotName\"\nНесколько предметов могут быть помещены в один и тот же слот (не все сразу), но один и тот же предмет не может быть помещен в несколько слотов.\nЧтобы изменения вступили в силу, игру необходимо перезапустить." );
            
            VB_RuFontFix.fontname = Config.Bind("10 - FontFix", "fontname", "Valheim-Norsebold", "Требуется перезапуск. Допустимые шрифты: NotoSansJP-Regular SDF, NotoSansSC-Regular SDF, Valheim-AveriaSansLibre, NotoSansThai-Regular SDF, Valheim-AveriaSerifLibre, NotoSerifJP-Regular SDF, NotoSerifArmenian-Regular SDF, NotoSerifDevanagari-Regular SDF, NotoSerifGeorgian-Regular SDF, NotoSerifThai-Regular SDF, NotoEmoji-Regular SDF, NotoSerifMalayalam-Regular SDF, NotoSerifBengali-Regular SDF, NotoSansHebrew-Regular SDF, NotoSansArabic-Regular SDF, Fallback-NotoSerifNormal, NotoSerifSC-Regular SDF, NotoSerifKR-Regular SDF, NotoSansBengali-Regular SDF, Valheim-Prstartk, NotoSansGeorgian-Regular SDF, NotoSansArmenian-Regular SDF, Fallback-NotoSansNormal, NotoSansKR-Regular SDF, NotoSansMalayalam-Regular SDF, NotoSansDevanagari-Regular SDF, Valheim-Norsebold, NotoSansSC-Thin SDF, NotoSansJP-Thin SDF, NotoSansDevanagari-ExtraLight SDF, NotoSansThai-ExtraLight SDF, NotoSansBengali-ExtraLight SDF, Valheim-Norse, NotoSansArmenian-ExtraLight SDF, NotoSansKR-Thin SDF, NotoSansHebrew-Light SDF, NotoEmoji-Light SDF, NotoSansGeorgian-ExtraLight SDF, NotoSansArabic-Light SDF, NotoSansMalayalam-ExtraLight SDF, Fallback-NotoSansThin, Valheim-Rune");
            
           Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGUID);
        }

        #region Recycle

        public void RebuildRecycleTab() 
        {
            if (self.recycleObject) return;

            Logger.LogInfo("Создана кнопка 'Разобрать'");

            recycleObject = Instantiate(InventoryGui.instance.m_tabUpgrade.gameObject, InventoryGui.instance.m_tabUpgrade.transform.parent);
            if (!recycleObject)
            {
                Logger.LogError("Не удалось создать кнопку 'Разобрать'.");
                return;
            }

            recycleObject.name = "Recycle";
            recycleObject.GetComponentInChildren<TMP_Text>().text = "Разбор";
            
            recycleButton = recycleObject.GetComponent<Button>();
            recycleButton.transform.localPosition = new Vector3(
                recycleObject.transform.localPosition.x + ((recycleObject.GetComponent<RectTransform>().rect.width + 10f) * ((int)tabPosition.Value + 1)),
                recycleObject.transform.localPosition.y, recycleObject.transform.localPosition.z
                );
            recycleButton.name = "RecycleButton";
            recycleButton.onClick.RemoveAllListeners();
            recycleButton.onClick.AddListener(() => 
            {
                Logger.LogDebug("Selected recycle");
                recycleButton.interactable = false;
                InventoryGui.m_instance.m_tabCraft.interactable = true;
                InventoryGui.m_instance.m_tabUpgrade.interactable = true;
                InventoryGui.m_instance.UpdateCraftingPanel();
            });
    
            recycleObject.SetActive(false);
        }
        #endregion
        
        private void OnDestroy()
        {
            Config.Save();
            Logger.LogInfo("DESTROY");
            Destroy(recycleObject);
        }
        

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
