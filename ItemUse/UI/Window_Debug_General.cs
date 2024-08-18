using System;
using System.Numerics;

using CheapLoc;

using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

using ImGuiNET;

using Lumina.Excel.GeneratedSheets;

namespace ItemUse;

public class Window_Debug_General : Window, IDisposable
{
	public Window_Debug_General( Plugin plugin, PluginUI pluginUI, Configuration configuration ) :
		base( Loc.Localize( "Window Title - Debug General", "\"Item Use\" Debug Data - General" ) + "###GeneralDebugWindow" )
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

		ImGuiHelpers.ScaledDummy( ImGuiUtils.SectionSpacingSize );

		ImGui.Text( $"Current Grand Company: {GrandCompanyUtils.GetCurrentGC()}" );
	}

	private readonly Plugin mPlugin;
	private readonly PluginUI mPluginUI;
	private readonly Configuration mConfiguration;
}
