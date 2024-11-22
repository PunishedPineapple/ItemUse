using Lumina.Excel;
using Lumina.Text.ReadOnly;

namespace ItemUse;

//	Lumina defines this sheet with fixed fields for each job flag, which would require extra work on our
//	part any time a new job is added.  Use our own definition that makes plugin maintenance simpler.

//	Unfortunately, with the change to using structs, we can't just inherit from Lumina's implementation anymore.
//	I'm not very comfortable with just copy-pasting the sheet attribute and some of the rest of this implementation,
//	but idk what else to do right now.

[Sheet( "ClassJobCategory", 0x65BBDB12 )]
readonly internal struct ClassJobCategory_Alternate( ExcelPage page, uint offset, uint row ) : IExcelRow<ClassJobCategory_Alternate>
{
	public uint RowId => row;

	public readonly ReadOnlySeString Name => page.ReadString( offset, offset );

	public bool IncludesClassJob( int classJobID )
	{
		if( classJobID < 0 || classJobID >= ClassJobColumnCount ) return false;

		var newOffset = offset + page.Sheet.GetColumnOffset( classJobID + numColumnsBeforeClassJobs );
		return page.ReadBool( newOffset );
	}

	public readonly int ClassJobColumnCount => page.Sheet.Columns.Count - numColumnsBeforeClassJobs;

	private const int numColumnsBeforeClassJobs = 1;

	static ClassJobCategory_Alternate IExcelRow<ClassJobCategory_Alternate>.Create( ExcelPage page, uint offset, uint row ) =>
		new( page, offset, row );
}
