using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ItemUse;

internal static class CofferManifests
{
	internal static void Init( string manifestFilePath )
	{
		ParseCofferData( manifestFilePath );
	}

	internal static void Uninit()
	{
		mCofferManifests.Clear();
	}

	private static void ParseCofferData( string filePath )
	{
		if( Path.Exists( filePath ) )
		{
			try
			{
				var manifestFile = File.OpenText( filePath );
				var line = manifestFile.ReadLine();
				int lineCount = 0;
				
				while( line != null )
				{
					++lineCount;

					bool lineValid = true;
					var tokens = line.Split( ',' );

					Int32 cofferID = 0;
					if( tokens.Length > 0 )
					{
						lineValid &= Int32.TryParse( tokens[0], out cofferID );
					}
					else
					{
						lineValid = false;
					}

					List<Int32> itemIDs = new();
					for( int i = 1; i < tokens.Length; ++i )
					{
						Int32 itemID;
						lineValid &= Int32.TryParse( tokens[i], out itemID );
						itemIDs.Add( itemID );
					}

					if( lineValid )
					{
						mCofferManifests.TryAdd( cofferID, [.. itemIDs] );
					}
					else
					{
						DalamudAPI.PluginLog.Error( $"Invalid coffer manifest at line {lineCount}, aborting manifest load." );
						mCofferManifests.Clear();
						break;
					}

					line = manifestFile.ReadLine();
				}

				manifestFile.Close();

				DalamudAPI.PluginLog.Information( $"Loaded {mCofferManifests.Count} coffer manifests." );
			}
			catch( Exception e )
			{
				DalamudAPI.PluginLog.Error( $"Unknown error while reading manifests file:\r\n{e}" );
			}
		}
		else
		{
			DalamudAPI.PluginLog.Warning( $"Unable to load coffer manifests; file \"{filePath}\" does not exist." );
		}
	}

	internal static bool ItemIsKnownCoffer( Int32 itemID )
	{
		return mCofferManifests.ContainsKey( itemID );
	}

	internal static ReadOnlySpan<Int32> GetCofferItems( Int32 cofferID )
	{
		if( mCofferManifests.TryGetValue( cofferID, out var itemArray ) ) return new( itemArray );
		else return null;
	}

	internal static HashSet<int> GetGCJobsForCoffer( Int32 cofferID )
	{
		HashSet<int> allGCItemJobs = new();

		foreach( var item in GetCofferItems( cofferID ) )
		{
			if( ItemCategorizer.IsGCItem( item ) )
			{
				allGCItemJobs.UnionWith( ItemCategorizer.GetJobsForItem( item ) );
			}
		}

		return allGCItemJobs;
	}

	internal static HashSet<int> GetLeveJobsForCoffer( Int32 cofferID )
	{
		HashSet<int> allLeveItemJobs = new();

		foreach( var item in GetCofferItems( cofferID ) )
		{
			if( ItemCategorizer.IsLeveItem( item ) )
			{
				allLeveItemJobs.UnionWith( ItemCategorizer.GetJobsForItem( item ) );
			}
		}

		return allLeveItemJobs;
	}

	//***** TODO: The caller can still modify the underlying arrays.  Not a big deal for a debug thing, but fix it at some point.
	internal static ReadOnlyDictionary<Int32, Int32[]> DEBUG_GetCofferManifests() => new( mCofferManifests );

	private static readonly SortedDictionary<Int32, Int32[]> mCofferManifests = new();
}
