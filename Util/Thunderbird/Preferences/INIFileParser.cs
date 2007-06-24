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
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Beagle.Util.Thunderbird.Utilities;

namespace Beagle.Util.Thunderbird.Preferences {
	
	public struct INISection {
		public string Section;
		public Dictionary<string, string> Parameters;
		public static readonly INISection Null = NewEmpty ();
		
		private static INISection NewEmpty ()
		{
			INISection s = new INISection ();
			s.Section = null;
			s.Parameters = null;
			return s;
		}
		
		public static INISection New (string section)
		{
			INISection s = new INISection ();
			s.Section = section;
			s.Parameters = new Dictionary<string, string> ();
			
			return s;
		}
		
		public override string ToString ()
		{
			return String.Format ("Section: {0}\n{1}", Section, Parameters);
		}
	}
	
	public class INIFileParser : IEnumerable<INISection> {
		private FileReader reader;
		private List<INISection> sections;
		private static Encoding enc = Encoding.Default;
		private bool debug = false;
		
		// Various parsing tokens
		private const int Assignment = 61;
		private const int Comment = 59;
		private const int NewLine = 10;
		private const int SectionStart = 91;
		private const int SectionEnd = 93;
		private const int Whitespace = 10;
		private const int SpaceSign = 32;
		
		public INIFileParser (string filename) : this (new FileStream (filename, FileMode.Open)) { }
		
		public INIFileParser (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			
			this.sections = new List<INISection> ();
			this.reader = new FileReader (stream);
			
			debug = (Environment.GetEnvironmentVariable ("BEAGLE_INIPARSER_DEBUG") != null);
			
			// This is the parsing main loop
			while (!reader.EOF) {
			
				switch (reader.Current) {
				case Comment:
					reader.IgnoreLine ();
					break;
				case SectionStart:
					// We take very easy on parsing errors (according to the Thunderbird code base),
					// so we just catch all exceptions and act as if nothing happned... ;)
					try {
						ReadSection ();
					} catch { 
						if (debug)
							Logger.Log.Debug ("Failed to parse section");
					}
					break;
				default:
					// Everything that is not a section nor a comment will be ignored
					reader.Read ();
					break;
				}
			}
		}
		
		private void ReadSection ()
		{
			// Make sure the section has a name (and isn't just [])
			if (reader.Read () == SectionEnd) // [ - end of section
				return;
			
			// Set the marker and find the end of the section declaration
			reader.SetMarker ();
			while (!reader.EOF && reader.Current != SectionEnd) {
				// The closing ] must exist on the same line
				if (reader.Current == NewLine)
					return;
				
				reader.Read ();
			}
			
			// Create the section
			byte[] name = reader.GetFromMarker (0, 0);
			INISection section = INISection.New (enc.GetString (name));
			
			// Extract all paramaters and all section
			reader.IgnoreLine ();
			while (!reader.EOF && reader.Current != SectionStart) {
				try {
					ParseParameter (section);
				} catch (Exception e) {
					if (debug)
						//Console.WriteLine (e);
						Logger.Log.Debug (e, "Failed to parse parameter");
				}
			}
			
			sections.Add (section);
		}
		
		private void ParseParameter (INISection section)
		{
			byte[] param_key = null, param_value = null;
			
			reader.ResetMarker ();
			do {
				// Check for comments and new lines, they determine when a parameter ends
				if (reader.Current == Comment || reader.Current == NewLine) {
					if (param_key != null) {
						param_value = reader.GetFromMarker (1, 0);
						reader.IgnoreLine ();
						break;
					}
					
					// Start all over from here
					reader.ResetMarker ();
					reader.IgnoreLine ();
				} else if (reader.Current == Assignment) {
					param_key = reader.GetFromMarker (0, 0);
					reader.SetMarker ();
				} else if (!reader.ActiveMarker) {
					reader.SetMarker ();
				}
			} while (!reader.EOF && reader.Read () != SectionStart);
			
			if (param_key == null)
				return;
			
			string key_str = enc.GetString (param_key);
			string value_str = (param_value != null ? enc.GetString (param_value) : string.Empty);
			
			// This section will be added if it doesn't exist or just update if it exists
			section.Parameters.Add (key_str, value_str);
		}
		
		public bool ContainsSection (string section)
		{
			foreach (INISection s in sections) {
				if (s.Section.Equals (section))
					return true;
			}
			
			return false;
		}

		public INISection GetSection (string section)
		{
			foreach (INISection s in sections) {
				if (s.Section.Equals (section))
					return s;
			}
			
			return INISection.Null;
		}
		
		public string GetValue (string section, string key)
		{
			INISection s = GetSection (section);
			
			if (!s.Equals (INISection.Null) && s.Parameters.ContainsKey (key))
				return s.Parameters [key];
			
			return null;
		}
		
		public void Close ()
		{
			if (reader != null)
				reader.Close ();
			
			reader = null;
		}
		
		public IEnumerator<INISection> GetEnumerator ()
		{
			return sections.GetEnumerator ();
		}
		
		/* public */ IEnumerator IEnumerable.GetEnumerator ()
		{
			return sections.GetEnumerator ();
		}
		
		public int Count {
			get {
				return sections.Count;
			}
		}
	}
}
