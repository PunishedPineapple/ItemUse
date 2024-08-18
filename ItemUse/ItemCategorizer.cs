using System;
using System.Collections.Generic;

using Lumina.Excel.GeneratedSheets;

using Serilog.Events;

namespace ItemUse;

internal static class ItemCategorizer
{
	internal static void Init()
	{
		InitGCItems();
		InitLeveItems();
		InitCraftingItems();
		InitAquariumFish();
		InitEhcatlItems();
	}

	internal static void Uninit()
	{

	}

	internal static ItemInfo GetItemInfo( Int32 item )
	{
		//	Cache item info when we compute it to save lookup time in the future.
		if( mItemInfoCache.TryGetValue( item, out var cachedInfo ) && cachedInfo != null )
		{
			return cachedInfo;
		}
		else
		{
			//	Handle HQ items.
			var itemNQ = item;
			if( itemNQ > 1_000_000 ) itemNQ -= 1_000_000;

			//	Handle coffer stuff.
			bool itemIsCoffer = CofferManifests.ItemIsKnownCoffer( item );

			ItemInfo newInfo = new(
				item,
				IsGCItem( itemNQ ),
				IsLeveItem( itemNQ ),
				IsCraftingItem( itemNQ ),
				IsAquariumFish( itemNQ ),
				IsEhcatlItem( itemNQ ),
				itemIsCoffer ? CofferManifests.GetGCJobsForCoffer( item ) : null,
				itemIsCoffer ? CofferManifests.GetLeveJobsForCoffer( item ) : null );

			mItemInfoCache.TryAdd( item, newInfo );

			return newInfo;
		}
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

	internal static bool IsEhcatlItem( Int32 item )
	{
		return mEhcatlItems?.Contains( item ) ?? false;
	}

	internal static HashSet<int> GetJobsForItem( Int32 itemID )
	{
		var itemSheet = DalamudAPI.DataManager.GetExcelSheet<Item>();
		var classJobSheet = DalamudAPI.DataManager.GetExcelSheet<ClassJob>();
		var classjobCategorySheet = DalamudAPI.DataManager.GetExcelSheet<ClassJobCategory_Alternate>();

		var classJobCategoryRowID = itemSheet.GetRow( (uint)itemID )?.ClassJobCategory?.Value?.RowId ?? 0;
		var classJobFlags = classjobCategorySheet.GetRow( classJobCategoryRowID )?.mClassJobFlags;

		HashSet<int> classJobs = new();

		for( int i = 0; i < classJobFlags?.Count; ++i )
		{
			if( classJobFlags[i] ) classJobs.Add( i );
		}

		return classJobs;
	}

	private static void InitGCItems()
	{
		var GCSupplySheet = DalamudAPI.DataManager.GetExcelSheet<GCSupplyDuty>();

		foreach( var row in GCSupplySheet )
		{
			HashSet<Int32> rowData = new();

			//***** TODO: Make our own sheet definition for this maybe.
			//	Lumina has this sheet defined kind of questionably.
			try
			{
				rowData.UnionWith( row.Unknown0 );

				rowData.Add( row.Unknown11 );
				rowData.Add( row.Unknown12 );
				rowData.Add( row.Unknown13 );
				rowData.Add( row.Unknown14 );
				rowData.Add( row.Unknown15 );
				rowData.Add( row.Unknown16 );
				rowData.Add( row.Unknown17 );
				rowData.Add( row.Unknown18 );
				rowData.Add( row.Unknown19 );
				rowData.Add( row.Unknown20 );
				rowData.Add( row.Unknown21 );
				rowData.Add( row.Unknown22 );
				rowData.Add( row.Unknown23 );
				rowData.Add( row.Unknown24 );
				rowData.Add( row.Unknown25 );
				rowData.Add( row.Unknown26 );
				rowData.Add( row.Unknown27 );
				rowData.Add( row.Unknown28 );
				rowData.Add( row.Unknown29 );
				rowData.Add( row.Unknown30 );
				rowData.Add( row.Unknown31 );
				rowData.Add( row.Unknown32 );
				rowData.Add( row.Unknown33 );
				rowData.Add( row.Unknown34 );
				rowData.Add( row.Unknown35 );
				rowData.Add( row.Unknown36 );
				rowData.Add( row.Unknown37 );
				rowData.Add( row.Unknown38 );
				rowData.Add( row.Unknown39 );
				rowData.Add( row.Unknown40 );
				rowData.Add( row.Unknown41 );
				rowData.Add( row.Unknown42 );
				rowData.Add( row.Unknown43 );
				rowData.Add( row.Unknown44 );
				rowData.Add( row.Unknown45 );
				rowData.Add( row.Unknown46 );
				rowData.Add( row.Unknown47 );
				rowData.Add( row.Unknown48 );
				rowData.Add( row.Unknown49 );
				rowData.Add( row.Unknown50 );
				rowData.Add( row.Unknown51 );
				rowData.Add( row.Unknown52 );
				rowData.Add( row.Unknown53 );
				rowData.Add( row.Unknown54 );
				rowData.Add( row.Unknown55 );
				rowData.Add( row.Unknown56 );
				rowData.Add( row.Unknown57 );
				rowData.Add( row.Unknown58 );
				rowData.Add( row.Unknown59 );
				rowData.Add( row.Unknown60 );
				rowData.Add( row.Unknown61 );
				rowData.Add( row.Unknown62 );
				rowData.Add( row.Unknown63 );
				rowData.Add( row.Unknown64 );
				rowData.Add( row.Unknown65 );
			}
			catch( Exception e )
			{
				DalamudAPI.PluginLog.Error( $"Error parsing GCSupplyDuty sheet row {row.RowId}:\r\n{e}" );
			}

			mGCItems.UnionWith( rowData );

			if( DalamudAPI.PluginLog.MinimumLogLevel <= LogEventLevel.Verbose )
			{
				string rowDataString = "";
				foreach( var item in rowData )
				{
					rowDataString += item + ", ";
				}
				DalamudAPI.PluginLog.Verbose( $"GCSupply Row: {row.RowId}, Entries: {rowDataString}" );
			}
		}

		//	Assume anything less than 100 is a quantity, since leves never require more than low double digits, and items below 100 are reserved for currencies.
		mGCItems.RemoveWhere( x => ( x < 100 ) );
	}

	private static void InitLeveItems()
	{
		var leveSheet = DalamudAPI.DataManager.GetExcelSheet<CraftLeve>();

		foreach( var row in leveSheet )
		{
			if( row?.UnkData3 != null )
			{
				HashSet<Int32> rowData = new();

				foreach( var item in row.UnkData3 )
				{
					if( item != null ) rowData.Add( item.Item );
				}

				mLeveItems.UnionWith( rowData );
			}
		};

		//	Items below 100 are reserved items, like currency.  We shouldn't have any issue with it on this sheet, but check just in case.
		mLeveItems.RemoveWhere( x => ( x < 100 ) );
	}

	private static void InitCraftingItems()
	{
		var itemSheet = DalamudAPI.DataManager.GetExcelSheet<Item>();

		foreach( var row in itemSheet )
		{
			if( row.FilterGroup == 12 )
			{
				mCraftingItems.Add( (Int32)row.RowId );
			}
		}

		//	Items below 100 are reserved items, like currency.  We shouldn't have any issue with it on this sheet, but check just in case.
		mAquariumFish.RemoveWhere( x => ( x < 100 ) );
	}

	private static void InitAquariumFish()
	{
		var fishSheet = DalamudAPI.DataManager.GetExcelSheet<AquariumFish>();

		foreach( var row in fishSheet )
		{
			if( row?.Item != null )
			{
				mAquariumFish.Add( (Int32)row.Item.Row );
			}
		};

		//	Items below 100 are reserved items, like currency.  We shouldn't have any issue with it on this sheet, but check just in case.
		mAquariumFish.RemoveWhere( x => ( x < 100 ) );
	}

	private static void InitEhcatlItems()
	{
		var ehcatlSheet = DalamudAPI.DataManager.GetExcelSheet<DailySupplyItem>();

		foreach( var row in ehcatlSheet )
		{
			HashSet<Int32> rowData = new();

			if( row?.UnkData0 != null )
			{
				foreach( var entry in row.UnkData0 )
				{
					rowData.Add( entry.Item );
				}
			}

			mEhcatlItems.UnionWith( rowData );
		};

		//	Items below 100 are reserved items, like currency.  We shouldn't have any issue with it on this sheet, but check just in case.
		mEhcatlItems.RemoveWhere( x => ( x < 100 ) );
	}

	internal static List<Int32> DEBUG_GetGCItems()
	{
		List<Int32> retVal = new( mGCItems );
		retVal.Sort();
		return retVal;
	}

	internal static List<Int32> DEBUG_GetLeveItems()
	{
		List<Int32> retVal = new( mLeveItems );
		retVal.Sort();
		return retVal;
	}

	internal static List<Int32> DEBUG_GetEhcatlItems()
	{
		List<Int32> retVal = new( mEhcatlItems );
		retVal.Sort();
		return retVal;
	}

	internal static List<Int32> DEBUG_GetCraftingMaterials()
	{
		List<Int32> retVal = new( mCraftingItems );
		retVal.Sort();
		return retVal;
	}

	internal static List<Int32> DEBUG_GetAquariumFish()
	{
		List<Int32> retVal = new( mAquariumFish );
		retVal.Sort();
		return retVal;
	}

	private static readonly HashSet<Int32> mGCItems = new();
	private static readonly HashSet<Int32> mLeveItems = new();
	private static readonly HashSet<Int32> mCraftingItems = new();
	private static readonly HashSet<Int32> mAquariumFish = new();
	private static readonly HashSet<Int32> mEhcatlItems = new();

	private static readonly SortedDictionary<Int32, ItemInfo> mItemInfoCache = new();
}
