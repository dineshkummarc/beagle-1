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

namespace Beagle.Util.Trackers {
	
	public class CollectorTracker : FileTracker {
		
		public CollectorTracker (FileTracker tracker)
		{
			throw new NotImplementedException ();
		}
		
		public void Watch (string path, TrackOperation operation)
		{
			throw new NotImplementedException ();
		}
		
		public void Watch (string path, TrackOperation operation, bool recursive, bool hidden)
		{
			throw new NotImplementedException ();
		}
		
		public uint Delay {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public uint Watches { 
			get {
				throw new NotImplementedException ();
			}
		}
		
		protected virtual void OnNotification (FileTrackerEventArgs args)
		{
			if (Notification != null)
				Notification (this, args);
		}
		
		public event FileTrackerEventHandler Notification;
	}
}
