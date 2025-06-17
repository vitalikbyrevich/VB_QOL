using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace VBQOL;

[HarmonyPatch]
internal static class VB_BossDespawn
{
    private static readonly List<Vector3> bossesOkayToDestroyAndWaiting_spawnPositions = new();
    private static readonly List<Vector3> bossesReadyToDestroy_spawnPositions = new();

    internal static ConfigEntry<float> radiusConfig;
    internal static ConfigEntry<float> despawnDelayConfig;
    
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.CustomFixedUpdate))] [HarmonyPostfix]
    public static void CheckAndCreateTimerIfNeeded(Humanoid __instance)
    {
        if (SceneManager.GetActiveScene().name != "main" || !ZNetScene.instance || !Player.m_localPlayer || !__instance) return;

        var baseAI = __instance.GetBaseAI();
        if (!baseAI) return;
        var spawnPoint = baseAI.m_spawnPoint;

        if (!IsOkayToDestroy(__instance))
        {
            if (bossesOkayToDestroyAndWaiting_spawnPositions.Contains(spawnPoint)) bossesOkayToDestroyAndWaiting_spawnPositions.Remove(spawnPoint);
            return;
        }

        if (bossesReadyToDestroy_spawnPositions.Contains(spawnPoint))
        {
            var localizedBossName = __instance.m_name;
            Debug.LogWarning($"Destroing {localizedBossName}...");
            ZNetScene.instance.Destroy(__instance.gameObject);
            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, $"Destroing {localizedBossName}...");

            bossesReadyToDestroy_spawnPositions.Remove(spawnPoint);
            bossesOkayToDestroyAndWaiting_spawnPositions.Remove(spawnPoint);
            return;
        }

        if (bossesOkayToDestroyAndWaiting_spawnPositions.Contains(spawnPoint)) return;

        bossesOkayToDestroyAndWaiting_spawnPositions.Add(spawnPoint);
        var milliseconds = TimeSpan.FromMinutes(despawnDelayConfig.Value).TotalMilliseconds;
        Debug.LogWarning($"Starting timers for {milliseconds} milliseconds");
        if (milliseconds > 0)
        {
            var timer = new System.Timers.Timer(milliseconds);
            timer.Elapsed += (_, _) => OnTimerElapsed(timer, spawnPoint);
            timer.Start();
        }
        else OnTimerElapsed(null, spawnPoint);
    }

    private static void OnTimerElapsed(System.Timers.Timer timer, Vector3 spawnPoint)
    {
        Debug.LogWarning("Timer elapsed");
        bossesReadyToDestroy_spawnPositions.Add(spawnPoint);
        if (timer != null)
        {
            timer.Stop();
            timer.Dispose();
        }
    }

    private static bool IsOkayToDestroy(Humanoid humanoid)
    {
        if (!humanoid) return false;
        var prefabName = humanoid.name;
        var havePlayerInRange = Player.IsPlayerInRange(humanoid.transform.position, radiusConfig.Value);
        var isOkay = humanoid.IsBoss() && !havePlayerInRange;

        return isOkay;
    }
}