//
// Modules.cs: A module example
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
using Beagle.Util.Modules;

namespace Examples {
	
	public interface ExampleModule {
		void Say (string message);
	}
	
	[Module ("First module")]
	public class FirstModule : ExampleModule {
		public void Say (string message)
		{
			Console.WriteLine (String.Format ("This message comes from FirstModule: {0}", message));
		}
	}
	
	[Module ("Second module")]
	public class SecondModule : ExampleModule {
		public void Say (string message)
		{
			Console.WriteLine (String.Format ("This message comes from SecondModule: {0}", message));
		}
	}
	
	public class Modules {

		public static void Main ()
		{
			ModuleLoader<ExampleModule> loader = new ModuleLoader<ExampleModule> ();
			loader.ScanAssembly (Assembly.GetCallingAssembly ());
			
			foreach (ExampleModule m in loader)
				m.Say ("Hello world!");
		}
	}
}
