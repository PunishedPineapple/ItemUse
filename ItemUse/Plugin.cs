using System.Collections.Generic;
using System.IO;

using CheapLoc;

using Dalamud.Game.Command;
using Dalamud.Plugin;

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
		CofferManifests.Init( Path.Join( mPluginInterface.AssemblyLocation.DirectoryName, "Resources\\CofferManifests.csv" ) );
		ItemCategorizer.Init();
		ItemDetailHandler.Init( mConfiguration );

		//	UI Initialization
		mUI = new PluginUI( this, mConfiguration, pluginInterface );
		mPluginInterface.UiBuilder.Draw += DrawUI;
		mPluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

		//	Event Subscription
		mPluginInterface.LanguageChanged += OnLanguageChanged;
	}

	//	Cleanup
	public void Dispose()
	{
		mPluginInterface.LanguageChanged -= OnLanguageChanged;
		mPluginInterface.UiBuilder.Draw -= DrawUI;
		mPluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUI;
		DalamudAPI.CommandManager.RemoveHandler( TextCommandName );

		ItemDetailHandler.Uninit();
		ItemCategorizer.Uninit();
		CofferManifests.Uninit();

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

	internal void ExportLocalizableStrings()
	{
		string pwd = Directory.GetCurrentDirectory();
		Directory.SetCurrentDirectory( mPluginInterface.AssemblyLocation.DirectoryName );
		Loc.ExportLocalizable();
		Directory.SetCurrentDirectory( pwd );
	}

	public static string Name => "Item Use";
	internal static string TextCommandName => "/pitemuse";
	internal static string SubcommandName_Config => "config";
	internal static string SubcommandName_Debug => "debug";

	private readonly PluginUI mUI;
	private readonly Configuration mConfiguration;

	private readonly IDalamudPluginInterface mPluginInterface;
}
