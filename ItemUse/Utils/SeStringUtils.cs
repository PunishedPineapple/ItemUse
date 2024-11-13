using System.Collections.Generic;

using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace ItemUse;

internal static class SeStringUtils
{
	internal static void HighlightLastOccuranceOfText( ref SeString str, string tagStr, ushort color, ushort glowColor )
	{
		//	Doing it this way is a bit on the slow side (up to a few hundred μs per call).  If we ever actually run into performance problems, we could try
		//	just UTF-8 encoding the tag string and searching and operating on the raw bytes instead (with pre-encoded foreground/glow payloads, even).

		int tagPayloadIndex = -1;
		var splitTextEndPayload = new TextPayload( "" );

		for( int i = 0; i < str.Payloads.Count; ++i )
		{
			if( str.Payloads[i].Type == PayloadType.RawText )
			{
				var payloadAsText = (TextPayload)str.Payloads[i];
				int tagStartIndex = payloadAsText.Text.LastIndexOf( tagStr );

				if( tagStartIndex >= 0 )
				{
					tagPayloadIndex = i;
					splitTextEndPayload = new TextPayload( payloadAsText.Text.Substring( tagStartIndex + tagStr.Length ) );
					payloadAsText.Text = payloadAsText.Text.Substring( 0, tagStartIndex );
				}
			}
		}

		if( tagPayloadIndex >= 0 )
		{
			var newPayloads = new List<Payload>( 6 );
			newPayloads.Add( new UIForegroundPayload( color ) );
			newPayloads.Add( new UIGlowPayload( glowColor ) );
			newPayloads.Add( new TextPayload( tagStr ) );
			newPayloads.Add( new UIForegroundPayload( 0 ) );
			newPayloads.Add( new UIGlowPayload( 0 ) );
			if( splitTextEndPayload.Text.Length > 0 ) newPayloads.Add( splitTextEndPayload );
			str.Payloads.InsertRange( tagPayloadIndex + 1, newPayloads );
		}
	}
}
