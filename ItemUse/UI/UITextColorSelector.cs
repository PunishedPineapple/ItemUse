using System;
using System.Buffers.Binary;
using System.Linq;

using CheapLoc;

using ImGuiNET;

using Lumina.Excel.Sheets;
using Lumina.Extensions;

namespace ItemUse;

internal class UITextColorSelector : IDisposable
{
	public UITextColorSelector( UIColor[] availableColors, string identifierPrefix )
	{
		mUIColors = availableColors ?? throw new Exception( "Provided UI color array must not be null." );

		mUIIdentifierPrefix = identifierPrefix;
		if( string.IsNullOrEmpty( mUIIdentifierPrefix ) )
		{
			throw new Exception( "A valid prefix for ImGui identifiers must be provided." );
		}
	}

	protected UITextColorSelector() {}

	public void Dispose()
	{

	}

	public void Draw( ref ushort currentTextColor, ref ushort currentGlowColor, bool sameLine = true )
	{
		if( sameLine ) ImGui.SameLine();

		ImGui.PushID( mUIIdentifierPrefix );

		try
		{
			if( ImGui.ArrowButton( "CraftingMaterialTextColorButton", mShowColorSelectors ? ImGuiDir.Down : ImGuiDir.Right ) )
			{
				mShowColorSelectors = !mShowColorSelectors;
			}

			ImGuiUtils.TooltipLastItem( Loc.Localize( "Help: Settings - Highlight Color Dropdown", "Click here to show/hide the text colors." ) );

			if( mShowColorSelectors )
			{
				EnsureValidColorID( ref currentTextColor );
				ImGui.Text( Loc.Localize( "Settings: Label - Text Color", "Text:" ) );
				ImGui.SameLine();
				DrawUIColorDropdown( ref currentTextColor, "", "UIColorSelectorTextColor" );

				EnsureValidColorID( ref currentGlowColor );
				ImGui.Text( Loc.Localize( "Settings: Label - Glow Color", "Glow:" ) );
				ImGui.SameLine();
				DrawUIColorDropdown( ref currentGlowColor, "", "UIColorSelectorGlowColor" );
			}
		}
		finally
		{
			ImGui.PopID();
		}
	}

	protected void DrawUIColorDropdown( ref ushort colorID, string dropdownTitle, string additionalDropdownID )
	{
		if( mUIColors?.Length > 0 )
		{
			var selectedItemDrawColor = GetDrawColorForColorID( colorID, mCurrentUITheme );

			ImGui.PushStyleColor( ImGuiCol.Text, selectedItemDrawColor );

			try
			{
				if( ImGui.BeginCombo( dropdownTitle + "###UIColorDropdown_" + additionalDropdownID, String.Format( Loc.Localize( "Settings: Dropdown - Highlight Text Color", "UI Color {0}" ), colorID ) ) )
				{
					foreach( var color in mUIColors )
					{
						if( color.RowId == 0 ) continue;

						var colorTextString = String.Format( Loc.Localize( "Settings: Dropdown - UI Text Color", "UI Color {0}" ), color.RowId );

						ImGui.PushStyleColor( ImGuiCol.Text, GetDrawColorForUIColor( color, mCurrentUITheme ) );
						if( ImGui.Selectable( colorTextString ) ) colorID = (ushort)color.RowId;
						if( color.RowId == colorID ) ImGui.SetItemDefaultFocus();
						ImGui.PopStyleColor();
					}

					ImGui.EndCombo();
				}
			}
			finally
			{
				ImGui.PopStyleColor();
			}
		}
	}

	protected void EnsureValidColorID( ref ushort colorID )
	{
		var localColorIDCopy = colorID;
		if( mUIColors?.Any( x => x.RowId == localColorIDCopy ) != true ) colorID = 0;
	}

	protected uint GetDrawColorForColorID( ushort colorID, byte theme )
	{
		if( mUIColors.TryGetFirst( x => x.RowId == colorID && x.RowId != 0, out UIColor color ) )
		{
			return GetDrawColorForUIColor( color, theme );
		}
		else
		{
			return ImGui.GetColorU32( ImGuiCol.Text );
		}
	}

	protected uint GetDrawColorForUIColor( UIColor color, byte theme )
	{
		UInt32 retVal;

		retVal = theme switch
		{
			1 => BinaryPrimitives.ReverseEndianness( color.UIGlow ),
			2 => BinaryPrimitives.ReverseEndianness( color.Unknown0 ),
			3 => BinaryPrimitives.ReverseEndianness( color.Unknown1 ),
			_ => BinaryPrimitives.ReverseEndianness( color.UIForeground ),
		};

		//	Low-limit the alpha so that the user can always see something.
		return retVal | 0x40_00_00_00;
	}

	//	We want to cache this on login so that the colors are always correct for the currently-displayed theme.
	public static void CacheUITheme()
	{
		if( DalamudAPI.ClientState.IsLoggedIn )
		{
			mCurrentUITheme = GetConfiguredUITheme();
		}
		else
		{
			//	The game always uses the default theme when not logged in to a character.
			mCurrentUITheme = 0;
		}
	}

	internal static byte GetConfiguredUITheme()
	{
		if( DalamudAPI.GameConfig.TryGet( Dalamud.Game.Config.SystemConfigOption.ColorThemeType, out uint theme ) )
		{
			return (byte)theme;
		}
		else
		{
			return 0;
		}
	}

	private static byte mCurrentUITheme = 0;

	private readonly UIColor[] mUIColors = null;
	private bool mShowColorSelectors = false;
	private readonly string mUIIdentifierPrefix;
}
