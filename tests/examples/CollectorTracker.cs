//
// CollectorTracker.cs: A collector tracker example
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
using System.Threading;
using Beagle.Util.Trackers;

namespace Examples {

	public sealed class Collector {
	
		public static FileTracker collector = null;
		public static GLib.MainLoop main_loop = new GLib.MainLoop ();

		public static string GetMods (TrackOperation operation)
		{
			return String.Format ("{0} {1} {2} {3}",
				(operation & TrackOperation.Changed),
				(operation & TrackOperation.Created),
				(operation & TrackOperation.Deleted),
				(operation & TrackOperation.Renamed));
		}
		
		public static void OnNotification (object o, FileTrackerEventArgs args)
		{
			Console.WriteLine ("Time: {0}", DateTime.Now);

			if ((args.Operation & TrackOperation.Renamed) != 0) {
				FileTrackerRenamedEventArgs rea = (FileTrackerRenamedEventArgs) args;
				Console.WriteLine ("* {0}/{1} -> {2}/{3}, directory: {4}, mods: {5}",
					rea.OldPath, rea.OldFile, rea.Path, rea.FileName, rea.IsDirectory,
					GetMods (rea.Operation));
			} else {
				Console.WriteLine ("* {0}/{1}, directory: {2}, mods: {3}",
					args.Path, args.FileName, args.IsDirectory, GetMods (args.Operation));
			}
		}
		
		public static void Main (string[] args)
		{
			FileTracker tracker = null;

			try {
				tracker = new InotifyTracker ();
				Console.WriteLine ("Using inotify to track changes!");
			} catch (NotSupportedException) {
				tracker = new DefaultTracker ();
				Console.WriteLine ("Inotify not supported! Using default tracker!");
			}
			
			collector = new CollectorTracker (tracker, 5000);
			collector.Notification += OnNotification;

			foreach (string path in args)
				collector.Watch (path, TrackOperation.All);

			main_loop.Run ();
		}
	}
}
