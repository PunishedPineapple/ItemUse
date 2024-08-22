using System;
using System.Buffers.Binary;
using System.Linq;

using CheapLoc;

using ImGuiNET;

using Lumina.Excel.GeneratedSheets;

namespace ItemUse;

internal class UITextColorSelector : IDisposable
{
	public UITextColorSelector( UIColor[] availableColors, string identifierPrefix )
	{
		mUIColors = availableColors ?? throw new Exception( "Provided UI color array must not be null." );
		mUIIdentifierPrefix = identifierPrefix;
		if( mUIIdentifierPrefix == null || mUIIdentifierPrefix.Length < 1 )
		{
			throw new Exception( "A valid prefix for ImGui identifiers must be provided." );
		}

		mUIIdentifierPrefix += '.';
	}

	private UITextColorSelector() {}

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
			var theme = GetCurrentUITheme();
			var selectedItemDrawColor = GetDrawColorForColorID( colorID, theme );

			ImGui.PushStyleColor( ImGuiCol.Text, selectedItemDrawColor );

			try
			{
				if( ImGui.BeginCombo( dropdownTitle + "###UIColorDropdown_" + additionalDropdownID, String.Format( Loc.Localize( "Settings: Dropdown - Highlight Text Color", "UI Color {0}" ), colorID ) ) )
				{
					foreach( var color in mUIColors )
					{
						if( color == null ) continue;
						if( color.RowId == 0 ) continue;

						var colorTextString = String.Format( Loc.Localize( "Settings: Dropdown - UI Text Color", "UI Color {0}" ), color.RowId );

						ImGui.PushStyleColor( ImGuiCol.Text, GetDrawColorForUIColor( color, theme ) );
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
		var color = mUIColors?.FirstOrDefault( x => x.RowId == colorID && x.RowId != 0, null );
		return GetDrawColorForUIColor( color, theme );
	}

	protected uint GetDrawColorForUIColor( UIColor color, byte theme )
	{
		if( color != null )
		{
			return theme switch
			{
				1 => BinaryPrimitives.ReverseEndianness( color.UIGlow ),
				2 => BinaryPrimitives.ReverseEndianness( color.Unknown2 ),
				3 => BinaryPrimitives.ReverseEndianness( color.Unknown3 ),
				_ => BinaryPrimitives.ReverseEndianness( color.UIForeground ),
			};
		}
		else
		{
			return ImGui.GetColorU32( ImGuiCol.Text );
		}
	}

	internal unsafe byte GetCurrentUITheme()
	{
		//	Getting this value is not working as expected.
		/*if( UIModule.Instance() != null &&
			UIModule.Instance()->GetConfigModule() != null &&
			UIModule.Instance()->GetConfigModule()->Values.Length > (int)ConfigOption.ColorThemeType )
		{
			byte[] themeOptionArray = null;
			fixed( void* pThemeOption = &UIModule.Instance()->GetConfigModule()->Values[(int)ConfigOption.ColorThemeType] )
			{
				var themeOption = new ReadOnlySpan<byte>( pThemeOption, Marshal.SizeOf<ConfigValue>() );
				themeOptionArray = themeOption.ToArray();
			}

			return themeOptionArray.Last();
		}*/

		return 0;
	}

	private readonly UIColor[] mUIColors = null;
	private bool mShowColorSelectors = false;
	private string mUIIdentifierPrefix;
}
