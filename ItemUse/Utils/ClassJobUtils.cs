using System.Collections.Generic;
using System.Linq;

using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Utility;

using Lumina.Excel.GeneratedSheets;

namespace ItemUse;

internal static class ClassJobUtils
{
	internal static void GetIconStringForJobs( ref SeStringBuilder str, IEnumerable<int> classJobs, bool condenseRoles, bool ignoreClasses )
	{
		if( classJobs != null )
		{
			if( ignoreClasses ) classJobs = RemoveClasses( classJobs );
			if( condenseRoles ) classJobs = CondenseRoles( classJobs, ignoreClasses );

			var sortedJobs = SortClassJobs( classJobs );

			foreach( var classJob in sortedJobs ) str.Add( GetIconPayloadForJob( classJob ) );
		}
	}

	internal static SeString GetIconStringForJobs( IEnumerable<int> classJobs, bool condenseRoles, bool ignoreClasses )
	{
		SeStringBuilder str = new();
		GetIconStringForJobs( ref str, classJobs, condenseRoles, ignoreClasses );
		return str.BuiltString;
	}

	internal static Payload GetIconPayloadForJob( int classJob )
	{
		//	Fallback to the job abbreviation if we don't have/know the icon.
		string jobStr = ClassJobDict.TryGetValue( classJob, out ClassJobData classJobData) ? classJobData.Abbreviation : "???";

		//	This relationship has to be defined somewhere in the game data, but I cannot find it, so we're doing it manually.
		return classJob switch
		{
			RoleIDTank => new IconPayload( BitmapFontIcon.Tank ),
			RoleIDHealer => new IconPayload( BitmapFontIcon.Healer ),

			1 => new IconPayload( BitmapFontIcon.Gladiator ),
			2 => new IconPayload( BitmapFontIcon.Pugilist ),
			3 => new IconPayload( BitmapFontIcon.Marauder ),
			4 => new IconPayload( BitmapFontIcon.Lancer ),
			5 => new IconPayload( BitmapFontIcon.Archer ),
			6 => new IconPayload( BitmapFontIcon.Conjurer ),
			7 => new IconPayload( BitmapFontIcon.Thaumaturge ),
			8 => new IconPayload( BitmapFontIcon.Carpenter ),
			9 => new IconPayload( BitmapFontIcon.Blacksmith ),
			10 => new IconPayload( BitmapFontIcon.Armorer ),
			11 => new IconPayload( BitmapFontIcon.Goldsmith ),
			12 => new IconPayload( BitmapFontIcon.Leatherworker ),
			13 => new IconPayload( BitmapFontIcon.Weaver ),
			14 => new IconPayload( BitmapFontIcon.Alchemist ),
			15 => new IconPayload( BitmapFontIcon.Culinarian ),
			16 => new IconPayload( BitmapFontIcon.Miner ),
			17 => new IconPayload( BitmapFontIcon.Botanist ),
			18 => new IconPayload( BitmapFontIcon.Fisher ),
			19 => new IconPayload( BitmapFontIcon.Paladin ),
			20 => new IconPayload( BitmapFontIcon.Monk ),
			21 => new IconPayload( BitmapFontIcon.Warrior ),
			22 => new IconPayload( BitmapFontIcon.Dragoon ),
			23 => new IconPayload( BitmapFontIcon.Bard ),
			24 => new IconPayload( BitmapFontIcon.WhiteMage ),
			25 => new IconPayload( BitmapFontIcon.BlackMage ),
			26 => new IconPayload( BitmapFontIcon.Arcanist ),
			27 => new IconPayload( BitmapFontIcon.Summoner ),
			28 => new IconPayload( BitmapFontIcon.Scholar ),
			29 => new IconPayload( BitmapFontIcon.Rogue ),
			30 => new IconPayload( BitmapFontIcon.Ninja ),
			31 => new IconPayload( BitmapFontIcon.Machinist ),
			32 => new IconPayload( BitmapFontIcon.DarkKnight ),
			33 => new IconPayload( BitmapFontIcon.Astrologian ),
			34 => new IconPayload( BitmapFontIcon.Samurai ),
			35 => new IconPayload( BitmapFontIcon.RedMage ),
			36 => new IconPayload( BitmapFontIcon.BlueMage ),
			37 => new IconPayload( BitmapFontIcon.Gunbreaker ),
			38 => new IconPayload( BitmapFontIcon.Dancer ),
			39 => new IconPayload( BitmapFontIcon.Reaper ),
			40 => new IconPayload( BitmapFontIcon.Sage ),
			41 => new IconPayload( (BitmapFontIcon)170 ),
			42 => new IconPayload( (BitmapFontIcon)171 ),

			_ => new TextPayload( jobStr ),
		};
	}

	internal static List<int> SortClassJobs( IEnumerable<int> classJobs )
	{
		List<int> sortedList = new( classJobs );
		sortedList.Sort( ClassJobSortCompare );
		return sortedList;
	}

	private static int ClassJobSortCompare( int x, int y )
	{
			 if( x == y ) return 0;
		else if( x < 0 && y < 0 ) return x.CompareTo( y );
		else if( x < 0 && y >= 0 ) return -1;
		else if( x >= 0 && y < 0 ) return 1;
		else if( ClassJobDict.ContainsKey( x ) && !ClassJobDict.ContainsKey( y ) ) return -1;
		else if( !ClassJobDict.ContainsKey( x ) && !ClassJobDict.ContainsKey( y ) ) return 0;
		else if( !ClassJobDict.ContainsKey( x ) && ClassJobDict.ContainsKey( y ) ) return 1;
		else return ClassJobDict[x].UIPriority.CompareTo( ClassJobDict[y].UIPriority );
	}

	internal static void RemoveClasses( ref HashSet<int> classJobs )
	{
		classJobs.ExceptWith( Classes );
	}

	internal static HashSet<int> RemoveClasses( IEnumerable<int> classJobs )
	{
		var processedJobs = new HashSet<int>( classJobs );
		processedJobs.ExceptWith( Classes );
		return processedJobs;
	}

	//	WARNING:	This will insert values that are not real ClassJobs and require special handling.  Don't
	//				use this function to produce any collections that will be consumed outside of this class.
	private static HashSet<int> CondenseRoles( IEnumerable<int> classJobs, bool ignoreClasses = true )
	{
		var processedClassJobs = new HashSet<int>( classJobs );

		if( processedClassJobs.IsSupersetOf( ignoreClasses ? TankJobs : Tanks ) )
		{
			processedClassJobs.ExceptWith( Tanks );
			processedClassJobs.Add( RoleIDTank );
		}

		if( processedClassJobs.IsSupersetOf( ignoreClasses ? HealerJobs : Healers ) )
		{
			processedClassJobs.ExceptWith( Healers );
			processedClassJobs.Add( RoleIDHealer );
		}

		return processedClassJobs;
	}

	internal static SortedDictionary<int, ClassJobData> ClassJobDict
	{
		get
		{
			if( mClassJobDict == null )
			{
				mClassJobDict = new();
				Lumina.Excel.ExcelSheet<ClassJob> classJobSheet_En = DalamudAPI.DataManager.GetExcelSheet<ClassJob>( Dalamud.Game.ClientLanguage.English );
				Lumina.Excel.ExcelSheet<ClassJob> classJobSheet_Local = DalamudAPI.DataManager.GetExcelSheet<ClassJob>();
				for( int i = 0; i < classJobSheet_En.RowCount; ++i )
				{
					var row = classJobSheet_En.GetRow( (uint)i );
					if( row != null )
					{
						ClassJobSortCategory sortCategory;
						if( row.UIPriority == 0 ) sortCategory = ClassJobSortCategory.Other;
						else if( row.UIPriority <= 10 ) sortCategory = row.JobIndex > 0 ? ClassJobSortCategory.Job_Tank : ClassJobSortCategory.Class_Tank;
						else if( row.UIPriority <= 20 ) sortCategory = row.JobIndex > 0 ? ClassJobSortCategory.Job_Healer : ClassJobSortCategory.Class_Healer;
						else if( row.UIPriority <= 30 ) sortCategory = row.JobIndex > 0 ? ClassJobSortCategory.Job_Melee : ClassJobSortCategory.Class_Melee;
						else if( row.UIPriority <= 40 ) sortCategory = row.JobIndex > 0 ? ClassJobSortCategory.Job_Ranged : ClassJobSortCategory.Class_Ranged;
						else if( row.UIPriority <= 50 ) sortCategory = row.JobIndex > 0 ? ClassJobSortCategory.Job_Caster : ClassJobSortCategory.Class_Caster;
						else sortCategory = row.JobIndex > 0 ? ClassJobSortCategory.HandLand : ClassJobSortCategory.HandLand;

						//	This needs to be unique (for use as a key).
						string abbreviation_En = row.Abbreviation;
						if( abbreviation_En.IsNullOrWhitespace() ) abbreviation_En = $"UNK{i}";

						mClassJobDict.TryAdd( i,
							new ClassJobData
							{
								Abbreviation = classJobSheet_Local.GetRow( (uint)i )?.Abbreviation ?? "",
								Abbreviation_En = abbreviation_En,
								DefaultSelected = row.DohDolJobIndex < 0,
								SortCategory = sortCategory,
								UIPriority = row.UIPriority,
							} );
					}
				}
			}

			return mClassJobDict;
		}
	}

	internal static HashSet<int> Classes
	{
		get
		{
			if( mClasses == null )
			{
				mClasses = new();

				foreach( var classJob in ClassJobDict )
				{
					if( classJob.Value.SortCategory is
						ClassJobSortCategory.Class_Tank or
						ClassJobSortCategory.Class_Healer or
						ClassJobSortCategory.Class_Melee or
						ClassJobSortCategory.Class_Ranged or
						ClassJobSortCategory.Class_Caster )
					{
						mClasses.Add( classJob.Key );
					}
				}
			}

			return mClasses;
		}
	}

	internal static HashSet<int> Jobs
	{
		get
		{
			if( mJobs == null )
			{
				mJobs = new();

				foreach( var classJob in ClassJobDict )
				{
					if( classJob.Value.SortCategory is
						ClassJobSortCategory.Job_Tank or
						ClassJobSortCategory.Job_Healer or
						ClassJobSortCategory.Job_Melee or
						ClassJobSortCategory.Job_Ranged or
						ClassJobSortCategory.Job_Caster or
						ClassJobSortCategory.HandLand )
					{
						mJobs.Add( classJob.Key );
					}
				}
			}

			return mJobs;
		}
	}

	internal static HashSet<int> Tanks
	{
		get
		{
			if( mTanks == null )
			{
				mTanks = new();

				foreach( var classJob in ClassJobDict )
				{
					if( classJob.Value.SortCategory is ClassJobSortCategory.Job_Tank or ClassJobSortCategory.Class_Tank ) mTanks.Add( classJob.Key );
				}
			}

			return mTanks;
		}
	}

	internal static HashSet<int> TankJobs => new( Tanks.Intersect( Jobs ) );

	internal static HashSet<int> TankClasses => new( Tanks.Intersect( Classes ) );

	internal static HashSet<int> Healers
	{
		get
		{
			if( mHealers == null )
			{
				mHealers = new();

				foreach( var classJob in ClassJobDict )
				{
					if( classJob.Value.SortCategory is ClassJobSortCategory.Job_Healer or ClassJobSortCategory.Class_Healer ) mHealers.Add( classJob.Key );
				}
			}

			return mHealers;
		}
	}

	internal static HashSet<int> HealerJobs => new( Healers.Intersect( Jobs ) );

	internal static HashSet<int> HealerClasses => new( Healers.Intersect( Classes ) );

	internal static HashSet<int> Melee
	{
		get
		{
			if( mMelee == null )
			{
				mMelee = new();

				foreach( var classJob in ClassJobDict )
				{
					if( classJob.Value.SortCategory is ClassJobSortCategory.Job_Melee or ClassJobSortCategory.Class_Melee ) mMelee.Add( classJob.Key );
				}
			}

			return mMelee;
		}
	}

	internal static HashSet<int> MeleeJobs => new( Melee.Intersect( Jobs ) );

	internal static HashSet<int> MeleeClasses => new( Melee.Intersect( Classes ) );

	internal static HashSet<int> Ranged
	{
		get
		{
			if( mRanged == null )
			{
				mRanged = new();

				foreach( var classJob in ClassJobDict )
				{
					if( classJob.Value.SortCategory is ClassJobSortCategory.Job_Ranged or ClassJobSortCategory.Class_Ranged ) mRanged.Add( classJob.Key );
				}
			}

			return mRanged;
		}
	}

	internal static HashSet<int> RangedJobs => new( Ranged.Intersect( Jobs ) );

	internal static HashSet<int> RangedClasses => new( Ranged.Intersect( Classes ) );

	internal static HashSet<int> Casters
	{
		get
		{
			if( mCasters == null )
			{
				mCasters = new();

				foreach( var classJob in ClassJobDict )
				{
					if( classJob.Value.SortCategory is ClassJobSortCategory.Job_Caster or ClassJobSortCategory.Class_Caster ) mCasters.Add( classJob.Key );
				}
			}

			return mCasters;
		}
	}

	internal static HashSet<int> CasterJobs => new( Casters.Intersect( Jobs ) );

	internal static HashSet<int> CasterClasses => new( Casters.Intersect( Classes ) );

	internal static HashSet<int> DPS => new( Melee.Union( Ranged.Union( Casters ) ) );

	internal static HashSet<int> DPSJobs => new( DPS.Intersect( Jobs ) );

	internal static HashSet<int> DPSClasses => new( DPS.Intersect( Classes ) );

	//	Keep something more convenient for our purposes than having to process the sheet each time.
	private static SortedDictionary<int, ClassJobData> mClassJobDict = null;

	//	These take sheet/dictionary processing to get, so keep them stored once computed.
	private static HashSet<int> mClasses = null;
	private static HashSet<int> mJobs = null;
	private static HashSet<int> mTanks = null;
	private static HashSet<int> mHealers = null;
	private static HashSet<int> mMelee = null;
	private static HashSet<int> mRanged = null;
	private static HashSet<int> mCasters = null;

	//	This is some made up garbage for convenience.  Don't leak these outside of this class.
	private const int RoleIDTank = -5;
	private const int RoleIDHealer = -4;

	//	Sadly no separate text icons for melee/ranged/caster DPS as far as I can tell, so not much point in these currently.
	//private const int RoleIDMelee = -3;
	//private const int RoleIDPhysRanged = -2;
	//private const int RoleIDCaster = -1;
}
