//
// IndexableFilter.cs
//
// Copyright (C) 2004 Novell, Inc.
// Copyright (C) 2008 Lukas Lipka <lukaslipka@gmail.com>
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
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;

using System.Xml;
using System.Xml.Serialization;

using Beagle.Filters;
using Beagle.Util;

namespace Beagle.Engine {

	public class IndexableFilter {

		private static bool Debug = false;

		private static bool ShouldWeFilterThis (Indexable indexable)
		{
			if (indexable.Filtering == IndexableFiltering.Never || indexable.NoContent)
				return false;

			if (indexable.Filtering == IndexableFiltering.Always)
				return true;

			// Our default behavior is to try to filter non-transient file
			// indexable and indexables with a specific mime type attached.

			if (indexable.IsNonTransient || indexable.MimeType != null)
				return true;
			
			return false;
		}

		public static bool Filter (Indexable indexable, TextCache text_cache, out Filter filter)
		{
			ICollection filters = null;

			filter = null;

			if (indexable.Filtering == IndexableFiltering.AlreadyFiltered)
				return false;

			if (! ShouldWeFilterThis (indexable)) {
				indexable.NoContent = true;
				return false;
			}

			string path = null;

			// First, figure out which filter we should use to deal with
			// the indexable.

			// If a specific mime type is specified, try to index as that type.
			if (indexable.MimeType != null)
				filters = FilterFactory.CreateFiltersFromMimeType (indexable.MimeType);

			if (indexable.ContentUri.IsFile) {
				path = indexable.ContentUri.LocalPath;

				// Otherwise, set the mime type for a directory,
				// or sniff it from the file.
				if (indexable.MimeType == null) {
					if (Directory.Exists (path)) {
						indexable.MimeType = "inode/directory";
						indexable.NoContent = true;
					} else if (File.Exists (path)) {
						indexable.MimeType = XdgMime.GetMimeType (path);
					} else {
						Log.Warn ("Unable to filter {0}.  {1} not found.", indexable.DisplayUri, path);
						return false;
					}
				}

				// Set the timestamp to the last write time, if it isn't
				// set by the backend.
				if (! indexable.ValidTimestamp && indexable.IsNonTransient)
					indexable.Timestamp = FileSystem.GetLastWriteTimeUtc (path);

				// Check the timestamp to make sure the file hasn't
				// disappeared from underneath us.
				if (! FileSystem.ExistsByDateTime (indexable.Timestamp)) {
					Log.Warn ("Unable to filter {0}.  {1} appears to have disappeared from underneath us", indexable.DisplayUri, path);
					return false;
				}

				if (filters == null || filters.Count == 0) {
					filters = FilterFactory.CreateFiltersFromIndexable (indexable);
				}
			}

			// We don't know how to filter this, so there is nothing else to do.
			if (filters.Count == 0) {
				if (! indexable.NoContent) {
					indexable.NoContent = true;

					Logger.Log.Debug ("No filter for {0} ({1}) [{2}]", indexable.DisplayUri, path, indexable.MimeType);
					return false;
				}
				
				return true;
			}

			foreach (Filter candidate_filter in filters) {
				if (Debug)
					Logger.Log.Debug ("Testing filter: {0}", candidate_filter);
				
				// Hook up the snippet writer.
				if (candidate_filter.SnippetMode && text_cache != null) {
					if (candidate_filter.OriginalIsText && indexable.IsNonTransient) {
						text_cache.MarkAsSelfCached (indexable.Uri);
					} else if (indexable.CacheContent) {
						TextWriter writer = text_cache.GetWriter (indexable.Uri);
						candidate_filter.AttachSnippetWriter (writer);
					}
				}

				// Set the indexable on the filter.
				candidate_filter.Indexable = indexable;

				// Open the filter, copy the file's properties to the indexable,
				// and hook up the TextReaders.

				bool successful_open = false;
				TextReader text_reader;
				Stream binary_stream;

				if (path != null)
					successful_open = candidate_filter.Open (path);
				else if ((text_reader = indexable.GetTextReader ()) != null)
					successful_open = candidate_filter.Open (text_reader);
				else if ((binary_stream = indexable.GetBinaryStream ()) != null)
					successful_open = candidate_filter.Open (binary_stream);
					
				if (successful_open) {
					// Set FileType
					indexable.AddProperty (Property.NewKeyword ("beagle:FileType", candidate_filter.FileType));

					indexable.SetTextReader (candidate_filter.GetTextReader ());
					indexable.SetHotTextReader (candidate_filter.GetHotTextReader ());

					if (Debug)
						Logger.Log.Debug ("Successfully filtered {0} with {1}", path, candidate_filter);

					filter = candidate_filter;
					return true;
				} else {
					Log.Warn ("Error in filtering {0} with {1}, falling back", path, candidate_filter);
					candidate_filter.Cleanup ();
				}
			}

			if (Debug)
				Logger.Log.Debug ("None of the matching filters could process the file: {0}", path);
			
			return false;
		}

		public static bool Filter (Indexable indexable, out Filter filter)
		{
			return Filter (indexable, null, out filter);
		}

		public static bool Filter (Indexable indexable)
		{
			Filter filter = null;
			return Filter (indexable, null, out filter);
		}
	}
}
