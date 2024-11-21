using System;
using System.Numerics;

using CheapLoc;

using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

using ImGuiNET;

using Lumina.Excel.Sheets;

namespace ItemUse;

internal sealed class Window_Debug_ClassJob : Window, IDisposable
{
	public Window_Debug_ClassJob( Plugin plugin, PluginUI pluginUI, Configuration configuration ) :
		base( Loc.Localize( "Window Title - Debug ClassJob", "\"Item Use\" Debug Data - ClassJob" ) + "###ClassJobDebugWindow" )
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
		var classjobCategorySheet = DalamudAPI.DataManager.GetExcelSheet<ClassJobCategory_Alternate>();
		var classjobSheet = DalamudAPI.DataManager.GetExcelSheet<ClassJob>();

		ImGui.Checkbox( "Resolve ClassJob Abbreviations", ref mResolveClassJobAbbreviations );

		ImGuiHelpers.ScaledDummy( ImGuiUtils.SectionSpacingSize );

		ImGui.Text( "ClassJob Info (Alt):" );

		ImGuiHelpers.ScaledDummy( ImGuiUtils.SectionSpacingSize );

		foreach( var row in classjobCategorySheet )
		{
			string str = $"Row: {row.RowId}, Name: {row.Name}, ClassJobs: ";
			for( int i = 0; i < row.ClassJobColumnCount; ++i )
			{
				if( row.IncludesClassJob( i ) )
				{
					if( mResolveClassJobAbbreviations && classjobSheet.TryGetRow( (uint)i, out var classJobRow ) )
					{
						str += $"{classJobRow.Abbreviation.ExtractText()}, ";
					}
					else
					{
						str += $"{i}, ";
					}
				}
			}
			ImGui.Text( str );
		}
	}

	private bool mResolveClassJobAbbreviations;

	private readonly Plugin mPlugin;
	private readonly PluginUI mPluginUI;
	private readonly Configuration mConfiguration;
}
