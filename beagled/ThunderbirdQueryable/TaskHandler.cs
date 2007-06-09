//
// TaskHandler.cs: A TaskHandler helps out when adding new data to the indexing queue
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
using Beagle.Util;

namespace Beagle.Daemon.ThunderbirdQueryable {
	
	public class TaskHandler {
	
		public TaskHandler (Scheduler scheduler)
		{
			throw new NotImplementedException ();
		}

		public void AddTask (IIndexableGenerator generator, string tag)
		{
			throw new NotImplementedException ();
		}
		
		public void ScheduleRemoval (Property prop, Scheduler.Priority prio)
		{
			throw new NotImplementedException ();
		}
		
		public void ScheduleRemoval (Uri uri, string tag, Scheduler.Priority prio)
		{
			throw new NotImplementedException ();
		}
		
		public void ScheduleRemoval (Uri[] uris, string tag, Scheduler.Priority prio)
		{
			throw new NotImplementedException ();
		}
	}
}
