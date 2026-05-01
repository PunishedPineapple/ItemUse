using System;
using System.Numerics;

using CheapLoc;

using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

namespace ItemUse;

internal sealed class Window_Debug_LoadedItems : Window, IDisposable
{
	public Window_Debug_LoadedItems( Plugin plugin, PluginUI pluginUI, Configuration configuration ) :
		base( Loc.Localize( "Window Title - Loaded Items", "\"Item Use\" Debug Data - Loaded Items" ) + "###LoadedItemsDebugWindow" )
	{
		Flags = ImGuiWindowFlags.NoCollapse;

		Size = new Vector2( 232, 90 );
		SizeCondition = ImGuiCond.FirstUseEver;

		mPlugin = plugin;
		mPluginUI = pluginUI;
		mConfiguration = configuration;
	}

	public void Dispose()
	{

	}

	public override void Draw()
	{
		ImGui.Combo( "Item Type", ref mSelectedItemType, mItemTypeComboStrings, mItemTypeComboStrings.Length );

		ImGuiHelpers.ScaledDummy( ImGuiUtils.SectionSpacingSize );

		switch( mSelectedItemType )
		{
			case 0:
				DrawGCItems();
				break;
			case 1:
				DrawLeveItems();
				break;
			case 2:
				DrawEhcatlItems();
				break;
			case 3:
				DrawCraftingMaterials();
				break;
			case 4:
				DrawAquariumFish();
				break;
			case 5:
				ImGui.Checkbox( "Resolve Coffer Manifest Names", ref mResolveCofferManifestItemNames );
				ImGuiHelpers.ScaledDummy( ImGuiUtils.SectionSpacingSize );
				DrawCofferManifests();
				break;
		}
	}

	private void DrawDebugItem( Int32 itemId )
	{
		if( ImGui.SmallButton( $"Link###ItemId_{itemId}" ) )
		{
			DalamudAPI.ChatGui.Print( new()
			{
				Type = Dalamud.Game.Text.XivChatType.SystemMessage,
				Message = SeString.CreateItemLink( (uint)itemId ),
			} );
		}
		ImGui.SameLine();
		ImGui.Text( $"{itemId}: {ItemUtils.GetUnformattedName( (uint)itemId )}" );
	}

	private void DrawGCItems()
	{
		foreach( var item in ItemCategorizer.DEBUG_GetGCItems() ) DrawDebugItem( item );
	}

	private void DrawLeveItems()
	{
		foreach( var item in ItemCategorizer.DEBUG_GetLeveItems() ) DrawDebugItem( item );
	}

	private void DrawEhcatlItems()
	{
		foreach( var item in ItemCategorizer.DEBUG_GetEhcatlItems() ) DrawDebugItem( item );
	}

	private void DrawCraftingMaterials()
	{
		foreach( var item in ItemCategorizer.DEBUG_GetCraftingMaterials() ) DrawDebugItem( item );
	}

	private void DrawAquariumFish()
	{
		foreach( var item in ItemCategorizer.DEBUG_GetAquariumFish() ) DrawDebugItem( item );
	}

	private void DrawCofferManifests()
	{
		foreach( var cofferManifest in CofferManifests.DEBUG_GetCofferManifests() )
		{
			if( ImGui.SmallButton( $"Link###ItemId_{cofferManifest.Key}" ) )
			{
				DalamudAPI.ChatGui.Print( new()
				{
					Type = Dalamud.Game.Text.XivChatType.SystemMessage,
					Message = SeString.CreateItemLink( (uint)cofferManifest.Key ),
				} );
			}

			string str = mResolveCofferManifestItemNames ? ItemUtils.GetUnformattedName( (uint)cofferManifest.Key ) : cofferManifest.Key.ToString();
			str += ": ";

			foreach( var item in cofferManifest.Value )
			{
				str += mResolveCofferManifestItemNames ? ItemUtils.GetUnformattedName( (uint)item ) : item.ToString();
				str += ", ";
			}

			ImGui.SameLine();
			ImGui.Text( str );
		}
	}

	//	Not going through the work of getting a proper Enum and extensions working with ImGui just for a quick debug window.
	private readonly string[] mItemTypeComboStrings = ["GC Items", "Leve Items", "Ehcatl Items", "Crafting Materials", "Aquarium Fish", "Coffer Manifests"];
	private int mSelectedItemType = 0;

	private bool mResolveCofferManifestItemNames = false;

	private readonly Plugin mPlugin;
	private readonly PluginUI mPluginUI;
	private readonly Configuration mConfiguration;
}
