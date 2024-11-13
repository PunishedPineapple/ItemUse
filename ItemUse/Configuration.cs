using System;

using Dalamud.Configuration;
using Dalamud.Plugin;

namespace ItemUse;

//	IMPORTANT: This class (and its serializable members) must be public in order to be properly serialized and deserialized.

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

	public bool mShowCombinedUsefulFlag = false;
	public bool mShowGCItemsFlag = true;
	public bool mShowLeveItemsFlag = true;
	public bool mShowCraftingMaterialsFlag = false;
	public bool mShowAquariumFishFlag = true;
	public bool mShowEhcatlItemsFlag = false;

	public bool mShowGCCofferJobs = true;
	public bool mShowLeveCofferJobs = true;
	public bool mCombineCofferJobs = false;

	public bool mHighlightCraftingMaterialText = true;
	public bool mHighlightAquariumFishText = false;

	public ushort mHighlightCraftingMaterialTextColor = 500;
	public ushort mHighlightCraftingMaterialGlowColor = 501;

	public ushort mHighlightAquariumFishTextColor = 500;
	public ushort mHighlightAquariumFishGlowColor = 501;
 
	#endregion
}
