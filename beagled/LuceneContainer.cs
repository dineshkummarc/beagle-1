//
// LuceneContainer.cs
//
// Copyright (C) 2004-2007 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;

using Beagle.Util;
using Stopwatch = Beagle.Util.Stopwatch;

namespace Beagle.Daemon {

	public class LuceneContainer {

		// VERSION HISTORY
		// ---------------
		//
		//  1: Original
		//  2: Changed format of timestamp strings
		//  3: Schema changed to be more Dashboard-Match-like
		//  4: Schema changed for files to include _Directory property
		//  5: Changed analyzer to support stemming.  Bumped version # to
		//     force everyone to re-index.
		//  6: lots of schema changes as part of the general refactoring
		//  7: incremented to force a re-index after our upgrade to lucene 1.4
		//     (in theory the file formats are compatible, we are seeing 'term
		//     out of order' exceptions in some cases)
		//  8: another forced re-index, this time because of massive changes
		//     in the file system backend (it would be nice to have per-backend
		//     versioning so that we didn't have to purge all indexes just
		//     because one changed)
		//  9: changed the way properties are stored, changed in conjunction
		//     with sane handling of multiple properties on hits.
		// 10: changed to support typed and mutable properties
		// 11: moved mime type and hit type into properties
		// 12: added year-month and year-month-day resolutions for all
		//     date properties
		// 13: moved source into a property
		// 14: allow wildcard queries to also match keywords
		// 15: analyze PropertyKeyword field, and store all properties as
		//     lower case so that we're truly case insensitive.
		// 16: add inverted timestamp to make querying substantially faster
		// 17: add boolean property to denote a child indexable
		// 18: add source to secondary index when used
		private const int INDEX_VERSION = 18;
		
		public const int NUM_BUCKETS = 16;

		private string top_dir;
		private string fingerprint;

		private bool read_only;

		private LuceneQueryingDriver[] drivers = new LuceneQueryingDriver [NUM_BUCKETS];

		private Lucene.Net.Store.Directory[] primary_stores = new Lucene.Net.Store.Directory [NUM_BUCKETS];
		private Lucene.Net.Store.Directory[] secondary_stores = new Lucene.Net.Store.Directory [NUM_BUCKETS];

		public Lucene.Net.Store.Directory GetPrimaryStore (int bucket)
		{
			if (primary_stores [bucket] == null) {
				Log.Debug ("Getting directory for bucket {0}", bucket);
				primary_stores [bucket] = Lucene.Net.Store.FSDirectory.GetDirectory (Path.Combine (PrimaryIndexDirectory, bucket.ToString ()),
												     LockDirectory, false, read_only);
			}

			return primary_stores [bucket];
		}

		public Lucene.Net.Store.Directory GetSecondaryStore (int bucket)
		{
			if (secondary_stores [bucket] == null) {
				secondary_stores [bucket] = Lucene.Net.Store.FSDirectory.GetDirectory (Path.Combine (SecondaryIndexDirectory, bucket.ToString ()),
												       LockDirectory, false, read_only);
			}

			return secondary_stores [bucket];
		}

		///////////////////////////////////////////////////////////////

		private static LuceneContainer singleton_container = null;

		public static LuceneContainer Singleton {
			get {
				if (singleton_container != null)
					return singleton_container;

				singleton_container = new LuceneContainer ("Singleton", false);
				
				return singleton_container;
			}
		}

		///////////////////////////////////////////////////////////////

		public string TopDirectory { get { return top_dir; } }

		public string Fingerprint { get { return fingerprint; } }

		public LuceneQueryingDriver[] Drivers {
			get { return drivers; }
		}

		public string VersionFile {
			get { return Path.Combine (top_dir, "version"); }
		}

		public string FingerprintFile {
			get { return Path.Combine (top_dir, "fingerprint"); }
		}

		public string LockDirectory {
			get { return Path.Combine (top_dir, "Locks"); }
		}

		private string GetBucketDirectory (int bucket)
		{
			return Path.Combine (top_dir, bucket.ToString ());
		}

		private string PrimaryIndexDirectory {
			get { return Path.Combine (top_dir, "PrimaryIndex"); }
		}

		private string SecondaryIndexDirectory {
			get { return Path.Combine (top_dir, "SecondaryIndex"); }
		}

		///////////////////////////////////////////////////////////////

		public int GetBucket (Uri uri)
		{
			return Math.Abs (UriFu.UriToEscapedString (uri).GetHashCode ()) % NUM_BUCKETS;
		}

#if false
		public int GetBucket (string uri)
		{
			return Math.Abs (uri.GetHashCode ()) % NUM_BUCKETS;
		}
#endif

		///////////////////////////////////////////////////////////////

		private TextCache text_cache = null;

		public TextCache TextCache {
			get { return text_cache; }
			//set { text_cache = value; }
		}

		///////////////////////////////////////////////////////////////

		public LuceneContainer (string index_path) : this (index_path, false) { }

		// XXX FIXME: This will create the text cache, which we probably
		// don't want to do if we're in read-only mode.
		public LuceneContainer (string index_path, bool read_only) : this (index_path, TextCache.UserCache, false) { }

		public LuceneContainer (string index_path, TextCache text_cache, bool read_only)
		{
			if (Path.IsPathRooted (index_path))
				top_dir = index_path;
			else
				top_dir = Path.Combine (PathFinder.IndexDir, index_path);

			if (ExistsAndValid ())
				Open ();
			else if (! read_only)
				Create ();
			else
				throw new InvalidOperationException ("Index is out of date, but is read-only");

			this.text_cache = text_cache;
		}

		private bool ExistsAndValid ()
		{
			if (! (Directory.Exists (top_dir)
			       && File.Exists (VersionFile)
			       && File.Exists (FingerprintFile)
			       && Directory.Exists (PrimaryIndexDirectory)
			       && Directory.Exists (SecondaryIndexDirectory)
			       && Directory.Exists (LockDirectory)))
				return false;

			// Check the index's version number.  If it is wrong,
			// declare the index non-existent.

			StreamReader version_reader;
			string version_str;
			version_reader = new StreamReader (VersionFile);
			version_str = version_reader.ReadLine ();
			version_reader.Close ();

			int current_version = -1;

			try {
				current_version = Convert.ToInt32 (version_str);
			} catch (FormatException) {
				// This is an old major.minor file and doesn't parse.
				// That's ok, it means it's out of date.
			}

			if (current_version != INDEX_VERSION) {
				Logger.Log.Debug ("Version mismatch in {0}", top_dir);
				Logger.Log.Debug ("Index has version {0}, expected {1}",
						  current_version, INDEX_VERSION);
				return false;
			}

			// Check the lock directory: If there is a dangling write lock,
			// assume that the index is corrupted and declare it non-existent.
			DirectoryInfo lock_dir_info;
			lock_dir_info = new DirectoryInfo (LockDirectory);
			foreach (FileInfo info in lock_dir_info.GetFiles ()) {
				if (IsDanglingLock (info)) {
					Logger.Log.Warn ("Found a dangling index lock on {0}", info.FullName);
					return false;
				}
			}

			return true;
		}

		// Create will kill your index dead.  Use it with care.
		// You don't need to call Open after calling Create.
		private void Create ()
		{
			// Purge any existing directories.
			if (Directory.Exists (top_dir)) {
				Logger.Log.Debug ("Purging {0}", top_dir);
				Directory.Delete (top_dir, true);
			}

			// Create any necessary directories.
			Directory.CreateDirectory (top_dir);
			Directory.CreateDirectory (LockDirectory);
			
			// Create the indexes.
			for (int i = 0; i < NUM_BUCKETS; i++) {
				string path;
				Lucene.Net.Store.Directory primary_store, secondary_store;

				path = Path.Combine (PrimaryIndexDirectory, i.ToString ());
				primary_store = LuceneCommon.CreateIndex (path, LockDirectory);

				path = Path.Combine (SecondaryIndexDirectory, i.ToString ());
				secondary_store = LuceneCommon.CreateIndex (path, LockDirectory);

				drivers [i] = new LuceneQueryingDriver ();
				drivers [i].PrimaryStore = primary_store;
				drivers [i].SecondaryStore = secondary_store;
			}

			// Generate and store the index fingerprint.
			fingerprint = GuidFu.ToShortString (Guid.NewGuid ());
			TextWriter writer;
			writer = new StreamWriter (FingerprintFile, false);
			writer.WriteLine (fingerprint);
			writer.Close ();

			// Store our index version information.
			writer = new StreamWriter (VersionFile, false);
			writer.WriteLine (INDEX_VERSION);
			writer.Close ();
		}

		private void Open ()
		{
			Open (false);
		}

		private void Open (bool read_only_mode)
		{
			// Read our index fingerprint.
			TextReader reader;
			reader = new StreamReader (FingerprintFile);
			fingerprint = reader.ReadLine ();
			reader.Close ();

			// Create stores for our indexes.
			for (int i = 0; i < NUM_BUCKETS; i++) {
				Lucene.Net.Store.Directory primary_store, secondary_store;
				
				primary_store = Lucene.Net.Store.FSDirectory.GetDirectory (Path.Combine (PrimaryIndexDirectory, i.ToString ()),
											   LockDirectory, false, read_only_mode);
				secondary_store = Lucene.Net.Store.FSDirectory.GetDirectory (Path.Combine (SecondaryIndexDirectory, i.ToString ()),
											     LockDirectory, false, read_only_mode);

				drivers [i] = new LuceneQueryingDriver ();
				drivers [i].PrimaryStore = primary_store;
				drivers [i].SecondaryStore = secondary_store;
			}
		}

		public bool CheckSourceVersion (string source_name, int source_version)
		{
			string version_file = Path.Combine (top_dir, "version-" + source_name);

			// If the file doesn't exist, we don't have any
			// documents from this source in the index, and we're
			// ok.
			if (! File.Exists (version_file)) {
				WriteSourceVersionFile (source_name, source_version);
				return true;
			}

			StreamReader reader = new StreamReader (version_file);
			string version_str = reader.ReadLine ();
			reader.Close ();

			int current_version = Convert.ToInt32 (version_str);

			return (current_version == source_version);
		}

		public void PurgeSource (string source_name, int source_version)
		{
			for (int i = 0; i < NUM_BUCKETS; i++)
				LuceneCommon.PurgeSource (source_name, primary_stores [i], secondary_stores [i]);

			WriteSourceVersionFile (source_name, source_version);
		}

		private void WriteSourceVersionFile (string source_name, int source_version)
		{
			string version_file = Path.Combine (top_dir, "version-" + source_name);
			StreamWriter writer = new StreamWriter (version_file);
			writer.WriteLine (source_version);
			writer.Close ();
		}

		// Deal with dangling locks
		private bool IsDanglingLock (FileInfo info)
		{
			Log.Debug ("Checking for dangling locks...");

			// It isn't even a lock file
			if (! info.Name.EndsWith (".lock"))
				return false;

			StreamReader reader;
			string pid = null;

			try {
				reader = new StreamReader (info.FullName);
				pid = reader.ReadLine ();
				reader.Close ();

			} catch {
				// We couldn't read the lockfile, so it probably went away.
				return false;
			}

                       
			if (pid == null) {
				// Looks like the lock file was empty, which really
				// shouldn't happen.  It should contain the PID of
				// the process which locked it.  Lets be on the safe
				// side and assume it's a dangling lock.
				Log.Warn ("Found an empty lock file, that shouldn't happen: {0}", info.FullName);
				return true;
			}

			string cmdline_file;
			cmdline_file = String.Format ("/proc/{0}/cmdline", pid);
                       
			string cmdline = "";
			try {
				reader = new StreamReader (cmdline_file);
				cmdline = reader.ReadLine ();
				reader.Close ();
			} catch {
				// If we can't open that file, either:
				// (1) The process doesn't exist
				// (2) It does exist, but it doesn't belong to us.
				//     Thus it isn't an IndexHelper
				// In either case, the lock is dangling --- if it
				// still exists.
				return info.Exists;
			}

			// The process exists, but isn't an IndexHelper.
			// If the lock file is still there, it is dangling.
			// FIXME: During one run of bludgeon I got a null reference
			// exception here, so I added the cmdline == null check.
			// Why exactly would that happen?  Is this logic correct
			// in that (odd and presumably rare) case?
			if (cmdline == null || cmdline.IndexOf ("IndexHelper.exe") == -1)
				return info.Exists;
                       
			// If we reach this point, we know:
			// (1) The process still exists
			// (2) We own it
			// (3) It is an IndexHelper process
			// Thus it almost certainly isn't a dangling lock.
			// The process might be wedged, but that is
			// another issue...
			return false;
		}
	}
}