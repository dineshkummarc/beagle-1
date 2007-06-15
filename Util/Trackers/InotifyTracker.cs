//
// InotifyTracker.cs: An inotify based file tracker
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
using System.Collections;
using Beagle.Util;

namespace Beagle.Util.Trackers {
	
	public class InotifyTracker : FileTracker {
		
		public InotifyTracker ()
		{
			Inotify.Start ();
			if (!Inotify.Enabled)
				throw new NotSupportedException ("Inotify is not supported on this system");
			
		}
		
		public void Watch (string path, TrackOperation operation)
		{
			Watch (path, operation, false);
		}

		public void Watch (string path, TrackOperation operation, bool recursive)
		{
			Inotify.EventType type = GetEventType (operation);
			
			if (!recursive) {
				Inotify.Subscribe (path, HandleNotification, type);
			} else {
				Queue pending = new Queue ();
				pending.Enqueue (path);
				
				while (pending.Count > 0) {
					string dir = pending.Dequeue () as string;
					
					foreach (string subdir in DirectoryWalker.GetDirectories (dir))
						pending.Enqueue (subdir);
					
					Inotify.Subscribe (path, HandleNotification, type);
				}
			}
		}
		
		private Inotify.EventType GetEventType (TrackOperation operation)
		{
			bool init = false;
			Inotify.EventType type = Inotify.EventType.Access;
			
			if ((operation & TrackOperation.Created) != 0) {
				type = Inotify.EventType.Create;
				init = true;
			}
			
			if ((operation & TrackOperation.Changed) != 0) {
				if (!init) {
					type = Inotify.EventType.Modify;
					init = true;
				} else
					type = (type | Inotify.EventType.Modify);
			}
			
			if ((operation & TrackOperation.Deleted) != 0) {
				if (!init) {
					type = Inotify.EventType.Delete | Inotify.EventType.DeleteSelf;
					init = true;
				} else 
					type = (type | Inotify.EventType.Delete | Inotify.EventType.DeleteSelf);
			}
			
			if ((operation & TrackOperation.Renamed) != 0) {
				if (!init)
					type = Inotify.EventType.MovedTo | Inotify.EventType.MovedFrom;
				else
					type = (type | Inotify.EventType.MovedTo | Inotify.EventType.MovedFrom);
			}
			
			return type;
		}
		
		private TrackOperation GetTrackOperation (Inotify.EventType type)
		{
			bool init = false;
			TrackOperation operation = TrackOperation.Created;
			
			if ((type & Inotify.EventType.Create) != 0) {
				operation = TrackOperation.Created;
				init = true;
			}
			
			if ((type & Inotify.EventType.Modify) != 0) {
				if (!init) {
					operation = TrackOperation.Changed;
					init = true;
				} else
					operation = (operation | TrackOperation.Changed);
			}
			
			if ((type & (Inotify.EventType.Delete | Inotify.EventType.DeleteSelf)) != 0) {
				if (!init) {
					operation = TrackOperation.Deleted;
					init = true;
				} else
					operation = (operation | TrackOperation.Deleted); 
			}
			
			if ((type & (Inotify.EventType.MovedTo | Inotify.EventType.MovedFrom)) != 0) {
				if (!init) 
					operation = TrackOperation.Renamed;
				else
					operation = (operation | TrackOperation.Renamed);
			}
			
			return operation;
		}

		public int Watches {
			get {
				return Inotify.WatchCount;
			}
		}
		
		protected virtual void HandleNotification (
				Inotify.Watch watch,
				string path,
				string subitem,
				string srcpath,
				Inotify.EventType type)
		{
			bool is_dir = ((type & Inotify.EventType.IsDirectory) != 0 ? true : false);
			TrackOperation operation = GetTrackOperation (type);
			
			if (operation == TrackOperation.Renamed) {
				OnNotification (
					new FileTrackerRenamedEventArgs (path, null, srcpath, subitem, is_dir));
			} else {
				OnNotification (
					new FileTrackerEventArgs (path, subitem, is_dir, operation));
			}
			
			// If the directory was removed, make sure we don't monitor it anymore
			if (operation == TrackOperation.Deleted && is_dir && subitem == null)
				watch.Unsubscribe ();
		}

		protected virtual void OnNotification (FileTrackerEventArgs args)
		{
			if (Notification != null)
				Notification (this, args);
		}
		
		public event FileTrackerEventHandler Notification;
	}
}
