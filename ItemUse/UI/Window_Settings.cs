﻿using System;

using CheapLoc;

using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

using ImGuiNET;

namespace ItemUse;

internal sealed class Window_Settings : Window, IDisposable
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
		ImGui.Text( Loc.Localize( "Settings: Header - Item Flags", "Item Flags:" ) );
		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Flag for GC Items", "Show Flag for GC Items" ) + "###ShowGCItemsFlagCheckbox", ref mConfiguration.mShowGCItemsFlag );
		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Flag for Leve Items", "Show Flag for Leve Items" ) + "###ShowLeveItemsFlagCheckbox", ref mConfiguration.mShowLeveItemsFlag );
		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Flag for Crafting Materials", "Show Flag for Crafting Materials" ) + "###ShowCraftingMaterialsFlagCheckbox", ref mConfiguration.mShowCraftingMaterialsFlag );
		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Flag for Aquarium Fish", "Show Flag for Aquarium Fish" ) + "###ShowAquariumFishFlagCheckbox", ref mConfiguration.mShowAquariumFishFlag );
		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Flag for Ehcatl Items", "Show Flag for Ehcatl Items" ) + "###ShowEhcatlItemsFlagCheckbox", ref mConfiguration.mShowEhcatlItemsFlag );

		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Combine Flags", "Combine all Flags" ) + "###CombineFlagsCheckbox", ref mConfiguration.mShowCombinedUsefulFlag );
		ImGuiUtils.HelpMarker( Loc.Localize( "Help: Settings - Combine Flags", "Instead of displaying separate flags for each item type, just shows a single star flag to indicate that an item is \"useful\"." ) );

		ImGuiHelpers.ScaledDummy( ImGuiUtils.SectionSpacingSize );

		ImGui.Text( Loc.Localize( "Settings: Header - Coffer Information", "Coffer Information:" ) );
		ImGuiUtils.HelpMarker( Loc.Localize( "Help: Settings - Coffer Information", "Please note that the contents of gear coffers are not directly available to the client, and have been semi-manually compiled.  If you find a coffer that you believe has incorrect information, please report it on this plugin's Github repo." ) ); 
		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Coffer GC Items", "Show Jobs with GC Items" ) + "###ShowCofferGCJobsCheckbox", ref mConfiguration.mShowGCCofferJobs );
		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Coffer Leve Items", "Show Jobs with Leve Items" ) + "###ShowCofferLeveJobsCheckbox", ref mConfiguration.mShowLeveCofferJobs );
		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Combine Coffer Jobs", "Combine Coffer Jobs" ) + "###CombineCofferJobsCheckbox", ref mConfiguration.mCombineCofferJobs );
		ImGuiUtils.HelpMarker( Loc.Localize( "Help: Settings - Combine Coffer Jobs", "Instead of displaying separate lists for GC deliveries and leves, just shows a single list with a star icon to indicate which jobs produce \"useful\" items when opening the coffer." ) );

		ImGuiHelpers.ScaledDummy( ImGuiUtils.SectionSpacingSize );

		ImGui.Text( Loc.Localize( "Settings: Header - Highlighting", "Highlighting:" ) );
		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Highlight Crafting Material", "Highlight \"Crafting Material\" Tag" ) + "###HighlightCraftingMaterialTextCheckbox", ref mConfiguration.mHighlightCraftingMaterialText );
		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Highlight Aquarium Fish", "Highlight Aquarium Fish Tag" ) + "###HighlightAquariumFishTextCheckbox", ref mConfiguration.mHighlightAquariumFishText );
		ImGuiUtils.HelpMarker( Loc.Localize( "Help: Settings - Highlight Aquarium Fish", "Please note that this is slightly less reliable than the aquarium fish flag above.  Due to how SE applies the aquarium tags in the descriptions for fish, it is possible for a typo in the item description to break highlighting for that item." ) );

		ImGuiHelpers.ScaledDummy( ImGuiUtils.SectionSpacingSize );

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

	private readonly Plugin mPlugin;
	private readonly PluginUI mPluginUI;
	private readonly Configuration mConfiguration;
}
