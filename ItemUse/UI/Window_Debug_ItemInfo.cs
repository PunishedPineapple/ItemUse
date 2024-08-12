using System;
using System.Numerics;

using CheapLoc;

using Dalamud.Interface.Windowing;

using Dalamud;
using Dalamud.Data;
using Dalamud.Game.Text;
using Dalamud.Utility;

using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

using ImGuiNET;

namespace ItemUse;

public class Window_Debug_ItemInfo : Window, IDisposable
{
	public Window_Debug_ItemInfo( Plugin plugin, PluginUI pluginUI, Configuration configuration ) :
		base( Loc.Localize( "Window Title - Debug Item Info", "\"Item Use\" Debug Data - Item Info" ) + "###ItemInfoDebugWindow" )
	{
		Flags = ImGuiWindowFlags.NoCollapse;

		Size = new Vector2( 232, 90 );
		SizeCondition = ImGuiCond.FirstUseEver;
	}

	public void Dispose()
	{

	}

	public override void Draw()
	{
		var itemInfo = ItemDetailHandler.CurrentItemInfo;

		if( itemInfo != null )
		{
			var itemSheet = DalamudAPI.DataManager.GetExcelSheet<Item>();
			var itemName = itemSheet?.GetRow( (UInt32)itemInfo.ItemID )?.Name ?? "Unknown";

			ImGui.Text( $"Item ID: {itemInfo.ItemID}" );
			ImGui.Text( $"Item Name: {itemName}" );
			ImGui.Text( $"GC: {itemInfo.IsGCItem}" );
			ImGui.Text( $"Leve: {itemInfo.IsLeveItem}" );
			ImGui.Text( $"Crafting: {itemInfo.IsCraftingMaterial}" );
			ImGui.Text( $"Aquarium: {itemInfo.IsAquariumFish}" );
		}
		else
		{
			ImGui.Text( "No item hovered." );
		}
	}

	private Plugin mPlugin;
	private PluginUI mPluginUI;
	private Configuration mConfiguration;
}
