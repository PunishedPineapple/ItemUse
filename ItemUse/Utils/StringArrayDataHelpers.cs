using System;

using Dalamud.Game.Text.SeStringHandling;

using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace ItemUse;

internal static unsafe class StringArrayDataHelpers
{
	internal static void SetString( uint arrayIndex, int stringIndex, SeString str )
	{
		if( Framework.Instance() != null &&
			Framework.Instance()->GetUIModule() != null &&
			Framework.Instance()->GetUIModule()->GetRaptureAtkModule() != null )
		{
			var atkArrayDataHolder = Framework.Instance()->GetUIModule()->GetRaptureAtkModule()->AtkModule.AtkArrayDataHolder;

			if( atkArrayDataHolder.StringArrayCount > arrayIndex )
			{
				SetString( atkArrayDataHolder.StringArrays[arrayIndex], stringIndex, str );
			}
		}
	}

	internal static void SetString( StringArrayData* pStringArrayData, int stringIndex, SeString str )
	{
		if( pStringArrayData != null && pStringArrayData->AtkArrayData.Size > stringIndex )
		{
			try
			{
				var encodedStr = str.EncodeWithNullTerminator();
				if( encodedStr.Length > 0 )
				{
					pStringArrayData->SetValue( stringIndex, encodedStr );
				}
			}
			catch( Exception e )
			{
				DalamudAPI.PluginLog.Error( $"Uknown error setting AtkArrayData string [0x{(nint)pStringArrayData:X}, {stringIndex}]:\r\n{e}" );
			}
		}
	}

	internal static SeString GetString( uint arrayIndex, int stringIndex )
	{
		if( Framework.Instance() != null &&
			Framework.Instance()->GetUIModule() != null &&
			Framework.Instance()->GetUIModule()->GetRaptureAtkModule() != null )
		{
			var atkArrayDataHolder = Framework.Instance()->GetUIModule()->GetRaptureAtkModule()->AtkModule.AtkArrayDataHolder;

			if( atkArrayDataHolder.StringArrayCount > arrayIndex )
			{
				return GetString( atkArrayDataHolder.StringArrays[arrayIndex], stringIndex );
			}
		}

		return null;
	}

	internal static SeString GetString( StringArrayData* pStringArrayData, int stringIndex )
	{
		if( pStringArrayData != null && pStringArrayData->AtkArrayData.Size > stringIndex )
		{
			try
			{
				return SeString.Parse( pStringArrayData->StringArray[stringIndex] );
			}
			catch( Exception e )
			{
				DalamudAPI.PluginLog.Error( $"Uknown error setting AtkArrayData string [0x{(nint)pStringArrayData:X}, {stringIndex}]:\r\n{e}" );
			}
		}

		return null;
	}
}
