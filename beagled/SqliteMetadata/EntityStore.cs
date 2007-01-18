
using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Text;

using Mono.Data.SqliteClient;

using Beagle.Util;

namespace Beagle.Daemon {

public class NotVerifiedException : ApplicationException {
	public NotVerifiedException(string message) : base(message) { }
	public NotVerifiedException(string message, Exception cause) : base(message, cause) { }
}

public class IdNegativeException : ApplicationException {
	public IdNegativeException(string message) : base(message) { }
	public IdNegativeException(string message, Exception cause) : base(message, cause) { }
}

public class IdNullException : ApplicationException {
	public IdNullException(string message) : base(message) { }
	public IdNullException(string message, Exception cause) : base(message, cause) { }
}


public class EntityStore : SqliteStore {

		// Version history:
		// 1: Original version
		const int VERSION = 1;
		
		
		public EntityStore (string directory) : base (directory, null) {
		
		}

		public EntityStore (EntityStore old) : base ((SqliteStore) old) {
		
		}

		protected override bool CheckVersion (int version) {
			return VERSION == version;
		}

		protected override void CreateTables (string index_fingerprint) {
			Logger.Log.Debug ("SqlMetadata Creating Tables...");
			SqliteUtils.DoNonQuery (connection,
						"CREATE TABLE db_info (             " +
						"  version       INTEGER NOT NULL,  " +
						"  fingerprint   STRING NOT NULL    " +
						")");

			SqliteUtils.DoNonQuery (connection,
						"INSERT INTO db_info (version, fingerprint) VALUES ({0}, '{1}')",
						VERSION, index_fingerprint);

			SqliteUtils.DoNonQuery (connection,
						"CREATE TABLE first_class (           " +
						"  id			INTEGER PRIMARY KEY AUTOINCREMENT," +
						"  class_name   STRING UNIQUE NOT NULL  " +
						")");
			
			SqliteUtils.DoNonQuery (connection,
						"CREATE TABLE second_class (           " +
						"  id			INTEGER PRIMARY KEY AUTOINCREMENT," +
						"  class_name     STRING UNIQUE NOT NULL  " +
						")");

			SqliteUtils.DoNonQuery (connection,
						"CREATE TABLE objects (		" +
						" id		INTEGER PRIMARY KEY AUTOINCREMENT," +
						" value_string	STRING )");
			
			SqliteUtils.DoNonQuery (connection,
						"CREATE UNIQUE INDEX object_idx on objects (" +
						"  value_string )");
			
			SqliteUtils.DoNonQuery (connection,
						"CREATE TABLE attributes (           " +
						"  id		  INTEGER PRIMARY KEY AUTOINCREMENT," +
						"  name       STRING NOT NULL,        " +
						"  for_class  INTEGER REFERENCES first_class,  " +
						"  value_type INTEGER REFERENCES second_class, " +
						"  refers_to  INTEGER REFERENCES first_class,  " +
						"  flags	  INTEGER NOT NULL            " +
						")");
			
			SqliteUtils.DoNonQuery (connection,
						"CREATE UNIQUE INDEX attribute_idx on attributes (" +
						"  name,            " +
						"  for_class        " +
						")");
			
			SqliteUtils.DoNonQuery (connection,
						"CREATE TABLE instances (           " +
						"  id		  INTEGER PRIMARY KEY AUTOINCREMENT," +
						"  uri            STRING NOT NULL, " +
						"  class_id       INTEGER REFERENCES first_class, " +
						"  indexed        STRING NOT NULL,       " +
						"  flags		INTEGER NOT NULL DEFAULT 0 " +
						")");

			SqliteUtils.DoNonQuery (connection,
						"CREATE UNIQUE INDEX instance_idx on instances (" +
						"  uri            " +
						")");

			
			// Trigger for the brute force removal of instances:			
			string on_delete = "CREATE TRIGGER on_delete BEFORE DELETE ON instances " +
						"  FOR EACH ROW " +
						"  BEGIN " +
						"  		DELETE FROM statements " +
						"  		WHERE instance_id = OLD.id ;" +
						"		UPDATE statements " +
						"		SET refers_to = null " +
						"		WHERE refers_to = OLD.id ;" +
						"  END ";
			Logger.Log.Debug (on_delete);
			SqliteUtils.DoNonQuery (connection, on_delete);
						
			// Trigger for the virtualization (soft removal) of instances:
			string on_virtual =	String.Format (
				"CREATE TRIGGER on_virtual BEFORE UPDATE ON instances " +
					"FOR EACH ROW " +
					"WHEN (NEW.flags & {0}) " +
					"AND OLD.id IN " +
						"(SELECT refers_to FROM statements) " +
					"BEGIN " +
						"DELETE FROM statements " +
						"WHERE instance_id = OLD.id " +
						"AND attribute_id IN " +
							"(SELECT id FROM attributes " +
							" WHERE NOT (flags & {1}) ) " +
							"; " +
					"END ",
				(int) InstanceFlags.is_virtual,
				(int) AttributeFlags.for_virtual + (int) AttributeFlags.is_identifier);
			Logger.Log.Debug (on_virtual);
			SqliteUtils.DoNonQuery (connection, on_virtual);
						
			// Trigger for the soft removal of instances (no references):
			
			string on_remove = String.Format (
						"CREATE TRIGGER on_remove BEFORE UPDATE ON instances " +
							"FOR EACH ROW " +
							"WHEN (NEW.flags & {0}) " +
							"AND OLD.id NOT IN " +
								"(SELECT refers_to FROM statements) " +
							"BEGIN " +
								"DELETE FROM instances " +
								"WHERE id = OLD.id ; " +
							"END ",
						(int) InstanceFlags.is_virtual
					);
			Logger.Log.Debug (on_remove);

			SqliteUtils.DoNonQuery (connection, on_remove);
					
			
			
			SqliteUtils.DoNonQuery (connection,
						"CREATE TABLE statements (           " +
						"  id		  INTEGER PRIMARY KEY AUTOINCREMENT," +
						"  instance_id    INTEGER REFERENCES instances, " +
						"  attribute_id   INTEGER REFERENCES attributes," +
						"  object_id      INTEGER REFERENCES objects,    " +
						"  refers_to      INTEGER REFERENCES instances  " +  
						")");

			SqliteUtils.DoNonQuery (connection,
						"CREATE UNIQUE INDEX statement_indx on statements (" +
						"  instance_id,         " +
						"  attribute_id,         " +
						"  object_id         " +
						")");


			SqliteUtils.DoNonQuery (connection,
						"CREATE INDEX statement_inst on statements (" +
						"  instance_id         " +
						")");

			// We need this mainly for refers_to creation.
			SqliteUtils.DoNonQuery (connection,
						"CREATE INDEX statement_obj on statements (" +
						"  object_id         " +
						")");
			
		
			
			// Create references to instances described by new statements:
			string on_new_statement =	String.Format (
				"CREATE TRIGGER on_new_statement BEFORE INSERT ON statements " +
					"FOR EACH ROW " +
					"WHEN NEW.attribute_id IN " +
						"(SELECT id FROM attributes where (flags & {0})) " +
					"BEGIN " +
						"UPDATE statements " +
						"SET refers_to = NEW.instance_id " +
						"WHERE " +
						"id IN" +
				 			"( SELECT " +
								"s.id " +
							"FROM " +
								"statements as s, " +
								"attributes as a, " +
								"instances as i " +
							"WHERE " +
								"s.object_id = NEW.object_id AND " + 
								"a.id = s.attribute_id AND " +
								"i.id = NEW.instance_id AND " +
								"a.refers_to = i.class_id " +
							") " +
							"; " +
					"END ",
				(int) AttributeFlags.is_identifier);
			Logger.Log.Debug (on_new_statement);
			SqliteUtils.DoNonQuery (connection, on_new_statement);
			
			LoadRecords ("");	
			//FIXME: This should be done in a clean way. We don't seem to
			//need the directory here - why does FileAttribute... need it?
			
		}

		
		
		protected override void LoadRecords (string directory)
	       	{
			lock (connection) {
	
			   if (! HasModel ()) {
				DateTime dt1 = DateTime.Now;
				BeginTransaction ();
				MaybeStartTransaction ();
				SqlMetaModel.CreateModel (this);
				transaction_count++;
				Flush();
				DateTime dt2 = DateTime.Now;

				Logger.Log.Debug ("Loaded MetaModell from {0} in {1:0.000}s", 
					 GetDbPath (directory), (dt2 - dt1).TotalSeconds) ;
			}
			}
			Logger.Log.Debug ("Load Records finished!");
		}

		///////////////////////////////////////////////////////////////////

		protected override string GetDbPath (string directory)
		{
			return Path.Combine (directory, "MetadataStore.db");
		}


		///////////////////////////////////////////////////////////////////
		
		
	

		protected void Check (string tablename, string whereclause)
		{
			string query = String.Format (
				"SELECT count(*) " +
				"FROM {0} WHERE {1}",
				tablename, whereclause);
			Check (query);
		}
		
		protected void Check (string query) 
		{
			SqliteCommand command;
			SqliteDataReader reader;
			command = new SqliteCommand ();
			command.Connection = connection;
			command.CommandText = query; 
			
			Logger.Log.Debug (query);
			reader = SqliteUtils.ExecuteReaderOrWait (command);
				
			SqliteUtils.ReadOrWait (reader);
			if (reader.GetInt32 (0) == 0) {
				reader.Close ();
				command.Dispose ();
				throw new ApplicationException ("Could not find data with: \n " +
						query);
			}
				
			reader.Close ();
			command.Dispose ();
		}	


		public uint GetId (string tablename, string whereclause) {
			uint id = 0;
			SqliteCommand command;
			SqliteDataReader reader;
			try {
				command = new SqliteCommand ();
				command.Connection = connection;
				command.CommandText = String.Format (
					"SELECT id " +
					"FROM {0} WHERE {1}",
					tablename, whereclause);
				// Logger.Log.Debug (command.CommandText);
				
				reader = SqliteUtils.ExecuteReaderOrWait (command);

				if (!SqliteUtils.ReadOrWait (reader)) 
					throw new Exception ("Could not find id where " + whereclause +
					" in " + tablename + ".");

				id = (uint) reader.GetInt32 (0);

				if (SqliteUtils.ReadOrWait (reader))
					throw new Exception ("Found more than one id where " + 
							whereclause + " in " + tablename +
							"- this should not happen.");
				reader.Close ();
				command.Dispose ();
			} catch (Exception ex) {
				Logger.Log.Debug (ex);
			}
			return id;	
		}


		// This function only attempts to insert a row into the db but does not check 
		// for the id of a already existing row. 
		// You can use this for INSERT INTO ... SELECT ... FROM ... WHERE
		// If you want to INSERT INTO ... VALUES (...) better use the function
		// InsertOrIgnore if you want the id of already existing rows if the
		// row can't be inserted due to lack of uniqueness.
		public uint Insert (string tablename, string fields, string what){
			uint id = 0;
		//FIXME: This might only work with Sqlite 3.x because of the OR 
		//IGNORE statement below.
		
			string insert_string = String.Format (
						"INSERT OR IGNORE INTO {0} ({1}) {2}",
						tablename, 
						fields, 
						what);
						
			Logger.Log.Debug (insert_string);
			
			
			// We don't catch anything here.
			// Exceptions should be handled by the calling function.
			// We do expect NoRowsException and MultipleRowsException
			BeginTransaction ();		
			MaybeStartTransaction ();		
			id = (uint) SqliteUtils.DoInsertQuery (connection, insert_string);
			transaction_count++;
			
			return id;
		
		}

		public uint Insert (string tablename, string fields, string format, params object [] args)
		{
			return Insert (tablename, fields, String.Format (format, args));		
		}
		
		// This function tryes to create a new entity. If the entity
		// already existed its id will be returned.
		public uint InsertOrIgnore (string tablename, string[] fields, string[] values) {
			int id = 0;
		//FIXME: This might only work with Sqlite 3.x because of the OR 
		//IGNORE statement below.
			
			if (fields.Length != values.Length)
				throw new System.ApplicationException (
					"Can only insert rows into database with" +
					"the same number of values and fields.");
			
			string insert_string = String.Format (
						"INSERT OR IGNORE INTO {0} ({1}) VALUES ('{2}')",
						tablename, 
						String.Join (", ", fields), 
						String.Join	("', '", values));
						
		//	Logger.Log.Debug (insert_string);
			try {	
				BeginTransaction ();		
				MaybeStartTransaction ();		
				id = SqliteUtils.DoInsertQuery (connection, insert_string);
				transaction_count++;
			} 
			catch (NoRowsException) {
				StringBuilder where = new StringBuilder ();
				
				for (int c = 0; c < fields.Length - 1; c++) 
					where.AppendFormat (" {0} = '{1}' AND",
						fields [c],
						values [c]);
				
				where.AppendFormat (" {0} = '{1}' ",
					fields [fields.Length - 1],
					values [fields.Length - 1]);
				
				id = (int) GetId (tablename, where.ToString ());
				
				// FIXME we have to query for the fields and values now
				// This is not easy - we might have to change the format from
				// two strings for fields and values to two ArrayLists or a Hash
				// so we can create a proper query.
				// id =
			}
			catch (MultipleRowsException multi) {
				throw new ApplicationException (
					String.Format("{0} caused MultipleRowsException", insert_string),
					multi);
			}
			catch (SystemException ex) {
				Logger.Log.Debug (ex);
				throw new Exception ("Could not Insert: \n" +
					"tablename:\t" + tablename + "\n" +
					"fields:\t" + String.Join (", ", fields) + "\n" +
					"what:\t" + String.Join (", ", values) + "\n", ex);
			}
			if (id == 0)
				throw new IdNullException ("");
			if (id < 0)
				throw new IdNegativeException ("");
			return (uint) id;
									
		}
		
		public void Delete (string tablename, string where)
		{
			string delete_string = String.Format (
				"DELETE FROM {0} WHERE {1}",
				tablename, where);
			Logger.Log.Debug (delete_string);
			
			// We don't catch anything here.
			// Exceptions should be handled by the calling function.
			// We do expect NoRowsException and MultipleRowsException
			BeginTransaction ();		
			MaybeStartTransaction ();		
			// DoInsertQuery should work for deletes as well. It just does
			// not return the correct row ids. So we ignore the return value.
			SqliteUtils.DoInsertQuery (connection, delete_string); 
			transaction_count++;
			return;
		}
			

		public void Update (string tablename, string[] fields, string[] values, string where)
		{
		
			StringBuilder update_string = new StringBuilder ("UPDATE OR IGNORE ");
			update_string.AppendFormat ("{0} SET ",tablename);
							
				for (int c = 0; c < fields.Length - 1; c++) 
					update_string.AppendFormat (" {0} = '{1}', ",
						fields [c],
						values [c]);
				
				update_string.AppendFormat (" {0} = '{1}' ",
					fields [fields.Length - 1],
					values [fields.Length - 1]);
			
			update_string.AppendFormat ("WHERE {0}", where);
		
			Logger.Log.Debug (update_string.ToString ());
			
			
			// We don't catch anything here.
			// Exceptions should be handled by the calling function.
			// We do expect NoRowsException and MultipleRowsException
			BeginTransaction ();		
			MaybeStartTransaction ();		
			// DoInsertQuery should work for updates as well. It just does
			// not return the correct row ids. So we ignore the return value.
			SqliteUtils.DoInsertQuery (connection, update_string.ToString ()); 
			transaction_count++;
		
			return;
		}
			


		public bool HasModel () 
		{
			bool res;
			SqliteDataReader reader;
			SqliteCommand command = new SqliteCommand ();
			command.Connection = connection;
			command.CommandText =
				"Select count(*) "+
				"FROM attributes";
			reader = SqliteUtils.ExecuteReaderOrWait (command);
	
			SqliteUtils.ReadOrWait (reader);
					
			// We might want to do some more detailed checks here.
			// Right now i assume that if we have the First Classes we
			// got everything.
			res = (reader.GetInt32 (0) > 0);

			reader.Close ();
			
			return res;
		}

		public uint NewFirstClass (string name) {
			string[] fields = {"class_name"};
			string[] values = {name};
			return InsertOrIgnore ("first_class", fields, values);
		}
		
		public uint NewSecondClass (string name) {
			string[] fields = {"class_name"};
			string[] values = {name};
			return InsertOrIgnore ("second_class", fields, values);
		}
		
		
		public void NewAttribute (string for_class, string name, string value_type, string refers_to, AttributeFlags flags)
		{
	  		Insert ("attributes", 
				"for_class, name, value_type, flags, refers_to",
				"SELECT for.id, '{1}', val.id, '{3}', ref.id " +
				"FROM   first_class AS for, " +
				"       second_class AS val " +
				"  LEFT OUTER JOIN " +
				"       first_class AS ref " +
				"  ON   ref.class_name = '{4}' " +
				"WHERE  for.class_name = '{0}' " +
				" AND   val.class_name = '{2}' ",
				for_class, name, value_type, (uint) flags, refers_to);

			
			// We don't catch the exceptions that might be thrown by Insert here.
			// NoRowExceptions might mean that the attribute already existed but
			// it can also mean that the select did not return anything.
			// Since attributes should only be created once both should throw 
			// exceptions. Multiple Rows Exceptions are symptoms of a bug as well.
			
		}
		
		public uint GetInstanceId (string uri) {
			string where = String.Format ("uri = '{0}'", 
					SqliteUtils.Sanitize (uri));
			return GetId ("instances", where );
		}

		public uint CreateInstance (string uri, string class_name) {
			uint id = 0;
			string fields = "uri, class_id, indexed";
			string values = String.Format ("SELECT '{0}', " + 
				"id, '{2}' FROM first_class WHERE class_name = '{1}'",
				SqliteUtils.Sanitize (uri), 
				class_name,  
				DateTime.Now.ToString ());
			try {
				id = Insert ("instances", fields, values);
			}
			catch (NoRowsException) {
				id = GetId ("instances", 
					String.Format ("uri = '{0}'", uri));
			}
			return id;				
		}

		public bool RemoveInstance (string uri) {
		// returns true if Instance has been removed and false if
		// instance has been virtualized due to references.
		
		// We use the triggers on instances to virtualize or delete.
			string[] fields = {"flags"};
			// this only works as long as we only have one flag:
			string[] values = {((int) InstanceFlags.is_virtual).ToString ()}; 
			try {
				Update ("instances",
					fields,
					values,
					String.Format ("uri = '{0}' ", uri)
					);
			} catch (NoRowsException) {
				// This means the trigger already removed the line
				// because there where no references to it.
				return true;
			}
			// Instance has been put to virtual due to references.
			return false;
		}

		public void ForceRemoveInstance (string uri) {
		// We use the triggers on instances to do the brute force removal			
			Delete ("instances",
				String.Format ("uri = '{0}' ", uri)
				);
		}

		public uint GetClassId (string of_class) {
			return GetId ("first_class", String.Format ("class_name = '{0}'", 
					of_class));
		}

		public uint GetObjectId (string val) {
			return GetId ("objects", String.Format ("value_string = '{0}'", 
					SqliteUtils.Sanitize (val)));
		}

		public uint CreateObject (string val) {
			string[] fields = {"value_string"};
			string[] values = { SqliteUtils.Sanitize (val)};
			return InsertOrIgnore ("objects", fields, values);
		}
		
		protected uint GetClassOfUri (string uri)
		{
			int res;
			SqliteCommand command;
			SqliteDataReader reader;
			lock (connection) {
				command = new SqliteCommand ();
				command.Connection = connection;
				command.CommandText = String.Format (
					"SELECT class_id " +
					"FROM instances WHERE uri='{0}'",
					SqliteUtils.Sanitize (uri));
				
				reader = SqliteUtils.ExecuteReaderOrWait (command);
				
				SqliteUtils.ReadOrWait (reader);
				res = reader.GetInt32 (0);
				if (res == 0)
					throw new ApplicationException ("Could not find class for " + uri);
				if (SqliteUtils.ReadOrWait (reader))
					throw new ApplicationException ("Found more than one class for " + 
							uri + " - this should not happen.");
				reader.Close ();
				command.Dispose ();
			}
			return (uint) res;
		}
		
		private uint AddStatement (uint instance_id, uint class_id, string attribute, uint object_id, string constraint)
		{
			// constraint can only contain constrains about the attribute starting with a.
			// Maybe this should be rewritten at some point to use bools for verified and non_verified 
			// or something alike.
			uint statement_id = 0;
			string fields = "instance_id, attribute_id, object_id, refers_to";
			string what = String.Format (
				"SELECT " +
					"{0}, " +
					"a.id, " +
					"{3}, " +   // value_id
					"reference " +
				"FROM " +
					"attributes AS a " +
				"LEFT OUTER JOIN " +
					"(SELECT " +
						"ref.instance_id as reference, " +
						"i.class_id as to_class " +
					"FROM " +
						"statements as ref, " +
						"instances as i, " +
						"attributes as ref_a " +
					"WHERE " +
						"ref_a.id = ref.attribute_id AND " +
						"i.id = ref.instance_id AND " +
						"ref.object_id = {3} AND " +
						"(ref_a.flags & {5}) " +
					") " +
				"ON " +
					"a.refers_to NOTNULL AND " +
					"to_class = a.refers_to " +
				"WHERE " +
					"a.name = '{2}' AND " +
					"a.for_class = {1} " +
					"{4}" ,
					instance_id,
					class_id,
					attribute,
					object_id,
					constraint,
					(int) AttributeFlags.is_identifier
				);
			try {
				//this one triggers the creation of references in old statements as well.
				statement_id = Insert ("statements", fields, what);
							
			} catch (MultipleRowsException) {
				// Did we write the statement successfully
				// If we did everything is fine. Creating multiple references with the trigger 
				// might have caused the exception to be thrown.
				// TODO: check if such exceptions would be thrown.
				// Otherwise we better throw an exception - we don't want multiple statements.
				if (statement_id == 0) {
					throw new MultipleRowsException (String.Format (
						"Multiple Rows inserting one statement" +
						"Trying to insert: {0}", what));
				}
				
			} catch (NoRowsException) {
				// Did we write the statement successfully
				if (statement_id != 0)
					return statement_id;	//TODO: Check if this can be caused by the trigger or not.
				// Otherwise this was probably happened because the same statement already was
				// inserted before and is unique. Or the attribute might not be verified.
				// So we check the select statement to see if the attribute is known.
				try { 
					Check ("attributes AS a",
						String.Format (
							"a.name = '{0}' AND " +
							"a.for_class = {1} " +
							"{2}",
							attribute, 
							class_id,
							constraint
						)
					);
					// looks like select worked but a constraint violation occured.
					Logger.Log.Debug ("Constraint violation occured when trying to insert: \n" +
						what);
						
				} catch (ApplicationException) {
					// This means the select did not work so the attribute
					// could not be verified. So we throw the proper exception:
					throw new NotVerifiedException (
						String.Format ("Attribute {0} could not be verified.", attribute));
				}
			
			}
			return statement_id;
		}
		
		public uint AddStatement (uint instance_id, uint class_id, string attribute, uint object_id)
		{
			uint res;
			try {
				res = AddStatement (instance_id, class_id, attribute, object_id, null);
				}
			catch (NotVerifiedException) {
				this.NonVerifiedAttribute (attribute, class_id);
				res = AddStatement (instance_id, class_id, attribute, object_id, null);
			}
			return res;
		}
		
		public uint AddVerifiedStatement (uint instance_id, uint class_id, string attribute, uint object_id)
		{
			return AddStatement (instance_id, class_id, attribute, object_id, String.Format (
					" AND NOT (a.flags & {0})", 
					(int) AttributeFlags.is_not_verified)
					);
		}
		
		
		
		private uint AddStatement (string instance, string attribute, uint object_id, string constraint) {

			// constraint can only contain constrains about the attribute starting with a.
			// because it is also used to determine if the attribute exists when the insert failes.
			// Maybe this should be rewritten at some point to use bools for verified and non_verified 
			// or something alike.

			uint statement_id = 0;
			string fields = "instance_id, attribute_id, object_id, refers_to";
			string what = String.Format (
				"SELECT " +
					"i.id, " +
					"a.id, " +
					"{2}, " +   // value_id
					"reference " +
				"FROM " +
					"attributes AS a, " +
					"instances AS i " +
				"LEFT OUTER JOIN " +
					"(SELECT " +
						"ref.instance_id as reference, " +
						"ref_i.class_id as to_class " +
					"FROM " +
						"statements as ref, " +
						"instances as ref_i, " +
						"attributes as ref_a " +
					"WHERE " +
						"ref_a.id = ref.attribute_id AND " +
						"ref_i.id = ref.instance_id AND " +
						"ref.object_id = {2} AND " +
						"(ref_a.flags & {4}) " +
					") " +
				"ON " +
					"a.refers_to NOTNULL AND " +
					"to_class = a.refers_to " +
				"WHERE " +
					"a.name = '{1}' AND " +
					"a.for_class = i.class_id AND " +
					"i.uri = '{0}' " +
					"{3}" ,
					instance,
					attribute,
					object_id,
					constraint,
					(int) AttributeFlags.is_identifier
				);
			try {
				//this one triggers the creation of references in old statements as well.
				statement_id = Insert ("statements", fields, what);
							
			} catch (MultipleRowsException) {
				// Did we write the statement successfully
				// If we did everything is fine. Creating multiple references with the trigger 
				// might have caused the exception to be thrown.
				// TODO: check if such exceptions would be thrown.
				// Otherwise we better throw an exception - we don't want multiple statements.
				if (statement_id == 0) {
					throw new MultipleRowsException (String.Format (
						"Multiple Rows inserting one statement" +
						"Trying to insert: {0}", what));
				}
				
			} catch (NoRowsException) {
				// Did we write the statement successfully
				if (statement_id != 0)
					return statement_id;	//TODO: Check if this can be caused by the trigger or not.
				// Otherwise this was probably happened because the same statement already was
				// inserted before and is unique. Or the attribute might not be verified.
				// So we check the select statement to see if the attribute is known.
				try { 
					Check (	"attributes AS a, " +
							"instances AS i " +
						String.Format (
							"i.uri = '{1}' AND " +
							"a.name = '{0}' AND " +
							"a.for_class = i.class_id " +
							"{2}",
							attribute, 
							instance,
							constraint
						)
					);
					// looks like select worked but a constraint violation occured.
					Logger.Log.Debug ("Constraint violation occured when trying to insert: \n" +
						what);
						
				} catch (ApplicationException) {
					// This means the select did not work so the attribute
					// could not be verified. So we throw the proper exception:
					throw new NotVerifiedException (
						String.Format ("Attribute {0} could not be verified.", attribute));
				}
			
			}
			return statement_id;
		}
		
		public uint AddVerifiedStatement (string instance, string attribute, uint object_id)
		{
			return AddStatement (instance, attribute, object_id, String.Format (
					" AND NOT (a.flags & {0})", 
					(int) AttributeFlags.is_not_verified)
					);
		}
		
		public uint AddStatement (string instance, string attribute, uint object_id)
		{
			uint res;
			try {
				res = AddStatement (instance, attribute, object_id, null);
				}
			catch (NotVerifiedException) {
				NonVerifiedAttribute (attribute, instance);
				res = AddStatement (instance, attribute, object_id, null);
			}
			return res;
		}

		protected uint NonVerifiedAttribute (string attribute, uint for_class) 
		{
			string[] fields = {"name", "for_class", "flags"};
			string[] values = {attribute, for_class.ToString (), String.Format ("{0}", (int) AttributeFlags.is_not_verified)};
			return InsertOrIgnore ("attributes", fields, values);
		}

		protected uint NonVerifiedAttribute (string attribute, string for_class) 
		{
			string fields = "name, for_class, flags";
			string what = String.Format ("SELECT '{0}', class_id, {2} " +
						"FROM instances " +
						"WHERE uri = '{1}' ", 
						attribute,
						for_class,
						(int) AttributeFlags.is_not_verified);
			return Insert ("attributes", fields, what);
		}

		public uint AddNonVerified (uint instance_id, uint class_id, string attribute, uint value_id) 
		{
			uint attribute_id = NonVerifiedAttribute (attribute, class_id);
			string[] fields = {"instance_id", "attribute_id", "object_id"};
			string[] values = {instance_id.ToString (),	
						attribute_id.ToString (), 
						value_id.ToString ()};
			return InsertOrIgnore ("statements", fields, values);
		}

		public uint AddNonVerified (string instance, string attribute, uint value_id) 
		{
		
			throw new NotImplementedException ();
			// uint attribute_id = NonVerifiedAttribute (attribute, class_id);
			// string[] fields = {"instance_id", "attribute_id", "object_id"};
			// string[] values = {instance_id,	attribute_id, value_id}; 
			// return InsertOrIgnore ("non_verified", fields, values);
		}


		private ArrayList GetProperties (string uri, string constraint) {
		
			ArrayList result = new ArrayList ();
			SqliteCommand command = new SqliteCommand ();
			SqliteDataReader reader;
			string refers_to;
			
			command.Connection = connection;
			command.CommandText = String.Format (
				"SELECT a.name, o.value_string, ref.uri " +
				"FROM statements AS s, " +
				"      instances AS i, " +
				"     attributes AS a, " +
				"        objects AS o " +
				"LEFT OUTER JOIN " +
				"      instances AS ref " +
				"ON " +
				"      ref.id = s.refers_to " +
				"WHERE " + 
				"i.uri='{0}' AND " +
			    "   s.instance_id = i.id AND " +
				"   s.attribute_id = a.id AND " +
				"   s.object_id = o.id " +
				"   {1}",
				SqliteUtils.Sanitize (uri),
				constraint);
				
			Logger.Log.Debug (command.CommandText);
			reader = SqliteUtils.ExecuteReaderOrWait (command);
			
			while (SqliteUtils.ReadOrWait (reader)) {
				Property prop = Property.New (reader.GetString (0), reader.GetString (1));
				// FIXME: We'll need to make use of references:
				try {
					refers_to = reader.GetString (2);
				}
				catch {
					refers_to = null;
				}
				prop.Type = PropertyType.Text;
				prop.RefersTo = refers_to;
				result.Add (prop);
			}
			command.Dispose ();
			return result;
		}
		
		public ArrayList GetProperties (string uri)
		{
			return GetProperties (uri, null);
		}
		
		public ArrayList GetNonVerified (string uri) 
		{
			return GetProperties (uri, String.Format (
			    "AND	(a.flags & {0})",
				(int) AttributeFlags.is_not_verified)
			);
		}
		
		public ArrayList GetNonVerified (string uri, string attribute)
		{
			return GetProperties (uri, String.Format (
				"AND	(a.flags & {1}) " +
				"AND a.name = {0} ",
				attribute,
				(int) AttributeFlags.is_not_verified)
			);
		}
		
		public ArrayList GetVerified (string uri) 
		{
			return GetProperties (uri, String.Format (
			    "AND NOT (a.flags & {0})",
				(int) AttributeFlags.is_not_verified)
				);
		}
		
		public ArrayList GetVerified (string uri, string attribute)
		{
			return GetProperties (uri, String.Format (
			    "AND NOT (a.flags & {1}) ",
				"AND a.name = {0} ",
				attribute,
				(int) AttributeFlags.is_not_verified)
				);
		}
						
			

	}

}
