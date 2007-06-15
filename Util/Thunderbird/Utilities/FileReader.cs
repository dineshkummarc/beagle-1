//
// MorkStream.cs: A stream object decicated to reading data from a Mork file
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
		private long bytes_in_buffer = -1;
		private long current_position = -1;
		private long current_marker = -1;
		private long file_length = -1;
		
		public FileReader (Stream stream) : this (stream, 512) { }
		
		public FileReader (Stream stream, int buffer_size)
		{
			this.stream = stream;
			file_length = stream.Length;
			buffer = new byte [buffer_size];
			Open ();
		}
		
		protected virtual void Open ()
		{
		}
		
		public int Read ()
		{
			// Move one step ahead
			current_position++;
			
			if (stream == null) {
				return -1;
			} else if (bytes_in_buffer == -1) {
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
				
			} else if (EOF && current_position >= bytes_in_buffer) {

				return -1;	// End of file
			} 
			
			return buffer [current_position];
		}
		
		public void IgnoreLine ()
		{
			while (!EOF && Current != 10)
				Read ();
		}

		public void Close ()
		{
			stream.Close ();
			Array.Clear (buffer, 0, buffer.Length);
			left_overs = null;
			bytes_in_buffer = -1;
			current_position = -1;
			current_marker = -1;
			file_length = -1;
		}
		
		public void SetMarker ()
		{
			left_overs = null;
			current_marker = (current_position < 0 ? 0 : current_position);
		}
		
		public byte[] GetFromMarker (uint cut_start, uint cut_end)
		{
			long start = 0;
			long length = current_position - (current_marker + cut_start) - cut_end;
			byte[] tmp;
			
			if (current_marker < 0)
				return null;
			
			if (left_overs != null) {
				tmp = new byte [length + left_overs.Length];
				Array.Copy (left_overs, cut_start, tmp, 0, left_overs.Length - cut_start);
				start = left_overs.Length;
			} else {
				tmp = new byte [length];
			}
			
			//Array.Copy (buffer, current_marker, tmp, start, (current_position - current_marker) - cut_end);
			Array.Copy (buffer, current_marker + cut_start, tmp, start, length);
			
			return tmp;
		}
		
		public void ResetMarker ()
		{
			current_marker = -1;
			left_overs = null;
		}
		
		public int Current {
			get {
				if (buffer == null || current_position >= bytes_in_buffer)
					return -1;
				
				return buffer [current_position]; 
			}
		}
		
		public long MarkerPosition {
			get { return current_marker; }
		}
		
		public bool ActiveMarker {
			get { return (current_marker < 0 ? false : true); }
		}
		
		public long Position {
			get { return stream.Position + current_position; }
		}
		
		public long InternalPosition {
			get { return current_position; }
		}
		
		public long Length {
			get { return file_length; }
		}
		
		public bool EOF {
			get { 
				if (buffer == null && stream == null)
					return true;
				else if (file_length == stream.Position) 
					return (current_position >= bytes_in_buffer ? true : false);
			
				return false;
			}
		}
	}
}
