namespace VBQOL;

[HarmonyPatch]
internal static class VB_BossDespawn
{
    private static readonly Dictionary<Vector3, BossData> bossDataDict = new();
    private static readonly HashSet<Vector3> bossesToRemove = new();

    private static readonly Dictionary<(string bossName, string playerName), int> bossKillStats = new();
    private static readonly HashSet<(string bossName, string playerName)> playersInRange = new();

    internal static ConfigEntry<float> radiusConfig;
    internal static ConfigEntry<float> despawnDelayConfig;
    internal static int tauntDeathThreshold = 1;

    internal static string[] despawnMessages = new string[]
    {
        "Ты не достоин моей ярости. Я исчезаю.",
        "Скука… Вернусь, когда найдётся смелый воин.",
        "Трусость твоя спасла тебя лишь на время.",
        "Я ухожу в тьму, но мы ещё встретимся.",
        "Ты сбежал? Тогда я заберу у тебя надежду.",
        "Я не трачу силы на слабых.",
        "Исчезаю, но твой страх останется со мной.",
        "Ты избежал битвы, но не избежишь судьбы.",
        "Я вернусь, когда ты осмелеешь.",
        "Смертный, ты не стоишь моего времени."
    };

    internal static string[] lostMessages = new string[]
    {
        "Не смей отворачиваться от меня!",
        "Вернись и сразись, если не трус!",
        "Ты не убежишь от своей гибели!",
        "Я ещё не насытился твоим страхом!",
        "Смертный, твой бег лишь продлевает муки!",
        "Ты думаешь, что спасёшься?",
        "Назад! Я не закончил с тобой!",
        "Ты не уйдёшь от моей ярости!",
        "Беги, но я настигну тебя!",
        "Трус! Сражайся до конца!"
    };

    internal static string[] killMessages = new string[]
    {
        "Вот так умирают слабые!",
        "Ещё один смертный пал предо мной!",
        "Ха-ха! Твоя жизнь окончена!",
        "Ты был лишь игрушкой для моей силы!",
        "Смерть твоя — моя забава!",
        "Никто не спасётся от моей мощи!",
        "Твоя кровь украсила мою победу!",
        "Такова судьба всех, кто бросает мне вызов!",
        "Ты пал, как и все до тебя!",
        "Смертный, ты был слишком слаб для этой битвы!"
    };

    internal static string[] tauntMessages = new string[]
    {
        "Снова ты? Ты ничему не учишься!",
        "Я уже привык видеть твою смерть!",
        "Ты вернулся за новым поражением?",
        "Ха-ха! Ты снова пришёл умирать?",
        "Я начинаю скучать без твоих криков!",
        "Ты мой любимый трофей!",
        "Сколько раз ещё ты придёшь падать?",
        "Я узнаю твой запах страха!",
        "Ты снова здесь? Я ждал тебя!",
        "Твоя гибель всегда радует меня!"
    };
    internal static string[] rareTauntMessages = new string[]
    {
        "Ты — мой вечный источник забавы!",
        "Я храню твои крики в своей памяти!",
        "Смерть твоя стала для меня привычкой!",
        "Ты словно тень, всегда возвращаешься ко мне!",
        "Я начинаю думать, что ты любишь умирать от моей руки!"
    };

    private struct BossData
    {
        public float LastPlayerSeenTime;
        public bool IsTimerRunning;
        public Humanoid BossRef;
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.CustomFixedUpdate))]
    [HarmonyPostfix]
    public static void CheckAndUpdateBossTimers(Humanoid __instance)
    {
        if (!ShouldProcess(__instance)) return;

        var spawnPoint = __instance.GetBaseAI()?.m_spawnPoint ?? Vector3.zero;
        var currentTime = Time.time;
        var hasPlayerInRange = Player.IsPlayerInRange(__instance.transform.position, radiusConfig.Value);

        // проверка на подшучивание при входе в радиус
        foreach (var player in Player.GetAllPlayers())
        {
            var bossName = Localization.instance.Localize(__instance.m_name);
            var playerName = player.GetPlayerName();
            var key = (bossName, playerName);

            bool inRange = Vector3.Distance(player.transform.position, __instance.transform.position) <= radiusConfig.Value;

            if (inRange)
            {
                if (!playersInRange.Contains(key))
                {
                    playersInRange.Add(key);

                    if (bossKillStats.TryGetValue(key, out int deaths) && deaths >= tauntDeathThreshold)
                    {
                        string msg;
                        if (Random.value < 0.1f) // 10% шанс
                        {
                            msg = rareTauntMessages[Random.Range(0, rareTauntMessages.Length)];
                        }
                        else
                        {
                            msg = tauntMessages[Random.Range(0, tauntMessages.Length)];
                        }
                        msg = msg.Replace("{player}", playerName);
                        SendMessageInChatNormal(__instance, new[] { msg }, "yellow");
                    }
                }
            }
            else
            {
                playersInRange.Remove(key);
            }
        }

        if (bossDataDict.TryGetValue(spawnPoint, out var data))
        {
            if (hasPlayerInRange)
            {
                data.LastPlayerSeenTime = currentTime;
                data.IsTimerRunning = false;
                bossDataDict[spawnPoint] = data;
            }
            else
            {
                if (!data.IsTimerRunning)
                {
                    data.LastPlayerSeenTime = currentTime;
                    data.IsTimerRunning = true;
                    bossDataDict[spawnPoint] = data;
                    SendMessageInChatNormal(__instance, lostMessages, "yellow");
                }
                else if (currentTime - data.LastPlayerSeenTime >= despawnDelayConfig.Value * 60f) DespawnBoss(__instance, spawnPoint);
            }
        }
        else if (!hasPlayerInRange)
        {
            // просто регистрируем босса, но не запускаем таймер
            bossDataDict[spawnPoint] = new BossData
            {
                LastPlayerSeenTime = currentTime,
                IsTimerRunning = false,   // <-- таймер пока не идёт
                BossRef = __instance
            };

            foreach (var player in Player.GetAllPlayers())
            {
                var bossName = Localization.instance.Localize(__instance.m_name);
                var playerName = player.GetPlayerName();
                var key = (bossName, playerName);

                if (bossKillStats.TryGetValue(key, out int deaths) && deaths >= tauntDeathThreshold)
                {
                    string msg = tauntMessages[Random.Range(0, tauntMessages.Length)].Replace("{player}", playerName);
                    SendMessageInChatNormal(__instance, new[] { msg }, "yellow");
                }
            }
        }

        CleanupDestroyedBosses();
    }

    private static bool ShouldProcess(Humanoid humanoid)
    {
        if (SceneManager.GetActiveScene().name != "main") return false;
        if (!ZNetScene.instance || !Player.m_localPlayer) return false;
        if (!humanoid || humanoid.IsDead()) return false;
        if (!humanoid.GetBaseAI()) return false;
        return humanoid.IsBoss();
    }

    private static void DespawnBoss(Humanoid __instance, Vector3 spawnPoint)
    {
        if (!__instance) return;

        SendMessageInChatShout(__instance, despawnMessages, "#FF0000");

        // Прямое удаление через ZNetScene
        if (ZNetScene.instance && __instance.gameObject)
        {
            if (__instance.m_nview && __instance.m_nview.IsValid()) __instance.m_nview.ClaimOwnership();
            ZNetScene.instance.Destroy(__instance.gameObject);
        }

        // Fallback через ZDO если объект все еще существует
        if (__instance != null && __instance.gameObject != null)
        {
         //   Debug.LogWarning($"Принудительное удаление босса {__instance.m_name} через ZDO");
            if (__instance.m_nview && __instance.m_nview.IsValid())
            {
                ZDO zdo = __instance.m_nview.GetZDO();
                if (zdo != null) ZDOMan.instance.DestroyZDO(zdo);
            }
        }

        bossesToRemove.Add(spawnPoint);
    }


    private static void SendMessageInChatShout(Humanoid boss, string[] messages, string m_color, string playerName = null)
    {
        if (!boss || !Chat.instance) return;

        string bossName = Localization.instance.Localize(boss.m_name);
        string randomMessage = messages[Random.Range(0, messages.Length)];

        string finalText = string.IsNullOrEmpty(playerName) ? $"<color={m_color}>{randomMessage}</color>" : $"<color={m_color}>{playerName}, {randomMessage}</color>";

        Chat.instance.m_hideTimer = 0f;
        Chat.instance.AddString(bossName, finalText, Talker.Type.Shout);
    }

    private static void SendMessageInChatNormal(Humanoid boss, string[] messages, string m_color, string playerName = null)
    {
        if (!boss || !Chat.instance) return;

        string bossName = Localization.instance.Localize(boss.m_name);
        string randomMessage = messages[Random.Range(0, messages.Length)];

        string finalText = string.IsNullOrEmpty(playerName) ? $"<color={m_color}>{randomMessage}</color>" : $"<color={m_color}>{playerName}, {randomMessage}</color>";

        Chat.instance.m_hideTimer = 0f;
        Chat.instance.AddString(bossName, finalText, Talker.Type.Normal);
    }

    private static void CleanupDestroyedBosses()
    {
        foreach (var spawnPoint in bossesToRemove) bossDataDict.Remove(spawnPoint);
        bossesToRemove.Clear();

        var invalidKeys = new List<Vector3>();
        foreach (var kvp in bossDataDict) if (!kvp.Value.BossRef) invalidKeys.Add(kvp.Key);
        foreach (var key in invalidKeys) bossDataDict.Remove(key);
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Destroy))]
    [HarmonyPrefix]
    public static void OnObjectDestroyed(GameObject go)
    {
        if (go.TryGetComponent<Humanoid>(out var humanoid) && humanoid.IsBoss())
        {
            var spawnPoint = humanoid.GetBaseAI()?.m_spawnPoint ?? Vector3.zero;
            if (spawnPoint != Vector3.zero) bossesToRemove.Add(spawnPoint);
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.OnDeath))]
    public static class BossKill_MessagePatch
    {
        private static void Prefix(Player __instance)
        {
            if (!Chat.instance || !__instance) return;

            foreach (var kvp in bossDataDict)
            {
                var boss = kvp.Value.BossRef;
                if (boss && boss.TryGetComponent<MonsterAI>(out var ai) && ai.m_targetCreature == __instance)
                {
                    string bossName = Localization.instance.Localize(boss.m_name);
                    string playerName = __instance.GetPlayerName();

                    // увеличиваем счётчик убийств
                    var key = (bossName, playerName);
                    if (bossKillStats.ContainsKey(key)) bossKillStats[key]++;
                    else bossKillStats[key] = 1;

                    // обычное сообщение при убийстве
                    SendMessageInChatShout(boss, killMessages, "#FF0000");

                    // если достигнут порог смертей — добавляем насмешки
                    if (bossKillStats[key] >= tauntDeathThreshold)
                    {
                        string msg = tauntMessages[Random.Range(0, tauntMessages.Length)].Replace("{player}", playerName);
                        SendMessageInChatNormal(boss, new[] { msg }, "yellow");
                    }
                    break;
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.Awake))]
    public static class Boss_LOD_DisablePatch
    {
        private static void Postfix(Humanoid __instance)
        {
            if (!__instance.IsBoss()) return;

            // ищем все LODGroup в дочерних объектах
            foreach (var lod in __instance.GetComponentsInChildren<LODGroup>(true))
            {
                lod.enabled = false; // отключаем компонент
            }
            var znv = __instance.GetComponent<ZNetView>();
            if (znv)
            {
                znv.m_distant = true;
                znv.m_type = ZDO.ObjectType.Prioritized;
            }
        }
    }
}
