//
// SqlMetadata.cs
//
// Copyright (C) 2004 Novell, Inc.
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
using System.IO;
using System.Threading;
using Lucene.Net.Documents;
using Beagle.Util;

namespace Beagle.Daemon {

	[Flags]
	public enum AttributeFlags: ushort {
		None = 0,
		is_identifier =	0x0001,		// attribute can identify the object
		is_not_verified = 0x0002,	// attribute does not exist in our ontology
		for_virtual = 0x0004		// attribute should be kept for virtual instances
		
		// identifiers and for_virtual will be kept for virtual instances to make
		// sure new references can be detected using the identifiers.
	}

	[Flags]
	public enum InstanceFlags: ushort {
		None = 0,
		is_virtual = 0x0001			// this instance only exists for references
	}
	

	public class SqlMetadata : IMetadata {

		// Version history:
		// 1: Original version
		const int VERSION = 1;
		
		private EntityStore store;
		
		public SqlMetadata (string directory) 
		{
			store = new EntityStore (directory);
		}

		public SqlMetadata (SqlMetadata old) 
		{
			store = new EntityStore (old.store);
		}
		
		public void Flush () 
		{
			store.Flush ();
		}
		
		public void New (string uri, string class_name) {

			store.CreateInstance (uri, class_name);
		}
	
		public void AddnVerify (string uri, Property prop) {
			AddnVerify (uri, prop.Key, prop.Value);
		}	
		
		
		public void AddnVerify (string uri, string att, string val, uint class_id) {
		// FIXME: Make use of the class_id if we know it anyway because 
				// we are adding a lot statements about the same class.	
			AddnVerify (uri, att, val); 	
		}
		
		public void AddnVerify (string uri, string att, string val) {
			
			uint val_id = store.CreateObject (val);
			
			try {
				store.AddStatement (uri, att, val_id);
			} catch (NotVerifiedException) {
				Logger.Log.Debug ("not verified: {0}\t{1}\t{2}\tfor {3}",
						uri, att, val);
				try { 
					store.AddNonVerified (uri, att, val_id);
					return;
				}
				catch (Exception ex) {
					Logger.Log.Debug ("Could not insert attribute to not_verified :" +
						"\n\t{0} {1} {2} " +
						"\n\t INSERT statement failed.",
						uri, att, val);
					Logger.Log.Debug (ex);
				}
					
			}
			
			

		}
		
		public void AddnVerifyDateTime (string uri, string att, DateTime val) {
			// Sqlite does not offer a special way to store DateTime values so 
			// i'll just store it as a string.
			string str = StringFu.DateTimeToString (val);
			AddnVerify (uri, att, str);
		}
		
		
		
		
		public ArrayList GetProperties (string uri) 
		{
			ArrayList result = new ArrayList();
				
			store.Flush ();
				
			result = store.GetProperties (uri);
			result.AddRange (store.GetNonVerified (uri));
			
			return result;
		}

	}
}
