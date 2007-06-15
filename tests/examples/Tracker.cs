//
// Modules.cs: A tracker example
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

	public sealed class Inotify {
	
		public static FileTracker tracker = null;
		
		public static void OnNotification (object o, FileTrackerEventArgs args)
		{
			Console.WriteLine ("New {0} created: {1}",
				(args.IsDirectory ? "directory" : "file"),
				(args.IsDirectory ? args.Path : String.Format ("{0}/{1}", args.Path, args.FileName)));
		}
		
		public static void Main ()
		{
			try {
				tracker = new InotifyTracker ();
				Console.WriteLine ("Using inotify to track changes!");
			} catch (NotSupportedException) {
				tracker = new DefaultTracker ();
				Console.WriteLine ("Inotify not supported! Using default tracker!");
			}
			
			tracker.Notification += OnNotification;
			tracker.Watch ("/home/postlund", TrackOperation.Created);
			Thread.Sleep (60); // Wait a minute before quitting
		}
	}
}
