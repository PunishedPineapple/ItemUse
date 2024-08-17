using System;
using System.Collections.Generic;
using System.Linq;

namespace ItemUse;

internal class ItemInfo
{
	internal ItemInfo( Int32 itemID, bool isGCItem, bool isLeveItem, bool isCraftingMaterial, bool isAquariumFish, bool isEhcatlItem, IEnumerable<Int32> cofferGCJobs, IEnumerable<Int32> cofferLeveJobs )
	{
		ItemID = itemID;
		IsGCItem = isGCItem;
		IsLeveItem = isLeveItem;
		IsCraftingMaterial = isCraftingMaterial;
		IsAquariumFish = isAquariumFish;
		IsEhcatlItem = isEhcatlItem;

		if( cofferGCJobs != null ) mCofferGCJobs = cofferGCJobs.ToArray();
		if( cofferLeveJobs != null ) mCofferLeveJobs = cofferLeveJobs.ToArray();
	}

	protected ItemInfo() { }

	internal Int32 ItemID { get; private set; } = 0;
	internal bool IsGCItem { get; private set; } = false;
	internal bool IsLeveItem { get; private set; } = false;
	internal bool IsCraftingMaterial { get; private set; } = false;
	internal bool IsAquariumFish { get; private set; } = false;
	internal bool IsEhcatlItem { get; private set; } = false;

	internal HashSet<Int32> CofferGCJobs => mCofferGCJobs != null ? new( mCofferGCJobs ) : null;
	internal HashSet<Int32> CofferLeveJobs => mCofferGCJobs != null ? new( mCofferLeveJobs ) : null;

	protected Int32[] mCofferGCJobs = null;
	protected Int32[] mCofferLeveJobs = null;
}
