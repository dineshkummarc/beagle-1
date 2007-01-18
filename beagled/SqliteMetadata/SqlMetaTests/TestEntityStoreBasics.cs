
using System;
using System.Collections;
using NUnit.Framework;
using Beagle.Util;

namespace Beagle.Daemon
{
	
	
	[TestFixture()]
	public class TestEntityStoreBasics
	{
		static EntityStore store = null;


		// a little bit of test data
		// TestGetProperties assumes that all 
		// non verified attributes are at the
		// end of the attributes array.
			
		static string instance = "test://TestMe";
		static string of_class = "meta:WebHistory";
		static string[] attributes = {
				"beagle:HitType",
				"beagle:MimeType",
				"beagle:Source",
				"omg:thatdoesnotexist"};	//omg:thatdoesnotexist is unknown to the model - so this should go into non_verified.
		static string[] values = {
				"HitType",
				"MimeType",
				"Source",
				"title"};
		
		ArrayList properties;
		
					
		
		[SetUp]
		public void SetUp ()
		{
		if (store == null)
			store = new EntityStore ("/tmp");
		
		properties = new ArrayList ();
		
		for (uint c = 0; c < values.Length; c++) {
			Property prop = Property.New (attributes [c], values [c]);
			properties.Add (prop);
		}
			
		}
		
		[Test]
		public void TestLowLevel()
		{
	
		string[] fields = {"value_string"};
		string[] values = {"bla"};
		
		// insert 
		 uint id_insert = store.InsertOrIgnore ("objects", fields, values);
		 store.Flush ();
		
		// ignored insert
		 uint id_ignore = store.InsertOrIgnore ("objects", fields, values);
		 Assert.AreEqual (id_ignore, id_insert);
		
		// get id
		 uint id_get = store.GetId ("objects", "value_string = 'bla'");
		 Assert.AreEqual (id_get, id_insert);
		 
		}
	

	
		[Test]
		public void TestObjects()
		{
			string val = "test://val" ;
		// insert
		 uint id_insert = store.CreateObject (val);
		 store.Flush ();
		
		// ignored insert
		uint id_ignore = store.CreateObject (val);
		Assert.AreEqual (id_ignore, id_insert);
		
		// get id
		uint id_get = store.GetObjectId (val);
		Assert.AreEqual (id_get, id_insert);
		}
		
		
		[Test]
		public void TestInstances()
		{
		
			string uri = "test://uri" ;
			string of_class = "meta:Application" ;
		// insert
		 uint id_insert = store.CreateInstance (uri, of_class);
		 store.Flush ();
		Assert.IsTrue (id_insert != 0);
			
		// ignored insert
		 uint id_ignore = store.CreateInstance (uri, of_class);
		 Assert.AreEqual (id_ignore, id_insert);
		
		// get id
		 uint id_get = store.GetInstanceId (uri);
		 Assert.AreEqual (id_get, id_insert);
			
		}

		[Test]
		public void TestStatement ()
		{
				
			Console.WriteLine ("Here we go...!");
			uint class_id = store.GetClassId (of_class);
			uint id = store.CreateInstance (instance, of_class);
			uint state_id = 0;
			
			for (uint c = 0; c < values.Length; c++) {
				Console.WriteLine (c);
				uint obj_id = store.CreateObject (values[c]);
				Console.WriteLine (attributes [c]);
				state_id = 0;
				try {				
					state_id = store.AddVerifiedStatement (id, class_id, attributes[c], obj_id);
					Assert.IsFalse (attributes [c] == "omg:thatdoesnotexist");
				}
				catch (NotVerifiedException) {
					Assert.AreEqual ("omg:thatdoesnotexist", attributes [c]);
					state_id = store.AddStatement (id, class_id, attributes[c], obj_id);
				}
				Assert.IsTrue (state_id >= 0);
			}
			
			store.Flush ();
			// This is not testing if the statements have really 
			// been written and can be retrieved.
		}

		[Test]
		public void TestGetProperties ()
		{
			TestStatement ();
			ArrayList verified = store.GetVerified (instance);
			ArrayList not_verified = store.GetNonVerified (instance);
			verified.AddRange (not_verified);
			ArrayList restored = store.GetProperties (instance);
			// This one assumes that all non verified where at
			// the end of attributes.
			Assert.AreEqual (properties.Count, verified.Count);
			Assert.AreEqual (properties.Count, restored.Count);
			for (int c = 0; c < restored.Count; c++)
			{
				Assert.AreEqual (properties [c].ToString (), restored [c].ToString ());
				Assert.AreEqual (properties [c].ToString (), verified [c].ToString ());
				Console.WriteLine ("{0} funzt.", properties [c].ToString ());
				
				//FIXME Should test getting single props here.
			}
		
		}
			
			

		[Test]
		public void TestRemoveInstance ()
		{
			string mail_uri_1 = "email://testuri_1";
			string mail_uri_2 = "email://testuri_2";
			string contact_uri = "contact://test";
			
			// we create the test data in references
			TestReferences ();
			
			// This should not remove anything due to existent references...
			Assert.IsFalse (store.RemoveInstance (contact_uri));
			store.Flush ();
			ArrayList props = store.GetProperties (contact_uri);
			Assert.IsTrue (props.Count > 0);
			
			Assert.IsTrue (store.RemoveInstance (mail_uri_1));
			Assert.IsTrue (store.RemoveInstance (mail_uri_2));
			
			// now contact should be removed...
			Assert.IsTrue (store.RemoveInstance (contact_uri));
			store.Flush ();
			props = store.GetProperties (contact_uri);
			Assert.IsTrue (props.Count == 0);
						
		}

		public void TestForceRemoveInstance ()
		{
			string mail_uri_1 = "email://testuri_1";
			string mail_uri_2 = "email://testuri_2";
			string contact_uri = "contact://test";
			
			// we create the test data in references
			TestReferences ();
			
			// This should not remove anything due to existent references...
			store.ForceRemoveInstance (contact_uri);
			store.Flush ();
			ArrayList props = store.GetProperties (contact_uri);
			Assert.IsTrue (props.Count == 0);
			
			props = store.GetProperties (mail_uri_1);
			
			Assert.IsTrue (props.Count > 0);
			
			foreach (Property prop in props) {
				if (prop.Key == "fixme:from_address")
					Assert.AreEqual (null, prop.RefersTo);
			}
			
			
			Assert.IsTrue (store.RemoveInstance (mail_uri_1));
			Assert.IsTrue (store.RemoveInstance (mail_uri_2));
			
			store.Flush ();
			props = store.GetProperties (contact_uri);
			Assert.IsTrue (props.Count == 0);
						
		}


		[Test]
		public void TestNonVerified ()
		{
			string instance = "test://TestMeNonVerified";
			string of_class = "meta:WebHistory";
			uint class_id = store.GetClassId (of_class);
			uint id = store.CreateInstance (instance, of_class);
			uint state_id = 0;
			uint obj_id = store.CreateObject ("test_nonverified");
			state_id = store.AddNonVerified (id, class_id, "omg:anotherone", obj_id);
			Assert.IsTrue (state_id >= 0);
			store.Flush ();
			// This is not testing if the statements have really 
			// been written and can be retrieved.
		}
	
		[Test]
		public void TestReferences ()
		{
			// This one is highly dependend on the MetaModell. Will use the following
			// schemas:
			// meta:MailMessage "beagle:from_address" meta:Email -> meta:Contact
			// meta:Contact "beagle:Email" meta:Email (identifies)
			// meta:MailMessage "beagle:from_name" meta:Name -> meta:Contact
			// meta:Contact "beagle:Name" meta:Name (ident)
			
			// FIXME: This test does not check yet if attributes without the 
			// is_identifier flag set create references. 
			
			uint email_id = store.CreateObject ("test@me.de");
			uint name_id = store.CreateObject ("Tester Person");
			string mail_uri_1 = "email://testuri_1";
			string mail_uri_2 = "email://testuri_2";
			string contact_uri = "contact://test";
			uint mail_class_id = store.GetClassId ("meta:MailMessage");
			uint contact_class_id = store.GetClassId ("meta:Contact");

			// we start by creating the first mail so we can test reference creation on 
			// old statements when the contact is added.
			uint mail_inst = store.CreateInstance (mail_uri_1, "meta:MailMessage");
			uint state_id = store.AddStatement (mail_inst, mail_class_id, "fixme:from_address", email_id);
			
			uint contact_inst = store.CreateInstance (contact_uri, "meta:Contact");
			// Adding the contact we'll refer to... this should add the reference to mail_1
			store.AddStatement (contact_inst, contact_class_id, "fixme:Email", email_id);
			store.AddStatement (contact_inst, contact_class_id, "fixme:Name", name_id);
			
			mail_inst = store.CreateInstance (mail_uri_2, "meta:MailMessage");
			state_id = store.AddStatement (mail_inst, mail_class_id, "fixme:from_address", email_id);
			// This should have added the reference. So we'll check:
			
			store.Flush ();
			
			ArrayList props = store.GetProperties (mail_uri_1);
			
			Assert.IsTrue (props.Count > 0);
			
			foreach (Property prop in props) {
				Console.WriteLine (prop);
				if (prop.Key == "fixme:from_address")
					Assert.AreEqual (contact_uri, prop.RefersTo);
			}
			
			props = store.GetProperties (mail_uri_2);
			
			Assert.IsTrue (props.Count > 0);
			
			foreach (Property prop in props) {
				Console.WriteLine (prop);
				if (prop.Key == "fixme:from_address")
					Assert.AreEqual (prop.RefersTo, contact_uri);
			}
			

		}

		
		[Test]
		public void TestMultithreat ()
		{
			throw new NotImplementedException ();
		}
		
		
		
		
	}
}
