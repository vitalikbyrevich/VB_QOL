namespace VBQOL
{
	public static class VB_CustomSlotManager
	{
		private static readonly Dictionary< Humanoid , Dictionary< string , ItemDrop.ItemData > > customSlotItemData = new Dictionary< Humanoid , Dictionary< string , ItemDrop.ItemData > >();

		public static void Register( Humanoid humanoid ) => customSlotItemData[ humanoid ] = new Dictionary< string , ItemDrop.ItemData >();

		public static void Unregister( Humanoid humanoid ) => customSlotItemData.Remove( humanoid );

		public static bool IsCustomSlotItem( ItemDrop.ItemData item ) => GetCustomSlotName( item ) != null;

		public static string GetCustomSlotName( ItemDrop.ItemData item ) => item?.m_dropPrefab?.GetComponent< VB_CustomSlotItem >()?.m_slotName;

		private static Dictionary< string , ItemDrop.ItemData > GetCustomSlots( Humanoid humanoid ) => humanoid && customSlotItemData.ContainsKey( humanoid ) ? customSlotItemData[ humanoid ] : null;

		public static bool DoesSlotExist( Humanoid humanoid , string slotName ) => GetCustomSlots( humanoid )?.ContainsKey( slotName ) ?? false;

		public static bool IsSlotOccupied( Humanoid humanoid , string slotName ) => GetSlotItem( humanoid , slotName ) != null;

		public static ItemDrop.ItemData GetSlotItem( Humanoid humanoid , string slotName )
		{
			var slots = slotName != null ? GetCustomSlots( humanoid ) : null;
			return slots != null && slots.ContainsKey( slotName ) ? slots[ slotName ] : null;
		}
		
		public static void SetSlotItem( Humanoid humanoid , string slotName , ItemDrop.ItemData item )
		{
			if( !humanoid || slotName == null ) return;

			customSlotItemData[ humanoid ][ slotName ] = item;
		}

		public static IEnumerable< ItemDrop.ItemData > AllSlotItems( Humanoid humanoid )
		{
			if( !humanoid || !customSlotItemData.ContainsKey( humanoid ) ) return Enumerable.Empty< ItemDrop.ItemData > ();

			return customSlotItemData[ humanoid ].Values.Where( x => x != null ).ToList();
		}

		public static void ApplyCustomSlotItem( GameObject gameObject , string slotName )
		{
			if( !gameObject ) throw new ArgumentNullException( "gameObject" );
			else if( slotName == null ) throw new ArgumentNullException( "slotName" );

			VB_CustomSlotItem customSlotData = gameObject.GetComponent< VB_CustomSlotItem >();
			if( customSlotData )
			{
				if( customSlotData.m_slotName != slotName ) throw new InvalidOperationException( $"GameObject \"{gameObject.name}\" already has component CustomSlotData! (\"{customSlotData.m_slotName}\" != \"{slotName}\")" );
				else return;
			}
			else if( gameObject.GetComponent< ItemDrop >() == null ) throw new InvalidOperationException( $"GameObject \"{gameObject.name}\" does not have component ItemDrop!" );

			gameObject.AddComponent< VB_CustomSlotItem >().m_slotName = slotName;
			gameObject.GetComponent< ItemDrop >().m_itemData.m_shared.m_itemType = ItemDrop.ItemData.ItemType.None;
		}
	}
}
