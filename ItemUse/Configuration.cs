using System;
using System.Collections.Generic;

using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;

namespace ItemUse;

[Serializable]
public class Configuration : IPluginConfiguration
{
	#region Interface

	public void Initialize( IDalamudPluginInterface pluginInterface )
	{
		mPluginInterface = pluginInterface;
	}

	public void Save()
	{
		mPluginInterface.SavePluginConfig( this );
	}
	public int Version { get; set; } = 0;

	[NonSerialized]
	private IDalamudPluginInterface mPluginInterface;

	#endregion

	#region Options

	#endregion
}
