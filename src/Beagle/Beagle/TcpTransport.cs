//
// TcpTransport.cs
//
// Copyright (C) 2005 Novell, Inc.
// Copyright (C) 2008 Lukas Lipka <lukaslipka@gmail.com>
//

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml.Serialization;
using System.Threading;

using Beagle.Util;

namespace Beagle {

	public class TcpTransport : Transport {
		
		private TcpClient client = null;
		private const int port = 2517;

		private byte[] network_data = new byte [4096];

		public TcpTransport () : base (true)
		{
		}

		public override void Close ()
		{
			bool previously_closed = this.IsClosed;

			// Important to set this before we close the
			// UnixClient, since that will trigger the
			// ReadCallback() method, reading 0 bytes off the
			// wire, and we check this.closed in there.
			this.IsClosed = true;

			if (client != null) {
				client.Close ();
				client = null;
			}

			if (!previously_closed)
				InvokeClosedEvent ();
		}

		protected override void SendRequest (RequestMessage request)
		{
			client = new TcpClient (new IPEndPoint (IPAddress.Loopback, port));
			NetworkStream stream = client.GetStream ();
			
			base.SendRequest (request, stream);
		}
		
		// This function will be called from its own thread
		protected override void ReadCallback (IAsyncResult ar)
		{
			if (this.IsClosed)
				return;

			try {
				NetworkStream stream = this.client.GetStream ();
				int bytes_read = 0;

				try { 
					bytes_read = stream.EndRead (ar);
				} catch (SocketException) {
					Logger.Log.Debug ("Caught SocketException in ReadCallback");
				} catch (IOException) {
					Logger.Log.Debug ("Caught IOException in ReadCallback");
				}

				// Connection hung up, we're through
				if (bytes_read == 0) {
					this.Close ();
					return;
				}

				int end_index = -1;
				int prev_index = 0;

				do {
					// 0xff signifies end of message
					end_index = ArrayFu.IndexOfByte (this.network_data, (byte) 0xff, prev_index);

					int bytes_count = (end_index == -1 ? bytes_read : end_index) - prev_index;
					this.BufferStream.Write (this.network_data, prev_index, bytes_count);

					if (end_index != -1) {
						MemoryStream deserialize_stream = this.BufferStream;
						this.BufferStream = new MemoryStream ();
						
						deserialize_stream.Seek (0, SeekOrigin.Begin);
						HandleResponse (deserialize_stream);
						
						// Move past the end-of-message marker
						prev_index = end_index + 1;
					}
				} while (end_index != -1);

				// Check to see if we're still connected, and keep
				// looking for new data if so.
				if (!this.IsClosed)
					BeginRead ();
			} catch (Exception e) {
				Logger.Log.Error (e, "Got an exception while trying to read data:");
			}
		}

		protected override void BeginRead ()
		{
			NetworkStream stream = this.client.GetStream ();

			Array.Clear (this.network_data, 0, this.network_data.Length);
			stream.BeginRead (this.network_data, 0, this.network_data.Length, new AsyncCallback (ReadCallback), null);
		}

		public override void SendAsyncBlocking (RequestMessage request)
		{
			Exception ex = null;

			try {
				SendRequest (request);
			} catch (IOException e) {
				ex = e;
			} catch (SocketException e) {
				ex = e;
			}

			if (ex != null) {
				ResponseMessage resp = new ErrorResponse (ex);				
				InvokeAsyncResponseEvent (resp);
				return;
			}
			
			NetworkStream stream = this.client.GetStream ();
			MemoryStream deserialize_stream = new MemoryStream ();

			// This buffer is annoyingly small on purpose, to avoid
			// having to deal with the case of multiple messages
			// in a single block.
			byte [] buffer = new byte [32];

			while (!this.IsClosed) {

				Array.Clear (buffer, 0, buffer.Length);

				int bytes_read = stream.Read (buffer, 0, buffer.Length);

				if (bytes_read == 0)
					break;

				int end_index;
				end_index = ArrayFu.IndexOfByte (buffer, (byte) 0xff);

				if (end_index == -1) {
					deserialize_stream.Write (buffer, 0, bytes_read);
				} else {
					deserialize_stream.Write (buffer, 0, end_index);
					deserialize_stream.Seek (0, SeekOrigin.Begin);

					ResponseMessage resp = null;

					try {
						ResponseWrapper wrapper;
						wrapper = (ResponseWrapper) resp_serializer.Deserialize (deserialize_stream);
						
						resp = wrapper.Message;
					} catch (Exception e) {
						resp = new ErrorResponse (e);
					}

					InvokeAsyncResponseEvent (resp);

					deserialize_stream.Close ();
					deserialize_stream = new MemoryStream ();
					if (bytes_read - end_index - 1 > 0)
						deserialize_stream.Write (buffer, end_index + 1, bytes_read - end_index - 1);
				}
			}
		}


		public override ResponseMessage Send (RequestMessage request)
		{
			if (request.Keepalive)
				throw new Exception ("A blocking connection on a keepalive request is not allowed");

			Exception throw_me = null;

			try {
				SendRequest (request);
			} catch (IOException e) {
				throw_me = e;
			} catch (SocketException e) {
				throw_me = e;
			}

			if (throw_me != null)
				throw new ResponseMessageException (throw_me);

			NetworkStream stream = this.client.GetStream ();
			int bytes_read, end_index = -1;

			do {
				bytes_read = stream.Read (this.network_data, 0, 4096);

				//Logger.Log.Debug ("Read {0} bytes", bytes_read);

				if (bytes_read > 0) {
					// 0xff signifies end of message
					end_index = ArrayFu.IndexOfByte (this.network_data, (byte) 0xff);
					
					this.BufferStream.Write (this.network_data, 0,
								  end_index == -1 ? bytes_read : end_index);
				}
			} while (bytes_read > 0 && end_index == -1);

			// It's possible that the server side shut down the
			// connection before we had a chance to read any data.
			// If this is the case, throw a rather descriptive
			// exception.
			if (this.BufferStream.Length == 0) {
				this.BufferStream.Close ();
				throw new ResponseMessageException ("Socket was closed before any data could be read");
			}

			this.BufferStream.Seek (0, SeekOrigin.Begin);
			
			ResponseMessage resp = null;

			try {
				ResponseWrapper wrapper = (ResponseWrapper)resp_serializer.Deserialize (this.BufferStream);
				resp = wrapper.Message;
			} catch (Exception e) {
				this.BufferStream.Seek (0, SeekOrigin.Begin);
				StreamReader r = new StreamReader (this.BufferStream);
				throw_me = new ResponseMessageException (e, "Exception while deserializing response", String.Format ("Message contents: '{0}'", r.ReadToEnd ()));
				this.BufferStream.Seek (0, SeekOrigin.Begin);
			}

			this.BufferStream.Close ();

			if (throw_me != null)
				throw throw_me;
			
			return resp;
		}
	}
}
