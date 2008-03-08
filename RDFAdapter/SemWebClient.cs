//
// SemWebClient.cs
//
// Copyright (C) 2007 Enrico Minack <minack@l3s.de>
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
using Beagle;
using Beagle.Util;
using SemWeb;

public class SemWebClient {
	public static void Main (string[] args)
	{
		BeagleSource source = new BeagleSource ();
		source.RDFToBeagle = new BeagleSource.RDFToBeagleHook (RDFToBeagle);
		source.BeagleToRDF = new BeagleSource.BeagleToRDFHook (EmailToEntity);

		System.Console.Out.WriteLine ();
		System.Console.Out.WriteLine ("Querying for all Triples with MimeType:");
		query (source, new Statement (null, new Entity ("prop:k:beagle:MimeType"), null));

		System.Console.Out.WriteLine ();
		System.Console.Out.WriteLine ("Querying for all Triples with FileSize:");
		query (source, new Statement (null, BeagleSource.BeaglePropertyToEntity ("prop:k:fixme:filesize"), null));
		
		System.Console.Out.WriteLine ();
		System.Console.Out.WriteLine ("Querying for all Triples:");
		query (source, Statement.All);
	}
	
	public static void query (SelectableSource source, Statement filter) {
		using (RdfWriter writer = new N3Writer (System.Console.Out))
			source.Select (filter, writer);
	}

	// Make URIs out of certain objects
	private static void RDFToBeagle (Entity subj, Entity pred, Resource obj, out Uri s, out string p, out string o)
	{
		s = (subj == null || String.IsNullOrEmpty (subj.Uri)) ? null : new Uri (subj.Uri);
		p = (pred == null || String.IsNullOrEmpty (pred.Uri)) ? null : pred.Uri.Substring (BeagleSource.Prefix.Length);
		o = null;

		if (obj != null) {
			if (obj is Literal) {
				Literal l = (Literal) obj;
				o = l.Value;
			} else {
				o = obj.Uri;
				if (o.StartsWith ("mailto://"))
					o = o.Substring (9);
			}
		}
	}

	private static void EmailToEntity (Property prop, out Resource _object)
	{
		_object = null;

		// Create URIs for email addresses
		if (prop.Key == "fixme:from_address" || prop.Key == "fixme:cc_address" || prop.Key == "fixme:to_address")
			_object = new Entity ("mailto://" + prop.Value);
		else
			_object = new Literal (prop.Value);
	}
}

