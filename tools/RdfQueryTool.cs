using System;
using System.IO;
using System.Text;

using Beagle;

public class RdfQueryTool {
	public static void Main (string[] args)
	{

		if (args.Length == 0)
			return;

		RDFQueryResult result;

		StringBuilder sb = new StringBuilder ();
		foreach (string arg in args)
			sb.Append (String.Format ("{0}{1}", (sb.Length > 0 ? " " : ""), arg));

		RDFQuery query = new RDFQuery (sb.ToString ());
		result = (RDFQueryResult) query.Send ();

		if (result == null) {
			Console.WriteLine ("null.......");
			return;
		}

		foreach (string uri in result.Matches)
			Console.WriteLine (" - [{0}]", uri);
	}
}
