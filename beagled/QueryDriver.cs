//
// QueryDriver.cs
//
// Copyright (C) 2004 Novell, Inc.
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
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using Beagle.Util;

namespace Beagle.Daemon {
	
	public class QueryDriver {

		//////////////////////////////////////////////////////////////////////////////////////

		private static List <IQueryable> queryables = null;

		private static List <IQueryable> Queryables {
			get { return queryables; }
		}

		public static void Init ()
		{
			int count = 0;

			// Populate our list of queryables and get the keyword
			// mappings at the same time.
			queryables = new List <IQueryable> ();

			foreach (IBackend backend in BackendDriver.Backends) {
				IQueryable queryable = backend.Queryable;

				if (! queryables.Contains (queryable))
					queryables.Add (queryable);

				foreach (PropertyKeywordMapping mapping in ReflectionFu.ScanTypeForAttribute (backend.GetType (), typeof (PropertyKeywordMapping))) {
					PropertyKeywordFu.RegisterMapping (mapping);
					++count;
				}
			}

			Log.Debug ("Found {0} queryables in {1} backends", queryables.Count, BackendDriver.Backends.Count);
			Log.Debug ("Registered {0} keyword mappings from backends", count);

			count = 0;

			ArrayList assemblies = ReflectionFu.ScanEnvironmentForAssemblies ("BEAGLE_FILTER_PATH", PathFinder.FilterDir);

			foreach (Assembly assembly in assemblies) {
				foreach (Type t in ReflectionFu.GetTypesFromAssemblyAttribute (assembly, typeof (FilterTypesAttribute))) {
					foreach (PropertyKeywordMapping mapping in ReflectionFu.ScanTypeForAttribute (t, typeof (PropertyKeywordMapping))) {
						PropertyKeywordFu.RegisterMapping (mapping);
						++count;
					}
				}
			}

			Log.Debug ("Registered {0} keyword mappings from filters", count);
		}

		////////////////////////////////////////////////////////

		public delegate void ChangedHandler (IQueryable           iqueryable,
						     IQueryableChangeData changeData);

		static public event ChangedHandler ChangedEvent;

		// A method to fire the ChangedEvent event.
		static public void QueryableChanged (IQueryable           iqueryable,
						     IQueryableChangeData change_data)
		{
			if (ChangedEvent != null) {
				ChangedEvent (iqueryable, change_data);
			}
		}

		////////////////////////////////////////////////////////

		private class QueryClosure : IQueryWorker {

			IQueryable iqueryable;
			Query query;
			IQueryResult result;
			IQueryableChangeData change_data;
			
			public QueryClosure (IQueryable           iqueryable,
					     Query                query,
					     QueryResult          result,
					     IQueryableChangeData change_data)
			{
				this.iqueryable = iqueryable;
				this.query = query;
				this.result = result;
				this.change_data = change_data;
			}

			public void DoWork ()
			{
				iqueryable.DoQuery (query, result, change_data);
			}
		}

		static public void DoOneQuery (IQueryable           iqueryable,
					       Query                query,
					       QueryResult          result,
					       IQueryableChangeData change_data)
		{
			if (iqueryable.AcceptQuery (query)) {
				QueryClosure qc = new QueryClosure (iqueryable, query, result, change_data);
				result.AttachWorker (qc);
			}
		}

		static void AddSearchTermInfo (QueryPart          part,
					       SearchTermResponse response, StringBuilder sb)
		{
			if (part.Logic == QueryPartLogic.Prohibited)
				return;

			if (part is QueryPart_Or) {
				ICollection sub_parts;
				sub_parts = ((QueryPart_Or) part).SubParts;
				foreach (QueryPart qp in sub_parts)
					AddSearchTermInfo (qp, response, sb);
				return;
			}

			if (! (part is QueryPart_Text))
				return;

			QueryPart_Text tp;
			tp = (QueryPart_Text) part;

			string [] split;
			split = tp.Text.Split (' ');
 
			// First, remove stop words
			for (int i = 0; i < split.Length; ++i)
				if (LuceneCommon.IsStopWord (split [i]))
					split [i] = null;

			// Assemble the phrase minus stop words
			sb.Length = 0;
			for (int i = 0; i < split.Length; ++i) {
				if (split [i] == null)
					continue;
				if (sb.Length > 0)
					sb.Append (' ');
				sb.Append (split [i]);
			}
			response.ExactText.Add (sb.ToString ());

			// Now assemble a stemmed version
			sb.Length = 0; // clear the previous value
			for (int i = 0; i < split.Length; ++i) {
				if (split [i] == null)
					continue;
				if (sb.Length > 0)
					sb.Append (' ');
				sb.Append (LuceneCommon.Stem (split [i]));
			}
			response.StemmedText.Add (sb.ToString ());
		}

		////////////////////////////////////////////////////////

		static private void DehumanizeQuery (Query query)
		{
			// We need to remap any QueryPart_Human parts into
			// lower-level part types.  First, we find any
			// QueryPart_Human parts and explode them into
			// lower-level types.
			ArrayList new_parts = null;
			foreach (QueryPart abstract_part in query.Parts) {
				if (abstract_part is QueryPart_Human) {
					QueryPart_Human human = abstract_part as QueryPart_Human;
					if (new_parts == null)
						new_parts = new ArrayList ();
					foreach (QueryPart sub_part in QueryStringParser.Parse (human.QueryString))
						new_parts.Add (sub_part);
				}
			}

			// If we found any QueryPart_Human parts, copy the
			// non-Human parts over and then replace the parts in
			// the query.
			if (new_parts != null) {
				foreach (QueryPart abstract_part in query.Parts) {
					if (! (abstract_part is QueryPart_Human))
						new_parts.Add (abstract_part);
				}
				
				query.ClearParts ();
				foreach (QueryPart part in new_parts)
					query.AddPart (part);
			}

		}

		static private SearchTermResponse AssembleSearchTermResponse (Query query)
		{
			StringBuilder sb = new StringBuilder ();
			SearchTermResponse search_term_response;
			search_term_response = new SearchTermResponse ();
			foreach (QueryPart part in query.Parts)
				AddSearchTermInfo (part, search_term_response, sb);
			return search_term_response;
		}

		static private void QueryEachQueryable (Query       query,
							QueryResult result)
		{
			// The extra pair of calls to WorkerStart/WorkerFinished ensures:
			// (1) that the QueryResult will fire the StartedEvent
			// and FinishedEvent, even if no queryable accepts the
			// query.
			// (2) that the FinishedEvent will only get called when all of the
			// backends have had time to finish.

			object dummy_worker = new object ();

			if (! result.WorkerStart (dummy_worker))
				return;
			
			foreach (IQueryable iqueryable in Queryables)
				DoOneQuery (iqueryable, query, result, null);
			
			result.WorkerFinished (dummy_worker);
		}
		
		static public void DoQueryLocal (Query       query,
						 QueryResult result)
		{
			DehumanizeQuery (query);

			SearchTermResponse search_term_response;
			search_term_response = AssembleSearchTermResponse (query);
			query.ProcessSearchTermResponse (search_term_response);

			QueryEachQueryable (query, result);
		}

		static public void DoQuery (Query                                query,
					    QueryResult                          result,
					    RequestMessageExecutor.AsyncResponse send_response)
		{
			DehumanizeQuery (query);

			SearchTermResponse search_term_response;
			search_term_response = AssembleSearchTermResponse (query);
			send_response (search_term_response);

			QueryEachQueryable (query, result);
		}
	}
}
