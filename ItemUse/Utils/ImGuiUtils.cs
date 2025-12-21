using Dalamud.Bindings.ImGui;

namespace ItemUse;

internal static class ImGuiUtils
{
	public static void HelpMarker( string description, bool sameLine = true, string marker = "(?)" )
	{
		if( sameLine ) ImGui.SameLine();
		ImGui.TextDisabled( marker );
		TooltipLastItem( description );
	}

	public static void TooltipLastItem( string description )
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

	public const float SectionSpacingSize = 20f;
}
