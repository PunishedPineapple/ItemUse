using System.Collections.Generic;

using Lumina;
using Lumina.Data;
using Lumina.Excel;

namespace ItemUse;

//	Lumina defines this sheet with fixed fields for each job flag, which would require extra work on our
//	part any time a new job is added.  Use our own definition that makes plugin maintenance simpler.

public class ClassJobCategory_Alternate : Lumina.Excel.GeneratedSheets.ClassJobCategory
{
	public override void PopulateData( RowParser parser, GameData gameData, Language language )
	{
		mClassJobFlags.Clear();

		for( int i = 1; i < parser.Sheet.ColumnCount; ++i )
		{
			mClassJobFlags.Add( parser.ReadColumn<bool>( i ) );
		}

		base.PopulateData( parser, gameData, language );
	}

	public readonly List<bool> mClassJobFlags = new();
}
