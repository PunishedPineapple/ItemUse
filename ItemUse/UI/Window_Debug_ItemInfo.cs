using System;
using System.Numerics;

using CheapLoc;

using Dalamud.Interface.Windowing;

using ImGuiNET;

using Lumina.Excel.GeneratedSheets;

namespace ItemUse;

public class Window_Debug_ItemInfo : Window, IDisposable
{
	public Window_Debug_ItemInfo( Plugin plugin, PluginUI pluginUI, Configuration configuration ) :
		base( Loc.Localize( "Window Title - Debug Item Info", "\"Item Use\" Debug Data - Item Info" ) + "###ItemInfoDebugWindow" )
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
		if( ImGui.Button( "Export Localizable Strings" ) )
		{
			mPlugin.ExportLocalizableStrings();
		}

		var itemInfo = ItemDetailHandler.CurrentItemInfo;

		if( itemInfo != null )
		{
			var itemSheet = DalamudAPI.DataManager.GetExcelSheet<Item>();
			var classJobsSheet = DalamudAPI.DataManager.GetExcelSheet<ClassJob>();

			//***** TODO: Handle HQ items.

			var itemName = itemSheet?.GetRow( (UInt32)itemInfo.ItemID )?.Name ?? "Unknown";

			ImGui.Text( $"Item ID: {itemInfo.ItemID}" );
			ImGui.Text( $"Item Name: {itemName}" );
			ImGui.Text( $"GC: {itemInfo.IsGCItem}" );
			ImGui.Text( $"Leve: {itemInfo.IsLeveItem}" );
			ImGui.Text( $"Ehcatl: {itemInfo.IsEhcatlItem}" );
			ImGui.Text( $"Crafting: {itemInfo.IsCraftingMaterial}" );
			ImGui.Text( $"Aquarium: {itemInfo.IsAquariumFish}" );

			var jobs = ItemCategorizer.GetJobsForItem( itemInfo.ItemID );
			string str = "";
			foreach( var job in jobs )
			{
				str += classJobsSheet.GetRow( (uint)job ).Abbreviation + ", ";
			}
			ImGui.Text( $"Jobs: {str}" );

			if( CofferManifests.ItemIsKnownCoffer( itemInfo.ItemID ) )
			{
				string cofferItemsStr = "";
				foreach( var item in CofferManifests.GetCofferItems( itemInfo.ItemID ) )
				{
					cofferItemsStr += itemSheet.GetRow( (uint)item ).Singular.ToString() + "\r\n";
				}
				ImGui.Text( $"Coffer Items:\r\n{cofferItemsStr}" );
			}

			if( itemInfo.CofferGCJobs != null )
			{
				string cofferGCJobsStr = "";
				foreach( var job in itemInfo.CofferGCJobs )
				{
					cofferGCJobsStr += classJobsSheet.GetRow( (uint)job ).Abbreviation + "\r\n";
				}
				ImGui.Text( $"GC Jobs:\r\n{cofferGCJobsStr}" );
			}

			if( itemInfo.CofferLeveJobs != null )
			{
				string cofferLeveJobsStr = "";
				foreach( var job in itemInfo.CofferLeveJobs )
				{
					cofferLeveJobsStr += classJobsSheet.GetRow( (uint)job ).Abbreviation + "\r\n";
				}
				ImGui.Text( $"Leve Jobs:\r\n{cofferLeveJobsStr}" );
			}
		}
		else
		{
			ImGui.Text( "No item hovered." );
		}
	}

	private readonly Plugin mPlugin;
	private readonly PluginUI mPluginUI;
	private readonly Configuration mConfiguration;
}
