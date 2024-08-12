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

public class Window_Settings : Window, IDisposable
{
	public Window_Settings( Plugin plugin, PluginUI pluginUI, Configuration configuration ) :
		base( Loc.Localize( "Window Title - Settings", "\"Item Use\" Settings" ) + "###ItemInfoSettingsWindow" )
	{
		Flags = ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse;

		mPlugin = plugin;
		mPluginUI = pluginUI;
		mConfiguration = configuration;
	}

	public void Dispose()
	{

	}

	public override void Draw()
	{
		ImGui.Text( "TODO: Placeholder for settings." );

		ImGui.Spacing();
		ImGui.Spacing();
		ImGui.Spacing();
		ImGui.Spacing();
		ImGui.Spacing();

		if( ImGui.Button( Loc.Localize( "Button: Save", "Save" ) + "###Save Button" ) )
		{
			mConfiguration.Save();
		}
		ImGui.SameLine();
		if( ImGui.Button( Loc.Localize( "Button: Save and Close", "Save and Close" ) + "###Save and Close" ) )
		{
			mConfiguration.Save();
			Toggle();
		}
	}

	private Plugin mPlugin;
	private PluginUI mPluginUI;
	private Configuration mConfiguration;
}
