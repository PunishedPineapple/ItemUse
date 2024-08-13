using Dalamud.IoC;
using Dalamud.Plugin.Services;

namespace ItemUse;

internal class DalamudAPI
{
	[PluginService] internal static IFramework Framework { get; private set; } = null!;
	[PluginService] internal static IClientState ClientState { get; private set; } = null!;
	[PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
	[PluginService] internal static IDataManager DataManager { get; private set; } = null!;
	[PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
	[PluginService] internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;
	[PluginService] internal static IGameGui GameGui { get; private set; } = null!;
}