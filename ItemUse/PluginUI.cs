using System;

using Dalamud.Interface.Windowing;
using Dalamud.Plugin;

namespace ItemUse;

public sealed class PluginUI : IDisposable
{
	//	Construction
	public PluginUI( Plugin plugin, Configuration configuration, IDalamudPluginInterface pluginInterface )
	{
		mPlugin = plugin;
		mConfiguration = configuration;
		mPluginInterface = pluginInterface;

		Window_Settings = new( mPlugin, this, mConfiguration );
		Window_Debug_ItemInfo = new( mPlugin, this, configuration );

		mWindowSystem.AddWindow( Window_Settings );
		mWindowSystem.AddWindow( Window_Debug_ItemInfo );
	}

	//	Destruction
	public void Dispose()
	{
		mWindowSystem.RemoveAllWindows();

		Window_Settings.Dispose();
		Window_Debug_ItemInfo.Dispose();

		Window_Settings = null;
		Window_Debug_ItemInfo = null;
	}

	public void Draw()
	{
		mWindowSystem.Draw();
	}

	private Plugin mPlugin;
	private IDalamudPluginInterface mPluginInterface;
	private Configuration mConfiguration;
	private readonly WindowSystem mWindowSystem = new( Plugin.Name );

	internal Window_Debug_ItemInfo Window_Debug_ItemInfo { get; private set; }
	internal Window_Settings Window_Settings { get; private set; }
}