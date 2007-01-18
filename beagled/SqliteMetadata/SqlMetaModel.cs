using System;
using Mono.Data.SqliteClient;
using Beagle;
using Beagle.Util;

namespace Beagle.Daemon {
	
	public class SqlMetaModel {

		public static void CreateModel (EntityStore store) {
		
			DateTime start = DateTime.Now;
			
			string[] SCs = new string[] { 
				"Address",
				"Type",
				"MimeType",
				"Source",
				"DateTime",
				"String",
				"Email",
				"Url",
				"Uid",		// unique identifier in the FCs App. or Beagle itself.
				"Name",
				"ApplicationName",
				"FileName",
				"Extension",
				"Tag",
				"IMAddress",
				"Alias",
				"Flags",
				"Category",
				"Mailfolder",
				"Emblem",
				"PhoneNumber",
				"Integer",
				"Float"
			};

			foreach (string SC_name in SCs) {
				store.NewSecondClass ("meta:" + SC_name);
			}

			Logger.Log.Debug("SCs Added!");

			string[] FCs = new string[] { "Application",
				"Audio",
				"Calendar",
				"Contact",
				"FeedItem",
				"File",
				"Folder",
				"Image",
				"IMLog",
				"MailAttachement",
				"MailMessage",
				"Note",
				"Presentation",
				"Spreadsheet",
				"TextDocument",
				"Video",
				"WebHistory"
			};

			
			foreach (string FC_name in FCs) {
				string metaFC = "meta:" + FC_name;
				store.NewFirstClass (metaFC);
				store.Flush ();
			// Now we go through the generall Properties
			// HitType...
				store.NewAttribute (metaFC, "beagle:HitType", "meta:Type", null, AttributeFlags.None); 
			// MimeType...
				store.NewAttribute (metaFC, "beagle:MimeType", "meta:MimeType", "meta:Application", AttributeFlags.None);
			// Source...
				store.NewAttribute (metaFC, "beagle:Source", "meta:Source", "meta:Application", AttributeFlags.None);
			// Timestamp...
				store.NewAttribute (metaFC, "Timestamp", "meta:DateTime", null, AttributeFlags.None);
			// NoContent	
				store.NewAttribute (metaFC, "beagle:NoContent", "meta:Flags", null, AttributeFlags.None);
			}

			Logger.Log.Debug("FCs Added!");
				
			store.Flush ();
			
			// ========= Going through all the FCs... ==============
			// We'll check every FC and try to prototype all the propertys we usually have..
			//
			// Let's Start with:
			//
		// === Application ===

			store.NewAttribute ("meta:Application", "beagle:Filename", "meta:FileName", null, AttributeFlags.None);
			store.NewAttribute ("meta:Application", "beagle:ExactFilename", "meta:FileName", null, AttributeFlags.is_identifier );
			store.NewAttribute ("meta:Application", "beagle:NoPunctFilename", "meta:FileName", null, AttributeFlags.None);

			store.NewAttribute ("meta:Application", "beagle:FilenameExtension", "meta:Extension", null, AttributeFlags.None);

			store.NewAttribute ("meta:Application", "beagle:Categories", "meta:Category", null, AttributeFlags.None);

		// === Calendar ===
			
			store.NewAttribute ("meta:Calendar", "fixme:attendee", "meta:String", "meta:Contact", AttributeFlags.None);

			store.NewAttribute ("meta:Calendar", "fixme:description", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:Calendar", "fixme:summary", "meta:String", null, AttributeFlags.None);

			store.NewAttribute ("meta:Calendar", "fixme:source_uid", "meta:Uid", null, AttributeFlags.None);
			store.NewAttribute ("meta:Calendar", "fixme:uid", "meta:Uid", null, AttributeFlags.None);

			store.NewAttribute ("meta:Calendar", "fixme:starttime", "meta:DateTime", null, AttributeFlags.None);
			store.NewAttribute ("meta:Calendar", "fixme:endtime", "meta:DateTime", null, AttributeFlags.None);

			store.NewAttribute ("meta:Calendar", "fixme:location", "meta:Address", null, AttributeFlags.None);
			
			store.NewAttribute ("meta:Calendar", "fixme:category", "meta:Category", null, AttributeFlags.None);

		// === Contact ===

			store.NewAttribute ("meta:Contact",
				"fixme:Email",
				"meta:Email",
				null,
				AttributeFlags.is_identifier);
			
			store.NewAttribute ("meta:Contact", "fixme:Assistant", "meta:Name", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:Contact", "fixme:Manager", "meta:Name", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:Contact", "fixme:Spouse", "meta:Name", "meta:Contact", AttributeFlags.None);

			store.NewAttribute ("meta:Contact", "fixme:BlogUrl", "meta:Url", null, AttributeFlags.is_identifier);
			store.NewAttribute ("meta:Contact", "fixme:HomepageUrl", "meta:Url", null, AttributeFlags.None);
			store.NewAttribute ("meta:Contact", "fixme:Caluri", "meta:Url", null, AttributeFlags.None);

			store.NewAttribute ("meta:Contact", "fixme:BusinessPhone", "meta:PhoneNumber", null, AttributeFlags.None);
			store.NewAttribute ("meta:Contact", "fixme:HomePhone", "meta:PhoneNumber", null, AttributeFlags.None);
			store.NewAttribute ("meta:Contact", "fixme:BusinessFax", "meta:PhoneNumber", null, AttributeFlags.None);
			store.NewAttribute ("meta:Contact", "fixme:MobilePhone", "meta:PhoneNumber", null, AttributeFlags.is_identifier);

			store.NewAttribute ("meta:Contact", "fixme:Categories", "meta:Category", null, AttributeFlags.None);

			store.NewAttribute ("meta:Contact",
				"fixme:FamilyName",
				"meta:Name",
				null,
				AttributeFlags.None);
			store.NewAttribute ("meta:Contact",
				"fixme:FileAs",
				"meta:Name",
				null,
				AttributeFlags.is_identifier);
			store.NewAttribute ("meta:Contact",
				"fixme:GivenName",
				"meta:Name",
				null,
				AttributeFlags.is_identifier);
			store.NewAttribute ("meta:Contact",
				"fixme:FullName",
				"meta:Name",
				null,
				AttributeFlags.is_identifier);
			store.NewAttribute ("meta:Contact",
				"fixme:Name",
				"meta:Name",
				null,
				AttributeFlags.is_identifier);
			store.NewAttribute ("meta:Contact",
				"fixme:Nickname",
				"meta:Name",
				null,
				AttributeFlags.is_identifier);
			
			store.NewAttribute ("meta:Contact",
				"fixme:ImJabber",
				"meta:IMAddress",
				null,
				AttributeFlags.is_identifier);
			store.NewAttribute ("meta:Contact",
				"fixme:ImAim",
				"meta:IMAddress",
				null,
				AttributeFlags.is_identifier);
			store.NewAttribute ("meta:Contact",
				"fixme:ImIcq",
				"meta:IMAddress",
				null,
				AttributeFlags.is_identifier);
			store.NewAttribute ("meta:Contact",
				"fixme:ImYahoo",
				"meta:IMAddress",
				null,
				AttributeFlags.is_identifier);
			store.NewAttribute ("meta:Contact",
				"fixme:ImGroupWise",
				"meta:IMAddress",
				null,
				AttributeFlags.is_identifier);

			store.NewAttribute ("meta:Contact",
				"fixme:ImMSN",
				"meta:IMAddress",
				null,
				AttributeFlags.is_identifier);

			store.NewAttribute ("meta:Contact", "fixme:Note", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:Contact", "fixme:Role", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:Contact", "fixme:Title", "meta:String", null, AttributeFlags.None);

			store.NewAttribute ("meta:Contact", "fixme:source_uid", "meta:Uid", null, AttributeFlags.is_identifier);
			store.NewAttribute ("meta:Contact", "fixme:uid", "meta:Uid", null, AttributeFlags.is_identifier);
			
			store.NewAttribute ("meta:Contact", "fixme:AddressLabelHome", "meta:Address", null, AttributeFlags.None);
			store.NewAttribute ("meta:Contact", "fixme:AddressLabelWork", "meta:Address", null, AttributeFlags.None);
			store.NewAttribute ("meta:Contact", "fixme:AddressLabelOther", "meta:Address", null, AttributeFlags.None);

		// === FeedItem ===

			store.NewAttribute ("meta:FeedItem", "dc:title", "meta:String", null, AttributeFlags.None);

			store.NewAttribute ("meta:FeedItem", "dc:identifier", "meta:Url", null, AttributeFlags.None);

			store.NewAttribute ("meta:FeedItem", "dc:source", "meta:Url", null, AttributeFlags.None);

			store.NewAttribute ("meta:FeedItem", "dc:publisher", "meta:String", null, AttributeFlags.None);

			store.NewAttribute ("meta:FeedItem", "dc:creator", "meta:String", "meta:Contact", AttributeFlags.None);

			store.NewAttribute ("meta:FeedItem", "fixme:source_uid", "meta:Uid", null, AttributeFlags.None);
			store.NewAttribute ("meta:FeedItem", "fixme:uid", "meta:Uid", null, AttributeFlags.None);

			store.NewAttribute ("meta:FeedItem", "fixme:Parent", "meta:Url", "meta:File", AttributeFlags.None);

		// === File ===
			store.NewAttribute ("meta:File", "beagle:Filename", "meta:FileName", null, AttributeFlags.None);
			store.NewAttribute ("meta:File", "beagle:FilenameExtension", "meta:Extension", null, AttributeFlags.None);
			store.NewAttribute ("meta:File", "beagle:ExactFilename", "meta:FileName", null, AttributeFlags.None);
			store.NewAttribute ("meta:File", "beagle:NoPunctFilename", "meta:FileName", null, AttributeFlags.None);

			store.NewAttribute ("meta:File", "_private:ParentDirUri", "meta:Uid", "meta:Folder", AttributeFlags.None);
			store.NewAttribute ("meta:File", "_private:IsDirectory", "meta:Flags", null, AttributeFlags.None);

		// === Image ===
			store.NewAttribute ("meta:Image", "beagle:Filename", "meta:FileName", null, AttributeFlags.None);
			store.NewAttribute ("meta:Image", "beagle:ExactFilename", "meta:FileName", null, AttributeFlags.None);
			store.NewAttribute ("meta:Image", "beagle:NoPunctFilename", "meta:FileName", null, AttributeFlags.None);
			
			store.NewAttribute ("meta:Image", "fixme:widht", "meta:Integer", null, AttributeFlags.None);
			store.NewAttribute ("meta:Image", "fixme:height", "meta:Integer", null, AttributeFlags.None);
			store.NewAttribute ("meta:Image", "exif:UserComment", "meta:String" ,null, AttributeFlags.None);
		        store.NewAttribute ("meta:Image", "exif:PixelXDimension", "meta:Integer" ,null, AttributeFlags.None);
		        store.NewAttribute ("meta:Image", "exif:PixelYDimension", "meta:Integer" ,null, AttributeFlags.None);
		        store.NewAttribute ("meta:Image", "exif:ShutterSpeedValue", "meta:String" , null, AttributeFlags.None);
		        store.NewAttribute ("meta:Image", "exif:ExposureTime", "meta:String", null, AttributeFlags.None);
		        store.NewAttribute ("meta:Image", "exif:FNumber", "meta:String", null, AttributeFlags.None);
		        store.NewAttribute ("meta:Image", "exif:ApertureValue", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:Image", "exif:FocalLength", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:Image", "exif:Flash", "meta:String", null, AttributeFlags.None);
		        store.NewAttribute ("meta:Image", "exif:Model", "meta:String", null, AttributeFlags.None);
		        store.NewAttribute ("meta:Image", "exif:DateTime", "meta:DateTime", null, AttributeFlags.None);
		       
				
		// === IMLog ===
			
			store.NewAttribute ("meta:IMLog", "fixme:client", "meta:ApplicationName", "meta:Application", AttributeFlags.None);
			store.NewAttribute ("meta:IMLog", "fixme:starttime", "meta:DateTime", null, AttributeFlags.None);
			store.NewAttribute ("meta:IMLog", "fixme:endtime", "meta:DateTime", null, AttributeFlags.None);
			store.NewAttribute ("meta:IMLog", "fixme:protocol", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:IMLog", "fixme:speaking_to", "meta:IMAddress", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:IMLog", "fixme:identity", "meta:Name", null, AttributeFlags.None);
			
		// === MailMessage ===
		
			store.NewAttribute ("meta:MailMessage", "dc:title", "meta:String", null, AttributeFlags.None);

			store.NewAttribute ("meta:MailMessage", "fixme:reference", "meta:String", "meta:MailMessage", AttributeFlags.None);
		
			store.NewAttribute ("meta:MailMessage", "fixme:date", "meta:DateTime", null, AttributeFlags.None);
			
			store.NewAttribute ("meta:MailMessage", "fixme:flags", "meta:Flags", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:isSeen", "meta:Flags", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:isSent", "meta:Flags", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:isDraft", "meta:Flags", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:isDeleted", "meta:Flags", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:isAnswered", "meta:Flags", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:hasAttachments", "meta:Flags", null, AttributeFlags.None);

			store.NewAttribute ("meta:MailMessage", "fixme:folder" , "meta:Mailfolder", null, AttributeFlags.None);

			store.NewAttribute ("meta:MailMessage", "fixme:gotFrom", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:from", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:from_sanitized", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:from_name", "meta:Name", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:from_address", "meta:Email", "meta:Contact", AttributeFlags.None);

			store.NewAttribute ("meta:MailMessage", "fixme:mlist", "meta:Email", null, AttributeFlags.None);
		
			store.NewAttribute ("meta:MailMessage", "fixme:source_uid", "meta:Uid", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:uid", "meta:Uid", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:msgid", "meta:Uid", null, AttributeFlags.None);

			store.NewAttribute ("meta:MailMessage", "fixme:to", "meta:String", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:sentTo", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:to_sanitized", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:cc", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:cc_sanitized", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:bcc", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:bcc_sanitized", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:to_name", "meta:Name", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:cc_name", "meta:Name", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:bcc_name", "meta:Name", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:to_address", "meta:Email", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:cc_address", "meta:Email", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:bcc_address", "meta:Email", "meta:Contact", AttributeFlags.None);

			store.NewAttribute ("meta:MailMessage", "fixme:Parent", "meta:Uid", null, AttributeFlags.None);
			
			// This makes indexing a lot slower because we look through all messages to find the parent. This can be done
			// a lot easier - maybe even at query time.
			// store.NewAttribute ("meta:MailMessage", "fixme:Parent", "meta:Uid", "meta:MailMessage", AttributeFlags.None);

			store.NewAttribute ("meta:MailMessage", "fixme:client", "meta:ApplicationName", "meta:Application", AttributeFlags.None);

			store.NewAttribute ("meta:MailMessage", "fixme:account", "meta:String", null, AttributeFlags.None);

			// --- For Attachements --- 
			// FIXME: Let them be indexed as Attachements and not as MailMessages!

			
			store.NewAttribute ("meta:MailMessage", "parent:fixme:from_name", "meta:Name", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "parent:fixme:to_name", "meta:Name", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "parent:fixme:cc_name", "meta:Name", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "parent:fixme:from_address", "meta:Email", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "parent:fixme:to_address", "meta:Email", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "parent:fixme:cc_address", "meta:Email", "meta:Contact", AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "parent:fixme:mlist", "meta:Email", null, AttributeFlags.None);

			store.NewAttribute ("meta:MailMessage", "parent:fixme:date", "meta:DateTime", null, AttributeFlags.None);
			
			store.NewAttribute ("meta:MailMessage", "parent:dc:title", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "meta:GENERATOR", "meta:String", null, AttributeFlags.None);
			store.NewAttribute ("meta:MailMessage", "fixme:attachment_title", "meta:String", null, AttributeFlags.None);






			DateTime end = DateTime.Now;
			
			Logger.Log.Debug("Creating MetaModel Sqlite Database took: " + (end - start).TotalSeconds);
			
		}  
	}
}


