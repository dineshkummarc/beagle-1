//
// Convert.cs: A few helper methods for converting between different formats
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
using System.Text;
using Beagle.Util;

namespace Beagle.Util.Thunderbird.Utilities {
	
	public static class Convert {
		
		public static DateTime ToDateTime (string hex_date)
		{
			DateTime time = DateTimeUtil.UnixToDateTimeUtc (0);
			
			return time.AddSeconds (HexToDec (hex_date));
		}
		
		public static int HexToDec (string hex)
		{
			return System.Convert.ToInt32 (hex, 16);
		}
		
		public static bool ToBoolean (byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException ("bytes");
			if (bytes.Length < 4)
				throw new ArgumentException ("bytes.Length != 4");
			
			string str = Encoding.Default.GetString (bytes, 0, 4);
			
			// String.ToLower () is _really_ expensive and we don't want to use that 
			// (using ToLower () allocates more than 1000 new objects, which in my test 
			// case made the heap grow)
			return (str [0].Equals ('t') || str [0].Equals ('T')) &&
					(str [1].Equals ('r') || str [1].Equals ('R')) &&
					(str [2].Equals ('u') || str [2].Equals ('U')) &&
					(str [3].Equals ('e') || str [3].Equals ('E'));
		}
		
		public static bool ToBoolean (string str)
		{
			if (String.IsNullOrEmpty (str))
				throw new ArgumentNullException ("str");
			else if (str.Equals ("1"))
				return true;
			else if (str.Equals ("0"))
				return false;
			
			return System.Convert.ToBoolean (str);
		}
		
		public static int ToInt32 (byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException ("bytes");

			return System.Convert.ToInt32 (Encoding.Default.GetString (bytes));
		}
	}
}
