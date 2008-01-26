//
// BeagleSource.cs
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

using Beagle;
using SemWeb;
using System;

public class BeagleSource : SelectableSource {

	// counts the statements that match the template and returns if this number > 0
	public bool Contains (Statement template)
	{
		StatementCounterSink sink = new StatementCounterSink ();
		this.Select (template, sink);
		return sink.StatementCount > 0;
	}

	public bool Contains (Resource resource)
	{
		// FIXME !
		throw new NotImplementedException ("BeagleSource.Contains (Resource)");
	}

	// we provide only distinct statements
	public bool Distinct {
		get { return true; }
	}

	// this simply forwards to a select all
	public void Select (StatementSink sink)
	{
		this.Select (Statement.All, sink);
	}

	public void Select (Statement template, StatementSink sink)
	{
		// extract the fields for easy access
		Entity subj = template.Subject;
		Entity pred = template.Predicate;
		Resource obj = template.Object;

		// convert the SemWeb fields to the RDFQuery fields
		Uri s    = (subj == null) ? null : new Uri (subj.Uri);
		string p = (pred == null) ? null : pred.Uri;
		string o = null;

		if (obj != null) {
			if (obj is Literal) {
				Literal l = (Literal) obj;
				o = l.Value;
			} else {
				o = obj.Uri;
			}
		}

		// extract the property type from the property
		// e.g. from prop:k:beagle:MimeType
		PropertyType ptype = PropertyType.Internal;

		if (p != null) {
			if ((p.Length > 7) && p.StartsWith ("prop:")) {
				switch (p [5]) {
					case 't': ptype = PropertyType.Text; break;
					case 'k': ptype = PropertyType.Keyword; break;
					case 'd': ptype = PropertyType.Date; break;
				}
				// remove the prop:?:, which will be added by beagle later
				p = p.Substring (7);
			}
		}

		RDFQuery query = new RDFQuery (s, p, ptype, o);
		RDFQueryResult result = (RDFQueryResult) query.Send ();
		
		foreach (Hit hit in result.Hits) {
			Entity subject = new Entity (hit.Uri.ToString ()); //FIXME: Do we have to use strings here?
			foreach (Property prop in hit.Properties) {
				Entity predicate = new Entity (prop.Key);
				Resource _object = null;
			
				// for some properties the object is actually an URI (Entity)
				if (predicate == "Uri" || predicate == "ParentUri" || predicate == "ParentDirUri")
					_object = new Entity(prop.Value);
					else
				_object = new Literal(prop.Value);

				// now create a the statement and add it to the result
				Statement st = new Statement (subject, predicate, _object);
				sink.Add (st);
			}
		}
	}

	public void Select (SelectFilter filter, StatementSink sink)
	{
		throw new NotImplementedException ("Select");
		// FIXME: not implemented yet, SelectFilter are a little more complex
		// than Statements with wildcards
	}

	// copied from SemWeb/Store.cs
	internal class StatementCounterSink : StatementSink {
		int counter = 0;

		public int StatementCount {
			get { return counter; }
		}

		public bool Add (Statement statement) {
			counter ++;
			return true;
		}
	}
}
