//
// BackendDriver.cs
//
// Copyright (C) 2004-2006 Novell, Inc.
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

using Beagle.Util;

namespace Beagle.Daemon {

	public static class BackendDriver {

		// Contains list of backends explicitly asked by --allow-backend or --backend name
		// These backends are exclusive, and the list of allowed backends isn't read from
		// the configuration.
		private static List <string> excl_allowed_backends = new List <string> ();

		// Contains list of disable backends, either from the configuration or through
		// --deny-baceknd or --backend -name.
		private static List <string> denied_backends = new List <string> ();
		
		public static void OnlyAllow (string name)
		{
			excl_allowed_backends.Add (name.ToLower ());
		}
		
		public static void Allow (string name)
		{
			ReadBackendsFromConf ();

			denied_backends.Remove (name.ToLower ());
		}
		
		public static void Deny (string name)
		{
			ReadBackendsFromConf ();

			name = name.ToLower ();
			if (!denied_backends.Contains (name))
				denied_backends.Add (name);
		}

		private static bool UseBackend (string name)
		{
			name = name.ToLower ();

			// Check if this is an exclusively allowed backend
			if (excl_allowed_backends.Contains (name))
				return true;

			// If it's not, and we have *any* exclusively allowed
			// backends, then we can't use this backend.
			if (excl_allowed_backends.Count > 0)
				return false;

			// Next, check to see if we're in the denied backends
			// list.
			if (denied_backends.Contains (name))
				return false;

			// Otherwise, we're good.
			return true;
		}

		///////////////////////////////////////////////////////////////

		// Paths to static backends
		private static List <string> static_backend_paths = new List <string> ();

		public static void AddStaticBackendPath (string path)
		{
			if (! static_backend_paths.Contains (path))
				static_backend_paths.Add (path);
		}

		///////////////////////////////////////////////////////////////

		private static bool done_reading_conf = false;

		private static void ReadBackendsFromConf ()
		{
			if (done_reading_conf)
				return;

			done_reading_conf = true;

			// If we have exclusively allowed backends, don't read
			// from our configuration.
			if (excl_allowed_backends.Count > 0)
				return;

			// To allow static indexes, "static" should be in
			// allowed_backends
			if (Conf.Daemon.AllowStaticBackend)
				Allow ("static");

			if (Conf.Daemon.DeniedBackends == null)
				return;
			
			foreach (string name in Conf.Daemon.DeniedBackends)
				denied_backends.Add (name.ToLower ());
		}

		///////////////////////////////////////////////////////////////

		private static List <IBackend> backends = null;

		private static void ScanAssemblyForBackends (Assembly assembly)
		{
			int count = 0;

			foreach (Type type in ReflectionFu.GetTypesFromAssemblyAttribute (assembly, typeof (IBackendTypesAttribute))) {
				foreach (BackendFlavor flavor in ReflectionFu.ScanTypeForAttribute (type, typeof (BackendFlavor))) {
					if (! UseBackend (flavor.Name))
						continue;

					IBackend backend = null;

					try {
						backend = Activator.CreateInstance (type) as IBackend;
					} catch (Exception e) {
						Log.Error (e, "Unable to instantiate backend {0}", flavor.Name);
					}

					if (backend == null)
						continue;

					backend.Name = flavor.Name;
					backend.Domain = flavor.Domain;

					if (backends == null)
						backends = new List <IBackend> ();

					backends.Add (backend);
					count++;
				}
			}

			Log.Debug ("Found {0} backends in {1}", count, assembly.Location);
		}

		///////////////////////////////////////////////////////////////

		// Delay before starting the indexing process

		static int indexing_delay = 60;  // Default to 60 seconds

		public static int IndexingDelay {
			set { indexing_delay = value; }
		}

		///////////////////////////////////////////////////////////////

		private static ArrayList assemblies = null;

		private static void PopulateAssemblies ()
		{
			if (assemblies != null)
				return;

			assemblies = ReflectionFu.ScanEnvironmentForAssemblies ("BEAGLE_BACKEND_PATH", PathFinder.BackendDir);

			// Only add the executing assembly if we haven't already loaded it.
			if (assemblies.IndexOf (Assembly.GetExecutingAssembly ()) == -1)
				assemblies.Add (Assembly.GetExecutingAssembly ());
		}

		public static void Init ()
		{
			ReadBackendsFromConf ();
			PopulateAssemblies ();
		}

		public static void LoadBackends ()
		{
			if (assemblies == null)
				throw new Exception ("BackendDriver.Init() must be called before BackendDriver.LoadBackends ()");

			foreach (Assembly assembly in assemblies) {
				ScanAssemblyForBackends (assembly);

				// This allows backends to define their own
				// executors.
				Server.ScanAssemblyForExecutors (assembly);
			}

			// No reason to keep around the assemblies list
			assemblies = null;

			LoadSystemIndexes ();
		}

		public static void StartBackends ()
		{
			foreach (IBackend backend in backends)
				Log.Debug (" - {0}", backend.Name);

			if (indexing_delay <= 0 || Environment.GetEnvironmentVariable ("BEAGLE_EXERCISE_THE_DOG") != null)
				ReallyStartBackends ();
			else {
				Log.Debug ("Waiting {0} seconds before starting backends", indexing_delay);
				GLib.Timeout.Add ((uint) indexing_delay * 1000, ReallyStartBackends);
			}
		}

		private static bool ReallyStartBackends ()
		{
			foreach (IBackend backend in backends)
				backend.Start ();

			return false;
		}

		public static ICollection <IBackend> Backends {
			get { 
				if (backends == null)
					throw new Exception ("BackendManager.LoadBackends() must be called before accessing BackendManager.Backends");

				return backends;
			}
		}

		///////////////////////////////////////////////////////////////

		private static void LoadSystemIndexes ()
		{
			if (! Directory.Exists (PathFinder.SystemIndexesDir))
				return;

			Log.Info ("Loading system-wide indexes.");

			int count = 0;

			foreach (DirectoryInfo index_dir in new DirectoryInfo (PathFinder.SystemIndexesDir).GetDirectories ()) {
				if (! UseBackend (index_dir.Name))
					continue;

				if (LoadStaticBackend (index_dir, QueryDomain.System))
					count++;
			}

			Log.Info ("Found {0} system-wide indexes.", count);
		}

		private static void LoadStaticBackends ()
		{
			int count = 0;

			if (UseBackend ("static")) {
				Log.Info ("Loading user-configured static indexes.");

				foreach (string path in Conf.Daemon.StaticQueryables)
					static_backend_paths.Add (path);
			}

			foreach (string path in static_backend_paths) {
				DirectoryInfo index_dir = new DirectoryInfo (StringFu.SanitizePath (path));

				if (!index_dir.Exists)
					continue;

				// FIXME: QueryDomain might be other than local
				if (LoadStaticBackend (index_dir, QueryDomain.Local))
					count++;
			}

			Log.Info ("Found {0} user-configured static indexes.", count);
		}

		// Instantiates and loads a StaticLuceneBackend from an index directory
		private static bool LoadStaticBackend (DirectoryInfo index_dir, QueryDomain query_domain)
		{
			StaticQueryable static_backend = null;
			
			if (!index_dir.Exists)
				return false;
			
			try {
				static_backend = new StaticQueryable (index_dir.FullName);
			} catch (InvalidOperationException) {
				Logger.Log.Warn ("Unable to create read-only index (likely due to index version mismatch): {0}", index_dir.FullName);
				return false;
			} catch (Exception e) {
				Logger.Log.Error (e, "Caught exception while instantiating static queryable: {0}", index_dir.Name);
				return false;
			}
			
			if (static_backend != null) {
				static_backend.Name = index_dir.Name;
				static_backend.Domain = query_domain;

				backends.Add (static_backend);

				return true;
			}

			return false;
		}

		///////////////////////////////////////////////////////////////

		static public IBackend GetBackend (string name)
		{
			foreach (IBackend backend in backends) {
				if (name == backend.Name)
					return backend;
			}

			return null;
		}

		static public bool IsIndexing {
			get {
				foreach (IBackend backend in Backends) {
					BackendStatus status = backend.BackendStatus;

					if (status == null)
						return false;

					if (status.IsIndexing)
						return true;
				}

				return false;
			}
		}					

		///////////////////////////////////////////////////////////////

		static public string ListBackends ()
		{
			string ret = "User:\n";

			PopulateAssemblies ();

			foreach (Assembly assembly in assemblies) {
				foreach (Type type in ReflectionFu.GetTypesFromAssemblyAttribute (assembly, typeof (IBackendTypesAttribute))) {
					foreach (BackendFlavor flavor in ReflectionFu.ScanTypeForAttribute (type, typeof (BackendFlavor)))
						ret += String.Format (" - {0}\n", flavor.Name);
				}
			}

			if (!Directory.Exists (PathFinder.SystemIndexesDir)) 
				return ret;
			
			ret += "System:\n";
			foreach (DirectoryInfo index_dir in new DirectoryInfo (PathFinder.SystemIndexesDir).GetDirectories ()) {
				ret += String.Format (" - {0}\n", index_dir.Name);
			}

			return ret;
		}

	}
}