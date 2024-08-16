using System.Collections.Generic;

using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

using Lumina.Excel.GeneratedSheets;

namespace ItemUse;

internal static class ClassJobUtils
{
	internal static Payload GetIconPayloadForJob( int classJob )
	{

		var classJobSheet = DalamudAPI.DataManager.GetExcelSheet<ClassJob>();
		string jobStr = classJobSheet?.GetRow( (uint)classJob )?.Abbreviation?.ToString() ?? "UNK";

		//	This relationship has to be defined somewhere in the game data, but I cannot find it.
		return classJob switch
		{
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

	private static HashSet<int> CondenseForRoles( IEnumerable<int> classJobs )
	{
		//***** TODO
		return new();
	}

	//	This is some made up bullshit.  Don't leak these outside of this class.
	private const int RoleIDTank = -1;
	private const int RoleIDHealer = -2;
	private const int RoleIDMelee = -3;
	private const int RoleIDPhysRanged = -4;
	private const int RoleIDCaster = -5;

}
