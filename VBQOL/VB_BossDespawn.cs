using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace VBQOL;

[HarmonyPatch]
internal static class VB_BossDespawn
{
    private static readonly Dictionary<Vector3, BossData> bossDataDict = new();
    private static readonly HashSet<Vector3> bossesToRemove = new();

    internal static ConfigEntry<float> radiusConfig;
    internal static ConfigEntry<float> despawnDelayConfig;
    internal static string despawntext = "ушёл";
    internal static string losttext = "собирается уйти";
    
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

        // Обработка существующих боссов
        if (bossDataDict.TryGetValue(spawnPoint, out var data))
        {
         //   Debug.LogWarning($"Обработка босса: hasPlayerInRange={hasPlayerInRange}, IsTimerRunning={data.IsTimerRunning}");

            if (hasPlayerInRange)
            {
                // Игрок в радиусе - сбрасываем таймер
             //   Debug.LogWarning("Игрок в радиусе - сбрасываем таймер");
                data.LastPlayerSeenTime = currentTime;
                data.IsTimerRunning = false;
                bossDataDict[spawnPoint] = data;
            }
            else
            {
                // Игрок НЕ в радиусе
                if (!data.IsTimerRunning)
                {
                    // Первый раз когда игрок покинул радиус - запускаем таймер
                //    Debug.LogWarning("Игрок покинул радиус - запускаем таймер");
                    data.LastPlayerSeenTime = currentTime;
                    data.IsTimerRunning = true;
                    bossDataDict[spawnPoint] = data;

                    // Показываем сообщение с проверкой
                    if (Player.m_localPlayer && __instance)
                    {
                        string message = "<color=yellow>" + __instance.m_name + " " + losttext + "</color>";
                        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, message);
                      //  Debug.LogWarning($"Сообщение отправлено: {message}");
                    }
                }
                else if (currentTime - data.LastPlayerSeenTime >= despawnDelayConfig.Value * 60f)
                {
                    // Время вышло - деспавним
                  //  Debug.LogWarning("Время вышло - деспавним");
                    DespawnBoss(__instance, spawnPoint);
                }
            }
        }
        else if (!hasPlayerInRange)
        {
            // Первое обнаружение босса без игрока в радиусе
         //   Debug.LogWarning("Добавляем нового босса в отслеживание и показываем сообщение");
            bossDataDict[spawnPoint] = new BossData
            {
                LastPlayerSeenTime = currentTime,
                IsTimerRunning = true,
                BossRef = __instance
            };

            // Показываем сообщение при первом обнаружении
            if (Player.m_localPlayer && __instance)
            {
                string message = "<color=yellow>" + __instance.m_name + " " + losttext + "</color>";
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, message);
          //      Debug.LogWarning($"Сообщение отправлено при добавлении: {message}");
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

 /*   private static bool ShouldStartTracking(Humanoid boss, bool hasPlayerInRange)
    {
        // Начинаем отслеживание, если босс и игрок не в радиусе
        return !hasPlayerInRange;
    }

    private static bool ShouldDespawnBoss(Humanoid boss, bool hasPlayerInRange)
    {
        // Проверяем, должен ли босс деспавниться
        return !hasPlayerInRange;
    }*/

    private static void DespawnBoss(Humanoid boss, Vector3 spawnPoint)
    {
        if (!boss) return;
     
        ZNetScene.instance.Destroy(boss.gameObject);
        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "<color=yellow>" + boss.m_name + " " + despawntext + "</color>");
        
      //  Debug.LogWarning("деспавн окончен");
        bossesToRemove.Add(spawnPoint);
    }

    private static void CleanupDestroyedBosses()
    {
        // Быстрая очистка уничтоженных боссов
        foreach (var spawnPoint in bossesToRemove) bossDataDict.Remove(spawnPoint);
        bossesToRemove.Clear();

        // Дополнительная очистка невалидных ссылок
        var invalidKeys = new List<Vector3>();
        foreach (var kvp in bossDataDict) if (!kvp.Value.BossRef) invalidKeys.Add(kvp.Key);

        foreach (var key in invalidKeys) bossDataDict.Remove(key);
    }

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Destroy))]
    [HarmonyPrefix]
    public static void OnObjectDestroyed(GameObject go)
    {
        // Автоматически очищаем данные при уничтожении объекта
        if (go.TryGetComponent<Humanoid>(out var humanoid) && humanoid.IsBoss())
        {
            var spawnPoint = humanoid.GetBaseAI()?.m_spawnPoint ?? Vector3.zero;
            if (spawnPoint != Vector3.zero) bossesToRemove.Add(spawnPoint);
        }
    }
}