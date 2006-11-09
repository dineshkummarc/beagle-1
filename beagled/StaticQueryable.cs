//
// StaticQueryable.cs
//
// Copyright (C) 2005 Novell, Inc.
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
using System.IO;
using System.Collections;
using System.Threading;

using System.Xml;
using System.Xml.Serialization;
	
using Beagle.Util;

namespace Beagle.Daemon {

	[PropertyKeywordMapping (Keyword="extension", PropertyName="beagle:FilenameExtension", IsKeyword=true, Description="File extension, e.g. extension:jpeg. Use extension: to search in files with no extension.")]
	[PropertyKeywordMapping (Keyword="ext", PropertyName="beagle:FilenameExtension", IsKeyword=true, Description="File extension, e.g. ext:jpeg. Use ext: to search in files with no extension.")]

	public class StaticQueryable : LuceneQueryable 	{
		
		protected TextCache text_cache;
		
		public StaticQueryable (string index_path) : base (index_path, true)
		{
			Logger.Log.Debug ("Initializing static queryable: {0}", index_path);

			if (Directory.Exists (Path.Combine (index_path, "TextCache"))) {
				try {
					text_cache = new TextCache (index_path, true);
				} catch (UnauthorizedAccessException) {
					Logger.Log.Warn ("Unable to purge static queryable text cache in {0}.  Will run without it.", index_path);
				}
			}
		}

		override protected LuceneQueryingDriver BuildLuceneQueryingDriver (string source_name, int source_version, bool read_only_mode)
		{
			// Return a new querying driver for static backends
			// instead of the normal singleton.
			return new LuceneQueryingDriver (source_name, source_version, read_only_mode);
		}

		override public string GetSnippet (string[] query_terms, Hit hit) 
		{
			if (text_cache == null)
				return null;

			// Look up the hit in our local text cache.
			TextReader reader = text_cache.GetReader (hit.Uri);
			if (reader == null)
				return null;
			
			string snippet = SnippetFu.GetSnippet (query_terms, reader);
			reader.Close ();
			
			return snippet;
		}

		override protected bool HitFilter (Hit hit)
		{
			// We can't cehck anything else than filr uris
			if (! hit.Uri.IsFile)
				return true;

			// FIXME: This is a hack.  We need to support parent Uris in some sane way
			int j = hit.Uri.LocalPath.LastIndexOf ('#');
			return File.Exists ((j == -1) ? hit.Uri.LocalPath : hit.Uri.LocalPath.Substring (0, j));
		}
	}
}
