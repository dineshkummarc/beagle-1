using System;
using System.Collections;
using System.Collections.Generic;

using NDesk.DBus;
using org.freedesktop.DBus;

namespace Beagle {

	public delegate void HitsAddedDelegate (IList<Hit> hits);
	public delegate void HitsSubtractedDelegate (IList<Uri> uris);
	public delegate void FinishedDelegate ();

	//[Interface ("org.freedesktop.Beagle.QueryManager.Query")]
	public interface IQuery {
		
		void AddText (string str);

		void AddPart (QueryPart part);
		void ClearParts ();

		void AddDomain (QueryDomain domain);
		void RemoveDomain (QueryDomain domain);
		bool AllowsDomain (QueryDomain domain);

		QueryDomain QueryDomain {
			get;
		}

		//IList<QueryPart> Parts {
		ICollection Parts {
			get;
		}

		ICollection<string> Text {
			get;
		}

		string QuotedText {
			get;
		}

		ICollection<string> StemmedText {
			get;
		}

		int MaxHits {
			get;
		}

		bool IsIndexListener {
			get;
		}

		bool IsEmpty {
			get;
		}

		event HitsAddedDelegate HitsAdded;
		event HitsSubtractedDelegate HitsSubtracted;
		event FinishedDelegate Finished;
	}
}