using System;

namespace ItemUse;

internal enum SpecialItemType : UInt32
{
	Normal = 0,
	Collectible = 500_000,
	HQ = 1_000_000,
	Event = 2_000_000,
}
