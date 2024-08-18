using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;

namespace ItemUse;

internal class SeStringUtils
{
	internal static void HighlightLastOccuranceOfText( ref SeString str, string tagStr )
	{
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
			newPayloads.Add( new UIForegroundPayload( 500 ) );
			newPayloads.Add( new UIGlowPayload( 501 ) );
			newPayloads.Add( new TextPayload( tagStr ) );
			newPayloads.Add( new UIForegroundPayload( 0 ) );
			newPayloads.Add( new UIGlowPayload( 0 ) );
			if( splitTextEndPayload.Text.Length > 0 ) newPayloads.Add( splitTextEndPayload );
			str.Payloads.InsertRange( tagPayloadIndex + 1, newPayloads );
		}
	}
}
