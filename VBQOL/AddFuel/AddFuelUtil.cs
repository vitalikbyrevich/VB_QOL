namespace VBQOL.AddFuel;

public static class AddFuelUtil
{
    public static ConfigEntry<KeyCode> AFModifierKeyConfig;
    public static KeyCode AFModifierKeyUseConfig = KeyCode.E;
    public static ConfigEntry<string> AFTextConfig;
    public static ConfigEntry<bool> AFEnable;
    
    private static Dictionary<Type, MethodInfo> _cachedMethods = new();
    private static Dictionary<string, List<ItemDrop.ItemData>> _inventoryCache = new();
    private static float _lastCacheTime;
    private const float CACHE_DURATION = 2f;

    public static ItemDrop.ItemData FindCookableItem(Smelter smelter, Inventory inventory)
    {
        if (smelter?.m_conversion == null) return null;
        
        var allowedNames = smelter.m_conversion.Select(conv => conv.m_from.m_itemData.m_shared.m_name).ToHashSet();
        
        UpdateInventoryCache(inventory);
        
        return _inventoryCache.Where(kvp => allowedNames.Contains(kvp.Key)).SelectMany(kvp => kvp.Value).FirstOrDefault();
    }

    public static int CalculateStackToAdd(bool isSingleMode, int availableStack, int spaceLeft) => isSingleMode ? 1 : Math.Min(availableStack, spaceLeft);

    public static float GetFuelSafe(Smelter smelter)
    {
        try
        {
            return GetPrivateMethod<float>(smelter, "GetFuel");
        }
        catch
        {
            return smelter.m_nview?.GetZDO()?.GetFloat("fuel") ?? 0f;
        }
    }

    public static int GetQueueSizeSafe(Smelter smelter)
    {
        try
        {
            return GetPrivateMethod<int>(smelter, "GetQueueSize");
        }
        catch
        {
            return smelter.m_nview?.GetZDO()?.GetInt("queued") ?? 0;
        }
    }

    private static T GetPrivateMethod<T>(object instance, string methodName)
    {
        var type = instance.GetType();
 //       var key = $"{type.FullName}.{methodName}";
        
        if (!_cachedMethods.TryGetValue(type, out var method))
        {
            method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            _cachedMethods[type] = method;
        }
        
        return method != null ? (T)method.Invoke(instance, null) : default(T);
    }

    private static void UpdateInventoryCache(Inventory inventory)
    {
        if (Time.time - _lastCacheTime <= CACHE_DURATION) return;
        
        _inventoryCache.Clear();
        foreach (var item in inventory.m_inventory)
        {
            if (!_inventoryCache.ContainsKey(item.m_shared.m_name)) _inventoryCache[item.m_shared.m_name] = new List<ItemDrop.ItemData>();
            _inventoryCache[item.m_shared.m_name].Add(item);
        }
        _lastCacheTime = Time.time;
    }

    public static void AddFuelBulk(Smelter smelter, int count)
    {
        if (!smelter.m_nview.IsOwner() || count <= 0) return;
        
        var currentFuel = GetFuelSafe(smelter);
        var newFuel = Mathf.Min(currentFuel + count, smelter.m_maxFuel);
        
        smelter.m_nview.GetZDO().Set("fuel", newFuel);
        smelter.m_fuelAddedEffects.Create(smelter.transform.position, smelter.transform.rotation);
    }

    public static void AddOreBulk(Smelter smelter, string oreName, int count)
    {
        if (!smelter.m_nview.IsOwner() || count <= 0) return;
        
        var currentQueue = GetQueueSizeSafe(smelter);
        for (int i = 0; i < count; i++) smelter.m_nview.GetZDO().Set($"item{currentQueue + i}", oreName);
        smelter.m_nview.GetZDO().Set("queued", currentQueue + count);
        smelter.m_oreAddedEffects.Create(smelter.transform.position, smelter.transform.rotation);
    }
}