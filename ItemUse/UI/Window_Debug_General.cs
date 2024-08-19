using System;
using System.Numerics;

using CheapLoc;

using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

using ImGuiNET;

namespace ItemUse;

internal sealed class Window_Debug_General : Window, IDisposable
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

		ImGuiHelpers.ScaledDummy( ImGuiUtils.SectionSpacingSize );

		ImGui.Text( $"Flag Update Time: {ItemDetailHandler.DEBUG_FlagUpdateTime_uSec}μs" );
		ImGui.Text( $"Total Item Detail Hook Time: {ItemDetailHandler.DEBUG_HookTime_uSec}μs" );
		ImGui.Text( $"Coffer String Generation Time: {ItemDetailHandler.DEBUG_CofferStringTime_uSec}μs" );
		ImGui.Text( $"Text Highlighting Time: {ItemDetailHandler.DEBUG_TextHighlightTime_uSec}μs" );
		ImGui.Text( $"Item Info Update Time: {ItemDetailHandler.DEBUG_ItemInfoUpdateTime_uSec}μs" );

		ImGuiHelpers.ScaledDummy( ImGuiUtils.SectionSpacingSize );

		ImGui.Text( $"Cached Items: {ItemCategorizer.DEBUG_ItemCacheCount}" );
	}

	private readonly Plugin mPlugin;
	private readonly PluginUI mPluginUI;
	private readonly Configuration mConfiguration;
}
