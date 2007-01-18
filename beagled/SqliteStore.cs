//
// SqliteStore.cs
//
// Copyright (C) 2006 Novell, Inc.
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

using Mono.Data.SqliteClient;

using Beagle.Util;

namespace Beagle.Daemon {

	public abstract class SqliteStore {

		protected SqliteConnection connection;
		protected BitArray path_flags;
		protected int transaction_count = 0;
		protected string directory;

		protected enum TransactionState {
			None,
			Requested,
			Started
		}
		protected TransactionState transaction_state;

		public SqliteStore (string directory, string index_fingerprint)
		{
			bool create_new_db = false;
			path_flags = new BitArray (65536);
			this.directory = directory;

			if (! File.Exists (GetDbPath (directory))) {
				create_new_db = true;
			} else {
				
				// Funky logic here to deal with sqlite versions.
				//
				// When sqlite 3 tries to open an sqlite 2 database,
				// it will throw an SqliteException with SqliteError
				// NOTADB when trying to execute a command.
				//
				// When sqlite 2 tries to open an sqlite 3 database,
				// it will throw an ApplicationException when it
				// tries to open the database.

				try {
					connection = Open (directory);
				} catch (ApplicationException) {
					Logger.Log.Warn ("Likely sqlite database version mismatch trying to open {0}.  Purging.", GetDbPath (directory));
					create_new_db = true;
				}

				if (! create_new_db) {
					SqliteCommand command;
					SqliteDataReader reader = null;
					int stored_version = 0;
					string stored_fingerprint = null;


					command = new SqliteCommand ();
					command.Connection = connection;
					command.CommandText =
						"SELECT version, fingerprint FROM db_info";
					try {
						reader = SqliteUtils.ExecuteReaderOrWait (command);
					} catch (Exception) {
						Logger.Log.Warn ("Likely sqlite database version mismatch trying to read from {0}.  Purging.", GetDbPath (directory));
						create_new_db = true;
					}
					if (reader != null && ! create_new_db) {
						if (SqliteUtils.ReadOrWait (reader)) {
							stored_version = reader.GetInt32 (0);
							stored_fingerprint = reader.GetString (1);
						}
						reader.Close ();
					}
					command.Dispose ();

					if (!CheckVersion (stored_version)
					    || (index_fingerprint != null && index_fingerprint != stored_fingerprint))
						create_new_db = true;
				}
			}

			if (create_new_db) {
				if (connection != null)
					connection.Dispose ();
				File.Delete (GetDbPath (directory));
				connection = Open (directory);

				CreateTables (index_fingerprint);
			
			} else {
			
				LoadRecords (directory);
			}
		}

		public SqliteStore (SqliteStore old) 
		{
			try {
				connection = Open (old.Directory);
			} 
			catch (Exception ex) {
				Logger.Log.Debug ("Can't open new connection to database!");
				Logger.Log.Debug (ex);
			}
		}
			
		
		protected abstract bool CheckVersion (int version);
		protected abstract void CreateTables (string index_fingerprint);
		protected abstract void LoadRecords (string directory);
	

		public string Directory {
			get { return directory ;}
		}	
		///////////////////////////////////////////////////////////////////

		protected abstract string GetDbPath (string directory);

		protected SqliteConnection Open (string directory)
		{
			SqliteConnection c;			
			c = new SqliteConnection ();
			c.ConnectionString = "version=" + ExternalStringsHack.SqliteVersion
				+ ",encoding=UTF-8,URI=file:" + GetDbPath (directory);
			c.Open ();
			return c;
		}

		public void MaybeStartTransaction ()
		{
			if (transaction_state == TransactionState.Requested) {
				Logger.Log.Debug ("BEGIN");
				transaction_state = TransactionState.Started;
				SqliteUtils.DoNonQuery (connection, "BEGIN");
			}
		}

		public void BeginTransaction ()
		{
			if (transaction_state == TransactionState.None) {
				Logger.Log.Debug ("Requested");
				transaction_state = TransactionState.Requested;
			}
		}

		public void CommitTransaction ()
		{
			if (transaction_state == TransactionState.Started) {
				lock (connection)
					Logger.Log.Debug ("COMMIT");
					SqliteUtils.DoNonQuery (connection, "COMMIT");
			}
			transaction_state = TransactionState.None;
		}

		public void Flush ()
		{
			lock (connection) {
				if (transaction_count > 0) {
					Logger.Log.Debug ("Flushing requested -- committing {0} sqlite transaction",
							transaction_count);
					try {
						SqliteUtils.DoNonQuery (connection, "COMMIT");
						transaction_count = 0;
						transaction_state = TransactionState.None;
					} catch (Exception ex) {
						Logger.Log.Debug (ex);
					}
				}
			}
		}

	}
}
