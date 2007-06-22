//
// CollectorTracker.cs: A summarizing file tracker
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
using System.Threading;

namespace Beagle.Util.Trackers {
	
	public class CollectorTracker : FileTracker {
	
		private struct InternalEvent {
			public string Path;
			public string FileName;
			public string OldPath;
			public string OldFileName;
			public bool IsDirectory;
			public TrackOperation Operation;
			
			public InternalEvent (string path, string filename, string old_path, 
								string old_filename,  bool is_dir, TrackOperation operation)
			{
				Path = path;
				FileName = filename;
				OldPath = old_path;
				OldFileName = old_filename;
				IsDirectory = is_dir;
				Operation = operation;
			}
		}
	
		private FileTracker host_tracker = null;
		private uint delay_time, old_delay;
		private Queue event_queue;
		private bool debug = false;
		
		public CollectorTracker (FileTracker tracker) : this (tracker, 15000) { }
		
		public CollectorTracker (FileTracker tracker, uint delay)
		{
			if (tracker == null)
				throw new ArgumentException ("tracker is null");
			
			debug = (Environment.GetEnvironmentVariable ("COLLECTOR_DEBUG") != null);
			
			delay_time = old_delay = (delay == 0 ? 1 : delay);
			
			event_queue = new Queue ();
			host_tracker = tracker;
			host_tracker.Notification += OnNotification;
			
			GLib.Timeout.Add (old_delay, new GLib.TimeoutHandler (ProcessEvents)); 
		}
		
		public void Watch (string path, TrackOperation operation)
		{
			host_tracker.Watch (path, operation, false);
		}
		
		public void Watch (string path, TrackOperation operation, bool recursive)
		{
			host_tracker.Watch (path, operation, recursive);
		}
		
		private bool ProcessEvents ()
		{
			if (debug)
				Logger.Log.Debug ("CollectorTracker is processing {0} events", event_queue.Count);
			
			// Make sure we don't do anything stupid
			lock (event_queue.SyncRoot) {
				while (event_queue.Count > 0) {
					InternalEvent ev = (InternalEvent) event_queue.Dequeue ();
					OnNotification (ev);
				}
			}
		
			// Make sure we update watch time in case it was changed
			if (delay_time != old_delay) {
				old_delay = delay_time;
				GLib.Timeout.Add (old_delay, new GLib.TimeoutHandler (ProcessEvents));
				return false;
			}
			
			return true;
		}
		
		public uint Delay {
			get {
				return (delay_time == old_delay ? delay_time : old_delay);
			}
			set {
				delay_time = (value == 0 ? 1 : value);
			}
		}
		
		public int Watches { 
			get {
				return host_tracker.Watches;
			}
		}
		
		protected virtual void OnNotification (object o, FileTrackerEventArgs args)
		{
			Queue tmp = new Queue ();
			
			if (debug) {
				Logger.Log.Debug ("CollectorTracker: {0}/{1}, {2}", 
					args.Path, args.FileName, args.Operation);
			}
			
			lock (event_queue.SyncRoot) {
				bool added = false;
				FileTrackerRenamedEventArgs rea = args as FileTrackerRenamedEventArgs;
				
				while (event_queue.Count > 0) {
					InternalEvent ev = (InternalEvent) event_queue.Dequeue ();
					
					if (!EqualTarget (args, ev)) {
						tmp.Enqueue (ev);
						continue;
					}
					
					switch (args.Operation) {
					case TrackOperation.Changed:
						ev.Operation = (ev.Operation | TrackOperation.Changed);
						tmp.Enqueue (ev);
						break;
					case TrackOperation.Created:
						ev.Operation = TrackOperation.Created;
						tmp.Enqueue (ev);
						break;
					case TrackOperation.Renamed:
						ev.Operation = (ev.Operation | TrackOperation.Renamed);
						ev.Path = rea.Path;
						ev.FileName = rea.FileName;
						ev.OldPath = rea.OldPath;
						ev.OldFileName = rea.OldFile;
						tmp.Enqueue (ev);
						break;
					case TrackOperation.Deleted:
						// We will arrive here if a file once was created (and also added to the
						// queue) and then deleted. This will happen during one "delay cycle".
						// Announcing that a file was created and deleted is quite useless and
						// that's why the event isn't added back to the list here.
						break;
					}
					added = true;
					break;
				}
				
				if (!added) {
					tmp.Enqueue (new InternalEvent (
						args.Path, args.FileName,
						(rea != null ? rea.OldPath : null),
						(rea != null ? rea.OldFile : null),
						args.IsDirectory, args.Operation));
				}
				
				event_queue = tmp;
			}
		}
		
		private bool EqualTarget (FileTrackerEventArgs args, InternalEvent ev)
		{
			FileTrackerRenamedEventArgs rea = args as FileTrackerRenamedEventArgs;
			
			return ((rea == null && args.Path == ev.Path && args.FileName == ev.FileName) ||
					(rea != null && rea.OldPath == ev.Path && rea.OldFile == ev.FileName));
		}
		
		private void OnNotification (InternalEvent args)
		{
			if (Notification != null) {
				if ((args.Operation & TrackOperation.Renamed) != 0) {
					Notification (this, new FileTrackerRenamedEventArgs (
						args.Path, args.FileName, args.OldPath, 
						args.OldFileName, args.IsDirectory, args.Operation));
				} else {
					Notification (this, new FileTrackerEventArgs (
						args.Path, args.FileName, args.IsDirectory, args.Operation));
				}
			}
		}
		
		public event FileTrackerEventHandler Notification;
	}
}
