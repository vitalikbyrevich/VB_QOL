namespace VBQOL
{
	public static class VB_CustomSlotManager
	{
		private static readonly Dictionary<Humanoid, Dictionary<string, ItemDrop.ItemData>> customSlotItemData = new Dictionary<Humanoid, Dictionary<string, ItemDrop.ItemData>>();
		private static readonly List<Humanoid> toRemove = new List<Humanoid>();
		private static readonly object lockObject = new object();

		public static bool EnableDebugLogging { get; set; } = false;

		private static void LogDebug(string message)
		{
			if (EnableDebugLogging) Debug.Log($"[CustomSlotManager] {message}");
		}

		public static void Register(Humanoid humanoid)
		{
			lock (lockObject)
			{
				customSlotItemData[humanoid] = new Dictionary<string, ItemDrop.ItemData>();
				LogDebug($"Registered humanoid: {humanoid}");
			}
		}

		public static void Unregister(Humanoid humanoid)
		{
			lock (lockObject)
			{
				customSlotItemData.Remove(humanoid);
				LogDebug($"Unregistered humanoid: {humanoid}");
			}
		}

		public static void CleanupDestroyed()
		{
			lock (lockObject)
			{
				toRemove.Clear();
				foreach (var humanoid in customSlotItemData.Keys) if (!humanoid) toRemove.Add(humanoid);

				foreach (var humanoid in toRemove)
				{
					customSlotItemData.Remove(humanoid);
					LogDebug($"Cleaned up destroyed humanoid");
				}
			}
		}

		public static bool IsCustomSlotItem(ItemDrop.ItemData item) => GetCustomSlotName(item) != null;

		public static string GetCustomSlotName(ItemDrop.ItemData item) => item?.m_dropPrefab?.GetComponent<VB_CustomSlotItem>()?.m_slotName;

		private static Dictionary<string, ItemDrop.ItemData> GetCustomSlots(Humanoid humanoid)
		{
			if (!humanoid) return null;
			lock (lockObject) return customSlotItemData.TryGetValue(humanoid, out var slots) ? slots : null;
		}

		public static bool DoesSlotExist(Humanoid humanoid, string slotName)
		{
			var slots = GetCustomSlots(humanoid);
			return slots?.ContainsKey(slotName) ?? false;
		}

		public static bool IsSlotOccupied(Humanoid humanoid, string slotName) => GetSlotItem(humanoid, slotName) != null;

		public static ItemDrop.ItemData GetSlotItem(Humanoid humanoid, string slotName)
		{
			if (!humanoid || slotName == null) return null;

			var slots = GetCustomSlots(humanoid);
			return slots != null && slots.TryGetValue(slotName, out var item) ? item : null;
		}

		public static void SetSlotItem(Humanoid humanoid, string slotName, ItemDrop.ItemData item)
		{
			if (!humanoid || slotName == null) return;

			lock (lockObject)
			{
				if (!customSlotItemData.TryGetValue(humanoid, out var slots))
				{
					slots = new Dictionary<string, ItemDrop.ItemData>();
					customSlotItemData[humanoid] = slots;
				}
				slots[slotName] = item;
				LogDebug($"Set slot '{slotName}' for {humanoid} to {item?.m_shared?.m_name}");
			}
		}

		public static IEnumerable<ItemDrop.ItemData> AllSlotItems(Humanoid humanoid)
		{
			if (!humanoid) return Enumerable.Empty<ItemDrop.ItemData>();

			var slots = GetCustomSlots(humanoid);
			if (slots == null) return Enumerable.Empty<ItemDrop.ItemData>();

			// Создаем список для избежания многократных итераций
			return slots.Values.Where(x => x != null).ToList();
		}

		public static void ApplyCustomSlotItem(GameObject gameObject, string slotName)
		{
			if (!gameObject) throw new ArgumentNullException("gameObject");
			else if (slotName == null) throw new ArgumentNullException("slotName");

			VB_CustomSlotItem customSlotData = gameObject.GetComponent<VB_CustomSlotItem>();
			if (customSlotData)
			{
				if (customSlotData.m_slotName != slotName) throw new InvalidOperationException($"GameObject \"{gameObject.name}\" already has component CustomSlotData! (\"{customSlotData.m_slotName}\" != \"{slotName}\")");
				else return;
			}
			else if (gameObject.GetComponent<ItemDrop>() == null) throw new InvalidOperationException($"GameObject \"{gameObject.name}\" does not have component ItemDrop!");

			gameObject.AddComponent<VB_CustomSlotItem>().m_slotName = slotName;
			gameObject.GetComponent<ItemDrop>().m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.None;
			
			LogDebug($"Applied custom slot '{slotName}' to {gameObject.name}");
		}
	}
}