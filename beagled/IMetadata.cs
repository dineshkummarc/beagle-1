//
// Metadata.cs
//
// Copyright (C) 2006 Novell, Inc.
//

//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using Beagle.Util;
using Lucene.Net.Documents;

namespace Beagle.Daemon {

	public interface IMetadata {

		// type will be handled internally so instead of 
		// void AddnVerify (string sub, string pre, string obj, string type);
		//
		// we'll have:
		// void AddProperties (Indexable indexme); 
		// to add all the Properties at once.
		
		
		void AddnVerify (string sub, string pre, string obj); 
		
		void AddnVerify (string sub, Property prop); 

		void AddnVerifyDateTime (string sub, string pre, DateTime val);
		
		void New (string uri, string class_name);
	

		// I think we should actually deal with docs in IMetadata.
		// void BuildDocuments (Indexable indexable, out Document primary_doc, out Document secondary_doc);	
		// Document RewriteDocument (Document old_secondary_doc, Indexable prop_only_indexable);	
		
		ArrayList GetProperties (string sub) ;
		
		void Flush ();
		
	}
		 
}
		
