namespace VBQOL;

[HarmonyPatch]
public class VB_FallDamage
{
    public static ConfigEntry<float> MaxFallDamage;
    public static ConfigEntry<string> ExcludedPrefabs;

    // Кэш для оптимизации
    private static HashSet<string> _excludedPrefabsCache;
    private static float _lastConfigUpdate;

    private const float VANILLA_MIN_FALL_HEIGHT = 4f;
    private const float VANILLA_DAMAGE_INTERVAL = 16f;

    public static void ForceUpdateCache()
    {
        _excludedPrefabsCache = null;
        _lastConfigUpdate = 0f;
        UpdateExcludedCache();
    }
    
    // Обновляем кэш при необходимости
    private static void UpdateExcludedCache()
    {
        string configValue = ExcludedPrefabs?.Value ?? "";
        float currentTime = Time.time;
        
        if (_excludedPrefabsCache == null || currentTime - _lastConfigUpdate > 2f)
        {
            _excludedPrefabsCache = new HashSet<string>();
            if (!string.IsNullOrEmpty(configValue))
            {
                foreach (string prefab in configValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string trimmed = prefab.Trim();
                    if (!string.IsNullOrEmpty(trimmed)) _excludedPrefabsCache.Add(trimmed);
                }
            }
            _lastConfigUpdate = currentTime;
        }
    }

    // Проверяем, исключен ли персонаж
    private static bool IsExcluded(Character character)
    {
        if (!character) return false;
        
        UpdateExcludedCache();
        
        // Проверяем по имени префаба (берем из ZNetView или из имени GameObject)
        string prefabName = "";
        ZNetView nview = character.m_nview;
        if (nview && nview.IsValid()) prefabName = nview.GetPrefabName();
        
        // Если не удалось получить через ZNetView - пробуем через имя GameObject
        if (string.IsNullOrEmpty(prefabName))
        {
            prefabName = character.name;
            // Убираем "(Clone)" если есть
            if (prefabName.EndsWith("(Clone)")) prefabName = prefabName.Replace("(Clone)", "").Trim();
        }
        
        // Проверяем полное совпадение
        if (_excludedPrefabsCache.Contains(prefabName)) return true;
            
        // Проверяем частичное совпадение (если префаб содержит ключевое слово)
    /*    foreach (string excluded in _excludedPrefabsCache)
        {
            if (prefabName.Contains(excluded))
                return true;
        }*/
        
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Character), nameof(Character.UpdateGroundContact))]
    public static bool Prefix(Character __instance)
    {
        if (__instance.IsDead()) return true;
        
        // === НОВАЯ ПРОВЕРКА ===
        // Если персонаж в черном списке - пропускаем
        if (IsExcluded(__instance)) return true;
        
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