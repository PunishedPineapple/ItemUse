using System;
using System.Linq;
using System.Runtime.InteropServices;

using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ItemUse;

internal unsafe static class ItemDetailHandler
{
	internal static void Init( Configuration configuration )
	{
		mConfiguration = configuration;

		mpItemFlagsString = (byte*)Marshal.AllocHGlobal( mItemFlagsStringMaxLength + 1 );
		if( mpItemFlagsString == null ) throw new Exception( "Unable to allocate text node string memory." );
		else Marshal.WriteByte( (nint)mpItemFlagsString, 0 );

		mpCofferJobsString = (byte*)Marshal.AllocHGlobal( mCofferJobsStringMaxLength + 1 );
		if( mpCofferJobsString == null ) throw new Exception( "Unable to allocate text node string memory." );
		else Marshal.WriteByte( (nint)mpCofferJobsString, 0 );

		DalamudAPI.AddonLifecycle.RegisterListener( AddonEvent.PostUpdate, "ItemDetail", ItemDetailUpdateCallback );
	}

	internal static void Uninit()
	{
		DalamudAPI.AddonLifecycle.UnregisterListener( AddonEvent.PostUpdate, "ItemDetail", ItemDetailUpdateCallback );
		mConfiguration = null;
		AtkNodeHelpers.RemoveTextNode( (AtkUnitBase*)DalamudAPI.GameGui.GetAddonByName( "ItemDetail" ), mItemFlagsTextNodeID );
		AtkNodeHelpers.RemoveTextNode( (AtkUnitBase*)DalamudAPI.GameGui.GetAddonByName( "ItemDetail" ), mCofferJobsTextNodeID );
		if( mpItemFlagsString != null ) Marshal.FreeHGlobal( (nint)mpItemFlagsString );
		if( mpCofferJobsString != null ) Marshal.FreeHGlobal( (nint)mpCofferJobsString );
	}

	private static void ItemDetailUpdateCallback( AddonEvent type, AddonArgs args )
	{
		UpdateCurrentItemInfo();
		SetItemFlagsString( CurrentItemInfo );
		SetCofferJobsString( CurrentItemInfo );
		UpdateItemFlagsTextNode();
		UpdateCofferJobsTextNode();
	}

	internal static void UpdateCurrentItemInfo()
	{
		mCurrentItemID = (Int32)DalamudAPI.GameGui.HoveredItem;

		if( mCurrentItemID <= 0 )
		{
			CurrentItemInfo = null;
		}
		else if( mCurrentItemID != CurrentItemInfo?.ItemID )
		{
			CurrentItemInfo = ItemCategorizer.GetItemInfo( mCurrentItemID );
			DalamudAPI.PluginLog.Verbose( $"ItemDetail addon updated with item {mCurrentItemID}." );
		}
	}

	private unsafe static void UpdateItemFlagsTextNode( bool show = true )
	{
		AtkUnitBase* pAddon = (AtkUnitBase*)DalamudAPI.GameGui.GetAddonByName( "ItemDetail" );
		AtkTextNode* pNode = null;
		AtkResNode* pTitleBarNode = null;
		AtkResNode* pIconsContainerNode = null;
		AtkResNode* pOtherFlagsLeftmostNode = null;
		AtkResNode* pOtherFlagsRightmostNode = null;
		AtkTextNode* pItemQuantityTextNode = null;

		if( pAddon != null )
		{
			pNode = AtkNodeHelpers.GetTextNodeByID( pAddon, mItemFlagsTextNodeID );
			pTitleBarNode = pAddon->GetNodeById( mTitleBarResNodeID );
			pIconsContainerNode = pAddon->GetNodeById( mIconsContainerResNodeID );
			pOtherFlagsLeftmostNode = pAddon->GetNodeById( mItemFlagsLeftmostIconResNodeID );
			pOtherFlagsRightmostNode = pAddon->GetNodeById( mItemFlagsRightmostIconResNodeID );
			var pItemQuantityTextNodeAsRes = pAddon->GetNodeById( mItemQuantityTextNodeID );
			if( pItemQuantityTextNodeAsRes != null ) pItemQuantityTextNode = pItemQuantityTextNodeAsRes->GetAsAtkTextNode();

			//	If we have our node, set the colors, size, and text from settings.
			if( pNode != null )
			{
				bool haveRequiredNodes = pTitleBarNode != null && pIconsContainerNode != null && pOtherFlagsLeftmostNode != null && pOtherFlagsRightmostNode != null && pItemQuantityTextNode != null;
				bool otherIconsVisible =  haveRequiredNodes && pTitleBarNode->IsVisible() && pIconsContainerNode->IsVisible() && pOtherFlagsLeftmostNode->IsVisible();
				bool visible = show && haveRequiredNodes;

				pNode->ToggleVisibility( visible );

				if( visible )
				{
					int xPosBase = pTitleBarNode->GetXShort() + pIconsContainerNode->GetXShort();
					int yPosBase = pTitleBarNode->GetYShort() + pIconsContainerNode->GetYShort();

					int xPos = pOtherFlagsLeftmostNode->IsVisible() ? pOtherFlagsLeftmostNode->GetXShort() : ( pOtherFlagsRightmostNode->GetXShort() + pOtherFlagsRightmostNode->Width );
					int yPos = pOtherFlagsLeftmostNode->GetYShort() + pOtherFlagsLeftmostNode->GetHeight();

					pNode->AtkResNode.SetPositionShort( (short)( xPosBase + xPos - pNode->Width ), (short)( yPosBase + yPos - pNode->Height ) );

					pNode->AtkResNode.Color.A = 255;

					pNode->TextColor.A = pItemQuantityTextNode->TextColor.A;
					pNode->TextColor.R = pItemQuantityTextNode->TextColor.R;
					pNode->TextColor.G = pItemQuantityTextNode->TextColor.G;
					pNode->TextColor.B = pItemQuantityTextNode->TextColor.B;

					pNode->EdgeColor.A = pItemQuantityTextNode->EdgeColor.A;
					pNode->EdgeColor.R = pItemQuantityTextNode->EdgeColor.R;
					pNode->EdgeColor.G = pItemQuantityTextNode->EdgeColor.G;
					pNode->EdgeColor.B = pItemQuantityTextNode->EdgeColor.B;

					pNode->FontSize = 14;
					pNode->AlignmentType = AlignmentType.BottomRight;
					pNode->FontType = FontType.Axis;
					pNode->TextFlags = (byte)TextFlags.Bold;
					pNode->LineSpacing = 1;
					pNode->CharSpacing = 1;

					pNode->SetText( mpItemFlagsString );
				}
			}

			//	Set up the node if it hasn't been.
			else if( pAddon->RootNode != null )
			{
				pNode = AtkNodeHelpers.CreateNewTextNode( pAddon, mItemFlagsTextNodeID );
			}
		}
	}

	//***** TODO: Ultimately it would be better to insert our coffer information string into the item description rather than maintaining a separate node.
	private unsafe static void UpdateCofferJobsTextNode( bool show = true )
	{
		AtkUnitBase* pAddon = (AtkUnitBase*)DalamudAPI.GameGui.GetAddonByName( "ItemDetail" );
		AtkTextNode* pNode = null;
		AtkTextNode* pItemQuantityTextNode = null;

		if( pAddon != null )
		{
			pNode = AtkNodeHelpers.GetTextNodeByID( pAddon, mCofferJobsTextNodeID );
			var pItemQuantityTextNodeAsRes = pAddon->GetNodeById( mItemQuantityTextNodeID );
			if( pItemQuantityTextNodeAsRes != null ) pItemQuantityTextNode = pItemQuantityTextNodeAsRes->GetAsAtkTextNode();

			//	If we have our node, set the colors, size, and text from settings.
			if( pNode != null )
			{
				bool haveRequiredNodes = pItemQuantityTextNode != null;
				bool visible = show && haveRequiredNodes;

				pNode->ToggleVisibility( visible );

				if( visible )
				{
					pNode->AtkResNode.SetPositionShort( 0, (short)-pNode->GetHeight() );

					pNode->AtkResNode.Color.A = 255;

					pNode->TextColor.A = pItemQuantityTextNode->TextColor.A;
					pNode->TextColor.R = pItemQuantityTextNode->TextColor.R;
					pNode->TextColor.G = pItemQuantityTextNode->TextColor.G;
					pNode->TextColor.B = pItemQuantityTextNode->TextColor.B;

					pNode->EdgeColor.A = pItemQuantityTextNode->EdgeColor.A;
					pNode->EdgeColor.R = pItemQuantityTextNode->EdgeColor.R;
					pNode->EdgeColor.G = pItemQuantityTextNode->EdgeColor.G;
					pNode->EdgeColor.B = pItemQuantityTextNode->EdgeColor.B;

					pNode->FontSize = 22;
					pNode->AlignmentType = AlignmentType.BottomLeft;
					pNode->FontType = FontType.Axis;
					pNode->TextFlags = (byte)( TextFlags.Bold | TextFlags.MultiLine );
					pNode->LineSpacing = pNode->FontSize;
					pNode->CharSpacing = 1;

					pNode->SetText( mpCofferJobsString );
				}
			}

			//	Set up the node if it hasn't been.
			else if( pAddon->RootNode != null )
			{
				pNode = AtkNodeHelpers.CreateNewTextNode( pAddon, mCofferJobsTextNodeID );
			}
		}
	}

	//***** TODO: Probably eventually get rid of this and make them real icons.
	private static void SetItemFlagsString( ItemInfo itemInfo )
	{
		SeStringBuilder str = new();

		//***** TODO: Can't find the player's GC in Dalamud anywhere (and not cleanly in ClientStructs).  Figure that out instead of using a manual config option.
		var GCIcon = mConfiguration.mGrandCompany switch
		{
			2 => BitmapFontIcon.BlackShroud,
			3 => BitmapFontIcon.Thanalan,
			_ => BitmapFontIcon.LaNoscea
		};

		if( mConfiguration.mShowGCItemsFlag && ( itemInfo?.IsGCItem ?? false ) ) str.Add( new IconPayload( GCIcon ) );
		if( mConfiguration.mShowLeveItemsFlag && ( itemInfo?.IsLeveItem ?? false ) ) str.Add( new IconPayload( BitmapFontIcon.Dice ) );
		if( mConfiguration.mShowEhcatlItemsFlag && ( itemInfo?.IsEhcatlItem ?? false ) ) str.Add( new IconPayload( BitmapFontIcon.FlyZone ) );
		if( mConfiguration.mShowCraftingMaterialsFlag && ( itemInfo?.IsCraftingMaterial ?? false ) ) str.Add( new IconPayload( BitmapFontIcon.Crafter ) );
		if( mConfiguration.mShowAquariumFishFlag && ( itemInfo?.IsAquariumFish ?? false ) ) str.Add( new IconPayload( BitmapFontIcon.Fisher ) );

		//	Combine all of the flags into one thing if desired.
		if( str.BuiltString.Payloads.Count > 0 && mConfiguration.mShowCombinedUsefulFlag )
		{
			str = new();
			str.Add( new IconPayload( BitmapFontIcon.GoldStar ) );
		}

		byte[] encodedStr = str.BuiltString.EncodeWithNullTerminator();
		if( encodedStr.Length <= mItemFlagsStringMaxLength )
		{
			Marshal.Copy( encodedStr, 0, (nint)mpItemFlagsString, encodedStr.Length );
		}
		else
		{
			Marshal.WriteByte( (nint)mpItemFlagsString, 0 );
		}
	}

	private static void SetCofferJobsString( ItemInfo itemInfo )
	{
		SeStringBuilder str = new();

		if( itemInfo != null )
		{
			if( mConfiguration.mCombineCofferJobs )
			{
				var GCJobs = mConfiguration.mShowGCCofferJobs ? itemInfo.CofferGCJobs : null;
				var leveJobs = mConfiguration.mShowLeveCofferJobs ? itemInfo.CofferLeveJobs : null;
				var combinedJobs = GCJobs?.Union( leveJobs ?? new() ) ?? leveJobs?.Union( GCJobs ?? new() );
				if( combinedJobs?.Count() > 0 )
				{
					str.AddIcon( BitmapFontIcon.GoldStar );
					str.AddText( " - " );
					ClassJobUtils.GetIconStringForJobs( ref str, combinedJobs, true, true );
				}
			}
			else
			{
				if( itemInfo.CofferGCJobs?.Count > 0 && mConfiguration.mShowGCCofferJobs )
				{
					//***** TODO: Can't find the player's GC in Dalamud anywhere (and not cleanly in ClientStructs).  Figure that out instead of using a manual config option.
					var GCIcon = mConfiguration.mGrandCompany switch
					{
						2 => BitmapFontIcon.BlackShroud,
						3 => BitmapFontIcon.Thanalan,
						_ => BitmapFontIcon.LaNoscea
					};

					str.AddIcon( GCIcon );
					str.AddText( " - " );
					ClassJobUtils.GetIconStringForJobs( ref str, itemInfo.CofferGCJobs, true, true );
				}
				if( itemInfo.CofferLeveJobs?.Count > 0 && mConfiguration.mShowLeveCofferJobs )
				{
					str.Add( new NewLinePayload() );
					str.AddIcon( BitmapFontIcon.Dice );
					str.AddText( " - " );
					ClassJobUtils.GetIconStringForJobs( ref str, itemInfo.CofferLeveJobs, true, true );
				}
			}
		}

		byte[] encodedStr = str.BuiltString.EncodeWithNullTerminator();
		if( encodedStr.Length <= mCofferJobsStringMaxLength )
		{
			Marshal.Copy( encodedStr, 0, (nint)mpCofferJobsString, encodedStr.Length );
		}
		else
		{
			Marshal.WriteByte( (nint)mpCofferJobsString, 0 );
		}
	}

	private static Int32 mCurrentItemID = 0;
	internal static ItemInfo CurrentItemInfo { get; private set; } = null;

	private static Configuration mConfiguration = null;

	private static byte* mpItemFlagsString = null;
	private const int mItemFlagsStringMaxLength = 255;	//	Doesn't matter that much; just something that will stay out of the way, but is not insane.

	private static byte* mpCofferJobsString = null;
	private const int mCofferJobsStringMaxLength = 1023;	//	Doesn't matter that much; just something that will stay out of the way, but is not insane.

	//	Note: Node IDs only need to be unique within a given addon.
	internal const uint mItemFlagsTextNodeID = 0x6C38B300;	//YOLO hoping for no collisions.
	internal const uint mCofferJobsTextNodeID = 0x6C38B400;	//YOLO hoping for no collisions.
	internal const uint mTitleBarResNodeID = 17;
	internal const uint mIconsContainerResNodeID = 24;
	internal const uint mItemFlagsRightmostIconResNodeID = 29;
	internal const uint mItemFlagsLeftmostIconResNodeID = 25;
	internal const uint mItemQuantityTextNodeID = 33;
}
