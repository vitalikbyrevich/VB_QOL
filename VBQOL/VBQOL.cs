using System.ComponentModel;
using System.IO;
using Jotunn;
using Jotunn.Extensions;
using Jotunn.Managers;
using VBQOL.AddFuel;
using VBQOL.Network;
using VBQOL.Recycle;
using Paths = BepInEx.Paths;

namespace VBQOL
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("VitByr.ParadoxBuild", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("shudnal.Seasons", BepInDependency.DependencyFlags.SoftDependency)]
    
    class VBQOL : BaseUnityPlugin
    {
        private const string ModName = "VBQOL";
        private const string ModVersion = "0.4.4";
        private const string ModGUID = "VitByr.VBQOL";
        internal static VBQOL self;
        internal static bool paradoxbuild;
        internal static bool seasons;
        private ConfigFile adminConfig;
        
        private void Awake()
        {
            self = this;
            paradoxbuild = CheckIfModIsLoaded("VitByr.ParadoxBuild");
            seasons = CheckIfModIsLoaded("shudnal.Seasons");
            
           // string adminConfigPath = Path.Combine(Paths.ConfigPath, "VitByr", "VBQOL", "admin.cfg");
          //  Directory.CreateDirectory(Path.GetDirectoryName(adminConfigPath));
           // ConfigFile adminConfig = null;
            adminConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "VitByr/VBQOL/admin.cfg"), true);
            
            ConfigurationManagerAttributes isAdminOnly = new ConfigurationManagerAttributes { IsAdminOnly = true};
            ConfigurationManagerAttributes isAdminOnlyReadOnly = new ConfigurationManagerAttributes { IsAdminOnly = true, ReadOnly = true};
            
            VB_FontChange.mainFontName = Config.Bind("01 - FontFix", "mainFontName", "Valheim-Norse", 
                            "Основной шрифт. Допустимые шрифты: Valheim-Norse, Valheim-Norsebold, Valheim-AveriaSansLibre, Valheim-AveriaSerifLibre, LiberationSans SDF, LiberationSans SDF - Fallback. Требуется перезапуск.");
            VB_FontChange.secondaryFontName = Config.Bind("01 - FontFix", "secondaryFontName", "Valheim-AveriaSerifLibre", 
                            "Остальной шрифт. Допустимые шрифты: Valheim-Norse, Valheim-Norsebold, Valheim-AveriaSansLibre, Valheim-AveriaSerifLibre, LiberationSans SDF, LiberationSans SDF - Fallback. Требуется перезапуск.");
                        
            AddFuelUtil.AFEnable = Config.Bind("02 - AddAllFuel", "AF_Enable", true, "Вкл/Выкл секцию"); 
            AddFuelUtil.AFModifierKeyConfig = Config.Bind("02 - AddAllFuel", "AF_ModifierKey", KeyCode.LeftShift, new ConfigDescription("Клавиша для добавления сразу стака в печь/ плавильню.")); 
            AddFuelUtil.AFTextConfig = Config.Bind("02 - AddAllFuel", "AF_Extinguish_Text", "Добавить стак", new ConfigDescription("Текст отображаемый при наведении печь/костер"));
          
            VB_BuildDamage.enableModBDConfig = adminConfig.BindConfig("03 - BuildDamage", "BD_Enable_Section", true, "Включите или отключите этот раздел", synced: true); 
            VB_BuildDamage.creatorDamageMultConfig = adminConfig.BindConfig("03 - BuildDamage", "BD_CreatorDamageMult", 0.75f, "Множитель урона от создателя постройки", synced: true);
            VB_BuildDamage.nonCreatorDamageMultConfig = adminConfig.BindConfig("03 - BuildDamage", "BD_NonCreatorDamageMult", 0.05f, "Множитель урона от не создателя постройки", synced: true);
            VB_BuildDamage.uncreatedDamageMultConfig = adminConfig.BindConfig("03 - BuildDamage", "BD_UncreatedDamageMult", 0.75f, "Множитель урона постройкам не созданным игроком", synced: true);
            VB_BuildDamage.naturalDamageMultConfig = adminConfig.BindConfig("03 - BuildDamage", "BD_NaturalDamageMult", 0.75f, "Множитель урона от погоды и монстров.",synced: true);

            
            VB_EquipInWater.EiW_Custom = adminConfig.Bind("04 - EquipInWater", "EiW_Custom",
                "KnifeFlint,KnifeCopper,KnifeChitin,KnifeSilver,KnifeBlackMetal,KnifeButcher,KnifeSkollAndHati,SpearFlint,SpearBronze,SpearElderbark,SpearWolfFang,SpearChitin,SpearCarapace,PickaxeAntler,PickaxeBronze,PickaxeIron,PickaxeBlackMetal,Hammer,Hoe,FistFenrirClaw",
                new ConfigDescription("Разрешить использовать оружие/инструмент в воде", null, isAdminOnly));
           VB_EquipInWater.EiW_Custom.SettingChanged += (_, _) =>  VB_EquipInWater.EiW_CustomStrings = [..  VB_EquipInWater.EiW_Custom.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)];
           VB_EquipInWater.EiW_CustomStrings = [..  VB_EquipInWater.EiW_Custom.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)];
            
            VB_FirePlaceUtilites.extinguishItemsConfig = Config.Bind("05 - EnableFire", "EF_Enable", true, "Вкл/Выкл секцию");
            VB_FirePlaceUtilites.extinguishStringConfig = Config.Bind("05 - EnableFire", "EF_Extinguish_Fire_Text", "Тушить огонь", "Текст отображаемый при наведении курсора на огонь");
            VB_FirePlaceUtilites.igniteStringConfig = Config.Bind("05 - EnableFire", "EF_Ignite_Fire_Text", "Разжечь огонь", "Текст, отображаемый при наведении курсора на огонь, если тот потушен");
            VB_FirePlaceUtilites.keyPOCodeStringConfig = Config.Bind("05 - EnableFire", "EF_Put_Out_Fire_Key", KeyCode.LeftAlt, "Клавиша чтобы потушить огонь.");
            VB_FirePlaceUtilites.configPOKey = (KeyCode)Enum.Parse(typeof(KeyCode), VB_FirePlaceUtilites.keyPOCodeStringConfig.Value.ToString());

            RecycleUtil.tabPosition = Config.Bind("06 - Recycle", "R_TabPosition", RecycleUtil.TabPositions.Left, "Положение вкладки Разобрать в меню крафта. (Требуется перезапуск)");
            RecycleUtil.recyclebuttontext = Config.Bind("06 - Recycle", "R_RecycleButtonText", "Разобрать", "Текст кнопки в меню крафта");
            RecycleUtil.resourceMultiplier = adminConfig.Bind("06 - Recycle", "R_ResourceMultiplier", 0.35f,
                new ConfigDescription("Количество ресурсов, возвращаемых в результате разбора (от 0 до 1, где 1 возвращает 100% ресурсов, а 0 - 0%)", new AcceptableValueRange<float>(0f, 1f), isAdminOnly));
            RecycleUtil.preserveOriginalItem = adminConfig.BindConfig("06 - Recycle", "R_PreserveOriginalItem", true, "Сохранять ли данные оригинального предмета при понижении уровня. Полезно для модов, добавляющих дополнительные свойства к предметам, например EpicLoot.\nОтключите, если возникли проблемы.", synced: true);

            VB_GraphicPatch.soft_particles = Config.Bind("07 - QualitySettings", "soft_particles", true, "Убирает резкое обрезание частиц при пересечении с геометрией, создавая плавное смешивание.");
            VB_GraphicPatch.particle_raycast_budget = Config.Bind("07 - QualitySettings", "particle_raycast_budget", 1024, "Ограничивает количество проверок столкновений частиц за кадр");
            VB_GraphicPatch.soft_vegetation = Config.Bind("07 - QualitySettings", "soft_vegetation", false, "Добавляет сглаживание к краям растительности");
            VB_GraphicPatch.streaming_mipmaps_memory_budget = Config.Bind("07 - QualitySettings", "streaming_mipmaps_memory_budget", 4096f, "Определяет сколько памяти выделено для потоковой загрузки текстур разного разрешения");
            VB_GraphicPatch.lod_bias = Config.Bind("07 - QualitySettings", "lod_bias", 4f, "Управляет расстоянием переключения между уровнями детализации");
            VB_GraphicPatch.anisotropic_filtering = Config.Bind("07 - QualitySettings", "anisotropic_filtering",  AnisotropicFiltering.Disable, "Улучшает четкость текстур под углом .");
            VB_GraphicPatch.anti_aliasing = Config.Bind("07 - QualitySettings", "anti_aliasing", 2, "Убирает \"лестничный эффект\" на краях объектов");
            
            VB_GraphicPatch.grass_size = Config.Bind("07 - QualitySettings", "grass_size", 10f, "Определяет размер участка (патча) травы.");
            VB_GraphicPatch.grass_distance = Config.Bind("07 - QualitySettings", "grass_distance", 100f, "Определяет, на каком расстоянии от игрока трава перестаёт отображаться.");
            VB_GraphicPatch.grass_playerPushFade = Config.Bind("07 - QualitySettings", "grass_playerPushFade", 0.075f, "Определяет, насколько трава \"притаптывается\" или отталкивается при ходьбе игрока.");
            VB_GraphicPatch.grass_amountScale = Config.Bind("07 - QualitySettings", "grass_amountScale", 1.5f, "Насколько густо растёт трава.");
            VB_GraphicPatch.SetGraphicsSettings();
          
            VB_CustomSlotItem.ItemSlotPairs = adminConfig.BindConfig("08 - CustomSlot", "ItemSlotPairs",
                "Demister,wisplight;Wishbone,wishbone;par_item_ring_25,par_item_ring;par_item_ring_50,par_item_ring;par_item_ring_75,par_item_ring;par_item_ring_100,par_item_ring",
                "\"ItemName1,SlotName;...;ItemNameN,SlotName\"\nНесколько предметов могут быть помещены в один и тот же слот (не все сразу), но один и тот же предмет не может быть помещен в несколько слотов.\nЧтобы изменения вступили в силу, игру необходимо перезапустить.", synced: true );
            
            CreateConfigWatcher();
            
           Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGUID);
        }
        
        private void OnDestroy()
        {
            Config.Save();
            Logger.LogInfo("DESTROY");
            Destroy(RecycleUtil.recycleObject);
        }
        
        private void CreateConfigWatcher()
        {
            ConfigFileWatcher configFileWatcher = new(Config, reloadDelay: 1000);
            configFileWatcher.OnConfigFileReloaded += () =>
            {
                VB_FontChange.RefreshAllUIElements();
                RecycleUtil.ForceRebuildRecycleTab();
                VB_GraphicPatch.SetGraphicsSettings();
            };
            ConfigFileWatcher adminConfigWatcher = new(adminConfig, reloadDelay: 1000);
            adminConfigWatcher.OnConfigFileReloaded += () =>
            {
                
            };
        }
        
        private bool CheckIfModIsLoaded(string modGUID)
        {
            foreach (KeyValuePair<string, PluginInfo> keyValuePair in Chainloader.PluginInfos) if (keyValuePair.Value.Metadata.GUID.Equals(modGUID)) return true;
            return false;
        }
    }
}
