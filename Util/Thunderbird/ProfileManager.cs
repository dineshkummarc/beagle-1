//
// ProfileManager.cs: A manager that takes care of all existing/avilable profiles
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
using Beagle.Util.Thunderbird.Preferences;
using TbUtil =  Beagle.Util.Thunderbird.Utilities;

namespace Beagle.Util.Thunderbird {

	public struct Profile { 
		public string Name;
		public bool IsRelative;
		public string Path;
		public bool Default;
		
		public static readonly Profile Null = NewEmpty ();
		
		private static Profile NewEmpty ()
		{
			Profile p = new Profile ();
			p.Name = null;
			p.IsRelative = false;
			p.Path = null;
			p.Default = false;
			return p;
		}
		
		public override string ToString ()
		{
			if (!Equals (Null)) {
				return String.Format ("Name: {0}, Path: {1}, IsRelative: {2}, Default: {3}",
						Name, Path, IsRelative, Default);
			}
				
			return string.Empty;
		}
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
	
	public class ProfileChangedEventArgs : ProfileEventArgs {
		private Profile old_profile;
		
		public ProfileChangedEventArgs (Profile new_profile, Profile old_profile) 
			: base (new_profile)
		{
			this.old_profile = old_profile;
		}
		
		public Profile OldProfile {
			get {
				return old_profile;
			}
		}
	}

	public class ProfileManager : IManager<Profile, ProfileEventArgs> {
		private string filename = null;
		private List<Profile> profiles = null;
		
		public ProfileManager (string file)
		{
			this.filename = file;
		}
		
		public void Load ()
		{
			if (Loaded) {
				Reload ();
				return;
			}
			
			profiles = new List<Profile> ();
			
			// Open file and add all profiles
			int profile_num = 0;
			INIFileParser parser = new INIFileParser (filename);
			while (true) {
				INISection sec = parser.GetSection (String.Format ("Profile{0}", profile_num++));
				if (!sec.Equals (INISection.Null)) {
					Add (ToProfile (sec));
				} else {
					break;
				}
			}

			parser.Close ();
			parser = null;
		}
		
		public void Reload ()
		{
			CheckLoaded ();
			
			List<Profile> new_profiles = new List<Profile> (),
					removals = new List<Profile> ();
			
			// Load the ini file again and save the content in a local list
			int profile_num = 0;
			INIFileParser parser = new INIFileParser (filename);
			while (true) {
				INISection sec = parser.GetSection (String.Format ("Profile{0}", profile_num++));
				if (!sec.Equals (INISection.Null)) {
					new_profiles.Add (ToProfile (sec));
				} else {
					break;
				}
			}
			
			// Add new profiles and update existing
			foreach (Profile p in new_profiles)
				Add (p);
			
			// Create a list of profile that doesn't exist anymore (since we can change the list
			// while iterating)
			foreach (Profile p in profiles) {
				bool found = false;
				
				// Figure out if we have this profile
				foreach (Profile new_p in new_profiles) {
					if (Equal (new_p, p)) {
						found = true;
						break;
					}
				}
				
				// If this profile does not exist in the new list, then it has been removed
				if (!found)
					removals.Add (p);
			}
			
			// Finally, remove non-existing profiles
			foreach (Profile p in removals)
				Remove (p);
		}
		
		public void Unload ()
		{	
			Clear ();
			profiles = null;
		}
		
		public void Add (Profile profile)
		{
			CheckLoaded ();
			
			foreach (Profile p in profiles) {
				bool update_stat = Update (p, profile);
				if (update_stat || (Equal (p, profile) && !update_stat))
					return;
			}
			
			// If we didn't already have this profile, add it
			profiles.Add (profile);
			OnAdded (new ProfileEventArgs (profile));
		}
		
		private bool Update (Profile old_profile, Profile new_profile)
		{
			CheckLoaded ();
			if (old_profile.Equals (Profile.Null))
				throw new ArgumentNullException ("old_profile");
			if (new_profile.Equals (Profile.Null))
				throw new ArgumentNullException ("new_profile");
			
			if (Equal (old_profile, new_profile) && profiles.Contains (old_profile)) {
				if (!old_profile.Name.Equals (new_profile.Name) 
					|| old_profile.IsRelative != new_profile.IsRelative
					|| old_profile.Default != new_profile.Default) {
					
					profiles.Remove (old_profile);
					profiles.Add (new_profile);
					OnChanged (new ProfileChangedEventArgs (new_profile, old_profile));
					
					return true;
				}
			}
			
			return false;
		}
		
		// A local version of the equal-method for profiles
		private bool Equal (Profile p1, Profile p2)
		{
			// The directory will be used as the default "comparing" value. Thunderbird will
			// automatically rearrange profile names which will render profile name useless
			// when comparing if two profiles are "equal".
			return p1.Path.Equals (p2.Path);
		}
		
		public bool Remove (Profile profile)
		{
			CheckLoaded ();
			foreach (Profile p in profiles) {
				if (p.Equals (profile)) {
					profiles.Remove (p);
					OnRemoved (new ProfileEventArgs (p));
					return true;
				}
			}
			
			return false;
		}
		
		public bool Contains (Profile profile)
		{
			CheckLoaded ();
			return profiles.Contains (profile);
		}
		
		public void Clear ()
		{
			CheckLoaded ();
			while (profiles.Count > 0) 
				Remove (profiles [0]);
		}
		
		public void CopyTo (Profile[] array, int index)
		{
			CheckLoaded ();
			profiles.CopyTo (array, index);
		}
		
		public IEnumerator<Profile> GetEnumerator ()
		{
			CheckLoaded ();
			return profiles.GetEnumerator ();
		}
		
		/* public */ IEnumerator IEnumerable.GetEnumerator ()
		{
			CheckLoaded ();
			return profiles.GetEnumerator ();
		}
		
		public static Profile ToProfile (INISection section)
		{
			Profile profile = new Profile ();
			
			profile.Name = section.Section;
			section.Parameters.TryGetValue ("Path", out profile.Path);
			
			// Try to parse the "IsRelative" attribute
			try {
				profile.IsRelative = TbUtil.Convert.ToBoolean (section.Parameters ["IsRelative"]);
			} catch (Exception e) {
				Console.WriteLine (e);
				// We default this to false
				profile.IsRelative = false;
			}
			
			// Now do the same thing with the "Default" attribute
			try {
				profile.Default = TbUtil.Convert.ToBoolean (section.Parameters ["Default"]);
			} catch {
				profile.Default = false;
			}
			
			return profile;
		}
		
		private void CheckLoaded ()
		{
			if (!Loaded)
				throw new InvalidOperationException ("Not loaded");
		}
		
		public bool Loaded {
			get {
				return (profiles != null);
			}
		}
		
		public int Count {
			get {
				return (Loaded ? profiles.Count : 0);
			}
		}
		
		public bool IsReadOnly {
			get {
				return false;
			}
		}
		
		public Profile Default {
			get {
				CheckLoaded ();
				foreach (Profile p in profiles) {
					if (p.Default)
						return p;
				}
				
				return Profile.Null;
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
		
		protected virtual void OnChanged (ProfileChangedEventArgs args)
		{
			if (Changed != null)
				Changed (this, args);
		}
		
		public event EventHandler<ProfileEventArgs> Added;
		public event EventHandler<ProfileEventArgs> Removed;
		public event EventHandler<ProfileEventArgs> Changed;
	}
}
