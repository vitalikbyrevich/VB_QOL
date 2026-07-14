namespace VBQOL;

[HarmonyPatch]
public class VB_FallDamage
{
    public static ConfigEntry<float> MaxFallDamage;

    private const float VANILLA_MIN_FALL_HEIGHT = 4f;
    private const float VANILLA_DAMAGE_INTERVAL = 16f;

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Character), nameof(Character.UpdateGroundContact))]
    public static bool Prefix(Character __instance)
    {
        if (__instance.IsDead()) return true;

        var maxAirAltitudeField = AccessTools.Field(typeof(Character), "m_maxAirAltitude");
        float maxAirAltitude = (float)maxAirAltitudeField.GetValue(__instance);
        float fallHeight = Mathf.Max(0f, maxAirAltitude - __instance.transform.position.y);

        var groundContactField = AccessTools.Field(typeof(Character), "m_groundContact");
        bool groundContact = (bool)groundContactField.GetValue(__instance);

        if (!groundContact || fallHeight <= VANILLA_MIN_FALL_HEIGHT) return true;

        float progress = (fallHeight - VANILLA_MIN_FALL_HEIGHT) / VANILLA_DAMAGE_INTERVAL;
        float damage = progress * 100f;

        __instance.GetSEMan().ModifyFallDamage(damage, ref damage);

        float maxDamage = MaxFallDamage.Value;
        if (maxDamage > 0f && damage > maxDamage) damage = maxDamage;

        if (damage <= 0f) return true;

        maxAirAltitudeField.SetValue(__instance, __instance.transform.position.y);

        var groundPointField = AccessTools.Field(typeof(Character), "m_groundContactPoint");
        var groundNormalField = AccessTools.Field(typeof(Character), "m_groundContactNormal");
        Vector3 point = (Vector3)groundPointField.GetValue(__instance);
        Vector3 dir = (Vector3)groundNormalField.GetValue(__instance);

        HitData hitData = new HitData();
        hitData.m_damage.m_damage = damage;
        hitData.m_point = point;
        hitData.m_dir = dir;
        hitData.m_hitType = HitData.HitType.Fall;

        __instance.Damage(hitData);

        return false;
    }
}