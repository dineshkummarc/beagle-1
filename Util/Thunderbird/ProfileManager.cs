//
// ProfileManager.cs: A manager that takes care of all existing profiles
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
using System.Collections.Generic;


namespace Beagle.Util.Thunderbird {

	public struct Profile { 
		public string Name;
		public bool IsRelative;
		public string Path;
		public bool Default;
	}

	public class ProfileEventArgs : EventArgs {
		private Profile profile;
		
		public ProfileEventArgs (Profile p)
		{
			this.profile = p;
		}
		
		public Profile Profile { 
			get {
				return profile;
			}
		}
	}

	public class ProfileManager : IManager<Profile, ProfileEventArgs> {
		
		public ProfileManager (string file)
		{
			throw new NotImplementedException ();
		}
		
		public void Load ()
		{
			throw new NotImplementedException ();
		}
		
		public void Unload ()
		{
			throw new NotImplementedException ();
		}
		
		public void Add (Profile profile)
		{
			throw new NotImplementedException ();
		}
		
		public bool Remove (Profile profile)
		{
			throw new NotImplementedException ();
		}
		
		public bool Contains (Profile profile)
		{
			throw new NotImplementedException ();
		}
		
		public void Clear ()
		{
			throw new NotImplementedException ();
		}
		
		public void CopyTo (Profile[] profiles, int index)
		{
			throw new NotImplementedException ();
		}
		
		public IEnumerator<Profile> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		
		/* public */ IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		
		public bool Loaded {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public int Count {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public bool IsReadOnly {
			get {
				throw new NotImplementedException ();
			}
		}
		
		protected virtual void OnAdded (ProfileEventArgs args)
		{
			if (Added != null)
				Added (this, args);
		}
		
		protected virtual void OnRemoved (ProfileEventArgs args)
		{
			if (Removed != null)
				Removed (this, args);
		}
		
		protected virtual void OnChanged (ProfileEventArgs args)
		{
			if (Changed != null)
				Changed (this, args);
		}
		
		public event EventHandler<ProfileEventArgs> Added;
		public event EventHandler<ProfileEventArgs> Removed;
		public event EventHandler<ProfileEventArgs> Changed;
	}
	
	public class InvalidProfileException : Exception {
		public InvalidProfileException (string message) : base (message) { }
	}
}
