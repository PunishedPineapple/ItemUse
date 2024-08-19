using System;

using Dalamud.Game;

using Lumina.Excel.GeneratedSheets;

namespace ItemUse;

internal static class LocalizationHelpers
{
	internal static string CraftingMaterialTag
	{
		get
		{
			if( mCraftingMaterialTag == null )
			{
				try
				{
					var addonSheet = DalamudAPI.DataManager.GetExcelSheet<Addon>();
					mCraftingMaterialTag = addonSheet?.GetRow( 996 )?.Text ?? "Crafting Material";
				}
				catch( Exception e )
				{
					mCraftingMaterialTag =  "You should never see this!";
					DalamudAPI.PluginLog.Error( $"Unknown error while attempting to retrieve crafting material string:\r\n{e}" );
				}
			}

			return mCraftingMaterialTag;
		}
	}

	private static string mCraftingMaterialTag = null;

	//	These are pretty lame, but there's not a much better way to get these exact strings, since SE just stuffs them right into the description column in the sheet itself.
	internal static string Grade1AquariumTag =>
		DalamudAPI.ClientState.ClientLanguage switch
		{
			ClientLanguage.Japanese => "[G1以上の水槽で飼育可能]",
			ClientLanguage.English => "[Suitable for display in aquariums tier 1 and higher.]",
			ClientLanguage.German => "Kann in Aquarien der Größe S oder größer gehalten werden.",
			ClientLanguage.French => "[Peut être élevé dans un aquarium de petite taille ou supérieure]",
			_ => "You should never see this!",
		};


	internal static string Grade2AquariumTag =>
		DalamudAPI.ClientState.ClientLanguage switch
		{
			ClientLanguage.Japanese => "[G2以上の水槽で飼育可能]",
			ClientLanguage.English => "[Suitable for display in aquariums tier 2 and higher.]",
			ClientLanguage.German => "Kann in Aquarien der Größe M oder größer gehalten werden.",
			ClientLanguage.French => "[Peut être élevé dans un aquarium de taille moyenne ou supérieure]",
			_ => "You should never see this!",
		};

	internal static string Grade3AquariumTag =>
		DalamudAPI.ClientState.ClientLanguage switch
		{
			ClientLanguage.Japanese => "[G3以上の水槽で飼育可能]",
			ClientLanguage.English => "[Suitable for display in aquariums tier 3 and higher.]",
			ClientLanguage.German => "Kann in Aquarien der Größe L oder größer gehalten werden.",
			ClientLanguage.French => "[Peut être élevé dans un aquarium de grande taille ou supérieur",
			_ => "You should never see this!",
		};

	internal static string Grade4AquariumTag =>
		DalamudAPI.ClientState.ClientLanguage switch
		{
			ClientLanguage.Japanese => "[G4以上の水槽で飼育可能]",
			ClientLanguage.English => "[Suitable for display in aquariums tier 4 and higher.]",
			ClientLanguage.German => "Kann in Aquarien der Größe XL gehalten werden.",
			ClientLanguage.French => "[Peut être élevé dans un aquarium de très grande taille ou supérieure]",
			_ => "You should never see this!",
		};
}
