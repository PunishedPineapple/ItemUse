using System;
using System.Linq;
using System.Numerics;

using CheapLoc;

using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

using ImGuiNET;

using Lumina.Excel.GeneratedSheets;

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

		var UIColors = DalamudAPI.DataManager.GetExcelSheet<UIColor>()?.ToArray();

		mCraftingMaterialHighlightColorSelector = new( UIColors, "CraftingMaterialHighlightColors" );
		mAquariumFishHighlightColorSelector = new( UIColors, "AquariumFishHighlightColors" );
	}

	public void Dispose()
	{
		mCraftingMaterialHighlightColorSelector.Dispose();
		mAquariumFishHighlightColorSelector.Dispose();
	}

	public override void Draw()
	{
		var GCIconStart_UV = GrandCompanyUtils.GetCurrentGC() switch
		{
			FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany.TwinAdder => mTwinAdderIconStart_UV,
			FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany.ImmortalFlames => mImmortalFlamesIconStart_UV,
			_ => mMaelstromIconStart_UV,
		};

		var GCUVEnd_UV = GrandCompanyUtils.GetCurrentGC() switch
		{
			FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany.TwinAdder => mTwinAdderIconEnd_UV,
			FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany.ImmortalFlames => mImmortalFlamesIconEnd_UV,
			_ => mMaelstromIconEnd_UV,
		};

		var fontIconTex = DalamudAPI.TextureProvider.GetFromGame( "common/font/fontIcon_Ps5.tex" ).GetWrapOrEmpty();
		var iconSize = new Vector2( ImGui.GetFrameHeight() );

		ImGui.Text( Loc.Localize( "Settings: Header - Item Flags", "Item Flags:" ) );

		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Flag for GC Items", "Show Flag for GC Items" ) + "###ShowGCItemsFlagCheckbox", ref mConfiguration.mShowGCItemsFlag );
		ImGui.SameLine();
		ImGui.Image( fontIconTex.ImGuiHandle, iconSize, GCIconStart_UV, GCUVEnd_UV );

		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Flag for Leve Items", "Show Flag for Leve Items" ) + "###ShowLeveItemsFlagCheckbox", ref mConfiguration.mShowLeveItemsFlag );
		ImGui.SameLine();
		ImGui.Image( fontIconTex.ImGuiHandle, iconSize, mDieIconStart_UV, mDieIconEnd_UV );

		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Flag for Crafting Materials", "Show Flag for Crafting Materials" ) + "###ShowCraftingMaterialsFlagCheckbox", ref mConfiguration.mShowCraftingMaterialsFlag );
		ImGui.SameLine();
		ImGui.Image( fontIconTex.ImGuiHandle, iconSize, mCrafterIconStart_UV, mCrafterIconEnd_UV );

		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Flag for Aquarium Fish", "Show Flag for Aquarium Fish" ) + "###ShowAquariumFishFlagCheckbox", ref mConfiguration.mShowAquariumFishFlag );
		ImGui.SameLine();
		ImGui.Image( fontIconTex.ImGuiHandle, iconSize, mFisherIconStart_UV, mFisherIconEnd_UV );

		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Flag for Ehcatl Items", "Show Flag for Ehcatl Items" ) + "###ShowEhcatlItemsFlagCheckbox", ref mConfiguration.mShowEhcatlItemsFlag );
		ImGui.SameLine();
		ImGui.Image( fontIconTex.ImGuiHandle, iconSize, mFlyingIconStart_UV, mFlyingIconEnd_UV );

		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Combine Flags", "Combine all Flags" ) + "###CombineFlagsCheckbox", ref mConfiguration.mShowCombinedUsefulFlag );
		ImGui.SameLine();
		ImGui.Image( fontIconTex.ImGuiHandle, iconSize, mGoldStarIconStart_UV, mGoldStarIconEnd_UV );
		ImGuiUtils.HelpMarker( Loc.Localize( "Help: Settings - Combine Flags", "Instead of displaying separate flags for each item type, just shows a single star flag to indicate that an item is \"useful\"." ) );

		ImGuiHelpers.ScaledDummy( ImGuiUtils.SectionSpacingSize );

		ImGui.Text( Loc.Localize( "Settings: Header - Coffer Information", "Coffer Information:" ) );
		ImGuiUtils.HelpMarker( Loc.Localize( "Help: Settings - Coffer Information", "Please note that the contents of gear coffers are not directly available to the client, and have been semi-manually compiled.  If you find a coffer that you believe has incorrect information, please report it on this plugin's Github repo." ) ); 

		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Coffer GC Items", "Show Jobs with GC Items" ) + "###ShowCofferGCJobsCheckbox", ref mConfiguration.mShowGCCofferJobs );
		ImGui.SameLine();
		ImGui.Image( fontIconTex.ImGuiHandle, iconSize, GCIconStart_UV, GCUVEnd_UV );

		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Show Coffer Leve Items", "Show Jobs with Leve Items" ) + "###ShowCofferLeveJobsCheckbox", ref mConfiguration.mShowLeveCofferJobs );
		ImGui.SameLine();
		ImGui.Image( fontIconTex.ImGuiHandle, iconSize, mDieIconStart_UV, mDieIconEnd_UV );

		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Combine Coffer Jobs", "Combine Coffer Jobs" ) + "###CombineCofferJobsCheckbox", ref mConfiguration.mCombineCofferJobs );
		ImGui.SameLine();
		ImGui.Image( fontIconTex.ImGuiHandle, iconSize, mGoldStarIconStart_UV, mGoldStarIconEnd_UV );
		ImGuiUtils.HelpMarker( Loc.Localize( "Help: Settings - Combine Coffer Jobs", "Instead of displaying separate lists for GC deliveries and leves, just shows a single list with a star icon to indicate which jobs produce \"useful\" items when opening the coffer." ) );

		ImGuiHelpers.ScaledDummy( ImGuiUtils.SectionSpacingSize );

		ImGui.Text( Loc.Localize( "Settings: Header - Highlighting", "Highlighting:" ) );

		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Highlight Crafting Material", "Highlight \"Crafting Material\" Tag" ) + "###HighlightCraftingMaterialTextCheckbox", ref mConfiguration.mHighlightCraftingMaterialText );
		mCraftingMaterialHighlightColorSelector?.Draw( ref mConfiguration.mHighlightCraftingMaterialTextColor, ref mConfiguration.mHighlightCraftingMaterialGlowColor );
		ImGui.Checkbox( Loc.Localize( "Settings: Checkbox - Highlight Aquarium Fish", "Highlight Aquarium Fish Tag" ) + "###HighlightAquariumFishTextCheckbox", ref mConfiguration.mHighlightAquariumFishText );
		ImGuiUtils.HelpMarker( Loc.Localize( "Help: Settings - Highlight Aquarium Fish", "Please note that this is slightly less reliable than the aquarium fish flag above.  Due to how SE applies the aquarium tags in the descriptions for fish, it is possible for a typo in the item description to break highlighting for that item." ) );
		mAquariumFishHighlightColorSelector?.Draw( ref mConfiguration.mHighlightAquariumFishTextColor, ref mConfiguration.mHighlightAquariumFishGlowColor );

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

	private readonly UITextColorSelector mCraftingMaterialHighlightColorSelector = null;
	private readonly UITextColorSelector mAquariumFishHighlightColorSelector = null;

	//	It would really be nice to use the gfd file to get these UVs, but Dalamud's implementation is
	//	not exposed, so we're probably out of luck without reimplementing and maintaining it ourselves.

	private static readonly Vector2 mMaelstromIconStart_UV = new( 250f / 512f, 342f / 1024f );
	private static readonly Vector2 mMaelstromIconEnd_UV = new( 285f / 512f, 377f / 1024f );

	private static readonly Vector2 mTwinAdderIconStart_UV = new( 289f / 512f, 342f / 1024f );
	private static readonly Vector2 mTwinAdderIconEnd_UV = new( 324f / 512f, 377f / 1024f );

	private static readonly Vector2 mImmortalFlamesIconStart_UV = new( 330f / 512f, 342f / 1024f );
	private static readonly Vector2 mImmortalFlamesIconEnd_UV = new( 365f / 512f, 377f / 1024f );

	private static readonly Vector2 mGoldStarIconStart_UV = new( 362f / 512f, 382f / 1024f );
	private static readonly Vector2 mGoldStarIconEnd_UV = new( 397f / 512f, 416f / 1024f );

	private static readonly Vector2 mDieIconStart_UV = new( 444f / 512f, 382f / 1024f );
	private static readonly Vector2 mDieIconEnd_UV = new( 476f / 512f, 416f / 1024f );

	private static readonly Vector2 mFlyingIconStart_UV = new( 218f / 512f, 462f / 1024f );
	private static readonly Vector2 mFlyingIconEnd_UV = new( 254f / 512f, 497f / 1024f );

	private static readonly Vector2 mCrafterIconStart_UV = new( 363f / 512f, 501f / 1024f );
	private static readonly Vector2 mCrafterIconEnd_UV = new( 398f / 512f, 536f / 1024f );

	private static readonly Vector2 mFisherIconStart_UV = new( 202f / 512f, 742f / 1024f );
	private static readonly Vector2 mFisherIconEnd_UV = new( 237f / 512f, 777f / 1024f );
}
