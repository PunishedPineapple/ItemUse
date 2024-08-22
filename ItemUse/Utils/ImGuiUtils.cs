using ImGuiNET;

namespace ItemUse;

internal static class ImGuiUtils
{
	internal static void HelpMarker( string description, bool sameLine = true, string marker = "(?)" )
	{
		if( sameLine ) ImGui.SameLine();
		ImGui.TextDisabled( marker );
		TooltipLastItem( description );
	}

	internal static void TooltipLastItem( string description )
	{
		if( ImGui.IsItemHovered() )
		{
			ImGui.BeginTooltip();
			ImGui.PushTextWrapPos( ImGui.GetFontSize() * 35.0f );
			ImGui.TextUnformatted( description );
			ImGui.PopTextWrapPos();
			ImGui.EndTooltip();
		}
	}

	internal const float SectionSpacingSize = 20f;
}
