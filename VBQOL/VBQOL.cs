using VBQOL.AddFuel;
using VBQOL.Network;
using VBQOL.Recycle;
using VBQOL.Util;

namespace VBQOL
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    [BepInDependency("VitByr.ParadoxBuild", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("shudnal.Seasons", BepInDependency.DependencyFlags.SoftDependency)]

    class VBQOL : BaseUnityPlugin
    {
        private const string ModName = "VBQOL";
        private const string ModVersion = "0.4.8";
        private const string ModGUID = "VitByr.VBQOL";
        internal static VBQOL self;
        internal static bool paradoxbuild;
        internal static bool seasons;
        private ConfigFile ServerConfig;
        private CustomRPC ServerConfigRPC;

        private void Awake()
        {
            self = this;
            paradoxbuild = Helper.CheckIfModIsLoaded("VitByr.ParadoxBuild");
            seasons = Helper.CheckIfModIsLoaded("shudnal.Seasons");

            ServerConfig = new ConfigFile(Path.Combine(Paths.ConfigPath, "VitByr/VBQOL/ServerConfig.cfg"), true);
            SynchronizationManager.Instance.RegisterCustomConfig(ServerConfig);

            ClientConfigInit();
            ServerConfigInit();

            ServerConfigRPC = NetworkManager.Instance.AddRPC("VBQOL_ServerConfigRPC", OnAdminConfigSync, OnClientConfigSync);

            CreateConfigWatcher();

            /* if (Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.mining"))
             {
                 Harmony.CreateAndPatchAll(typeof(Patch_MineRock_LeviathanExplosion));
                 Debug.Log("[LeviathanPatch] Mining mod patched to use vanilla explosion mechanics");
             }*/ 
            Harmony.CreateAndPatchAll(typeof(VB_LeviathanPatch));
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGUID);
        }

        private void Start()
        {
            StartCoroutine(WaitForLocalPlayer());
            StartCoroutine(WaitForInventoryGui());
        }

        public void ClientConfigInit()
        {
            VB_FontChange.mainFontName = Config.Bind("01 - FontFix", "mainFontName", "Valheim-Norse",
                "Основной шрифт. Допустимые шрифты: Valheim-Norse, Valheim-Norsebold, Valheim-AveriaSansLibre, Valheim-AveriaSerifLibre, LiberationSans SDF, LiberationSans SDF - Fallback. Требуется перезапуск.");
            VB_FontChange.secondaryFontName = Config.Bind("01 - FontFix", "secondaryFontName", "Valheim-AveriaSerifLibre",
                "Остальной шрифт. Допустимые шрифты: Valheim-Norse, Valheim-Norsebold, Valheim-AveriaSansLibre, Valheim-AveriaSerifLibre, LiberationSans SDF, LiberationSans SDF - Fallback. Требуется перезапуск.");

            AddFuelUtil.AFEnable = Config.Bind("02 - AddAllFuel", "AF_Enable", true, "Вкл/Выкл секцию");
            AddFuelUtil.AFModifierKeyConfig = Config.Bind("02 - AddAllFuel", "AF_ModifierKey", KeyCode.LeftShift, new ConfigDescription("Клавиша для добавления сразу стака в печь/ плавильню."));
            AddFuelUtil.AFTextConfig = Config.Bind("02 - AddAllFuel", "AF_Extinguish_Text", "Добавить стак", new ConfigDescription("Текст отображаемый при наведении печь/костер"));

            VB_FirePlaceUtilites.extinguishItemsConfig = Config.Bind("03 - EnableFire", "EF_Enable", true, "Вкл/Выкл секцию");
            VB_FirePlaceUtilites.extinguishStringConfig = Config.Bind("03 - EnableFire", "EF_Extinguish_Fire_Text", "Тушить огонь", "Текст отображаемый при наведении курсора на огонь");
            VB_FirePlaceUtilites.igniteStringConfig = Config.Bind("03 - EnableFire", "EF_Ignite_Fire_Text", "Разжечь огонь", "Текст, отображаемый при наведении курсора на огонь, если тот потушен");
            VB_FirePlaceUtilites.keyPOCodeStringConfig = Config.Bind("03 - EnableFire", "EF_Put_Out_Fire_Key", KeyCode.LeftAlt, "Клавиша чтобы потушить огонь.");
            VB_FirePlaceUtilites.configPOKey = (KeyCode)Enum.Parse(typeof(KeyCode), VB_FirePlaceUtilites.keyPOCodeStringConfig.Value.ToString());

            RecycleUtil.tabPosition = Config.Bind("04 - Recycle", "R_TabPosition", RecycleUtil.TabPositions.Left, "Положение вкладки Разобрать в меню крафта. (Требуется перезапуск)");
            RecycleUtil.recyclebuttontext = Config.Bind("04 - Recycle", "R_RecycleButtonText", "Разобрать", "Текст кнопки в меню крафта");

            VB_GraphicPatch.soft_particles = Config.Bind("05 - QualitySettings", "soft_particles", true, "Убирает резкое обрезание частиц при пересечении с геометрией, создавая плавное смешивание.");
            VB_GraphicPatch.particle_raycast_budget = Config.Bind("05 - QualitySettings", "particle_raycast_budget", 1024, "Ограничивает количество проверок столкновений частиц за кадр");
            VB_GraphicPatch.soft_vegetation = Config.Bind("05 - QualitySettings", "soft_vegetation", false, "Добавляет сглаживание к краям растительности");
            VB_GraphicPatch.streaming_mipmaps_memory_budget = Config.Bind("05 - QualitySettings", "streaming_mipmaps_memory_budget", 4096f,
                "Определяет сколько памяти выделено для потоковой загрузки текстур разного разрешения");
            VB_GraphicPatch.lod_bias = Config.Bind("05 - QualitySettings", "lod_bias", 4f, "Управляет расстоянием переключения между уровнями детализации");
            VB_GraphicPatch.anisotropic_filtering = Config.Bind("05 - QualitySettings", "anisotropic_filtering", AnisotropicFiltering.Disable, "Улучшает четкость текстур под углом .");
            VB_GraphicPatch.anti_aliasing = Config.Bind("05 - QualitySettings", "anti_aliasing", 2, "Убирает \"лестничный эффект\" на краях объектов");
            VB_GraphicPatch.grass_size = Config.Bind("05 - QualitySettings", "grass_size", 10f, "Определяет размер участка (патча) травы.");
            VB_GraphicPatch.grass_distance = Config.Bind("05 - QualitySettings", "grass_distance", 100f, "Определяет, на каком расстоянии от игрока трава перестаёт отображаться.");
            VB_GraphicPatch.grass_playerPushFade = Config.Bind("05 - QualitySettings", "grass_playerPushFade", 0.075f, "Определяет, насколько трава \"притаптывается\" или отталкивается при ходьбе игрока.");
            VB_GraphicPatch.grass_amountScale = Config.Bind("05 - QualitySettings", "grass_amountScale", 1.5f, "Насколько густо растёт трава.");
            VB_GraphicPatch.SetGraphicsSettings();

            VB_CraftingFilter.modEnabled = Config.Bind("06 - CraftingFilter", "CF_Enabled", true, "Включить фильтр крафтового меню");
            VB_CraftingFilter.LoadCategories();
            VB_CraftingFilter.Instance = new VB_CraftingFilter();
        }

        public void ServerConfigInit()
        {
                VB_LeviathanPatch.m_resetLeviathanOn = ServerConfig.BindConfig("00 - LeviathanPatch", "LP_resetLeviathanOn", true, "Восстанавливать ли Левиафаны в Океане?", synced: true);
                VB_LeviathanPatch.m_resetLeviathanLavaOn = ServerConfig.BindConfig("00 - LeviathanPatch", "LP_resetLeviathanLavaOn", true, "Восстанавливать ли Левиафаны в Пепельных землях?", synced: true);
                VB_LeviathanPatch.m_riseDelay = ServerConfig.BindConfig("00 - LeviathanPatch", "LP_riseDelay", 60f, "Время через сколько поднимется Левиафан в сек.", synced: true);
              
            VB_BuildDamage.enableModBDConfig = ServerConfig.BindConfig(
                "01 - BuildDamage", "BD_Enable_Section", true, "Включите или отключите этот раздел", synced: true);
            VB_BuildDamage.creatorDamageMultConfig = ServerConfig.BindConfig(
                "01 - BuildDamage", "BD_CreatorDamageMult", 0.75f, "Множитель урона от создателя постройки", acceptableValues: new AcceptableValueRange<float>(0f, 1f), synced: true);
            VB_BuildDamage.nonCreatorDamageMultConfig = ServerConfig.BindConfig(
                "01 - BuildDamage", "BD_NonCreatorDamageMult", 0.05f, "Множитель урона от не создателя постройки", acceptableValues: new AcceptableValueRange<float>(0f, 1f), synced: true);
            VB_BuildDamage.uncreatedDamageMultConfig = ServerConfig.BindConfig(
                "01 - BuildDamage", "BD_UncreatedDamageMult", 0.75f, "Множитель урона постройкам не созданным игроком", acceptableValues: new AcceptableValueRange<float>(0f, 1f), synced: true);
            VB_BuildDamage.naturalDamageMultConfig = ServerConfig.BindConfig(
                "01 - BuildDamage", "BD_NaturalDamageMult", 0.75f, "Множитель урона от погоды и монстров.", acceptableValues: new AcceptableValueRange<float>(0f, 1f), synced: true);

            VB_EquipInWater.EiW_Custom = ServerConfig.BindConfig("02 - EquipInWater", "EiW_Custom",
                "KnifeFlint,KnifeCopper,KnifeChitin,KnifeSilver,KnifeBlackMetal,KnifeButcher,KnifeSkollAndHati,SpearFlint,SpearBronze,SpearElderbark,SpearWolfFang,SpearChitin,SpearCarapace,PickaxeAntler,PickaxeBronze,PickaxeIron,PickaxeBlackMetal,Hammer,Hoe,FistFenrirClaw",
                "Разрешить использовать оружие/инструмент в воде", synced: true);
            VB_EquipInWater.EiW_Custom.SettingChanged += (_, _) => VB_EquipInWater.EiW_CustomStrings = [.. VB_EquipInWater.EiW_Custom.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)];
            VB_EquipInWater.EiW_CustomStrings = [.. VB_EquipInWater.EiW_Custom.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)];

            RecycleUtil.resourceMultiplier = ServerConfig.BindConfig("03 - Recycle", "R_ResourceMultiplier", 0.35f,
                "Количество ресурсов, возвращаемых в результате разбора (от 0 до 1, где 1 возвращает 100% ресурсов, а 0 - 0%)", acceptableValues: new AcceptableValueRange<float>(0f, 1f), synced: true);
            RecycleUtil.preserveOriginalItem = ServerConfig.BindConfig(
                "03 - Recycle", "R_PreserveOriginalItem", true,
                "Сохранять ли данные оригинального предмета при понижении уровня. Полезно для модов, добавляющих дополнительные свойства к предметам, например EpicLoot.\nОтключите, если возникли проблемы.",
                synced: true);

            VB_CustomSlotItem.ItemSlotPairs = ServerConfig.BindConfig("04 - CustomSlot", "ItemSlotPairs",
                "Demister,wisplight;Wishbone,wishbone;par_item_ring_25,par_item_ring;par_item_ring_50,par_item_ring;par_item_ring_75,par_item_ring;par_item_ring_100,par_item_ring",
                "\"ItemName1,SlotName;...;ItemNameN,SlotName\"\nНесколько предметов могут быть помещены в один и тот же слот (не все сразу), но один и тот же предмет не может быть помещен в несколько слотов.\nЧтобы изменения вступили в силу, игру необходимо перезапустить.",
                synced: true);

            NoIntroNoValkyrie.m_enableNINV = ServerConfig.BindConfig("05 - NoIntroNoValkyrie", "NINV_Enable", true, "Полностью отключить начальные титры и полет в когтях Валькирии", synced: true);
        }

        private IEnumerator WaitForLocalPlayer()
        {
            while (!Player.m_localPlayer) yield return null;

            Logger.LogInfo("Локальный игрок найден - клиент подключен");

            if (!ZNet.instance.IsServer()) ServerConfig.Reload();
            else
            {
                ServerConfig.Reload();
                Logger.LogInfo("Сервер запущен, админ-конфиг загружен");
            }
        }

        private IEnumerator WaitForInventoryGui()
        {
            while (!InventoryGui.instance) yield return null;
            yield return null;
            VB_CraftingFilter.Instance?.OnInventoryGuiReady();
        }

        private void ApplyConfigFromPackage(ZPackage pkg)
        {
            if (pkg == null || pkg.GetArray().Length == 0)
            {
                Debug.LogWarning("[VBQOL] Received empty config package");
                return;
            }

            try
            {
                pkg.SetPos(0);

                VB_BuildDamage.enableModBDConfig.Value = pkg.ReadBool();
                VB_BuildDamage.creatorDamageMultConfig.Value = pkg.ReadSingle();
                VB_BuildDamage.nonCreatorDamageMultConfig.Value = pkg.ReadSingle();
                VB_BuildDamage.uncreatedDamageMultConfig.Value = pkg.ReadSingle();
                VB_BuildDamage.naturalDamageMultConfig.Value = pkg.ReadSingle();
                VB_EquipInWater.EiW_Custom.Value = pkg.ReadString();
                RecycleUtil.resourceMultiplier.Value = pkg.ReadSingle();
                RecycleUtil.preserveOriginalItem.Value = pkg.ReadBool();
                VB_CustomSlotItem.ItemSlotPairs.Value = pkg.ReadString();

                VB_CustomSlotManager.ReapplyItemSlotPairs();
                VB_EquipInWater.EiW_CustomStrings = new HashSet<string>(
                    VB_EquipInWater.EiW_Custom.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

                Debug.Log("[VBQOL] Config applied successfully");
            }
            catch (Exception e)
            {
                Debug.LogError($"[VBQOL] Error applying config package: {e.Message}");
            }
        }

        private ZPackage BuildConfigPackage()
        {
            ZPackage pkg = new ZPackage();
            try
            {
                pkg.Write(VB_BuildDamage.enableModBDConfig.Value);
                pkg.Write(VB_BuildDamage.creatorDamageMultConfig.Value);
                pkg.Write(VB_BuildDamage.nonCreatorDamageMultConfig.Value);
                pkg.Write(VB_BuildDamage.uncreatedDamageMultConfig.Value);
                pkg.Write(VB_BuildDamage.naturalDamageMultConfig.Value);
                pkg.Write(VB_EquipInWater.EiW_Custom.Value ?? "");
                pkg.Write(RecycleUtil.resourceMultiplier.Value);
                pkg.Write(RecycleUtil.preserveOriginalItem.Value);
                pkg.Write(VB_CustomSlotItem.ItemSlotPairs.Value ?? "");
            }
            catch (Exception e)
            {
                Debug.LogError($"[VBQOL] Error building config package: {e.Message}");
                return new ZPackage();
            }

            return pkg;
        }

        public static readonly WaitForSeconds OneSecondWait = new WaitForSeconds(1f);

        private IEnumerator OnAdminConfigSync(long sender, ZPackage pkg)
        {
            Logger.LogInfo($"[VBQOL] Сервер получил вызов OnClientConfigSync от {sender}");

            ApplyConfigFromPackage(pkg);

            if (ZNet.instance && ZNet.instance.IsServer())
            {
                byte[] data = pkg.GetArray();
                foreach (var peer in ZNet.instance.GetPeers())
                {
                    if (peer.m_uid != sender)
                    {
                        ZPackage copyPkg = new ZPackage(data);
                        ServerConfigRPC.SendPackage(new List<ZNetPeer> { peer }, copyPkg);
                    }
                }
            }

            yield break;
        }

        private IEnumerator OnClientConfigSync(long sender, ZPackage pkg)
        {
            Logger.LogInfo($"[VBQOL] Клиент получил пакет OnAdminConfigSync от {sender}");

            ApplyConfigFromPackage(pkg);

            yield break;
        }

        public static readonly WaitForSeconds HalfSecondWait = new WaitForSeconds(0.5f);

        private void CreateConfigWatcher()
        {
            ConfigFileWatcher configFileWatcher = new ConfigFileWatcher(Config, reloadDelay: 1000);
            configFileWatcher.OnConfigFileReloaded += () =>
            {
                if (ZNet.instance)
                {
                    StartCoroutine(ApplyClientConfigChanges());
                }
            };

            ConfigFileWatcher adminConfigWatcher = new ConfigFileWatcher(ServerConfig, reloadDelay: 1000);
            adminConfigWatcher.OnConfigFileReloaded += () =>
            {
                if (!ZNet.instance || !ZNet.instance.IsServer()) return;

                StartCoroutine(ApplyServerConfigChanges());
            };
        }

        private IEnumerator ApplyClientConfigChanges()
        {
            yield return null;
            VB_FontChange.RefreshAllUIElements();
            RecycleUtil.ForceRebuildRecycleTab();
            VB_GraphicPatch.SetGraphicsSettings();
        }

        private IEnumerator ApplyServerConfigChanges()
        {
            yield return null;

            ZPackage pkg = BuildConfigPackage();
            if (pkg.GetArray().Length > 0)
            {
                VB_CustomSlotManager.ReapplyItemSlotPairs();

                byte[] data = pkg.GetArray();
                foreach (var peer in ZNet.instance.GetPeers())
                {
                    ZPackage copyPkg = new ZPackage(data);
                    ServerConfigRPC.SendPackage(new List<ZNetPeer> { peer }, copyPkg);
                }

                Logger.LogInfo("[VBQOL] AdminConfig изменён, данные отправлены клиентам");
            }
        }

        private void OnDestroy()
        {
            Config.Save();
            Logger.LogInfo("DESTROY");
            Destroy(RecycleUtil.recycleObject);
        }
    }
}