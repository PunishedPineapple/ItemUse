using System;

using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Utility;


using Lumina.Excel.Sheets;

namespace ItemUse;

internal static class ItemUtils
{
	public static SeString GetItemName( UInt32 itemID )
	{
		//	Handle special item IDs.
		var specialItemType = GetSpecialItemType( itemID );
		var itemNQ = GetNQItem( itemID );

		if( specialItemType == SpecialItemType.Event )
		{
			var eventItemSheet = DalamudAPI.DataManager.GetExcelSheet<EventItem>();

			if( eventItemSheet != null &&
				eventItemSheet.TryGetRow( itemID, out var eventItemRow ) )
			{
				return eventItemRow.Name.ToDalamudString();
			}
			else
			{
				var str = new SeStringBuilder();
				str.Add( new TextPayload( $"Unknown EventItem ({itemID})" ) );
				return str.BuiltString;
			}
		}
		else
		{
			var itemSheet = DalamudAPI.DataManager.GetExcelSheet<Item>();
			
			if( itemSheet != null &&
				itemSheet.TryGetRow( itemNQ, out var itemRow ) )
			{
				return itemRow.Name.ToDalamudString();
			}
			else
			{
				var str = new SeStringBuilder();
				str.Add( new TextPayload( $"Unknown ({itemID})" ) );
				return str.BuiltString;
			}
		}
	}

	public static string GetUnformattedName( UInt32 itemID )
	{
		return GetItemName( itemID ).TextValue;
	}

	public static SpecialItemType GetSpecialItemType( UInt32 itemID )
	{
		if( itemID > 2_000_000 ) return SpecialItemType.Event;
		else if( itemID > 1_000_000 ) return SpecialItemType.HQ;
		else if( itemID > 500_000 ) return SpecialItemType.Collectible;
		else return SpecialItemType.Normal;
	}

	public static UInt32 GetNQItem( UInt32 itemID )
	{
		var itemNQ = itemID;
		if( itemNQ > 2_000_000 ) { }    //	EventItems are handled separately, and don't get subtracted.
		else if( itemNQ > 1_000_000 ) itemNQ -= 1_000_000;
		else if( itemNQ > 500_000 ) itemNQ -= 500_000;

		return itemNQ;
	}
}
