namespace VBQOL.IndividualKeys
{
    [HarmonyPatch]
    public static class VB_IndividualBossKeys
    {
        [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.SetGlobalKey), new Type[] { typeof(string) })]
        class Patch_SetGlobalKey
        {
            static bool Prefix(string name)
            {
                if (VB_BossKeyUtils.IsBossKey(name))
                {
                    Debug.Log($"[IndividualKeys] Блокируем запись ключа босса в глобальную систему: {name}");
                    return false;
                }
                return true;
            }
        }
        
       [HarmonyPatch(typeof(RandEventSystem), nameof(RandEventSystem.HaveGlobalKeys))]
        private static class Patch_HaveGlobalKeys
        {
            private static bool Prefix(RandomEvent ev, List<RandEventSystem.PlayerEventData> players, ref bool __result)
            {
                foreach (string requiredGlobalKey in ev.m_requiredGlobalKeys)
                {
                    if (VB_BossKeyUtils.IsBossKey(requiredGlobalKey))
                    {
                        if (!VB_BossKeyUtils.AllPlayersHaveKey(requiredGlobalKey, Player.GetAllPlayers()))
                        {
                            __result = false;
                            return false;
                        }
                    }
                    else if (!ZoneSystem.instance.GetGlobalKey(requiredGlobalKey))
                    {
                        __result = false;
                        return false;
                    }
                }

                foreach (string notRequiredGlobalKey in ev.m_notRequiredGlobalKeys)
                {
                    if (VB_BossKeyUtils.IsBossKey(notRequiredGlobalKey))
                    {
                        foreach (Player player in Player.GetAllPlayers())
                        {
                            if (VB_BossKeyUtils.PlayerHasBossKey(player, notRequiredGlobalKey))
                            {
                                __result = false;
                                return false;
                            }
                        }
                    }
                    else if (ZoneSystem.instance.GetGlobalKey(notRequiredGlobalKey))
                    {
                        __result = false;
                        return false;
                    }
                }
                __result = true;
                return false;
            }
        }
        
        [HarmonyPatch(typeof(SpawnSystem), nameof(SpawnSystem.HaveGlobalKeys))]
        public static class Patch_SpawnSystem_HaveGlobalKeys_BlockNewbies
        {
            private static bool Prefix(SpawnSystem __instance, SpawnSystem.SpawnData ev, ref bool __result)
            {
                if (string.IsNullOrEmpty(ev.m_requiredGlobalKey))
                {
                    __result = true;
                    return false;
                }

                if (!VB_BossKeyUtils.IsBossKey(ev.m_requiredGlobalKey))
                {
                    __result = ZoneSystem.instance.GetGlobalKey(ev.m_requiredGlobalKey);
                    return false;
                }

                var nearPlayers = SpawnSystem.m_tempNearPlayers;
                if (nearPlayers == null || nearPlayers.Count == 0)
                {
                    __result = false;
                    return false;
                }

                __result = VB_BossKeyUtils.AllNearPlayersHaveKey(nearPlayers, ev.m_requiredGlobalKey);
                return false;
            }
        }
    }
}