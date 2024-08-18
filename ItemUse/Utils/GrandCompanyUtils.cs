using Dalamud.Game.Text.SeStringHandling;

using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace ItemUse;

internal class GrandCompanyUtils
{
	internal static unsafe GrandCompany GetCurrentGC()
	{
		if( DalamudAPI.ClientState.IsLoggedIn && UIState.Instance() != null && UIState.Instance()->PlayerState.IsLoaded > 0 )
		{
			return (GrandCompany)UIState.Instance()->PlayerState.GrandCompany;
		}
		else
		{
			return 0;
		}
	}

	internal static BitmapFontIcon GetGCFontIcon( GrandCompany gc )
	{
		return gc switch
		{
			GrandCompany.TwinAdder => BitmapFontIcon.BlackShroud,
			GrandCompany.ImmortalFlames => BitmapFontIcon.Thanalan,
			_ => BitmapFontIcon.LaNoscea
		};
	}

	internal static BitmapFontIcon GetCurrentGCFontIcon()
	{
		return GetGCFontIcon( GetCurrentGC() );
	}
}
