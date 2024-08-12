using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CheapLoc;

using Dalamud;
using Dalamud.Data;
using Dalamud.Game.Text;
using Dalamud.Utility;

using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

using Serilog.Events;

namespace ItemUse;

internal static class ItemCategorizer
{
	internal static void Init()
	{
		InitGCItems();
	}

	internal static void Uninit()
	{

	}

	internal static ItemInfo GetItemInfo( Int32 item )
	{
		//***** TODO: Cache items that have already been hovered to reduce time spent checking the lists.

		//***** TODO: Handle coffer stuff.

		return new(
			item,
			IsGCItem( item ),
			IsLeveItem( item ),
			IsCraftingItem( item ),
			IsAquariumFish( item ),
			null,
			null );
	}

	internal static bool IsGCItem( Int32 item )
	{
		return mGCItems?.Contains( item ) ?? false;
	}

	internal static bool IsLeveItem( Int32 item )
	{
		return mLeveItems?.Contains( item ) ?? false;
	}

	internal static bool IsCraftingItem( Int32 item )
	{
		return mCraftingItems?.Contains( item ) ?? false;
	}

	internal static bool IsAquariumFish( Int32 item )
	{
		return mAquariumFish?.Contains( item ) ?? false;
	}

	private static void InitGCItems()
	{
		var GCSupplySheet = DalamudAPI.DataManager.GetExcelSheet<GCSupplyDuty>();

		for( uint i = 1; i < GCSupplySheet.RowCount; ++i )
		{
			HashSet<Int32> rowData = new( GCSupplySheet.GetRow( i ).Unknown0 );
			try
			{
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown11 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown12 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown13 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown14 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown15 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown16 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown17 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown18 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown19 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown20 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown21 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown22 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown23 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown24 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown25 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown26 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown27 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown28 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown29 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown30 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown31 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown32 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown33 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown34 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown35 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown36 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown37 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown38 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown39 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown40 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown41 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown42 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown43 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown44 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown45 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown46 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown47 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown48 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown49 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown50 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown51 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown52 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown53 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown54 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown55 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown56 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown57 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown58 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown59 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown60 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown61 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown62 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown63 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown64 );
				rowData.Add( GCSupplySheet.GetRow( i ).Unknown65 );
			}
			catch( Exception e )
			{
				DalamudAPI.PluginLog.Error( $"Error parsing GCSupplyDuty sheet row {i}:\r\n{e}" );
			}

			mGCItems.UnionWith( rowData );

			if( DalamudAPI.PluginLog.MinimumLogLevel <= LogEventLevel.Verbose )
			{
				string rowDataString = "";
				foreach( var item in rowData )
				{
					rowDataString += item + ", ";
				}
				DalamudAPI.PluginLog.Verbose( $"GCSupply Row: {i}, Entries: {rowDataString}" );
			}
		}

		//	Assume anything less than 100 is a quantity, since leves never require more than low double digits, and items below 100 are reserved for currencies.
		mGCItems.RemoveWhere( x => ( x < 100 ) );

		if( DalamudAPI.PluginLog.MinimumLogLevel <= LogEventLevel.Debug )
		{
			string itemIDs = "";
			foreach( var item in mGCItems )
			{
				itemIDs += item + ", ";
			}
			DalamudAPI.PluginLog.Debug( $"Loaded GC item IDs: {itemIDs}" );
		}
	}

	private static void InitLeveItems()
	{

	}

	private static void InitCraftingItems()
	{

	}

	private static void InitAquariumFish()
	{

	}

	private static readonly HashSet<Int32> mGCItems = new();
	private static readonly HashSet<Int32> mLeveItems = new();
	private static readonly HashSet<Int32> mCraftingItems = new();
	private static readonly HashSet<Int32> mAquariumFish = new();
}
