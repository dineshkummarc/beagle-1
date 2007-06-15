//
// Paths.cs: All paths needed when discovering things to index
//
// Copyright (C) 2007 Pierre Östlund
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
using System.IO;
using System.Collections;

namespace Beagle.Util.Thunderbird.Utilities {
	
	public static class Paths {
		private static readonly string [] roots = new string [] {
			Path.Combine (Environment.GetEnvironmentVariable ("HOME"), ".thunderbird"),
			Path.Combine (Environment.GetEnvironmentVariable ("HOME"), ".mozilla-thunderbird")
		};
	
		public static string[] GetRootPaths ()
		{
			bool use_original = false, overriden = RootPathsOverriden;
			ArrayList avail_roots = new ArrayList ();
			
			// Add overriden roots (if any)
			if (overriden) {
				foreach (string root in GetOverridenRootPaths (out use_original)) 
					if (!avail_roots.Contains (root))
						avail_roots.Add (root);
			}
			
			// Add "original" roots. Make sure we add them if no root paths were overriden OR
			// if the roots paths were overriden but the original root paths were request as well
			// (using the $THUNDERBIRD_ROOTS-variable)
			if (!overriden | (overriden && use_original)) {
				foreach (string root in roots) {
					try {
						if (!avail_roots.Contains (root) && IsRootPath (root))
						avail_roots.Add (root);
					} catch (Exception e) {
						if (Environment.GetEnvironmentVariable ("THUNDERBIRD_ROOTS_DEBUG") != null)
							Logger.Log.Debug (e);
					}
				}
			}
			
			return (string []) avail_roots.ToArray (typeof (string));
		}
		
		private static string[] GetOverridenRootPaths (out bool use_original)
		{
			ArrayList overriden_roots = new ArrayList ();
			use_original = false;
			
			foreach (string root in 
				Environment.GetEnvironmentVariable ("THUNDERBIRD_ROOTS").Split (':')) {
				
				try {
					if (root.Equals ("THUNDERBIRD_ROOTS"))
						use_original = true;
					else if (IsRootPath (root))
						overriden_roots.Add (root);
				} catch (Exception e) {
					if (Environment.GetEnvironmentVariable ("THUNDERBIRD_ROOTS_DEBUG") != null)
						Logger.Log.Debug (e);
				}
			}
			
			return (string[]) overriden_roots.ToArray (typeof (string)); 
		}
		
		public static bool IsRootPath (string directory)
		{
			int found = 0;

			// Find required files
			foreach (string file in DirectoryWalker.GetFiles (directory)) {
				string file_name = Path.GetFileName (file);
				
				if (file_name.Equals ("profiles.ini") && !IO.IsEmpty (file))
					found++;
				else if (file_name.Equals ("appreg") && !IO.IsEmpty (file))
					found++;
			}
			
			// Make sure we also have at least one profile directory
			foreach (string dir in DirectoryWalker.GetDirectories (directory)) {
				string prefs_file = Path.Combine (dir, "prefs.js");
				
				if (File.Exists (prefs_file) && !IO.IsEmpty (prefs_file))
					found++;
			}
			
			return (found >= 3);
		}
		
		public static bool RootPathsOverriden {
			get {
				return (Environment.GetEnvironmentVariable ("THUNDERBIRD_ROOTS") != null);
			}
		}
	}
}
