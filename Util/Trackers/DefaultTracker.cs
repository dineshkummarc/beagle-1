//
// DefaultTracker.cs: The most basic file tracker
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
using System.Collections.Generic;

namespace Beagle.Util.Trackers {
	
	public class DefaultTracker : FileTracker {
		private List<FileSystemWatcher> watchers;
		
		public DefaultTracker ()
		{
			watchers = new List<FileSystemWatcher> (); 
		}
		
		public void Watch (string path, TrackOperation operation)
		{
			Watch (path, operation, false);
		}

		public void Watch (string path, TrackOperation operation, bool recursive)
		{
			FileSystemWatcher watch = new FileSystemWatcher (path);
			watch.IncludeSubdirectories = recursive;
			watch.NotifyFilter = (NotifyFilters.FileName | NotifyFilters.DirectoryName | 
							NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.Security);
			
			if ((operation & TrackOperation.Changed) != 0) {
				watch.Changed += HandleNotification;
				Console.WriteLine ("change");
			}
			
			if ((operation & TrackOperation.Created) != 0) {
				watch.Created += HandleNotification;
					Console.WriteLine ("create");
			}
			
			if ((operation & TrackOperation.Deleted) != 0) {
				watch.Deleted += HandleNotification;
				Console.WriteLine ("delete");
			}
			
			if ((operation & TrackOperation.Renamed) != 0) {
				watch.Renamed += HandleRenameNotification;
				Console.WriteLine ("rename");
			}
			
			watch.EnableRaisingEvents = true;
			watchers.Add (watch);
		}
		
		public int Watches {
			get {
				return watchers.Count;
			}
		}
		
		private void RemoveWatcher (FileSystemWatcher watcher)
		{
			if (watcher != null && watchers.Contains (watcher))
				watchers.Remove (watcher);
		}
		
		private FileSystemWatcher GetWatcher (string path)
		{
			foreach (FileSystemWatcher w in watchers) {
				if (path.StartsWith (w.Path))
					return w;
			}
			
			return null;
		}
		
		protected virtual void HandleNotification (object o, FileSystemEventArgs args)
		{
			FileSystemWatcher watcher = GetWatcher (args.FullPath);
			
			if (watcher == null)
				return;
			
			bool is_dir = Directory.Exists (args.FullPath);
			string path = Path.GetDirectoryName (args.FullPath);
			TrackOperation operation = TrackOperation.Created;
			
			if (args.ChangeType == WatcherChangeTypes.Created)
				operation = TrackOperation.Created;
			else if (args.ChangeType == WatcherChangeTypes.Deleted) {
				// If our host directory is deleted, stop watching it
				if (path == watcher.Path)
					RemoveWatcher (watcher);
					
				operation = TrackOperation.Deleted;
			} else if (args.ChangeType == WatcherChangeTypes.Changed)
				operation = TrackOperation.Changed;
			else return;
			
			OnNotification (new FileTrackerEventArgs (path, args.Name, is_dir, operation));
		}
		
		protected virtual void HandleRenameNotification (object o, RenamedEventArgs args)
		{
			FileSystemWatcher watcher = GetWatcher (args.FullPath);
			
			if (watcher == null)
				return;
			
			bool is_dir = Directory.Exists (args.FullPath);
			string path = Path.GetDirectoryName (args.FullPath);
			string old_path = Path.GetDirectoryName (args.OldFullPath);
			
			OnNotification (new FileTrackerRenamedEventArgs 
				(path, args.Name, old_path, args.OldName, is_dir, TrackOperation.Renamed));
		}
		
		protected virtual void OnNotification (FileTrackerEventArgs args)
		{
			if (Notification != null)
				Notification (this, args);
		}
		
		public event FileTrackerEventHandler Notification;
	}
}
