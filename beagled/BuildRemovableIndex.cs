//
// BuildRemovableIndex.cs
//
// Copyright (C) 2006 Debajyoti Bera <dbera.web@gmail.com>
// Copyright (C) 2005 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

using System.Xml;
using System.Xml.Serialization;

using Lucene.Net.Documents;
using Lucene.Net.Index;
using LNS = Lucene.Net.Search;

using Beagle;
using Beagle.Util;
using FSQ = Beagle.Daemon.FileSystemQueryable.FileSystemQueryable;

namespace Beagle.Daemon 
{
	class BuildRemovableIndex : BuildIndex
	{
		private string media_name;
		private string mount_point;

		public BuildRemovableIndex (string[] args) : base (args)
		{
			Source = "RemovableIndex";
			Recursive = true;
			EnableDelete = false;
		}

		private void GetParams (string[] args)
		{
			if (args.Length < 2)
				PrintUsage ();
		
			int i = 0;
			while (i < args.Length) {
			
				string arg = args [i];
				++i;
				string next_arg = i < args.Length ? args [i] : null;
			
				switch (arg) {
				case "-h":
				case "--help":
					PrintUsage ();
					break;
					
				case "--disable-directories":
					IndexDirectories = false;
					break;
					
				case "--enable-text-cache":
					CacheText = true;
					break;

				case "--disable-filtering":
					DisableFiltering = true;
					break;

				case "--allow-pattern":
					if (next_arg == null)
						break;

					if (next_arg.IndexOf (',') != -1) {
						foreach (string pattern in next_arg.Split (','))
							allowed_patterns.Add (new ExcludeItem (ExcludeType.Pattern, pattern));
						
					} else {
						allowed_patterns.Add (new ExcludeItem (ExcludeType.Pattern, next_arg));
					}
					
					++i;
					break;

				case "--deny-pattern":
					if (next_arg == null)
						break;

					if (next_arg.IndexOf (',') != -1) {
						foreach (string pattern in next_arg.Split (','))
							denied_patterns.Add (new ExcludeItem (ExcludeType.Pattern, pattern));

					} else {
						denied_patterns.Add (new ExcludeItem (ExcludeType.Pattern, next_arg));
					}

					++i;
					break;

				case "--disable-restart":
					DisableRestart = true;
					break;

				case "--media-name":
					if (next_arg != null)
						media_name = next_arg;
					++i;
					break;

				case "--debug":
					break;

				case "--mount-point":
					if (next_arg != null) {
						if (! Path.IsPathRooted (next_arg))
							next_arg = Path.GetFullPath (next_arg);

						if (next_arg != "/" && next_arg.EndsWith ("/"))
							mount_point = next_arg.TrimEnd ('/');
						else
							mount_point = next_arg;
			
					}
					++i;
					break;

				default:
					PrintUsage ();
					break;
				}
			}
			
		}

		protected override void ProcessParams (string[] args)
		{
			GetParams (args);

			// Process mount point
			if (mount_point == null) {
				Log.Error ("--mount-point must be specified");
				Environment.Exit (1);
			}

			if (Directory.Exists (mount_point)) {
				pending_directories.Enqueue (new DirectoryInfo (mount_point));
				Log.Info ("Paths will be stored relative to {0} as mount point.", mount_point);
			} else {
				Log.Error ("--mount-point must point to a directory");
				Environment.Exit (2);
			}

			// Process media name
			if (media_name == null) {
				Logger.Log.Error ("--media-name must be specified");
				Environment.Exit (3);
			} else {
				Target = Path.Combine (PathFinder.IndexDir, media_name);
				Tag = media_name;
				Log.Debug ("Storing index in {0}", Target);
			}
		}

		protected override Uri RemapUri (Uri uri)
		{
			// sanity check
			string path = uri.LocalPath;
			if (path == mount_point)
				return new Uri (String.Format ("removable://{0}/", media_name), true);

			if (path.IndexOf (mount_point) != 0) {
				Log.Warn ("Outside mounted directory: {0}", uri);
				return uri;
			}

			path = path.Remove (0, mount_point.Length);
			//Log.Debug ("Remapped {0} to removable://{1}", uri.LocalPath, path);
			return new Uri (String.Format ("removable://{0}/{1}", media_name, path), true);
		}

		private void AddToConf ()
		{
			Conf.Indexing.AddRemovableIndex (media_name, mount_point);
			Conf.Save ();
		}

		//////////////////////////////////////////////

		static void PrintUsage ()
		{
			string usage = 
				"beagle-removable-index: Build or update an index for removable media.\n" + 
				"Web page: http://www.gnome.org/projects/beagle\n";

			usage += 
				"Usage: beagle-build-index [OPTIONS] --media-name <media_name> --mount-point <mount_point>\n\n" +
				"Options:\n" +
				"  --enable-text-cache\t\tBuild text-cache of documents used for snippets.\n" +
				"  --disable-directories\t\tDon't add directories to the index.\n" +
				"  --disable-filtering\t\tDisable all filtering of files. Only index attributes.\n" + 
				"  --allow-pattern [pattern]\tOnly allow files that match the pattern to be indexed.\n" + 
				"  --deny-pattern [pattern]\tKeep any files that match the pattern from being indexed.\n" + 
				"  --disable-restart\t\tDon't restart when memory usage gets above a certain threshold.\n" +
				"  --debug\t\t\tEcho verbose debugging information.\n\n";

			
			Console.WriteLine (usage);
			Environment.Exit (0);
		}
		
		static void Main (string [] args)
		{
			try {
				SystemInformation.SetProcessName ("beagle-removable-index");

				BuildRemovableIndex build_removable_index = new BuildRemovableIndex (args);
				build_removable_index.DoBuildIndex ();
				build_removable_index.AddToConf ();
			} catch (Exception ex) {
				Logger.Log.Error (ex, "Unhandled exception thrown.  Exiting immediately.");
				Environment.Exit (1);
			}
		}

	}
}

