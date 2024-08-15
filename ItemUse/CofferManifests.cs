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
			Int32 cofferID = 0;
			List<Int32> cofferItemIDs = new();

			//***** TODO: Read the file, one row per coffer.

			mCofferManifests.TryAdd( cofferID, cofferItemIDs.ToArray() );
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
