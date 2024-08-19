using System;
using System.Collections.Generic;
using System.Collections.Immutable;

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

		if( cofferGCJobs != null ) CofferGCJobs = cofferGCJobs.ToImmutableHashSet();
		if( cofferLeveJobs != null ) CofferLeveJobs = cofferLeveJobs.ToImmutableHashSet();
	}

	protected ItemInfo() { }

	internal Int32 ItemID { get; private set; } = 0;
	internal bool IsGCItem { get; private set; } = false;
	internal bool IsLeveItem { get; private set; } = false;
	internal bool IsCraftingMaterial { get; private set; } = false;
	internal bool IsAquariumFish { get; private set; } = false;
	internal bool IsEhcatlItem { get; private set; } = false;

	internal ImmutableHashSet<Int32> CofferGCJobs { get; private set; } = null;
	internal ImmutableHashSet<Int32> CofferLeveJobs { get; private set; } = null;
}
