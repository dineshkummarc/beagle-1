//
// RangeList.cs: A list specialized in storing integer ranges
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
using System.Collections;
using System.Collections.Generic;

namespace Beagle.Util.Thunderbird.Utilities {
	
	public struct Range {
		private int start, end;
		
		public static readonly Range Null = new Range (0, -1);
		
		private Range (int start, int end)
		{	
			this.start = start;
			this.end = end;
		}
		
		public static Range New (int start, int end)
		{
			if (start > end)
				throw new InvalidRangeException ("start > end");
			
			return new Range (start, end);
		}
		
		public static Range NewMax (params Range [] ranges)
		{
			if (ranges == null || ranges.Length < 1)
				throw new ArgumentNullException ("No ranges");
			
			Range r = ranges [0];
			r.Maximize (ranges);
			return r;
		}
		
		public void Maximize (params Range[] ranges)
		{
			if (ranges == null)
				throw new ArgumentNullException ("No ranges");
			
			for (int i = 0; i < ranges.Length; i++) {
				start = (ranges [i].Start < start ? ranges [i].Start : start);
				end = (ranges [i].End > end ? ranges [i].End : end);
			}
		}
		
		public bool IntersectsWith (Range r)
		{
			if (r.Equals (Range.Null))
				throw new ArgumentNullException ("r");
			
			return (r.End >= Start && r.End <= End) ||
					(r.Start >= Start && r.Start <= End) ||
					(Start >= r.Start && End <= r.End) ||
					(Start <= r.Start && End >= r.End);
		}
		
		public bool CoveredBy (Range r)
		{
			if (r.Equals (Range.Null))
				throw new ArgumentNullException ("r");
		
			return (r.Start <= Start && r.End >= End);
		}

		public bool Overlap (Range r)
		{
			if (r.Equals (Range.Null))
				throw new ArgumentNullException ("r");
			
			return !r.CoveredBy (this) && !CoveredBy (r)
					&& ((r.End >= Start && r.End <= End) || (r.Start >= Start && r.Start <= End));
		}
		
		public bool NextTo (Range r)
		{
			if (r.Equals (Range.Null))
				throw new ArgumentNullException ("r");
			
			return (r.End+1 == Start) || (r.Start-1 == End); 
		}
		
		public override bool Equals (object o)
		{
			if (o == null || GetType () != o.GetType ())
				return false;
				
			Range r = (Range) o;
			return (r.Start == Start) && (r.End == End);
		}
		
		public override int GetHashCode ()
		{
			return Start ^ End;
		}
		
		public override string ToString ()
		{
			return String.Format ("Start: {0}, End: {1}", Start, End);
		}
		
		public int Start {
			get {
				return start;
			}
		}
		
		public int End {
			get {
				return end;
			}
		}
	}
	
	public class RangeList : ICollection<Range> {
		
		internal class ListNode {
			public Range Range;
			public ListNode Next;
			
			public static ListNode New (Range range, ListNode next)
			{
				ListNode n = new ListNode ();
				n.Range = range;
				n.Next = next;
				return n;
			}
		}
		
		private ListNode list = null, last = null;
		
		public RangeList ()
		{
		}
		
		public void Add (Range range)
		{
			if (range.Equals (Range.Null)) {
				throw new ArgumentNullException ("null ranage");
			} else if (list == null) {
				Append (range);
			} else {
				ListNode old_list = list;
				list = null;
				
				while (old_list != null) {
					if (last.Range.IntersectsWith (old_list.Range) && !last.Range.Equals (old_list.Range)) {
						last.Range.Maximize (old_list.Range);
					} else if (range.IntersectsWith (old_list.Range)) {
						old_list.Range.Maximize (range);
						Append (old_list);
					} else if (last.Range.NextTo (old_list.Range)) {
						last.Range.Maximize (old_list.Range);
					} else if (old_list.Range.NextTo (range)) {
						old_list.Range.Maximize (range);
						Append (old_list);
					} else if (range.Start > old_list.Range.End && old_list.Next == null) {
						Append (old_list);
						Append (range);
						break;
					} else if (range.Start > old_list.Range.End && range.End < old_list.Next.Range.Start) {
						Append (old_list.Range);
						Append (range);
						last.Next = old_list.Next;last.Next = null;
						return; // We are done here
					} else if (range.End < old_list.Range.Start) {
						if (last.Range.End < range.Start || list == null) 
							Append (range);
						last.Next = old_list;
						return; // We are done here
					} else {
						Append (old_list);
					}
					
					old_list = old_list.Next;
				}
				
				last.Next = null;
			}
		}
		
		public bool Remove (Range range)
		{
			if (range.Equals (Range.Null))
				throw new ArgumentNullException ("range null");
				
			bool removed = false;
			ListNode old_list = list;
			list = null;
			
			while (old_list != null) {
				if (old_list.Range.CoveredBy (range)) {
					old_list = old_list.Next;
					continue;
				} else if (range.CoveredBy (old_list.Range)) {
					Append (Range.New (old_list.Range.Start, range.Start-1));
					Append (Range.New (range.End+1, old_list.Range.End));
					removed = true;
				} else if (old_list.Range.Overlap (range)) {
					int start = old_list.Range.Start, end = old_list.Range.End;
					
					if (range.End >= old_list.Range.Start && range.End <= old_list.Range.End)
						start = range.End+1;
					if (range.Start >= old_list.Range.Start && range.Start <= old_list.Range.End)
						end = range.Start-1;
					
					Append (Range.New (start, end));
					removed = true;
				} else {
					Append (old_list.Range);
				}			

				old_list = old_list.Next;
			}
			
			return removed;
		}
		
		private void Append (Range r)
		{
			if (list == null) {
				list = ListNode.New (r, null);
				last = list;
			} else {
				last.Next = ListNode.New (r, null);
				last = last.Next;
			}
		}
		
		private void Append (ListNode node)
		{
			if (list == null) {
				list = node;
				last = list;
			} else {
				last.Next = node;
				last = last.Next;
			}
		}
		
		public void Clear ()
		{
			list = null;
		}
		
		public void CopyTo (Range[] ranges, int index)
		{
			throw new NotSupportedException ();
			/*if (ranges == null)
				throw new ArgumentNullException ("ranges");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			if (ranges.Rank > 1)
				throw new ArgumentException ("too many dimensions");
			if (index >= ranges.Length)
				throw new ArgumentException ("index >= ranges.Length");
			if (index+RangeCount > ranges.Length)
				throw new ArgumentException ("ranges is to little");*/
		}
		
		public bool Contains (Range range)
		{
			ListNode pointer = list;
			
			while (pointer != null) {
				if (range.Start >= pointer.Range.Start && range.End <= pointer.Range.End)
					return true;
				
				pointer = pointer.Next;
			}
			
			return false;
		}
		
		public bool Contains (int n)
		{
			return Contains (Range.New (n, n));
		}
		
		public IEnumerator<Range> GetEnumerator ()
		{
			return new RangeEnumerator (list);
		}
		
		/* public */ IEnumerator IEnumerable.GetEnumerator ()
		{
			return new RangeEnumerator (list);
		}
		
		public override string ToString ()
		{
			ListNode tmp = list;
			StringBuilder builder = new StringBuilder ();
			
			while (tmp != null) {
				if (tmp.Next != null)
					builder.AppendLine (tmp.Range.ToString ());
				else
					builder.Append (tmp.Range.ToString ());
				tmp = tmp.Next;
			}
			
			return builder.ToString ();
		}
		
		public int Count {
			get {
				int count = 0;
				ListNode pointer = list;
				
				while (pointer != null) {
					count += (pointer.Range.End - pointer.Range.Start) + 1;
					pointer = pointer.Next;
				}
				
				return count;
			}
		}
		
		public int RangeCount {
			get {
				int count = 0;
				ListNode node = list;	
				
				while (node != null) {
					count++;
					node = node.Next;
				}
				
				return count;
			}
		}
		
		public bool IsReadOnly {
			get {
				return false;
			}
		}
		
		public class RangeEnumerator : IEnumerator<Range> {
			private ListNode current, list_start;
		
			internal RangeEnumerator (ListNode list)
			{
				list_start = list;
				Reset ();
			}
			
			public void Dispose ()
			{
				current = null;
				list_start = null;
			}
		
			public bool MoveNext ()
			{
				if (current == null && list_start != null) {
					current = list_start;
					return true;
				} else if (current != null && current.Next != null) {
					current = current.Next;
					return true;
				}
				
				return false;
			}
			
			public void Reset ()
			{
				current = null;
			}
			
			public Range Current {
				get {
					return current.Range;
				}
			}
			
			object System.Collections.IEnumerator.Current {
				get {
					return (object) current.Range;
				}
			}
		}
	}
	
	public class InvalidRangeException : Exception {
	
		public InvalidRangeException (string message) : base (message) { }
	}
}
