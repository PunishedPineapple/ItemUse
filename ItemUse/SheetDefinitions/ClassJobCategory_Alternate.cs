using System;

using Lumina.Data.Structs.Excel;
using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace ItemUse;

//	Lumina defines this sheet with fixed fields for each job flag, which would require extra work on our
//	part any time a new job is added.  Use our own definition that makes plugin maintenance simpler.

//	Now that you can extend structs, it may be better to turn this into an extension class like with UIColor,
//	but we will leave it as-is for now.  If we change it, make sure to keep column hash comparison working
//	because otherwise we could be silently showing incorrect data to the player.

[Sheet( "ClassJobCategory", 0x6733E334 )]
readonly internal struct ClassJobCategory_Alternate( ExcelPage page, uint offset, uint row ) : IExcelRow<ClassJobCategory_Alternate>
{
	public ExcelPage ExcelPage => page;
	public uint RowOffset => offset;
	public uint RowId => row;

	public readonly ReadOnlySeString Name => page.ReadString( offset, offset );

	public bool IncludesClassJob( int classJobID )
	{
		if( classJobID < 0 || classJobID >= ClassJobColumnCount ) return false;

		int classJobColumnIndex = classJobID + numColumnsBeforeClassJobs;

		if( classJobColumnIndex < 0 || classJobColumnIndex >= page.Sheet.Columns.Count )
		{
			throw new Exception( $"ClassJobCategory sheet column {classJobColumnIndex} is out of range!" );
		}
		else if( page.Sheet.Columns[classJobColumnIndex].Type != ExcelColumnDataType.Bool )
		{
			throw new Exception( $"ClassJobCategory sheet column {classJobColumnIndex} is not of the expected type!" );
		}
		else
		{
			var valueOffset = offset + page.Sheet.GetColumnOffset( classJobColumnIndex );
			return page.ReadBool( valueOffset );
		}
	}

	public readonly int ClassJobColumnCount => page.Sheet.Columns.Count - numColumnsBeforeClassJobs;

	private const int numColumnsBeforeClassJobs = 1;

	static ClassJobCategory_Alternate IExcelRow<ClassJobCategory_Alternate>.Create( ExcelPage page, uint offset, uint row ) =>
		new( page, offset, row );
}
