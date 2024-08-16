using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
				while( line != null )
				{
					var tokens = line.Split( ',' );
					if( tokens.Length > 1 )
					{
						List<int> tokens2 = new();
						for( int i = 1; i < tokens.Length; ++i ) tokens2.Add( int.Parse( tokens[i] ));
						mCofferManifests.TryAdd( int.Parse( tokens[0] ), tokens2.ToArray() );
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

	private static readonly SortedDictionary<Int32, Int32[]> mCofferManifests = new();
}
