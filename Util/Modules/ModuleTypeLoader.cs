//
// ModuleLoader.cs: This loader will locate modules with a ModuleAttribute
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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Beagle.Util;

namespace Beagle.Util.Modules {
	
	public class ModuleTypeLoader<T> : IEnumerable<Type> {
		private List<Type> modules;
		
		public ModuleTypeLoader()
		{
			this.modules = new List<Type> ();
		}
		
		public void ScanAssembly (Assembly assembly, Type attr_type)
		{
			foreach (Type type in ReflectionFu.GetTypesFromAssemblyAttribute (assembly, attr_type)) {
				
				T obj = (T) Activator.CreateInstance (type);
				ArrayList attr = ReflectionFu.ScanTypeForAttribute (type, typeof (ModuleAttribute));
				if (attr.Count > 0 && Inherits (typeof (T), obj))
					modules.Add (type);
			}
		}
		
		private bool Inherits (Type t, object o)
		{
			if (t.IsInterface) {
				foreach (Type intface in o.GetType ().GetInterfaces ()) {
					if (intface.Equals (t))
						return true;
				}
			} else if (t.IsClass) {
				return o.GetType ().IsSubclassOf (t);
			}
			
			return false;
		}
		
		public string GetName (Type module)
		{
			foreach (Type t in modules) {
				if (module.Equals (t)) {
					ArrayList attr = 
						ReflectionFu.ScanTypeForAttribute (t, typeof (ModuleAttribute));
					return ((ModuleAttribute) attr [0]).Name;
				}
			}
			
			throw new ModuleException ("Module does not exist");
		}
		
		public void UnloadAll ()
		{
			modules.Clear ();
		}

		/* public */ IEnumerator IEnumerable.GetEnumerator ()
		{
			return modules.GetEnumerator ();
		}
		
		public IEnumerator<Type> GetEnumerator ()
		{
			return modules.GetEnumerator ();
		}
		
		public int Count {
			get {
				return modules.Count;
			}
		}
	}
	
	public class ModuleException : Exception {
	
		public ModuleException (string message) : base (message) { }
	}
}
