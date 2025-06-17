namespace VBQOL
{
	internal class VB_CustomSlotItem : MonoBehaviour
	{
		public string m_slotName;
		public static ConfigEntry< string > ItemSlotPairs;
        
		[HarmonyPatch( typeof( Humanoid ) )]
		[HarmonyPriority( Priority.VeryHigh )]
		public class HumanoidPatch
		{
			public static HashSet< StatusEffect > GetStatusEffectsFromCustomSlotItems( Humanoid __instance )
			{
				HashSet< StatusEffect > statuses = new HashSet< StatusEffect >();

				foreach( ItemDrop.ItemData itemData in VB_CustomSlotManager.AllSlotItems( __instance ) )
				{
					if( itemData.m_shared.m_equipStatusEffect ) statuses.Add( itemData.m_shared.m_equipStatusEffect );

					bool hasSetEffect = Traverse.Create( __instance ).Method( "HaveSetEffect" , new[] { typeof( ItemDrop.ItemData ) } ).GetValue< bool >( itemData );
					if( hasSetEffect ) statuses.Add( itemData.m_shared.m_setStatusEffect );
				}
				return statuses;
			}

			[HarmonyPatch( "Awake" )]
			[HarmonyPostfix]
			static void AwakePostfix( ref Humanoid __instance ) => VB_CustomSlotManager.Register( __instance );

			[HarmonyPatch( "EquipItem" )]
			[HarmonyPostfix]
			static void EquipItemPostfix( ref bool __result , ref Humanoid __instance , ItemDrop.ItemData item , bool triggerEquipEffects = true )
			{
				if( !__result || !VB_CustomSlotManager.IsCustomSlotItem( item ) ) return;

				string slotName = VB_CustomSlotManager.GetCustomSlotName( item );
				ItemDrop.ItemData existingSlotItem = VB_CustomSlotManager.GetSlotItem( __instance , slotName );
				if( existingSlotItem != null ) __instance.UnequipItem( existingSlotItem , triggerEquipEffects );
				VB_CustomSlotManager.SetSlotItem( __instance , slotName , item );

				item.m_equipped = __instance.IsItemEquiped( item );
				Traverse.Create( __instance ).Method( "SetupEquipment" ).GetValue();

				if( item.m_equipped && triggerEquipEffects ) Traverse.Create( __instance ).Method( "TriggerEquipEffect" , new[] { typeof( ItemDrop.ItemData ) } ).GetValue( item );
				__result = true;
			}

			[HarmonyPatch( "GetEquipmentWeight" )]
			[HarmonyPostfix]
			static void GetEquipmentWeightPostfix( ref float __result , ref Humanoid __instance )
			{
				foreach( ItemDrop.ItemData itemData in VB_CustomSlotManager.AllSlotItems( __instance ) ) __result += itemData.m_shared.m_weight;
			}

			[HarmonyPatch( "GetSetCount" )]
			[HarmonyPostfix]
			static void GetSetCountPostfix( ref int __result , ref Humanoid __instance , string setName )
			{
				foreach( ItemDrop.ItemData itemData in VB_CustomSlotManager.AllSlotItems( __instance ) ) if( itemData.m_shared.m_setName == setName ) __result++;
			}

			[HarmonyPatch( "IsItemEquiped" )]
			[HarmonyPostfix]
			static void IsItemEquipedPostfix( ref bool __result , ref Humanoid __instance , ItemDrop.ItemData item )
			{
				if( !VB_CustomSlotManager.IsCustomSlotItem( item ) ) return;

				string slotName = VB_CustomSlotManager.GetCustomSlotName( item );
				__result |= VB_CustomSlotManager.GetSlotItem( __instance , slotName ) == item;
			}

			[HarmonyPatch( "UnequipAllItems" )]
			[HarmonyPostfix]
			static void UnequipAllItemsPostfix( ref Humanoid __instance )
			{
				foreach( ItemDrop.ItemData itemData in VB_CustomSlotManager.AllSlotItems( __instance ) ) __instance.UnequipItem( itemData , false );
			}

			[HarmonyPatch( "UnequipItem" )]
			[HarmonyPostfix]
			static void UnequipItemPostfix( ref Humanoid __instance , ItemDrop.ItemData item , bool triggerEquipEffects = true )
			{
				if( item == null ) return;

				string slotName = VB_CustomSlotManager.GetCustomSlotName( item );
				if( item == VB_CustomSlotManager.GetSlotItem( __instance , slotName ) )
				{
					VB_CustomSlotManager.SetSlotItem( __instance , slotName , null );
					item.m_equipped = __instance.IsItemEquiped( item );
					Traverse.Create( __instance ).Method( "UpdateEquipmentStatusEffects" ).GetValue();
				}
			}

			[HarmonyPatch( "UpdateEquipmentStatusEffects" )]
			[HarmonyTranspiler]
			static IEnumerable< CodeInstruction > UpdateEquipmentStatusEffectsTranspiler( IEnumerable< CodeInstruction > instructionsIn )
			{
				List< CodeInstruction > instructions = instructionsIn.ToList();
				if( instructions[ 0 ].opcode != OpCodes.Newobj || instructions[ 1 ].opcode != OpCodes.Stloc_0 ) throw new Exception( "CustomSlotItemLib transpiler injection point not found!" );

				yield return instructions[ 0 ];
				yield return instructions[ 1 ];

				// Add GetStatusEffectsFromCustomSlotItems() results to the set
				yield return new CodeInstruction( OpCodes.Ldloc_0 );
				yield return new CodeInstruction( OpCodes.Ldarg_0 );
				yield return CodeInstruction.Call( typeof( HumanoidPatch ) , nameof( HumanoidPatch.GetStatusEffectsFromCustomSlotItems ) );
				yield return CodeInstruction.Call( typeof( HashSet< StatusEffect > ) , nameof( HashSet< StatusEffect >.UnionWith ) );

				for( int index = 2 ; index < instructions.Count ; index++ )
				{
					CodeInstruction instruction = instructions[ index ];
					yield return instruction;
				}
			}
		}
		
		[HarmonyPatch( typeof( ItemDrop.ItemData ) )]
		[HarmonyPriority( Priority.VeryHigh )]
		private class ItemDropItemDataPatch
		{
			[HarmonyPatch( "IsEquipable" )]
			[HarmonyPostfix]
			static void IsEquipablePostfix( ref bool __result , ref ItemDrop.ItemData __instance ) => __result |= VB_CustomSlotManager.IsCustomSlotItem( __instance );
		}
		
		[HarmonyPatch( typeof( Player ) )]
		[HarmonyPriority( Priority.VeryHigh )]
		internal class PlayerPatch
		{
			[HarmonyPatch( "Load" )]
			[HarmonyPostfix]
			static void LoadPostfix( ref Player __instance )
			{
				foreach( ItemDrop.ItemData itemData in __instance.GetInventory().GetEquippedItems() )
				{
					string slotName = VB_CustomSlotManager.GetCustomSlotName( itemData );
					if( slotName != null ) VB_CustomSlotManager.SetSlotItem( __instance , slotName , itemData );
				}
			}
		}
		
		private static string[] ValidateItemSlotPair( string rawPair )
		{
			if( rawPair == null ) throw new ArgumentNullException( "rawPair" );

			string[] keyValue = rawPair.Split( ',' );
			if( keyValue.Length < 2 ) throw new ArgumentException( "Item slot pair does not name a slot!" );
			else if( keyValue.Length > 2 ) throw new ArgumentException( "Item slot pair lists more than a Item and a slot!" );
			else if( keyValue[ 0 ].IsNullOrWhiteSpace() ) throw new ArgumentException( "Item name is null or whitespace!" );
			else if( keyValue[ 1 ].IsNullOrWhiteSpace() ) throw new ArgumentException( "Slot name is null or whitespace!" );
			else if( !ZNetScene.instance.GetPrefab( keyValue[ 0 ] ) ) throw new NullReferenceException( $"Item \"{keyValue[ 0 ]}\" is NULL!" );

			return keyValue;
		}
		
		[HarmonyPatch( typeof( ZNetScene ) )]
		[HarmonyPriority( Priority.VeryHigh )]
		public class ZNetScenePatch
		{
			[HarmonyPatch( "Awake" )]
			[HarmonyPostfix]
			static void AwakePostfix( ref ZNetScene __instance )
			{
				if( ItemSlotPairs.Value.IsNullOrWhiteSpace() ) return;

				try
				{
					foreach( string pair in ItemSlotPairs.Value.Split( ';' ) )
					{
						string[] keyValue = ValidateItemSlotPair( pair );
						GameObject gameObject = __instance.GetPrefab( keyValue[ 0 ] );
						VB_CustomSlotManager.ApplyCustomSlotItem( gameObject , keyValue[ 1 ] );
					}
				}
				catch( Exception e )
				{
					System.Console.WriteLine( e );
				}
			}
		}
    }
}
