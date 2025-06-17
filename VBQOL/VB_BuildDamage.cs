namespace VBQOL
{
    [HarmonyPatch]
    public class VB_BuildDamage
    {
        public static ConfigEntry<bool> enableModBDConfig;
        public static ConfigEntry<float> creatorDamageMultConfig;
        public static ConfigEntry<float> nonCreatorDamageMultConfig;
        public static ConfigEntry<float> uncreatedDamageMultConfig;
        public static ConfigEntry<float> naturalDamageMultConfig;

        public static class RPC_HealthChanged_Patch
        {
            public static bool Prefix(long peer, Piece ___m_piece, WearNTear __instance) => ShouldApplyDamage(__instance, ___m_piece, peer);
        }

        [HarmonyPatch(typeof(WearNTear), "RPC_Damage")]
        public static class RPC_Damage_Patch
        {
            public static void Prefix(ref HitData hit, Piece ___m_piece, WearNTear __instance)
            {
                if (!enableModBDConfig.Value || !IsInsidePrivateArea(__instance)) return;

                float mult = CalculateDamageMultiplier(hit, ___m_piece);
                MultiplyDamage(ref hit, mult);
            }
        }

        [HarmonyPatch(typeof(WearNTear), "ApplyDamage")]
        public static class ApplyDamage_Patch
        {
            public static void Prefix(ref float damage, WearNTear __instance)
            {
                if (!enableModBDConfig.Value || !IsInsidePrivateArea(__instance) || Environment.StackTrace.Contains("RPC_Damage")) return;
                damage *= naturalDamageMultConfig.Value;
            }
        }

        private static bool IsInsidePrivateArea(WearNTear instance)
        {
            foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
            {
                if (privateArea.IsEnabled() && privateArea.IsInside(instance.transform.position, 0)) return true;
            }
            return false;
        }

        private static bool ShouldApplyDamage(WearNTear instance, Piece piece, long peer)
        {
            if (!piece) return true;

            foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
            {
                if (privateArea.IsEnabled() && privateArea.IsInside(instance.transform.position, 0))
                {
                    if (uncreatedDamageMultConfig.Value == 0 && piece.GetCreator() == 0) return false;
                    if (nonCreatorDamageMultConfig.Value == 0 && (piece.GetCreator() != 0 && peer != piece.GetCreator())) return false;
                    if (creatorDamageMultConfig.Value == 0 && (piece.GetCreator() != 0 && peer == piece.GetCreator())) return false;
                }
            }
            return true;
        }

        private static float CalculateDamageMultiplier(HitData hit, Piece piece)
        {
            if (hit.m_attacker.IsNone()) return naturalDamageMultConfig.Value;

            if (piece?.GetCreator() == 0) return uncreatedDamageMultConfig.Value;
            if (hit.m_attacker == Player.m_localPlayer.GetZDOID() && piece && piece.IsCreator()) return creatorDamageMultConfig.Value;

            return nonCreatorDamageMultConfig.Value;
        }

        private static void MultiplyDamage(ref HitData hit, float value)
        {
            value = Math.Max(0, value);
            hit.m_damage.m_damage *= value;
            hit.m_damage.m_blunt *= value;
            hit.m_damage.m_slash *= value;
            hit.m_damage.m_pierce *= value;
            hit.m_damage.m_chop *= value;
            hit.m_damage.m_pickaxe *= value;
            hit.m_damage.m_fire *= value;
            hit.m_damage.m_frost *= value;
            hit.m_damage.m_lightning *= value;
            hit.m_damage.m_poison *= value;
            hit.m_damage.m_spirit *= value;
        }
    }
}

/*namespace VBQOL
{
    [HarmonyPatch]
    public class VB_BuildDamage
    {
        public static ConfigEntry<bool> enableModBDConfig;
        public static ConfigEntry<float> creatorDamageMultConfig;
        public static ConfigEntry<float> nonCreatorDamageMultConfig;
        public static ConfigEntry<float> uncreatedDamageMultConfig;
        public static ConfigEntry<float> naturalDamageMultConfig;
        
        public static class RPC_HealthChanged_Patch
        {
            public static bool Prefix(long peer, Piece ___m_piece, WearNTear __instance)
            {
                foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
                {
                    if (privateArea.IsEnabled() && privateArea.IsInside(__instance.transform.position, 0))
                    {
                        if (___m_piece is null) return true;
                        if (uncreatedDamageMultConfig.Value == 0 && ___m_piece.GetCreator() == 0) return false;
                        if (nonCreatorDamageMultConfig.Value == 0 && (___m_piece.GetCreator() != 0 && peer != ___m_piece.GetCreator())) return false;
                        if (creatorDamageMultConfig.Value == 0 && (___m_piece.GetCreator() != 0 && peer == ___m_piece.GetCreator())) return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(WearNTear), "RPC_Damage")]
        public static class RPC_Damage_Patch
        {
            public static void Prefix(ref HitData hit, Piece ___m_piece, WearNTear __instance)
            {
                foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
                {
                    if (privateArea.IsEnabled() && privateArea.IsInside(__instance.transform.position, 0))
                    {
                      if (!enableModBDConfig.Value) return;
                
                      float mult;
                      if (!hit.m_attacker.IsNone())
                      {
                          if (___m_piece?.GetCreator() == 0) mult = uncreatedDamageMultConfig.Value;
                          else if (hit.m_attacker == Player.m_localPlayer.GetZDOID() && ___m_piece && ___m_piece.IsCreator()) mult = creatorDamageMultConfig.Value;
                          else mult = nonCreatorDamageMultConfig.Value;
                      }
                      else mult = naturalDamageMultConfig.Value;

                      MultiplyDamage(ref hit, mult);
                    }
                }
            }

            private static void MultiplyDamage(ref HitData hit, float value)
            {
                value = Math.Max(0, value);
                hit.m_damage.m_damage *= value;
                hit.m_damage.m_blunt *= value;
                hit.m_damage.m_slash *= value;
                hit.m_damage.m_pierce *= value;
                hit.m_damage.m_chop *= value;
                hit.m_damage.m_pickaxe *= value;
                hit.m_damage.m_fire *= value;
                hit.m_damage.m_frost *= value;
                hit.m_damage.m_lightning *= value;
                hit.m_damage.m_poison *= value;
                hit.m_damage.m_spirit *= value;
            }
        }

        [HarmonyPatch(typeof(WearNTear), "ApplyDamage")]
        public static class ApplyDamage_Patch
        {
            public static void Prefix(ref float damage, WearNTear __instance)
            {
                foreach (PrivateArea privateArea in PrivateArea.m_allAreas)
                {
                    if (privateArea.IsEnabled() && privateArea.IsInside(__instance.transform.position, 0))
                    {
                        if (!enableModBDConfig.Value || Environment.StackTrace.Contains("RPC_Damage")) return;
                        damage *= naturalDamageMultConfig.Value;
                    }
                }
            }
        }
    }
}*/