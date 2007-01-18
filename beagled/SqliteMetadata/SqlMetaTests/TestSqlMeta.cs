
using System;
using System.Collections;
using NUnit.Framework;
using Beagle.Util;
using Lucene.Net.Documents;

namespace Beagle.Daemon
{
	
	
	[TestFixture()]
	public class TestSqlMeta
	{
		static SqlMetadata store = null;


		// a little bit of test data
		//
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
		
		
		
		
	}
}
