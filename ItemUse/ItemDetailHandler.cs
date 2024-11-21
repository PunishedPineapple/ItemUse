using System;
using System.Collections.Immutable;
using System.Diagnostics;
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

		//	Sig and delegate stolen from Allagan Tools.
		IntPtr fpGenerateItemDetail = DalamudAPI.SigScanner.ScanText( "48 89 5C 24 ?? 55 56 57 41 54 41 55 41 56 41 57 48 83 EC 50 48 8B 42 28" );
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
		mFlagUpdateStopwatch.Restart();
		SetItemFlagsString( CurrentItemInfo );
		UpdateItemFlagsTextNode();
		mFlagUpdateStopwatch.Stop();
	}

	private static unsafe nint GenerateItemDetailDetour( AtkUnitBase* pAddonItemDetail, NumberArrayData* pNumberArrayData, StringArrayData* pStringArrayData )
	{
		mHookTimeStopwatch.Restart();

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
			pStringArrayData != null /*&&
			!mBlockItemTooltip*/ )
		{
			try
			{
				var itemDescription = StringArrayDataHelpers.GetString( pStringArrayData, 13 );
				bool descriptionModified = false;

				mTextHighlightStopwatch.Restart();

				if( mConfiguration.mHighlightCraftingMaterialText && CurrentItemInfo?.IsCraftingMaterial == true )
				{
					SeStringUtils.HighlightLastOccuranceOfText( ref itemDescription, LocalizationHelpers.CraftingMaterialTag, mConfiguration?.mHighlightCraftingMaterialTextColor ?? 500, mConfiguration?.mHighlightCraftingMaterialGlowColor ?? 501 );
					descriptionModified = true;
				}

				if( mConfiguration.mHighlightAquariumFishText && CurrentItemInfo?.IsAquariumFish == true )
				{
					//	This is pretty lame, but I cannot think of a better way to do it that isn't even messier when considering all client languages.
					SeStringUtils.HighlightLastOccuranceOfText( ref itemDescription, LocalizationHelpers.Grade1AquariumTag, mConfiguration?.mHighlightAquariumFishTextColor ?? 500, mConfiguration?.mHighlightAquariumFishGlowColor ?? 501 );
					SeStringUtils.HighlightLastOccuranceOfText( ref itemDescription, LocalizationHelpers.Grade2AquariumTag, mConfiguration?.mHighlightAquariumFishTextColor ?? 500, mConfiguration?.mHighlightAquariumFishGlowColor ?? 501 );
					SeStringUtils.HighlightLastOccuranceOfText( ref itemDescription, LocalizationHelpers.Grade3AquariumTag, mConfiguration?.mHighlightAquariumFishTextColor ?? 500, mConfiguration?.mHighlightAquariumFishGlowColor ?? 501 );
					SeStringUtils.HighlightLastOccuranceOfText( ref itemDescription, LocalizationHelpers.Grade4AquariumTag, mConfiguration?.mHighlightAquariumFishTextColor ?? 500, mConfiguration?.mHighlightAquariumFishGlowColor ?? 501 );
					descriptionModified = true;
				}

				mTextHighlightStopwatch.Stop();

				mCofferStringStopwatch.Restart();

				if( mConfiguration.mShowGCCofferJobs && CurrentItemInfo?.CofferGCJobs != null ||
					mConfiguration.mShowLeveCofferJobs && CurrentItemInfo?.CofferLeveJobs != null )
				{
					itemDescription.Payloads.Add( new NewLinePayload() );
					itemDescription.Payloads.Add( new NewLinePayload() );
					itemDescription.Append( GetCofferJobsString( CurrentItemInfo ) );
					descriptionModified = true;
				}

				mCofferStringStopwatch.Stop();

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

		mHookTimeStopwatch.Stop();

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
		mItemInfoUpdateStopwatch.Restart();

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

		mItemInfoUpdateStopwatch.Stop();
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
				var combinedJobs = GCJobs?.Union( leveJobs ?? ImmutableHashSet<Int32>.Empty ) ?? leveJobs?.Union( GCJobs ?? ImmutableHashSet<Int32>.Empty );
				if( combinedJobs?.Count > 0 )
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

	private static readonly Stopwatch mFlagUpdateStopwatch = new();
	private static readonly Stopwatch mHookTimeStopwatch = new();
	private static readonly Stopwatch mCofferStringStopwatch = new();
	private static readonly Stopwatch mTextHighlightStopwatch = new();
	private static readonly Stopwatch mItemInfoUpdateStopwatch = new();

	internal static long DEBUG_FlagUpdateTime_uSec => mFlagUpdateStopwatch.ElapsedMicroseconds();
	internal static long DEBUG_HookTime_uSec => mHookTimeStopwatch.ElapsedMicroseconds();
	internal static long DEBUG_CofferStringTime_uSec => mCofferStringStopwatch.ElapsedMicroseconds();
	internal static long DEBUG_TextHighlightTime_uSec => mTextHighlightStopwatch.ElapsedMicroseconds();
	internal static long DEBUG_ItemInfoUpdateTime_uSec => mItemInfoUpdateStopwatch.ElapsedMicroseconds();

	//	Note: Node IDs only need to be unique within a given addon.
	private const uint mItemFlagsTextNodeID = 0x6C38B300;	//YOLO hoping for no collisions.
	private const uint mTitleBarResNodeID = 17;
	private const uint mIconsContainerResNodeID = 24;
	private const uint mItemFlagsRightmostIconResNodeID = 29;
	private const uint mItemFlagsLeftmostIconResNodeID = 25;
	private const uint mItemQuantityTextNodeID = 33;
}
