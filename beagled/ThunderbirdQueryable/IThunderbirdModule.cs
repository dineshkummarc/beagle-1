//
// IThunderbirdModule.cs: These types are used when creating a new Thunderbird module
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

using Beagle.Util.Thunderbird;
using Beagle.Util.Trackers;

namespace Beagle.Daemon.ThunderbirdQueryable {
	
	public struct ModuleEnvironment {
		public readonly Account Account;
		public readonly TaskHandler Handler;
		public readonly FileTracker Tracker;
		public readonly string WorkingDirectory;
		
		public ModuleEnvironment (Account account, TaskHandler handler,
								FileTracker tracker, string working)
		{
			this.Account = account;
			this.Handler = handler;
			this.Tracker = tracker;
			this.WorkingDirectory = working;
		}
	}
	
	public interface IThunderbirdModule {
       void Initialize (ModuleEnvironment environment);
       void Unload ();
       void OnFileUpdate (FileTrackerEventArgs update);
       ModuleEnvironment Environment { get; }
	}
}
