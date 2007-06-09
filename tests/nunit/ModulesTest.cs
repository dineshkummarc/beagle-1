//
// ModulesTest.cs: Module test case
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
using NUnit.Framework;
using Beagle.Util.Modules;

namespace Regressions.Modules {

	public interface IExampleModule {

	}
	
	public interface ITest {
		void Say (string message);
	}
	
	[Module (ModulesTest.ModuleOne)]
	public class ModuleOne : IExampleModule {

	}
	
	[Module (ModulesTest.ModuleTwo)]
	public class ModuleTwo : IExampleModule {

	}
	
	[Module (ModulesTest.TestModule)]
	public class TestModule : ITest {
		public void Say (string message)
		{
			Console.WriteLine (message);
			
			// Just for the fun of it...
			throw new ArgumentException ("This should happen...");
		}
	}
	
	[TestFixture] 
	public class ModulesTest : Assert {	
	
		public const string ModuleOne = "Module one";
		public const string ModuleTwo = "Module two";
		public const string TestModule = "Test module";
		
		[Test] public void TestLoad1 ()
		{
			ModuleLoader<IExampleModule> loader = new ModuleLoader<IExampleModule> ();
			Assert.AreEqual (loader.Count, 0);
			loader.ScanAssembly (Assembly.GetCallingAssembly ());
			Assert.AreEqual (loader.Count, 2);
			loader.UnloadAll ();
			Assert.AreEqual (loader.Count, 0);
		}
		
		[Test] public void TestMethods1 ()
		{
			ModuleLoader<IExampleModule> loader = new ModuleLoader<IExampleModule> ();
			loader.ScanAssembly (Assembly.GetCallingAssembly ());
			
			IEnumerator<IExampleModule> enm = loader.GetEnumerator ();
			
			enm.MoveNext ();
			Assert.Equals (loader.GetName (enm.Current), ModuleOne);
			
			enm.MoveNext ();
			Assert.Equals (loader.GetName (enm.Current), ModuleTwo);
		}
		
		[Test] public void TestLoad2 ()
		{
			ModuleLoader<ITest> loader = new ModuleLoader<ITest> ();
			Assert.AreEqual (loader.Count, 0);
			loader.ScanAssembly (Assembly.GetCallingAssembly ());
			Assert.AreEqual (loader.Count, 1);
			loader.UnloadAll ();
			Assert.AreEqual (loader.Count, 0);
		}
		
		[ExpectedException (typeof (ArgumentException))]
		[Test] public void TestMethods2 ()
		{
			ModuleLoader<ITest> loader = new ModuleLoader<ITest> ();
			loader.ScanAssembly (Assembly.GetCallingAssembly ());
			
			IEnumerator<ITest> enm = loader.GetEnumerator ();
			
			enm.MoveNext ();
			Assert.Equals (loader.GetName (enm.Current), TestModule);
			
			enm.Current.Say ("Hello world!");
		}
	}
}
