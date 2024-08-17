using System;

using Dalamud.Configuration;
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

	public bool mShowCombinedUsefulFlag = false;
	public bool mShowGCItemsFlag = true;
	public bool mShowLeveItemsFlag = true;
	public bool mShowCraftingMaterialsFlag = true;
	public bool mShowAquariumFishFlag = true;
	public bool mShowEhcatlItemsFlag = false;

	public byte mGrandCompany = 1;

	public bool mShowGCCofferJobs = true;
	public bool mShowLeveCofferJobs = true;
	public bool mCombineCofferJobs = false;

	//***** TODO
	//public bool mHighlightCraftingMaterialText = false;
	//public bool mHighlightAquariumFishText = false;

	#endregion
}
