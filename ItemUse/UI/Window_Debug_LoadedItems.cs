using System;
using System.Numerics;

using CheapLoc;

using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

using ImGuiNET;

using Lumina.Excel.Sheets;

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

	private void DrawGCItems()
	{
		var itemSheet = DalamudAPI.DataManager.GetExcelSheet<Item>();

		foreach( var item in ItemCategorizer.DEBUG_GetGCItems() )
		{
			ImGui.Text( $"{item}: {itemSheet?.GetRow( (uint)item ).Name.ToString()}" );
		}
	}

	private void DrawLeveItems()
	{
		var itemSheet = DalamudAPI.DataManager.GetExcelSheet<Item>();

		foreach( var item in ItemCategorizer.DEBUG_GetLeveItems() )
		{
			ImGui.Text( $"{item}: {itemSheet?.GetRow( (uint)item ).Name.ToString()}" );
		}
	}

	private void DrawEhcatlItems()
	{
		var itemSheet = DalamudAPI.DataManager.GetExcelSheet<Item>();

		foreach( var item in ItemCategorizer.DEBUG_GetEhcatlItems() )
		{
			ImGui.Text( $"{item}: {itemSheet?.GetRow( (uint)item ).Name.ToString()}" );
		}
	}

	private void DrawCraftingMaterials()
	{
		var itemSheet = DalamudAPI.DataManager.GetExcelSheet<Item>();

		foreach( var item in ItemCategorizer.DEBUG_GetCraftingMaterials() )
		{
			ImGui.Text( $"{item}: {itemSheet?.GetRow( (uint)item ).Name.ToString()}" );
		}
	}

	private void DrawAquariumFish()
	{
		var itemSheet = DalamudAPI.DataManager.GetExcelSheet<Item>();

		foreach( var item in ItemCategorizer.DEBUG_GetAquariumFish() )
		{
			ImGui.Text( $"{item}: {itemSheet?.GetRow( (uint)item ).Name.ToString()}" );
		}
	}

	private void DrawCofferManifests()
	{
		var itemSheet = DalamudAPI.DataManager.GetExcelSheet<Item>();

		if( itemSheet == null )
		{
			ImGui.Text( "Item sheet is null!" );
			return;
		}

		foreach( var cofferManifest in CofferManifests.DEBUG_GetCofferManifests() )
		{
			string str = mResolveCofferManifestItemNames && itemSheet.HasRow( (uint)cofferManifest.Key) ? itemSheet.GetRow( (uint)cofferManifest.Key ).Name.ToString() : cofferManifest.Key.ToString();
			str += ": ";

			foreach( var item in cofferManifest.Value )
			{
				str += mResolveCofferManifestItemNames && itemSheet.HasRow( (uint)item ) ? itemSheet.GetRow( (uint)item ).Name.ToString() : item.ToString();
				str += ", ";
			}

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
