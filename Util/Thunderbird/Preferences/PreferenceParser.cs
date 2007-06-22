//
// PreferenceParser.cs: A parser for the "Mozilla User Preference" file format
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
using TbUtil = Beagle.Util.Thunderbird.Utilities;

namespace Beagle.Util.Thunderbird.Preferences {
	
	public class PreferenceStore : PropertyStore {
		
		public PreferenceStore ()
		{
		}
	}
	
	public class PreferenceParser {
		private TbUtil.FileReader reader;
		private PreferenceStore store;
		private bool debug = false;
		private static Encoding enc = Encoding.Default;
		
		// Various parsing tokens
		private const int NumberSign = 35;
		private const int StarSign = 42;
		private const int CommentSign = 47;
		private const int EscapeSign = 92;
		private const int PrefStart = 112;
		private const int UserPrefStart = 117;
		private const int QuoteSign = 34;
		private const int CommaSign = 44;
		private const int SpaceSign = 32;
		private const int NewlineSign = 10;
		
		public PreferenceParser (PreferenceStore store, string file) :
			this (store, new FileStream (file, FileMode.Open)) { }
		
		public PreferenceParser (PreferenceStore store, Stream stream)
		{
			if (store == null)
				throw new ArgumentNullException ("store");
			if (stream == null)
				throw new ArgumentNullException ("stream");
			
			this.store = store;
			reader = new TbUtil.FileReader (stream);
			
			debug = (Environment.GetEnvironmentVariable ("BEAGLE_PREFPARSER_DEBUG") != null);
			
			// Main parsing loop
			while (!reader.EOF) {
				switch (reader.Current) {
				case NumberSign: // We parse this as a single line comment
					reader.IgnoreLine ();
					break;
				case CommentSign:
					try {
						ParseSlashStarComment ();
					} catch (Exception e) {
						if (debug)
							Logger.Log.Warn (e, "Error when parsing /* comment");
					}
					break;
				case UserPrefStart:
					// We currently are on the character "u" and want to check if the sequence
					// following is "ser_".
					//if (reader.Read () == 115 && reader.Read () == 101 
					//	&& reader.Read () == 114 && reader.Read () == 95) {
					if (reader.Match (115, 101, 114, 95)) {
						reader.Read ();
						try {
							ParsePreference ();
						} catch (Exception e) {
							if (debug)
								Logger.Log.Warn (e, "Failed to parse preference");
						}
					}
					break;
				case PrefStart:
					try {
						ParsePreference ();
					} catch (Exception e) {
						if (debug)
							Logger.Log.Warn (e, "Failed to parse preference");
					}
					break;
				default:
					reader.Read ();
					break;
				}
			}
		}
		
		private void ParseSlashStarComment ()
		{
			// Check if we have a comment (we have to have a / followed by a *)
			if (reader.Current != CommentSign || reader.Read () != StarSign)
				return;
			
			// Just continue ignoring until we reach end of comment
			bool escaped = false, star = false;
			while (reader.Read () != TbUtil.FileReader.EndOfFile) {
				if (reader.Current == EscapeSign) {
					escaped = true;
				} else if (reader.Current == StarSign) {
					star = true;
				} else if (reader.Current == CommentSign && star && !escaped) {
					reader.Read ();
					break;
				} else {
					escaped = false;
					star = false;
				}
			}
		}
		
		private void ParsePreference ()
		{
			// We must check if we have the sequence "pref(" in order to continue
			if (reader.Current != 112 && reader.Read () != 114 
				&& reader.Read () != 101 && reader.Read () != 102 && reader.Read () != 40)
			return;
			
			bool escaped = false, separator = false;
			byte[] key_part = null, value_part = null;
			
			reader.ResetMarker ();
			while (reader.Read () != TbUtil.FileReader.EndOfFile) {
				if (!reader.ActiveMarker && reader.Current == QuoteSign && key_part == null && !separator) {
					// If there is no marker, current character is a " and we haven't yet
					// saved a key value, then we have found the start of a key.
					reader.SetMarker ();
				} else if (reader.ActiveMarker && reader.Current == EscapeSign) {
					// In case we find an escape sign, save this so that we can ignore the
					// status of the next character
					escaped = true;
				} else if (reader.ActiveMarker && reader.Current == QuoteSign && key_part == null && !escaped) {
					// We are now at the ending " of the key
					key_part = reader.GetFromMarker (1, 0);
					reader.ResetMarker ();
				} else if (!reader.ActiveMarker && reader.Current == CommaSign && key_part != null) {
					// This is the comma sign that separates the key from the value. We know
					// that what follows must be the key. But we just save this state for know
					// and wait out all unnecessary spaces.
					separator = true;
					reader.ResetMarker ();
				} else if (separator && reader.Current != SpaceSign && !reader.ActiveMarker) {
					// Set marker for the value (all preamble spaces have been ignored)
					reader.SetMarker ();
				} else if (!reader.ActiveMarker && key_part != null && (reader.Current != SpaceSign && reader.Current != NewlineSign)) {
					// Nothing except spaces, newlines or a comma is allowed to follow a key
					return;
				} else if (reader.ActiveMarker && reader.Current == 41) {
					// We reached the end of the value part.
					value_part = reader.GetFromMarker (0, 0);
					break;
				} else {
					// Since the escape sign is only valid for one character, reset it
					escaped = false;
				}
			}
			
			AddPreference (key_part, value_part);
		}
		
		private void AddPreference (byte[] key_part, byte[] value_part)
		{
			if (key_part == null)
				return;
			
			string key = enc.GetString (key_part);
			
			if (value_part == null) {
				// We interpret a null value as a string
				store.Add (key, PropertyValue.New (""));
			} else if ((int) value_part [0] == QuoteSign) {
				// If the first sign is a quote sign, then we have a string
				store.Add (key, PropertyValue.New (
					enc.GetString (value_part, 1, value_part.Length-2)));
			} else {
				// We try to convert the value into an integer value
				try {
					store.Add (key, PropertyValue.New (TbUtil.Convert.ToInt32 (value_part)));
				} catch {
					// Not a string nor an integer. Must be a boolean. If it can't be parsed as
					// a boolean, an exception will be thrown an caught in the main loop. Thus,
					// this preference won't be added.
					store.Add (key, PropertyValue.New (TbUtil.Convert.ToBoolean (value_part)));
				}
			}
			
			if (debug) {
				Logger.Log.Debug ("*** {0}={1}, {2} ({3})", 
					key, store [key].ToString (),
					(value_part != null ? enc.GetString (value_part) : string.Empty), 
					store [key].Type);
			}
		}
	}
}
