//
// ModulesTest.cs: Utility test case
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
using System.IO;
using NUnit.Framework;
using Util = Beagle.Util.Thunderbird.Utilities;

namespace Regressions.Utilities {

	[TestFixture]
	public class UtilityTest : Assert {
	
		[Test] public void TestHexConvert1 ()
		{
			// Just try a few to see if it works (relies on heavily tested .NET code,
			// so it should probably work ok).
			int tmp = Util.Convert.HexToDec ("AB");
			Assert.AreEqual (tmp, 171);
			tmp = Util.Convert.HexToDec ("BEEF");
			Assert.AreEqual (tmp, 48879);
			tmp = Util.Convert.HexToDec ("FF");
			Assert.AreEqual (tmp, 255);
		}

		[ExpectedException (typeof (FormatException))]
		[Test] public void TestHexConvert2 ()
		{
			// This will/should fail!
			Util.Convert.HexToDec ("beagle");
		}

		[Test] public void TestDateConvert ()
		{
			// Just a random test case (same situation here as with HexToDec).
			DateTime time = Util.Convert.ToDateTime ("DEADBEEF");
			Assert.AreEqual (time.Year,1952);
			Assert.AreEqual (time.Month, 4);
			Assert.AreEqual (time.Day, 14);
			Assert.AreEqual (time.Hour, 15);
			Assert.AreEqual (time.Minute, 27);
			Assert.AreEqual (time.Second, 43);
		}

		[Test] public void TestGetFileSize ()
		{
			string tmp_file = Path.GetTempFileName ();
			FileStream stream = null;

			// We need to try this so that the file can be removed if was created
			try {
				stream = new FileStream (tmp_file, FileMode.Create);

				for (int i = 0; i < 10; i++)
					stream.WriteByte (Byte.Parse (" "));

				Assert.AreEqual (Util.IO.GetFileSize (tmp_file), 10);
				File.Delete (tmp_file);
					
			} catch (Exception e) {
				File.Delete (tmp_file);
				throw e;
			}
		}

		[Test] public void TestIsEmpty ()
		{
			string tmp_file = Path.GetTempFileName ();
			FileStream stream = null;

			// We need to try this so that the file can be removed if was created
			try {
				stream = new FileStream (tmp_file, FileMode.Create);

				Assert.IsTrue (Util.IO.IsEmpty (tmp_file));

				for (int i = 0; i < 10; i++)
					stream.WriteByte (Byte.Parse (" "));

				Assert.IsFalse (Util.IO.IsEmpty (tmp_file));
				File.Delete (tmp_file);
					
			} catch (Exception e) {
				File.Delete (tmp_file);
				throw e;
			}
		}

		[Test] public void TestRootsOverriden ()
		{
			Environment.SetEnvironmentVariable ("THUNDERBIRD_ROOTS", null);
			Assert.IsFalse (Util.Paths.RootPathsOverriden);
			Environment.SetEnvironmentVariable ("THUUtil.NDERBIRD_ROOTS", "test");
			Assert.IsTrue (Util.Paths.RootPathsOverriden);
		}

		/*[Test] public void TestGetRoots ()
		{

		}

		[Test] public void IsRoot ()
		{

		}

		[Test] public void TestRangeList ()
		{

		}*/
		
	}
}
