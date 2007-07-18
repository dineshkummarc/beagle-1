//
// MorkStream.cs: A stream object decicated to sequential file reading
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

namespace Beagle.Util.Thunderbird.Utilities {

	public class FileReader {		
		private Stream stream;
		private byte[] buffer;
		private byte[] left_overs = null;
		private bool end_of_file = false;
		private long bytes_in_buffer = -1;
		private long current_position = -1;
		private long current_marker = -1;
		private long file_length = -1;
		
		public const int EndOfFile = -1;
		public const long NoMarker = -1;
		
		public FileReader (Stream stream) : this (stream, 512) { }
		
		public FileReader (Stream stream, int buffer_size)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			if (buffer_size < 1)
				throw new ArgumentException ("buffer_size < 1");
			
			this.stream = stream;
			file_length = stream.Length;
			stream.Position = 0;
			buffer = new byte [buffer_size];
			Open ();
		}
		
		protected virtual void Open ()
		{
		}
		
		public virtual int Read ()
		{
			CheckOpen ();
			
			if (EOF)
				return EndOfFile;
			
			// Move one step ahead
			current_position++;
			
			if (bytes_in_buffer == -1) {
				bytes_in_buffer = stream.Read (buffer, 0, buffer.Length);
			} else if (!EOF && current_position >= bytes_in_buffer) {
				
				if (current_marker >= 0 && left_overs == null) {
					left_overs = new byte [current_position - current_marker];
					Array.Copy (buffer, current_marker, left_overs, 0, left_overs.Length);
					current_marker = 0;
				} else if (current_marker >= 0 && left_overs != null) {
					long length = current_position - current_marker;
					byte[] tmp = new byte [left_overs.Length + length];
					Array.Copy (left_overs, 0, tmp, 0, left_overs.Length);
					
					Array.Copy (buffer, current_marker, tmp, left_overs.Length, length);
					left_overs = tmp;
					current_marker = 0;
				}

				// Read data from stream and reset current position
				bytes_in_buffer = stream.Read (buffer, 0, buffer.Length);
				current_position = 0;
			} 
			
			if (Current == EndOfFile)
				end_of_file = true;
			
			return buffer [current_position];
		}
		
		public virtual void IgnoreLine ()
		{
			CheckOpen ();
			while (!EOF && Current != 10)
				Read ();
		}
		
		public virtual void Reset ()
		{
			Array.Clear (buffer, 0, buffer.Length);
			left_overs = null;
			end_of_file = false;
			bytes_in_buffer = -1;
			current_position = -1;
			current_marker = -1;
		}

		public virtual void Close ()
		{
			if (stream != null)
				stream.Close ();
			Reset ();
			end_of_file = true;
			file_length = -1;
		}
		
		public void SetMarker ()
		{
			CheckOpen ();
			left_overs = null;
			current_marker = (current_position < 0 ? 0 : current_position);
		}
		
		// Calculates the buffer length needed after cutting. Everything less than zero can be
		// considered invalid and everything above zero is ok. Equal to zero has to be judged 
		// according to situation (since its just an empty buffer).
		private long GetCutLength (uint cut_start, uint cut_end)
		{
			CheckOpen ();
			long len = (current_position - current_marker) +
					(left_overs != null ? left_overs.Length : 0);
			
			return (len - cut_start - cut_end);
		}
		
		public byte[] GetFromMarker (uint cut_start, uint cut_end)
		{
			CheckOpen ();
			
			long len = current_position - current_marker, 
				left = (left_overs != null ? left_overs.Length : 0);
			long buf_length = GetCutLength (cut_start, cut_end);
			
			if (current_marker == NoMarker || buf_length < 1)
				return null;
			
			// Make sure we have an array with enough room
			byte[] tmp = new byte [buf_length]; 

			if (left_overs == null || cut_start >= left) {
				// Everything is in main buffer
				long cut = cut_start - left;
				Array.Copy (buffer, (current_marker+cut), tmp, 0, (len-cut_end-cut));
			} else if (cut_end >= current_position-current_marker) {
				// Everything is in left_overs
				long cut = cut_end - len;
				Array.Copy (left_overs, cut_start, tmp, 0, (left_overs.Length-cut_start-cut));
			} else {
				// Copy from both
				int start = (left_overs != null ? left_overs.Length : 0);

				if (left_overs != null)
					Array.Copy (left_overs, cut_start, tmp, 0, (start-cut_start));
				
				Array.Copy (buffer, current_marker, tmp, start-cut_start, (len-cut_end));
			}
			
			return tmp;
		}
		
		public bool Match (params int[] values)
		{
			CheckOpen ();
			if (values == null)
				throw new ArgumentNullException ("values");
			
			foreach (int a in values) {
				if (Read () != a)
					return false;
			}
			
			return true;
		}
		
		public void ResetMarker ()
		{
			CheckOpen ();
			current_marker = -1;
			left_overs = null;
		}
		
		protected void CheckOpen ()
		{
			if (stream == null || !stream.CanRead)
				throw new IOException ("stream is not open or readable");
		}
		
		public int Current {
			get {
				if (buffer == null || current_position >= bytes_in_buffer)
					return -1;
				
				return buffer [current_position]; 
			}
		}
		
		public long MarkerPosition {
			get { 
				CheckOpen ();
				return current_marker;
			}
		}
		
		public long MarkerLength {
			get {
				CheckOpen ();
				return (current_position - current_marker) + 
						(left_overs != null ? left_overs.Length : 0);
			}
		}
		
		public bool ActiveMarker {
			get { 
				CheckOpen ();
				return (current_marker < 0 ? false : true);
			}
		}
		
		public long Position {
			get { 
				CheckOpen ();
				return ((stream.Position / buffer.Length) * buffer.Length) + current_position; 
			}
		}
		
		public long InternalPosition {
			get {
				CheckOpen ();
				return current_position;
			}
		}
		
		public long Length {
			get {
				CheckOpen ();
				return file_length;
			}
		}
		
		public bool EOF {
			get {
				CheckOpen ();
				return end_of_file;
			}
		}
	}
}
