//
// ModuleManager.cs: A manager that takes care of running modules
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
using Beagle.Util.Thunderbird;

namespace Beagle.Daemon.ThunderbirdQueryable {

	public enum ThunderbirdModuleType {
		Regular,
		Standalone
	}
	
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = false)]
	public class ModuleSettings : Attribute {
		private string descriptor = null;
		private	ThunderbirdModuleType type = ThunderbirdModuleType.Regular;
		
		public ModuleSettings (string descriptor)
		{
			this.descriptor = descriptor;
		}
		
		public ModuleSettings (ThunderbirdModuleType type)
		{
			this.type = type;
		}
		
		public string Descriptor {
			get {
				return descriptor;
			}
		}
		
		public ThunderbirdModuleType ModuleType {
			get {
				return type;
			}
			set {
				type = value;
			}
		}
	}
	
	public class ModuleEventArgs : EventArgs {
		private IThunderbirdModule module;
		
		public ModuleEventArgs (IThunderbirdModule module)
		{
			this.module = module;
		}
		
		public IThunderbirdModule Module {
			get {
				return module;
			}
		}
	}
	
	public class ModuleManager : IManager<IThunderbirdModule, ModuleEventArgs> {
		
		public ModuleManager ()
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
		
		public void Add (IThunderbirdModule module)
		{
			throw new NotImplementedException ();
		}
		
		public bool Remove (IThunderbirdModule module)
		{
			throw new NotImplementedException ();
		}
		
		public bool Contains (IThunderbirdModule module)
		{
			throw new NotImplementedException ();
		}
		
		public void Clear ()
		{
			throw new NotImplementedException ();
		}
		
		public void CopyTo (IThunderbirdModule[] modules, int index)
		{
			throw new NotImplementedException ();
		}
		
		public IThunderbirdModule GetOwner (string file)
		{
			throw new NotImplementedException ();
		}
		
		public IEnumerator<IThunderbirdModule> GetEnumerator ()
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
		
		protected virtual void OnAdded (ModuleEventArgs args)
		{
			if (Added != null)
				Added (this, args);
		}
		
		protected virtual void OnRemoved (ModuleEventArgs args)
		{
			if (Removed != null)
				Removed (this, args);
		}
		
		protected virtual void OnChanged (ModuleEventArgs args)
		{
			if (Changed != null)
				Changed (this, args);
		}
		
		public event EventHandler<ModuleEventArgs> Added;
		public event EventHandler<ModuleEventArgs> Removed;
		public event EventHandler<ModuleEventArgs> Changed;
	}
}
