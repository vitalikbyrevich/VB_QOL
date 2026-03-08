namespace VBQOL
{
    [HarmonyPatch]
    public static class NoIntroNoValkyrie
    {
        public static ConfigEntry<bool> m_enableNINV;
    
        [HarmonyPatch(typeof(Player), nameof(Player.OnSpawned))]
        [HarmonyPostfix]
        public static void OnPlayerSpawned(Player __instance)
        {
            if (!m_enableNINV.Value) return;
            if (__instance != Player.m_localPlayer) return;
        
            if (Game.instance.m_queuedIntro)
            {
                Game.instance.m_queuedIntro = false;
                Game.instance.m_inIntro = false;
            }
        }
    
        [HarmonyPatch(typeof(Game), nameof(Game.UpdateRespawn))]
        public static class UpdateRespawnPatch
        {
            private static bool _introSkipped = false;
    
            [HarmonyPostfix]
            public static void Postfix(Game __instance)
            {
                if (!m_enableNINV.Value || _introSkipped) return;
        
                if (__instance.m_firstSpawn) VBQOL.self.StartCoroutine(DelayedIntroSkip(__instance));
            }
    
            private static IEnumerator DelayedIntroSkip(Game game)
            {
                yield return new WaitForSeconds(2f);
        
                if (game.m_inIntro)
                {
                    game.m_queuedIntro = false;
                    game.m_inIntro = false;
                    _introSkipped = true;
                }
            }
        }
    }
}