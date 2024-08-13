using System;
using System.Collections.Generic;

namespace ItemUse;

internal class ItemInfo
{
	internal ItemInfo( Int32 itemID, bool isGCItem, bool isLeveItem, bool isCraftingMaterial, bool isAquariumFish, IEnumerable<Int32> cofferGCJobs, IEnumerable<Int32> cofferLeveJobs )
	{
		ItemID = itemID;
		IsGCItem = isGCItem;
		IsLeveItem = isLeveItem;
		IsCraftingMaterial = isCraftingMaterial;
		IsAquariumFish = isAquariumFish;

		if( cofferGCJobs != null )
		{
			CofferGCJobs = new( cofferGCJobs );
		}

		if( cofferLeveJobs != null )
		{
			CofferLeveJobs = new( cofferLeveJobs );
		}
	}

	private ItemInfo() {}

	internal Int32 ItemID { get; private set; } = 0;
	internal bool IsGCItem { get; private set; } = false;
	internal bool IsLeveItem { get; private set; } = false;
	internal bool IsCraftingMaterial { get; private set; } = false;
	internal bool IsAquariumFish { get; private set; } = false;

	internal List<Int32> CofferGCJobs { get; private set; } = null;
	internal List<Int32> CofferLeveJobs { get; private set; } = null;
}
