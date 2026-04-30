using System;

using Lumina.Data.Structs.Excel;
using Lumina.Excel.Sheets;

namespace ItemUse;

internal static class UIColorExtensions
{
	extension( UIColor row )
	{
		public UInt32 GetColorForTheme( byte themeId )
		{
			if( themeId < 0 || themeId >= row.ThemeCount ) themeId = 0;

			int themeColumnIndex = themeId + numColumnsBeforeColors;

			if( themeColumnIndex < 0 || themeColumnIndex >= row.ExcelPage.Sheet.Columns.Count )
			{
				throw new Exception( $"UIColor sheet column {themeColumnIndex} is out of range!" );
			}
			else if( row.ExcelPage.Sheet.Columns[themeColumnIndex].Type != ExcelColumnDataType.UInt32 )
			{
				throw new Exception( $"UIColor sheet column {themeColumnIndex} is not of the expected type!" );
			}
			else
			{
				var valueOffset = row.RowOffset + row.ExcelPage.Sheet.GetColumnOffset( themeColumnIndex );
				return row.ExcelPage.ReadUInt32( valueOffset );
			}
		}

		public int ThemeCount => row.ExcelPage.Sheet.Columns.Count - numColumnsBeforeColors;
	}

	private const int numColumnsBeforeColors = 0;
}

//	Lumina defines this sheet with fixed fields for each theme, when it would be nicer to just
//	index columns by theme.  This is our own definition that makes plugin maintenance simpler.

/*[Sheet( "UIColor", 0x182F49E3 )]
readonly internal struct UIColor_Indexable( ExcelPage page, uint offset, uint row ) : IExcelRow<UIColor_Indexable>
{
	public ExcelPage ExcelPage => page;
	public uint RowOffset => offset;
	public uint RowId => row;

	public UInt32 GetColor( byte themeId )
	{
		if( themeId < 0 || themeId >= ThemeCount ) themeId = 0;

		int themeColumnIndex = themeId + numColumnsBeforeColors;

		if( themeColumnIndex >= page.Sheet.Columns.Count )
		{
			throw new Exception( $"UIColor sheet column {themeColumnIndex} is out of range!" );
		}
		else if( page.Sheet.Columns[themeColumnIndex].Type != ExcelColumnDataType.UInt32 )
		{
			throw new Exception( $"UIColor sheet column {themeColumnIndex} is not of the expected type!" );
		}
		else
		{
			var valueOffset = offset + page.Sheet.GetColumnOffset( themeColumnIndex );
			return page.ReadUInt32( valueOffset );
		}
	}

	public readonly int ThemeCount => page.Sheet.Columns.Count - numColumnsBeforeColors;

	private const int numColumnsBeforeColors = 0;

	static UIColor_Indexable IExcelRow<UIColor_Indexable>.Create( ExcelPage page, uint offset, uint row ) =>
		new( page, offset, row );
}*/
