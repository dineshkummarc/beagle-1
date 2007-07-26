//
// ThunderbirdQueryable.cs: The backend starting point
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
using System.Xml;
using System.Text;
using System.Threading;
using System.Collections;
using Beagle.Util;
using Mono.Unix.Native;
using GMime;

[assembly: Beagle.Daemon.IQueryableTypes (typeof (Beagle.Daemon.ThunderbirdQueryable.ThunderbirdQueryable))]

namespace Beagle.Daemon.ThunderbirdQueryable {
	
	[QueryableFlavor (Name = "Thunderbird", Domain = QueryDomain.Local, RequireInotify = false)]
	public class ThunderbirdQueryable : LuceneQueryable {
		private ThunderbirdIndexer indexer = null;
		private string overriden_toindex = null;
		
		public ThunderbirdQueryable () : base ("ThunderbirdIndex")
		{
			// In case we use another directory
			overriden_toindex = Environment.GetEnvironmentVariable ("TOINDEX_DIR");
			
			GMime.Global.Init ();
		}
		
		public override void Start ()
		{
			base.Start ();
			ExceptionHandlingThread.Start (new ThreadStart (StartWorker));
		}
		
		private void StartWorker ()
		{
			Logger.Log.Debug ("Starting Thunderbird backend");
			Stopwatch watch = new Stopwatch ();
			watch.Start ();
			
			string toindex_dir = ToIndexDirectory;
			if (!Directory.Exists (toindex_dir)) {
				GLib.Timeout.Add (60000, new GLib.TimeoutHandler (IndexDataCheck));
				Logger.Log.Debug ("No Thunderbird data to index in {0}", toindex_dir);
				return;
			}
			
			indexer = new ThunderbirdIndexer (this);
			indexer.Start ();
			
			watch.Stop ();
			Logger.Log.Debug ("Thunderbird backend done in {0}s", watch.ElapsedTime);
		}
		
		private bool IndexDataCheck ()
		{
			if (!Directory.Exists (ToIndexDirectory))
				return true;
			
			StartWorker ();
			return false;
		}
		
		// This is the directory where all metafiles goes
		public string ToIndexDirectory {
			get {
				if (overriden_toindex != null)
					return overriden_toindex;
				else
					return Path.Combine (IndexDirectory, "ToIndex");
			}
		}
	}
	
	public class ThunderbirdIndexer {	
		private ThunderbirdQueryable queryable;
		private ThunderbirdIndexableGenerator indexable_generator;
		
		private const string TAG = "ThunderbirdIndexer";

		public ThunderbirdIndexer (ThunderbirdQueryable queryable)
		{
			this.queryable = queryable;
			this.indexable_generator = null;
		}
		
		public void Start ()
		{
			// Make sure we catch file system changes
			Inotify.Subscribe (queryable.ToIndexDirectory, OnInotifyEvent, Inotify.EventType.Create);
			
			// Start the indexable generator and begin adding things to the index
			LaunchIndexable ();
		}
		
		private void LaunchIndexable ()
		{
			// Cancel running task before adding a new one
			if (indexable_generator != null && queryable.ThisScheduler.ContainsByTag (TAG))
				queryable.ThisScheduler.GetByTag (TAG).Cancel ();
			
			// Add the new indexable generator
			indexable_generator = new ThunderbirdIndexableGenerator (this, queryable.ToIndexDirectory);
			
			Scheduler.Task task = queryable.NewAddTask (indexable_generator);
			task.Tag = TAG;
			queryable.ThisScheduler.Add (task);
		}

		public void RemoveFolder (string folderURL)
		{
			if (queryable.ThisScheduler.ContainsByTag (folderURL)) {
				Logger.Log.Debug ("Not adding task for already running {0}", folderURL);
				return;
			}
			
			Property prop = Property.NewUnsearched ("fixme:folderURL", folderURL);
			Scheduler.Task task = queryable.NewRemoveByPropertyTask (prop);
			task.Tag = folderURL;
			task.Priority = Scheduler.Priority.Immediate;
			queryable.ThisScheduler.Add (task);
		}

		private void OnInotifyEvent (
				Inotify.Watch watch,
				string path,
				string subitem,
				string srcpath,
				Inotify.EventType type)
		{
			// We need to have a filename
			if (subitem == null)
				return;
			
			// If the running indexable generator has more items, we don't have to care here.
			// Otherwise launch a new indexable generator.
			if (indexable_generator != null && !indexable_generator.Done)
				return;
			
			LaunchIndexable ();
		}
	}
	
	public class ThunderbirdIndexableGenerator : IIndexableGenerator {
		private ThunderbirdIndexer indexer;
		private Queue processed_files;
		private string path = null;
		private bool done = false;
		private int max_obj = -1, min_obj = -1, current_obj = 0, obj_count = 0;
		
		private const string ADD_MAILMESSAGE = "MailMessage";
		private const string ADD_RSS = "FeedItem";
		private const string REMOVE_MESSAGE = "DeleteHdr";
		private const string REMOVE_FOLDER = "DeleteFolder";
		
		public ThunderbirdIndexableGenerator (ThunderbirdIndexer indexer, string path)
		{
			Logger.Log.Debug ("New Thunderbird indexable generator launched for {0}", path);
			
			this.indexer = indexer;
			this.path = path;
			this.processed_files = new Queue ();
			ReadToIndexDirectory ();
		}
		
		private void ReadToIndexDirectory ()
		{
			// Reset values before we do anything
			max_obj = -1;
			min_obj = -1;
			current_obj = 0;
			obj_count = 0;
			
			foreach (string filename in DirectoryWalker.GetFiles (path)) {
				int cur_index;
				
				try {
					cur_index = Convert.ToInt32 (Path.GetFileName (filename));
				} catch {
					continue;
				}
				
				if (max_obj == -1 || min_obj == -1)
					max_obj = min_obj = cur_index;
				else if (cur_index < min_obj)
					min_obj = cur_index;
				else if (cur_index > max_obj)
					max_obj = cur_index;
				else
					continue; // This should _never_ happen
				
				obj_count++;
			}
			
			current_obj = min_obj;
		}
		
		private XmlDocument OpenCurrent ()
		{
			while (current_obj <= max_obj) {
				string filename = Path.Combine (path, Convert.ToString (current_obj));
				
				// Try to open file
				try {
					current_obj++;
					processed_files.Enqueue (filename);
					
					// We need to use the StreamReader to get full UTF-8 support
					XmlDocument document = new XmlDocument ();
					StreamReader reader = new StreamReader (filename);
					document.Load (reader);
					
					return document;
				} catch (Exception e) {
					Logger.Log.Debug (e, "Failed to parse file {0}", filename);
				}
			}
			current_obj++;
			
			return null;
		}
		
		private static string GetText (XmlDocument doc, string child)
		{
			if (doc == null || doc.DocumentElement == null || String.IsNullOrEmpty (child))
				return string.Empty;
			
			try {
				return doc.DocumentElement [child].InnerText;
			} catch {
			}
			
			return string.Empty;
		}
		
		private static bool ToBool (string str)
		{
			string lower = str.ToLower ();
			
			if (lower == "false")
				return false;
			else if (lower == "true")
				return true;
			
			return Convert.ToBoolean (str);
		}
		
		private GMime.Message GetGMimeMessage (string file, int offset, int size)
		{
			GMime.Message msg = null;
			
			if (!File.Exists (file))
				return null;
				
			StreamFs stream = null;
			Parser parser = null;
			try {
				int fd = Syscall.open (file, OpenFlags.O_RDONLY);
				stream = new StreamFs (fd, offset, offset + size);
				parser = new Parser (stream);
				msg = parser.ConstructMessage ();
			} catch (Exception e) {
			} finally {
				if (stream != null)
					stream.Dispose ();
				if (parser != null)
					parser.Dispose ();
			}
			
			return msg;
		}
		
		private GMime.Message GetStubMessage (XmlDocument document)
		{
			GMime.Message message = new GMime.Message (true);
			
			message.Subject = GetText (document, "Subject");
			message.Sender = GetText (document, "Author");
			message.MessageId = GetText (document, "MessageId");
			message.SetDate (DateTimeUtil.UnixToDateTimeUtc (Convert.ToInt64 (GetText (document, "Date"))), 0);
			message.AddRecipientsFromString ("To", GetText (document, "Recipients"));
			
			return message;
		}
		
		private Indexable ToAddMailMessageIndexable (XmlDocument document)
		{
			GMime.Message message = null;

			// Check if the entire message is available
			if (ToBool (GetText (document, "HasOffline"))) {
				// We must make sure we don't get an exception here since we can fallback to
				// other information
				try {
					int offset = Convert.ToInt32 (GetText (document, "MessageOffset")),
						size = Convert.ToInt32 (GetText (document, "OfflineSize"));
					message = GetGMimeMessage (GetText (document, "FolderFile"), offset, size);
				} catch (Exception e) {
					Logger.Log.Debug (e, "Failed to parse GMime message");
				}
			}

			if (message == null)
				message = GetStubMessage (document);
			
			Indexable indexable = new Indexable (new Uri (GetText (document, "Uri")));
			indexable.HitType = "MailMessage";
			indexable.MimeType = "message/rfc822";
			indexable.Timestamp = DateTimeUtil.UnixToDateTimeUtc (Convert.ToInt64 (GetText (document, "Date")));
			indexable.CacheContent = true;
			indexable.Crawled = true;
			indexable.SetBinaryStream (message.Stream);
			
			indexable.AddProperty (Property.NewUnsearched ("fixme:client", "Thunderbird"));
			indexable.AddProperty (Property.NewUnsearched ("fixme:folder", GetText (document, "Folder")));
			indexable.AddProperty (Property.NewUnsearched ("fixme:folderURL", GetText (document, "FolderURL")));
			
			message.Dispose ();
			
			return indexable;
		}
		
		private static StringReader GetRssBody (string file, int position, int len)
		{
			// Check if file exists to begin with
			if (!File.Exists (file))
				return null;
		
			FileStream stream = new FileStream (file, FileMode.Open);
			stream.Position = position;
			
			// We want to skip http headers since we are not interested in those. The normal scenario is
			// that a content begins once two newlines have been found. This is a bit different in 
			// Thunderbird though. Thunderbird specific headers are added first, then two newlines 
			// followed by http headers and then another three newlines followed by the content. So we
			// want to find the three newlines and read from there.
			byte[] buffer = null;
			int c, header_length = 0, newlines = 0;
			try {
				do {
					c = stream.ReadByte ();
					newlines = (c == 10 ? ++newlines : 0);
					header_length++;
				} while (c != -1 && newlines != 3);
				
				// We now know what to read
				buffer = new byte [len - header_length];
				stream.Read (buffer, 0, buffer.Length);
			} catch {
			} finally {
				stream.Close ();
			}
			
			return new StringReader (Encoding.ASCII.GetString (buffer));
		}
		
		private string ExtractUrl (string url)
		{
			if (url == null)
				return string.Empty;
			
			return url.Substring (0, url.IndexOf ('@'));
		}
		
		private Indexable ToAddRssIndexable (XmlDocument document)
		{
			StringReader reader = null;
			
			if (ToBool (GetText (document, "HasOffline"))) {
				try {
					// RSS does not use OfflineSize but MessageSize instead (for some reason...)
					int offset = Convert.ToInt32 (GetText (document, "MessageOffset")),
						size = Convert.ToInt32 (GetText (document, "MessageSize"));
					reader = GetRssBody (GetText (document, "FolderFile"), offset, size);
				} catch (Exception e) {
					Logger.Log.Debug (e, "Failed to parse RSS body");
				}
			}
			
			Uri uri = new Uri (String.Format ("{0}/?id={1}",
				GetText (document, "FolderURL"), GetText (document, "MessageKey")));
			
			// FeedURL är inte tillgänglig första nedladdningen. Generera en annan URI!
			//Uri uri = new Uri (String.Format ("{0}/id={1}", 
			//	GetText (document, "FeedURL"), GetText (document, "MessageKey")));
			Indexable indexable = new Indexable (uri);
			indexable.HitType = "FeedItem";
			indexable.MimeType = "text/html";
			indexable.Timestamp = DateTimeUtil.UnixToDateTimeUtc (Convert.ToInt64 (GetText (document, "Date")));
			indexable.CacheContent = true;
			indexable.Crawled = true;
			
			indexable.AddProperty (Property.NewUnsearched ("fixme:client", "Thunderbird"));
			indexable.AddProperty (Property.NewUnsearched ("fixme:folder", GetText (document, "Folder")));
			indexable.AddProperty (Property.NewUnsearched ("fixme:folderURL", GetText (document, "FolderURL").Substring (8)));
			indexable.AddProperty (Property.NewUnsearched ("fixme:uri", GetText (document, "Uri")));
			
			indexable.AddProperty (Property.NewKeyword ("dc:identifier", ExtractUrl (GetText (document, "MessageId"))));
			indexable.AddProperty (Property.NewKeyword ("dc:source", GetText (document, "FeedURL")));
			indexable.AddProperty (Property.New ("dc:publisher", GetText (document, "Author")));
			
			// The title will be added by the filter. In case we add it twice we will just get
			// an empty tile in the search tool (a bug maybe?).
			if (reader != null) 
				indexable.SetTextReader (reader);
			else
				indexable.AddProperty (Property.New ("dc:title", GetText (document, "Subject")));
			
			return indexable;
		}
		
		private Indexable ToRemoveMessageIndexable (XmlDocument document)
		{
			Uri uri = new Uri (GetText (document, "Uri"));
			Indexable indexable = new Indexable (IndexableType.Remove, uri);
			
			return indexable;
		}
		
		public Indexable GetNextIndexable ()
		{
			XmlDocument document = OpenCurrent ();
			if (document == null || document.DocumentElement == null)
				return null;
			
			// Compare our document element type to expected ones
			string name = document.DocumentElement.Name;
			if (name.Equals (ADD_MAILMESSAGE))
				return ToAddMailMessageIndexable (document);
			else if (name.Equals (ADD_RSS))
				return ToAddRssIndexable (document);
			else if (name.Equals (REMOVE_MESSAGE))
				return ToRemoveMessageIndexable (document);
			else if (name.Equals (REMOVE_FOLDER))
				indexer.RemoveFolder (GetText (document, "FolderURL"));

			return null;
		}
		
		public bool HasNextIndexable ()
		{
			if (obj_count == 0 || current_obj > max_obj) {
				// We need to flush old files here. Otherwise we might end up in an eternal loop.
				PostFlushHook ();

				// Read directory information again since content could have been added since we
				// read last time
				ReadToIndexDirectory ();
				
				// If we are still zero here, then we're done
				if (obj_count == 0) {
					done = true;
					return false;
				} else
					return true;
			}
			
			if (current_obj <= max_obj) 
				return true;
				
			done = true;
			return false;
		}
		
		public void PostFlushHook ()
		{
			// Remove the files we just processed
			while (processed_files.Count > 0) {
				string filename = processed_files.Dequeue () as string;
				
				try {
					File.Delete (filename);
				} catch (Exception e) {
					Logger.Log.Warn (e, "Failed to remove metafile: {0}", filename);
				}
			}
		}
		
		public string StatusName {
			get {
				return String.Format ("Thunderbird object {0} of {1}", current_obj, obj_count);
			}
		}
		
		public bool Done {
			get {
				return done;
			}
		}
	}
}

