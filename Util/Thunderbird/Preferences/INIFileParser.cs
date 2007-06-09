//
// INIFileParser.cs: A basic INI-file parser
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

namespace Beagle.Util.Thunderbird.Preferences {
	
	public struct INISection {
		public string Section;
		public Dictionary <string, string> Parameters;
		
		public static INISection New (string section)
		{
			throw new NotImplementedException ();
		}
	}
	
	public class INIFileParser : IEnumerable<INISection> {
		
		public INIFileParser (string filename)
		{
			throw new NotImplementedException ();
		}
		
		public void Load ()
		{
			throw new NotImplementedException ();
		}
		
		public string GetSection (string section)
		{
			throw new NotImplementedException ();
		}
		
		public string GetValue (string section, string key)
		{
			throw new NotImplementedException ();
		}
		
		public IEnumerator<INISection> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		
		/* public */ IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		
		public string Filename { 
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
	}
}
