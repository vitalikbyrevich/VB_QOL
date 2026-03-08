using UnityEngine.EventSystems;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VBQOL;

[Serializable]
public class CategoryData
{
    public List<string> categories { get; set; } = new List<string>();

    public CategoryData()
    {
        string[] names = Enum.GetNames(typeof(ItemDrop.ItemData.ItemType));
        foreach (string name in names)
        {
            categories.Add(name + ":" + name);
        }
    }
}

public class VB_CraftingFilter
{
    private static Dictionary<string, List<ItemDrop.ItemData.ItemType>> categoryDict = new Dictionary<string, List<ItemDrop.ItemData.ItemType>>();
    private static List<string> categoryNames = new List<string>();
    private static List<GameObject> dropDownList = new List<GameObject>();
    private static int lastCategoryIndex = 0;
    private static bool isShowing = false;
    private static int tabCraftPressed = 0;
    private Vector3 lastMousePos;
    
    public static ConfigEntry<bool> modEnabled;
    public static VB_CraftingFilter Instance;

    public static void LoadCategories()
    {
        try
        {
            CategoryData categoryData = null;

            // Загружаем из embedded resource
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = assembly.GetManifestResourceNames()
                    .FirstOrDefault(name => name.EndsWith("categories.yaml") || name.EndsWith("categories.yml"));

                if (resourceName != null)
                {
                    Debug.Log($"Found embedded resource: {resourceName}");
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string yaml = reader.ReadToEnd();

                        if (!string.IsNullOrWhiteSpace(yaml))
                        {
                            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
                            categoryData = deserializer.Deserialize<CategoryData>(yaml);
                            Debug.Log("Loaded categories from embedded resource");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Embedded categories.yaml not found, using defaults");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load embedded categories: {e.Message}");
            }

            // Если не удалось загрузить из ресурса, создаем дефолтные
            if (categoryData?.categories == null || categoryData.categories.Count == 0)
            {
                Debug.Log("Creating default categories");
                categoryData = new CategoryData();
            }

            Debug.Log($"Loaded {categoryData.categories.Count} categories");

            categoryDict.Clear();
            categoryNames.Clear();

            foreach (string category in categoryData.categories)
            {
                if (string.IsNullOrEmpty(category) || !category.Contains(":"))
                {
                    Debug.LogWarning($"Skipping invalid category: {category}");
                    continue;
                }

                string[] parts = category.Split(':');
                if (parts.Length < 2)
                {
                    Debug.LogWarning($"Skipping malformed category: {category}");
                    continue;
                }

                string categoryName = parts[0].Trim();
                string[] types = parts[1].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if (string.IsNullOrEmpty(categoryName) || types.Length == 0)
                {
                    Debug.LogWarning($"Skipping category with no types: {category}");
                    continue;
                }

                categoryNames.Add(categoryName);
                categoryDict[categoryName] = new List<ItemDrop.ItemData.ItemType>();

                foreach (string type in types)
                {
                    string trimmedType = type.Trim();
                    if (Enum.TryParse<ItemDrop.ItemData.ItemType>(trimmedType, out var itemType))
                    {
                        categoryDict[categoryName].Add(itemType);
                    }
                    else
                    {
                        Debug.LogWarning($"Invalid item type '{trimmedType}' in category '{categoryName}'");
                    }
                }

                if (categoryDict[categoryName].Count == 0)
                {
                    Debug.LogWarning($"Category '{categoryName}' has no valid item types, removing");
                    categoryDict.Remove(categoryName);
                    categoryNames.RemoveAt(categoryNames.Count - 1);
                }
            }

            // Добавляем категорию "Все" если её нет
            if (!categoryDict.ContainsKey("Все") && !categoryDict.ContainsKey("All"))
            {
                Debug.Log("Adding default 'All' category");
                string allName = "Все";
                categoryNames.Insert(0, allName);
                categoryDict[allName] = new List<ItemDrop.ItemData.ItemType> { ItemDrop.ItemData.ItemType.None };
            }

            // Сортируем, категория "Все" всегда первая
            categoryNames.Sort((a, b) =>
            {
                bool aIsAll = categoryDict[a].Contains(ItemDrop.ItemData.ItemType.None);
                bool bIsAll = categoryDict[b].Contains(ItemDrop.ItemData.ItemType.None);

                if (aIsAll && !bIsAll) return -1;
                if (!aIsAll && bIsAll) return 1;
                return string.Compare(a, b, StringComparison.Ordinal);
            });

            lastCategoryIndex = 0;
            Debug.Log($"Successfully loaded {categoryNames.Count} categories");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading categories: {e.Message}");
            CreateDefaultCategories();
        }
    }

    private static void CreateDefaultCategories()
    {
        categoryDict = new Dictionary<string, List<ItemDrop.ItemData.ItemType>>();
        categoryNames = new List<string>();

        categoryNames.Add("Все");
        categoryDict["Все"] = new List<ItemDrop.ItemData.ItemType> { ItemDrop.ItemData.ItemType.None };

        var defaultCategories = new Dictionary<string, ItemDrop.ItemData.ItemType[]>
        {
            { "Шлем", new[] { ItemDrop.ItemData.ItemType.Helmet } },
            { "Торс", new[] { ItemDrop.ItemData.ItemType.Chest } },
            { "Ноги", new[] { ItemDrop.ItemData.ItemType.Legs } },
            { "Плащ", new[] { ItemDrop.ItemData.ItemType.Shoulder } },
            { "Оружие", new[] { ItemDrop.ItemData.ItemType.OneHandedWeapon, ItemDrop.ItemData.ItemType.TwoHandedWeapon } },
            { "Лук", new[] { ItemDrop.ItemData.ItemType.Bow } },
            { "Щит", new[] { ItemDrop.ItemData.ItemType.Shield } }
        };

        foreach (var cat in defaultCategories)
        {
            categoryNames.Add(cat.Key);
            categoryDict[cat.Key] = new List<ItemDrop.ItemData.ItemType>(cat.Value);
        }

        lastCategoryIndex = 0;
        Debug.Log($"Created {categoryNames.Count} default categories");
    }

    public void OnInventoryGuiReady()
    {
        VBQOL.self.StartCoroutine(FilterUpdateCoroutine());
    }

    private IEnumerator FilterUpdateCoroutine()
    {
        while (true)
        {
            try
            {
                if (modEnabled.Value) UpdateCraftingFilter();
            }
            catch (Exception e)
            {
                Debug.LogError($"CraftingFilter update error: {e.Message}");
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void UpdateCraftingFilter()
    {
        if (!modEnabled.Value || !Player.m_localPlayer || !InventoryGui.IsVisible())
        {
            if (lastCategoryIndex != 0) lastCategoryIndex = 0;
            UpdateDropDown(false);
            return;
        }

        if (!InventoryGui.instance.InCraftTab())
        {
            UpdateDropDown(false);
            return;
        }

        bool mouseOverCraftButton = false;
        Vector3 mousePosition = Input.mousePosition;

        if (lastMousePos == Vector3.zero) lastMousePos = mousePosition;

        PointerEventData eventData = new PointerEventData(EventSystem.current) { position = lastMousePos };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.layer != LayerMask.NameToLayer("UI")) continue;

            if (result.gameObject.name == "Craft")
            {
                mouseOverCraftButton = true;
                if (tabCraftPressed == 0) UpdateDropDown(true);
            }
            else if (dropDownList.Contains(result.gameObject)) mouseOverCraftButton = true;
        }

        if (!mouseOverCraftButton)
        {
            if (tabCraftPressed > 0) tabCraftPressed--;
            UpdateDropDown(false);
        }

        lastMousePos = Input.mousePosition;
    }

    private static void SwitchFilter(int idx)
    {
        if (categoryNames?.Count == 0) return;
        idx = Mathf.Clamp(idx, 0, categoryNames.Count - 1);
        
        if (lastCategoryIndex == idx) return;
        
        lastCategoryIndex = idx;
        UpdateDropDown(false);
        ApplyFilter();
    }

    private static void ApplyFilter()
    {
        if (!InventoryGui.instance || !Player.m_localPlayer) return;
        if (categoryNames?.Count == 0 || lastCategoryIndex < 0 || lastCategoryIndex >= categoryNames.Count)
        {
            Debug.LogWarning("CraftingFilter: Cannot apply filter - invalid state");
            return;
        }

        List<Recipe> available = new List<Recipe>();
        Player.m_localPlayer.GetAvailableRecipes(ref available);

        Traverse.Create(InventoryGui.instance).Method("UpdateRecipeList", available).GetValue();
        Traverse.Create(InventoryGui.instance).Method("SetRecipe", 0, true).GetValue();

        string currentCategory = categoryNames[lastCategoryIndex];
        bool isAllCategory = categoryDict[currentCategory].Contains(ItemDrop.ItemData.ItemType.None);

        InventoryGui.instance.m_tabCraft.gameObject.GetComponentInChildren<TMP_Text>().text =
            Localization.instance.Localize("$inventory_craftbutton") + (isAllCategory ? "" : $"\n{currentCategory}");
    }

    private static void UpdateDropDown(bool show)
    {
        if (dropDownList?.Count == 0) return;
        if (show == isShowing) return;

        if (show)
        {
            List<Recipe> available = new List<Recipe>();
            Player.m_localPlayer.GetAvailableRecipes(ref available);

            float scaleFactor = Hud.instance.GetComponent<CanvasScaler>().scaleFactor;
            Vector2 craftButtonPos = InventoryGui.instance.m_tabCraft.gameObject.transform.GetComponent<RectTransform>().position;
            float buttonHeight = InventoryGui.instance.m_tabCraft.gameObject.transform.GetComponent<RectTransform>().rect.height * scaleFactor;

            int visibleCount = 0;
            for (int i = 0; i < categoryNames.Count; i++)
            {
                int recipeCount = available.FindAll(r => 
                {
                    if (r?.m_item?.m_itemData?.m_shared == null) return false;
                    return categoryDict[categoryNames[i]].Contains(r.m_item.m_itemData.m_shared.m_itemType);
                }).Count;
                
                bool showCategory = recipeCount > 0 || categoryDict[categoryNames[i]].Contains(ItemDrop.ItemData.ItemType.None);

                dropDownList[i].SetActive(showCategory);

                if (showCategory)
                {
                    dropDownList[i].GetComponent<RectTransform>().position = craftButtonPos - new Vector2(0f, buttonHeight * (visibleCount++ + 1));
                    dropDownList[i].GetComponentInChildren<TMP_Text>().text = categoryNames[i] + (recipeCount == 0 ? "" : $" ({recipeCount})");
                }
            }
        }
        else
        {
            foreach (var button in dropDownList)
            {
                button.SetActive(false);
            }
        }

        isShowing = show;
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
    public static class InventoryGui_Awake_Patch
    {
        public static void Postfix(InventoryGui __instance)
        {
            if (!modEnabled.Value) return;

            dropDownList.Clear();

            for (int i = 0; i < categoryNames.Count; i++)
            {
                int idx = i;
                GameObject button = Object.Instantiate(__instance.m_tabCraft.gameObject, __instance.m_tabCraft.gameObject.transform.parent.parent, true);
                button.name = categoryNames[i];
                button.GetComponent<Button>().interactable = true;
                button.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                button.GetComponent<Button>().onClick.AddListener(() => SwitchFilter(idx));
                button.SetActive(false);
                dropDownList.Add(button);
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.UpdateRecipeList))]
    public static class UpdateRecipeList_Patch
    {
        public static void Prefix(ref List<Recipe> recipes)
        {
            try
            {
                if (!modEnabled.Value) return;
                if (categoryNames?.Count == 0) return;

                if (lastCategoryIndex < 0 || lastCategoryIndex >= categoryNames.Count)
                {
                    lastCategoryIndex = 0;
                }

                string currentCategory = categoryNames[lastCategoryIndex];

                if (!categoryDict.ContainsKey(currentCategory)) return;

                if (InventoryGui.instance.InCraftTab() && !categoryDict[currentCategory].Contains(ItemDrop.ItemData.ItemType.None))
                {
                    recipes = recipes.FindAll(r =>
                    {
                        if (r?.m_item?.m_itemData?.m_shared == null) return false;
                        return categoryDict[currentCategory].Contains(r.m_item.m_itemData.m_shared.m_itemType);
                    });
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"CraftingFilter error in UpdateRecipeList: {e.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Hide))]
    public static class Hide_Patch
    {
        public static void Prefix()
        {
            if (modEnabled.Value)
            {
                lastCategoryIndex = 0;
                UpdateDropDown(false);
            }
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.OnTabCraftPressed))]
    public static class OnTabCraftPressed_Patch
    {
        public static void Prefix()
        {
            if (modEnabled.Value) tabCraftPressed = 2;
        }
    }

    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Show))]
    public static class InventoryGui_Show_Patch
    {
        public static void Postfix()
        {
            if (modEnabled.Value) lastCategoryIndex = 0;
        }
    }
}