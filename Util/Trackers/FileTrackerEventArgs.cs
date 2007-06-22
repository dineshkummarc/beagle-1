//
// FileTrackerEventArgs.cs: Event data used when announcing new file changes
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

namespace Beagle.Util.Trackers {

	public delegate void FileTrackerEventHandler (object sender, FileTrackerEventArgs args);
	
	public class FileTrackerEventArgs : EventArgs {
		private string path;
		private string filename;
		private bool is_directory;
		private TrackOperation operation;
		
		public FileTrackerEventArgs (string path, string file, 
									bool is_directory, TrackOperation operation)
		{
			this.path = path.TrimEnd ('/');
			this.filename = file;
			this.is_directory = is_directory;
			this.operation = operation;
		}
		
		public string Path {
			get { return path; }
		}
		
		public string FileName {
			get { return filename; }
		}
		
		public bool IsDirectory {
			get { return is_directory; }
		}
		
		public TrackOperation Operation {
			get { return operation; }
		}
	}
	
	public class FileTrackerRenamedEventArgs : FileTrackerEventArgs {
		private string old_path;
		private string old_file;
		
		public FileTrackerRenamedEventArgs (string path, string file, string old_path, 
											string old_file, bool is_directory, TrackOperation op) 
			: base (path, file, is_directory, op | TrackOperation.Renamed)
		{
			this.old_path = old_path;
			this.old_file = old_file;
		}
		
		public string OldPath {
			get { return old_path; }
		}
		
		public string OldFile {
			get { return old_file; }
		}
	}
}
