using VBQOL.AddFuel;
using VBQOL.Recycle;

namespace VBQOL
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("VitByr.ParadoxBuild", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("shudnal.Seasons", BepInDependency.DependencyFlags.SoftDependency)]
    class VBQOL : BaseUnityPlugin
    {
        private const string ModName = "VBQOL";
        private const string ModVersion = "0.3.8";
        private const string ModGUID = "VitByr.VBQOL";
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
       
        #endregion
        
        internal static bool paradoxbuild;
        internal static bool seasons;

        private void Awake()
        {
            self = this;
            paradoxbuild = CheckIfModIsLoaded("VitByr.ParadoxBuild");
            seasons = CheckIfModIsLoaded("shudnal.Seasons");
            ConfigurationManagerAttributes isAdminOnly = new ConfigurationManagerAttributes { IsAdminOnly = true };
            
            VB_BossDespawn.radiusConfig = Config.Bind("01 - BossDespawn", "Despawn radius", 150f, new ConfigDescription("Радиус обнаружения игроков", null, isAdminOnly)); 
            VB_BossDespawn.despawnDelayConfig = Config.Bind("01 - BossDespawn", "Despawn delay", 5f, new ConfigDescription("Через сколько минут босс деспавнится", null, isAdminOnly));
            
            AddFuelUtil.AFEnable = Config.Bind("02 - AddAllFuel", "AF_Enable", true, "Вкл/Выкл секцию"); 
            AddFuelUtil.AFModifierKeyConfig = Config.Bind("02 - AddAllFuel", "AF_ModifierKey", KeyCode.LeftShift, new ConfigDescription("Клавиша для добавления сразу стака в печь/ плавильню.")); 
            AddFuelUtil.AFTextConfig = Config.Bind("02 - AddAllFuel", "AF_Extinguish_Text", "Добавить стак", new ConfigDescription("Текст отображаемый при наведении печь/костер"));

            #region BuildDamage

            VB_BuildDamage.enableModBDConfig = Config.Bind("03 - BuildDamage", "BD_Enable_Section", true, new ConfigDescription("Включите или отключите этот раздел", null, isAdminOnly)); 
            VB_BuildDamage.creatorDamageMultConfig = Config.Bind("03 - BuildDamage", "BD_CreatorDamageMult", 0.75f, new ConfigDescription("Множитель урона от создателя постройки", null, isAdminOnly));
            VB_BuildDamage.nonCreatorDamageMultConfig = Config.Bind("03 - BuildDamage", "BD_NonCreatorDamageMult", 0.05f, new ConfigDescription("Множитель урона от не создателя постройки", null, isAdminOnly));
            VB_BuildDamage.uncreatedDamageMultConfig = Config.Bind("03 - BuildDamage", "BD_UncreatedDamageMult", 0.75f, new ConfigDescription("Множитель урона постройкам не созданным игроком", null, isAdminOnly));
            VB_BuildDamage.naturalDamageMultConfig = Config.Bind("03 - BuildDamage", "BD_NaturalDamageMult", 0.75f, new ConfigDescription("Множитель урона от погоды и монстров.", null, isAdminOnly));

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
                new ConfigDescription("Разрешить использовать оружие/инструмент в воде", null, isAdminOnly));
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

            RecycleUtil.tabPosition = Config.Bind("07 - Recycle", "R_TabPosition", RecycleUtil.TabPositions.Left, "Положение вкладки Разобрать в меню крафта. (Требуется перезапуск)");
            RecycleUtil.resourceMultiplier = Config.Bind("07 - Recycle", "R_ResourceMultiplier", 0.35f, "Количество ресурсов, возвращаемых в результате разбора (от 0 до 1, где 1 возвращает 100% ресурсов, а 0 - 0%)");
            RecycleUtil.preserveOriginalItem = Config.Bind("07 - Recycle", "R_PreserveOriginalItem", true, "Сохранять ли данные оригинального предмета при понижении уровня. Полезно для модов, добавляющих дополнительные свойства к предметам, например EpicLoot.\nОтключите, если возникли проблемы.");
            RecycleUtil.recyclebuttontext = Config.Bind("07 - Recycle", "R_RecycleButtonText", "Разобрать", "Текст кнопки в меню крафта");

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
            
            VB_FontChange.mainFontName = Config.Bind("10 - FontFix", "mainFontName", "Valheim-Norse", 
                "Основной шрифт. Допустимые шрифты: Valheim-Norse, Valheim-Norsebold, Valheim-AveriaSansLibre, Valheim-AveriaSerifLibre, LiberationSans SDF, LiberationSans SDF - Fallback. Требуется перезапуск.");
            VB_FontChange.secondaryFontName = Config.Bind("10 - FontFix", "secondaryFontName", "Valheim-AveriaSerifLibre", 
                "Остальной шрифт. Допустимые шрифты: Valheim-Norse, Valheim-Norsebold, Valheim-AveriaSansLibre, Valheim-AveriaSerifLibre, LiberationSans SDF, LiberationSans SDF - Fallback. Требуется перезапуск.");
       
           Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGUID);
        }
        
        private void OnDestroy()
        {
            Config.Save();
            Logger.LogInfo("DESTROY");
            Destroy(RecycleUtil.recycleObject);
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
