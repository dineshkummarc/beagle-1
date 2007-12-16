using System;
using System.IO;
using System.Text;

using Beagle;

public class RdfQueryTool {
	public static void Main (string[] args)
	{

		if (args.Length != 3) {
			Console.WriteLine ("Usage: program-name <subject> <predicate> <object>");
			return;
		}

		RDFQueryResult result;

		Console.WriteLine ("subject:'{0}' predicate:'{1}' object:'{2}'", args [0], args [1], args [2]);
		RDFQuery query = new RDFQuery (args [0], args [1], args [2]);
		result = (RDFQueryResult) query.Send ();

		if (result == null) {
			Console.WriteLine ("null.......");
			return;
		}

		foreach (string uri in result.Matches)
			Console.WriteLine (" - [{0}]", uri);
	}
}
