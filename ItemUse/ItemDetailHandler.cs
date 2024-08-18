using System;
using System.Linq;
using System.Runtime.InteropServices;

using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;

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

		DalamudAPI.GameGui.HoveredItemChanged += OnHoveredItemChanged;

		IntPtr fpGenerateItemDetail = DalamudAPI.SigScanner.ScanText( "48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC 50 48 8B 42 20" );
		if( fpGenerateItemDetail != IntPtr.Zero )
		{
			DalamudAPI.PluginLog.Information( $"GenerateItemDetail function signature found at 0x{fpGenerateItemDetail:X}." );
			mGenerateItemDetailHook = DalamudAPI.GameInteropProvider.HookFromAddress<GenerateItemDetailDelegate>( fpGenerateItemDetail, GenerateItemDetailDetour );
			mGenerateItemDetailHook?.Enable();
		}
		else
		{
			throw new Exception( "Unable to find the specified function signature for GenerateItemDetail." );
		}

		DalamudAPI.AddonLifecycle.RegisterListener( AddonEvent.PostUpdate, "ItemDetail", ItemDetailUpdateCallback );
	}

	internal static void Uninit()
	{
		mGenerateItemDetailHook?.Disable();
		mGenerateItemDetailHook?.Dispose();

		DalamudAPI.AddonLifecycle.UnregisterListener( AddonEvent.PostUpdate, "ItemDetail", ItemDetailUpdateCallback );
		DalamudAPI.GameGui.HoveredItemChanged -= OnHoveredItemChanged;
		AtkNodeHelpers.RemoveTextNode( (AtkUnitBase*)DalamudAPI.GameGui.GetAddonByName( "ItemDetail" ), mItemFlagsTextNodeID );

		if( mpItemFlagsString != null ) Marshal.FreeHGlobal( (nint)mpItemFlagsString );
		mConfiguration = null;
	}

	private static void ItemDetailUpdateCallback( AddonEvent type, AddonArgs args )
	{
		SetItemFlagsString( CurrentItemInfo );
		UpdateItemFlagsTextNode();
	}

	private static unsafe nint GenerateItemDetailDetour( AtkUnitBase* pAddonItemDetail, NumberArrayData* pNumberArrayData, StringArrayData* pStringArrayData )
	{
		try
		{
			DalamudAPI.PluginLog.Verbose( $"In GenerateItemDetail Detour.  Blocking update: {mBlockItemTooltip}" );
		}
		catch
		{
			//	If Logging threw, we can't really do anything about it besides make sure that the hook completes and disables.
			mGenerateItemDetailHook.Disable();
		}

		if( pAddonItemDetail != null &&
			pStringArrayData != null &&
			!mBlockItemTooltip )
		{
			try
			{
				var itemDescription = StringArrayDataHelpers.GetString( pStringArrayData, 13 );
				bool descriptionModified = false;

				if( mConfiguration.mHighlightCraftingMaterialText && CurrentItemInfo?.IsCraftingMaterial == true )
				{
					SeStringUtils.HighlightLastOccuranceOfText( ref itemDescription, LocalizationHelpers.CraftingMaterialTag );
					descriptionModified = true;
				}

				if( mConfiguration.mHighlightAquariumFishText && CurrentItemInfo?.IsAquariumFish == true )
				{
					//	This is pretty lame, but I cannot think of a better way to do it that isn't even messier when considering all client languages.
					SeStringUtils.HighlightLastOccuranceOfText( ref itemDescription, LocalizationHelpers.Grade1AquariumTag );
					SeStringUtils.HighlightLastOccuranceOfText( ref itemDescription, LocalizationHelpers.Grade2AquariumTag );
					SeStringUtils.HighlightLastOccuranceOfText( ref itemDescription, LocalizationHelpers.Grade3AquariumTag );
					SeStringUtils.HighlightLastOccuranceOfText( ref itemDescription, LocalizationHelpers.Grade4AquariumTag );
					descriptionModified = true;
				}

				if( mConfiguration.mShowGCCofferJobs && CurrentItemInfo?.CofferGCJobs != null ||
					mConfiguration.mShowLeveCofferJobs && CurrentItemInfo?.CofferLeveJobs != null )
				{
					itemDescription.Payloads.Add( new NewLinePayload() );
					itemDescription.Payloads.Add( new NewLinePayload() );
					itemDescription.Append( GetCofferJobsString( CurrentItemInfo ) );
					descriptionModified = true;
				}

				if( descriptionModified ) StringArrayDataHelpers.SetString( pStringArrayData, 13, itemDescription );
			}
			catch( Exception e )
			{
				mGenerateItemDetailHook.Disable();

				try
				{
					DalamudAPI.PluginLog.Error( $"Unknown error when generating item tooltip.  Disabling hook.  Error:\r\n{e}" );
				}
				catch
				{
					//	If Logging threw, we can't really do anything about it besides make sure that the hook completes.
				}
			}
		}

		if( mBlockItemTooltip ) mBlockItemTooltip = false;

		return mGenerateItemDetailHook.Original( pAddonItemDetail, pNumberArrayData, pStringArrayData );
	}

	private static void OnHoveredItemChanged( object sender, ulong e )
	{
		UpdateCurrentItemInfo( (Int32)e );

		if( mPreviousItemID == 0 && mCurrentItemID != 0 ) mBlockItemTooltip = true;
		else if( mPreviousItemID != 0 && mCurrentItemID == 0 ) mBlockItemTooltip = true;
		else mBlockItemTooltip = false;

		mPreviousItemID = mCurrentItemID;
	}

	internal static void UpdateCurrentItemInfo( Int32 itemID )
	{
		mCurrentItemID = itemID;

		if( mCurrentItemID <= 0 )
		{
			CurrentItemInfo = null;
		}
		else if( mCurrentItemID != CurrentItemInfo?.ItemID )
		{
			CurrentItemInfo = ItemCategorizer.GetItemInfo( mCurrentItemID );
		}

		DalamudAPI.PluginLog.Verbose( $"Hovered item updated to {mCurrentItemID}." );
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

	//***** TODO: Probably eventually get rid of this and make them real icons.
	private static void SetItemFlagsString( ItemInfo itemInfo )
	{
		SeStringBuilder str = new();

		if( mConfiguration.mShowGCItemsFlag && ( itemInfo?.IsGCItem ?? false ) ) str.AddIcon( GrandCompanyUtils.GetCurrentGCFontIcon() );
		if( mConfiguration.mShowLeveItemsFlag && ( itemInfo?.IsLeveItem ?? false ) ) str.AddIcon( BitmapFontIcon.Dice );
		if( mConfiguration.mShowEhcatlItemsFlag && ( itemInfo?.IsEhcatlItem ?? false ) ) str.AddIcon( BitmapFontIcon.FlyZone );
		if( mConfiguration.mShowCraftingMaterialsFlag && ( itemInfo?.IsCraftingMaterial ?? false ) ) str.AddIcon( BitmapFontIcon.Crafter );
		if( mConfiguration.mShowAquariumFishFlag && ( itemInfo?.IsAquariumFish ?? false ) ) str.AddIcon( BitmapFontIcon.Fisher );

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

	private static SeString GetCofferJobsString( ItemInfo itemInfo )
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
					str.AddText( ": " );
					ClassJobUtils.GetIconStringForJobs( ref str, combinedJobs, true, true );
					str.Add( new NewLinePayload() );
				}
			}
			else
			{
				if( itemInfo.CofferGCJobs?.Count > 0 && mConfiguration.mShowGCCofferJobs )
				{
					str.AddIcon( GrandCompanyUtils.GetCurrentGCFontIcon() );
					str.AddText( ": " );
					ClassJobUtils.GetIconStringForJobs( ref str, itemInfo.CofferGCJobs, true, true );
					str.Add( new NewLinePayload() );
				}
				if( itemInfo.CofferLeveJobs?.Count > 0 && mConfiguration.mShowLeveCofferJobs )
				{
					str.Add( new NewLinePayload() );
					str.AddIcon( BitmapFontIcon.Dice );
					str.AddText( ": " );
					ClassJobUtils.GetIconStringForJobs( ref str, itemInfo.CofferLeveJobs, true, true );
					str.Add( new NewLinePayload() );
				}
			}
		}

		return str.BuiltString;
	}

	private static Int32 mCurrentItemID = 0;
	private static Int32 mPreviousItemID = 0;

	internal static ItemInfo CurrentItemInfo { get; private set; } = null;

	private static Configuration mConfiguration = null;

	private static byte* mpItemFlagsString = null;
	private const int mItemFlagsStringMaxLength = 255;  //	Doesn't matter that much; just something that will stay out of the way, but is not insane.

	//	Lifted this logic from Allagan Tools in order to help prevent double-appending strings to the item description when initially hovering an item from nothing.
	private static bool mBlockItemTooltip;

	private unsafe delegate nint GenerateItemDetailDelegate( AtkUnitBase* pAddonItemDetail, NumberArrayData* pNumberArrayData, StringArrayData* pStringArrayData );
	private static Hook<GenerateItemDetailDelegate> mGenerateItemDetailHook = null;

	//	Note: Node IDs only need to be unique within a given addon.
	internal const uint mItemFlagsTextNodeID = 0x6C38B300;	//YOLO hoping for no collisions.
	internal const uint mTitleBarResNodeID = 17;
	internal const uint mIconsContainerResNodeID = 24;
	internal const uint mItemFlagsRightmostIconResNodeID = 29;
	internal const uint mItemFlagsLeftmostIconResNodeID = 25;
	internal const uint mItemQuantityTextNodeID = 33;
}
