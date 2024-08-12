using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;

using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ItemUse;

internal unsafe static class ItemDetailHandler
{
	internal static void Init()
	{
		DalamudAPI.AddonLifecycle.RegisterListener( AddonEvent.PostUpdate, "ItemDetail", ItemDetailUpdateCallback );
	}

	internal static void Uninit()
	{

	}

	private static void ItemDetailUpdateCallback( AddonEvent type, AddonArgs args )
	{
		UpdateCurrentItemInfo();
		UpdateItemFlagsTextNode( GetItemFlagsString() );
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

	private unsafe static void UpdateItemFlagsTextNode( string str, bool show = true )
	{
		AtkUnitBase* pAddon = (AtkUnitBase*)DalamudAPI.GameGui.GetAddonByName( "ItemDetail" );
		AtkTextNode* pNode = null;
		AtkResNode* pTitleBarNode = null;
		AtkResNode* pIconsContainerNode = null;
		AtkResNode* pOtherFlagsLeftmostNode = null;
		AtkResNode* pOtherFlagsRightmostNode = null;

		if( pAddon != null )
		{
			//	Find our node by ID.  Doing this allows us to not have to deal with freeing the node resources and removing connections to sibling nodes (we'll still leak, but only once).
			pNode = AtkNodeHelpers.GetTextNodeByID( pAddon, mItemFlagsTextNodeID );
			pTitleBarNode = pAddon->GetNodeById( mTitleBarResNodeID );
			pIconsContainerNode = pAddon->GetNodeById( mIconsContainerResNodeID );
			pOtherFlagsLeftmostNode = pAddon->GetNodeById( mItemFlagsLeftmostIconResNodeID );
			pOtherFlagsRightmostNode = pAddon->GetNodeById( mItemFlagsRightmostIconResNodeID );

			//	If we have our node, set the colors, size, and text from settings.
			if( pNode != null )
			{
				bool haveRequiredNodes = pTitleBarNode != null && pIconsContainerNode != null && pOtherFlagsLeftmostNode != null && pOtherFlagsRightmostNode != null;
				bool otherIconsVisible =  haveRequiredNodes && pTitleBarNode->IsVisible() && pIconsContainerNode->IsVisible() && pOtherFlagsLeftmostNode->IsVisible();
				bool visible = show && haveRequiredNodes;

				pNode->ToggleVisibility( visible );

				if( visible )
				{
					int xPosBase = pTitleBarNode->GetXShort() + pIconsContainerNode->GetXShort();
					int yPosBase = pTitleBarNode->GetYShort() + pIconsContainerNode->GetYShort();

					int xPos = pOtherFlagsLeftmostNode->IsVisible() ? pOtherFlagsLeftmostNode->GetXShort() : ( pOtherFlagsRightmostNode->GetXShort() + pOtherFlagsRightmostNode->Width );
					int yPos = pOtherFlagsLeftmostNode->GetYShort() + pOtherFlagsLeftmostNode->GetHeight();

					pNode->AtkResNode.SetPositionShort( (short)(xPosBase + xPos - pNode->Width), (short)(yPosBase + yPos - pNode->Height) );

					pNode->AtkResNode.Color.A = 255;

					pNode->TextColor.A = 255;
					pNode->TextColor.R = 255;
					pNode->TextColor.G = 255;
					pNode->TextColor.B = 255;

					pNode->EdgeColor.A = 255;
					pNode->EdgeColor.R = 0;
					pNode->EdgeColor.G = 0;
					pNode->EdgeColor.B = 0;

					pNode->FontSize = 14;
					pNode->AlignmentType = AlignmentType.BottomRight;
					pNode->FontType = FontType.Axis;
					pNode->LineSpacing = 1;
					pNode->CharSpacing = 1;

					pNode->SetText( str );
				}
			}
			//	Set up the node if it hasn't been.
			else if( pAddon->RootNode != null )
			{
				pNode = AtkNodeHelpers.CreateNewTextNode( pAddon, mItemFlagsTextNodeID );
			}
		}
	}

	private static string GetItemFlagsString()
	{
		return GetItemFlagsString( CurrentItemInfo );
	}

	//***** TODO: Eventually get rid of this and make them icons.
	private static string GetItemFlagsString( ItemInfo itemInfo )
	{
		string str = "";

		if( itemInfo?.IsGCItem ?? false ) str += "G";
		if( itemInfo?.IsLeveItem ?? false ) str += "L";
		if( itemInfo?.IsCraftingMaterial ?? false ) str += "C";
		if( itemInfo?.IsAquariumFish ?? false ) str += "A";

		return str;
	}

	private static Int32 mCurrentItemID = 0;
	internal static ItemInfo CurrentItemInfo { get; private set; } = null;

	//	Note: Node IDs only need to be unique within a given addon.
	internal const uint mItemFlagsTextNodeID = 0x6C38B300;    //YOLO hoping for no collisions.
	internal const uint mTitleBarResNodeID = 17;
	internal const uint mIconsContainerResNodeID = 24;
	internal const uint mItemFlagsRightmostIconResNodeID = 29;
	internal const uint mItemFlagsLeftmostIconResNodeID = 25;
}
