using System;
using System.Collections.Generic;

using Lumina.Excel.Sheets;

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
			//	Handle special item IDs.
			var itemNQ = item;
			if( itemNQ > 2_000_000 ) { }	//	These are EventItems, about which we don't really care.
			else if( itemNQ > 1_000_000 ) itemNQ -= 1_000_000;
			else if( itemNQ > 500_000 ) itemNQ -= 500_000;

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
		var classjobCategorySheet = DalamudAPI.DataManager.GetExcelSheet<ClassJobCategory_Alternate>();

		HashSet<int> classJobs = new();

		if( itemSheet.TryGetRow( (uint)itemID, out var itemRow ) &&
			classjobCategorySheet.TryGetRow( itemRow.ClassJobCategory.Value.RowId, out var classJobCategoryRow ) )
		{
			for( int i = 0; i < classJobCategoryRow.ClassJobColumnCount; ++i )
			{
				if( classJobCategoryRow.IncludesClassJob( i ) ) classJobs.Add( i );
			}
		}

		return classJobs;
	}

	private static void InitGCItems()
	{
		var GCSupplySheet = DalamudAPI.DataManager.GetExcelSheet<GCSupplyDuty>();

		foreach( var row in GCSupplySheet )
		{
			HashSet<Int32> rowData = new();

			foreach( var mission in row.SupplyData )
			{
				foreach( var item in mission.Item )
				{
					if( item.IsValid ) rowData.Add( (int)item.RowId );
				}
			}

			mGCItems.UnionWith( rowData );
		}

		//	Items below 100 are reserved items, like currency.  We should no longer have any issues with this, but there's no real downside to validating.
		mGCItems.RemoveWhere( x => ( x < 100 ) );
	}

	private static void InitLeveItems()
	{
		var leveSheet = DalamudAPI.DataManager.GetExcelSheet<CraftLeve>();

		foreach( var row in leveSheet )
		{
			HashSet<Int32> rowData = new();

			foreach( var item in row.Item )
			{
				if( item.IsValid ) rowData.Add( (int)item.RowId );
			}

			mLeveItems.UnionWith( rowData );
		}

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
			if( row.Item.IsValid ) mAquariumFish.Add( (Int32)row.Item.RowId );
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

			foreach( var item in row.Item )
			{
				if( item.IsValid ) rowData.Add( (int)item.RowId );
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

	internal static int DEBUG_ItemCacheCount => mItemInfoCache?.Count ?? 0;

	private static readonly HashSet<Int32> mGCItems = new();
	private static readonly HashSet<Int32> mLeveItems = new();
	private static readonly HashSet<Int32> mCraftingItems = new();
	private static readonly HashSet<Int32> mAquariumFish = new();
	private static readonly HashSet<Int32> mEhcatlItems = new();

	private static readonly SortedDictionary<Int32, ItemInfo> mItemInfoCache = new();
}
