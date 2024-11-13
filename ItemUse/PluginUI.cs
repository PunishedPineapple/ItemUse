using System;

using Dalamud.Interface.Windowing;
using Dalamud.Plugin;

namespace ItemUse;

internal sealed class PluginUI : IDisposable
{
	public PluginUI( Plugin plugin, Configuration configuration, IDalamudPluginInterface pluginInterface )
	{
		mPlugin = plugin;
		mConfiguration = configuration;
		mPluginInterface = pluginInterface;

		Window_Settings = new( mPlugin, this, mConfiguration );
		Window_Debug_General = new( mPlugin, this, configuration );
		Window_Debug_ItemInfo = new( mPlugin, this, configuration );
		Window_Debug_LoadedItems = new( mPlugin, this, configuration );

		mWindowSystem.AddWindow( Window_Settings );
		mWindowSystem.AddWindow( Window_Debug_General );
		mWindowSystem.AddWindow( Window_Debug_ItemInfo );
		mWindowSystem.AddWindow( Window_Debug_LoadedItems );
	}

	public void Dispose()
	{
		mWindowSystem.RemoveAllWindows();

		Window_Settings?.Dispose();
		Window_Debug_General?.Dispose();
		Window_Debug_ItemInfo?.Dispose();
		Window_Debug_LoadedItems?.Dispose();

		Window_Settings = null;
		Window_Debug_General = null;
		Window_Debug_ItemInfo = null;
		Window_Debug_LoadedItems = null;
	}

	public void Draw()
	{
		mWindowSystem.Draw();
	}

	private readonly Plugin mPlugin;
	private readonly IDalamudPluginInterface mPluginInterface;
	private readonly Configuration mConfiguration;
	private readonly WindowSystem mWindowSystem = new( Plugin.Name );

	internal Window_Settings Window_Settings { get; private set; }
	internal Window_Debug_General Window_Debug_General { get; private set; }
	internal Window_Debug_ItemInfo Window_Debug_ItemInfo { get; private set; }
	internal Window_Debug_LoadedItems Window_Debug_LoadedItems { get; private set; }
}