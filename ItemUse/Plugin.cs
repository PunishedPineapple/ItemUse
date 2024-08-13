using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using CheapLoc;

using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Memory;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

using FFXIVClientStructs.FFXIV.Client.Game.Object;

using ItemUse;

using Lumina.Excel.GeneratedSheets;

namespace ItemUse;

public sealed class Plugin : IDalamudPlugin
{
	//	Initialization
	public Plugin( IDalamudPluginInterface pluginInterface )
	{
		//	API Access
		pluginInterface.Create<DalamudAPI>();
		mPluginInterface = pluginInterface;
		
		//	Configuration
		mConfiguration = mPluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
		mConfiguration.Initialize( mPluginInterface );

		//	Localization and Command Initialization
		OnLanguageChanged( mPluginInterface.UiLanguage );

		//	Other Initialization
		ItemCategorizer.Init();
		ItemDetailHandler.Init( mConfiguration );

		//	UI Initialization
		mUI = new PluginUI( this, mConfiguration, pluginInterface );
		mPluginInterface.UiBuilder.Draw += DrawUI;
		mPluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

		//	Event Subscription
		mPluginInterface.LanguageChanged += OnLanguageChanged;
		DalamudAPI.Framework.Update += OnGameFrameworkUpdate;
	}

	//	Cleanup
	public void Dispose()
	{
		DalamudAPI.Framework.Update -= OnGameFrameworkUpdate;
		mPluginInterface.LanguageChanged -= OnLanguageChanged;
		mPluginInterface.UiBuilder.Draw -= DrawUI;
		mPluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUI;
		DalamudAPI.CommandManager.RemoveHandler( TextCommandName );

		ItemDetailHandler.Uninit();
		ItemCategorizer.Uninit();

		mUI?.Dispose();
	}

	private void OnLanguageChanged( string langCode )
	{
		var allowedLang = new List<string>{ /*"es", "fr", "ja"*/ };

		DalamudAPI.PluginLog.Information( "Trying to set up Loc for culture {0}", langCode );

		if( allowedLang.Contains( langCode ) )
		{
			Loc.Setup( File.ReadAllText( Path.Join( Path.Join( mPluginInterface.AssemblyLocation.DirectoryName, "Resources\\Localization\\" ), $"loc_{langCode}.json" ) ) );
		}
		else
		{
			Loc.SetupWithFallbacks();
		}

		//	Set up the command handler with the current language.
		if( DalamudAPI.CommandManager.Commands.ContainsKey( TextCommandName ) )
		{
			DalamudAPI.CommandManager.RemoveHandler( TextCommandName );
		}
		DalamudAPI.CommandManager.AddHandler( TextCommandName, new CommandInfo( ProcessTextCommand )
		{
			HelpMessage = Loc.Localize( "Plugin Text Command Description", "Opens the settings window." )
		} );
	}

	private void ProcessTextCommand( string command, string args )
	{
		if( args.ToLower() == SubcommandName_Debug.ToLower() )
		{
			mUI.Window_Debug_ItemInfo.Toggle();
		}
		else
		{
			ToggleConfigUI();
		}
	}

	private void DrawUI()
	{
		mUI.Draw();
	}

	private void ToggleConfigUI()
	{
		mUI.Window_Settings.Toggle();
	}

	unsafe private void OnGameFrameworkUpdate( IFramework framework )
	{
		if( !DalamudAPI.ClientState.IsLoggedIn ) return;
	}

	unsafe private static SeString SeStringDeepCopy( SeString str )
	{
		var bytes = str.Encode();
		SeString newStr;
		fixed( byte* pBytes = bytes )
		{
			newStr = MemoryHelper.ReadSeStringNullTerminated( new IntPtr( pBytes ) );
		}
		newStr ??= SeString.Empty;
		return newStr;
	}

	public static string Name => "Item Use";
	internal static string TextCommandName => "/pitemuse";
	internal static string SubcommandName_Config => "config";
	internal static string SubcommandName_Debug => "debug";

	private readonly PluginUI mUI;
	private readonly Configuration mConfiguration;

	private readonly IDalamudPluginInterface mPluginInterface;
}
