//
// Fingerprint.cs
// Stores text fingerprints and compares them
//
// Paul Betts <paul.betts@gmail.com>
// Licenced under the Lesser General Public Licence (LGPL)
// Based on TextCat by Thomas Hammerl but with much less silly design

using System;
using System.Collections;
using System.Text;
using System.IO;

namespace Beagle.Util {


// This is a dumb utility class to get SortedList to sort correctly
public class NGramDuple : IComparable, IComparer
{
        public String NGram;
        public int Count;
        static Comparer strcomp = new Comparer (System.Globalization.CultureInfo.CurrentCulture);

        public NGramDuple(String s)     
        { 
                if (s == null)
                        throw new ArgumentException ("s");
                NGram = s;      Count = 1; 
        }
        public NGramDuple()     { NGram = "";   Count = 0; }
        public int CompareTo(object other)
        {
                return Compare (this, other);
        }
        
        public int Compare(object lhs, object rhs)
        {
                // We assume x and y are NGramDuple's. If not, we die here
                NGramDuple ng_lhs = lhs as NGramDuple;
                NGramDuple ng_rhs = rhs as NGramDuple;
                if (ng_lhs == null || ng_rhs == null)    
                        throw new ApplicationException ();
                        //return strcomp.Compare (lhs.ToString (), rhs.ToString ());

                // This sorts so that the highest count is first in the list 
                int ret = (ng_rhs.Count - ng_lhs.Count);
                if (ret == 0) {
                        ret = strcomp.Compare (ng_lhs.NGram, ng_rhs.NGram);
                        //Console.WriteLine ("Sorting by name instead, ret = {0}", ret);
                }
                else
                {
                        //Console.WriteLine ("ret = {0}", ret);
                }
                return ret;
        }

        public override string ToString() { return String.Format ("{0} - {1}", NGram, Count); }
}

public class TextFingerprint 
{
        // We use two lists here because the key for SortedList must be an 
        // NGramDuple, so we can't index by string; we'd have to use 
        // ContainsValue which I suspect will be quite slow.
        private SortedList fingerprintList;             // key = NGramDuple, value = NGram Name
        private Hashtable fingerprintTable;             // key = NGram Name, value = NGramDuple

        public TextFingerprint() 
        { 
                fingerprintList = new SortedList (new NGramDuple ());
                fingerprintTable = new Hashtable ();

                Clear (); 
        }

        public void Clear() { fingerprintTable.Clear (); fingerprintList.Clear (); }

        public void Read(TextReader input) 
        {
                Clear ();

                string s;
                while ( (s = input.ReadLine ()) != null)
                {
                        //Console.WriteLine ("string is '{0}'", s);
                        String [] arr = s.Split ('\t');
                        if (arr.Length != 2 || arr [0].Trim().Length == 0)
                                continue;

                        //Console.WriteLine ("Adding {0} - {1}...", arr [0], arr [1]);
                        setNGram ( arr [0].Replace ('_', ' '), Int32.Parse (arr [1].Trim ()) );
                }
                analyze_finish ();
                if (fingerprintTable.Count == 0)
                        throw new ApplicationException ("File is not correctly formatted");
        }

        
        public const int MAX_NGRAM_WRITE = 1000;
        public void Write(TextWriter output)
        {
                if (fingerprintList.Count == 0)  throw new ApplicationException ("Fingerprint is empty!");
                for (int i=0; i < fingerprintList.Count && i < MAX_NGRAM_WRITE; i++)
                {
                        NGramDuple current = (NGramDuple)fingerprintList.GetKey (i);
                        if (current == null) { 
                                //Console.WriteLine ("Crap for Crap!\n");
                                continue;
                        }

                        string chr = current.NGram.Replace (' ', '_');
                        int count = current.Count;
                        output.WriteLine (string.Format ("{0}\t{1}", chr, count));
                }
        }

        public int GetIndexOfNGram(string s)
        {
                NGramDuple ng = (NGramDuple)fingerprintTable [s];
                int ret = (ng != null ? fingerprintList.IndexOfKey (ng) : -1);
                //Console.WriteLine ("Name {0}, Index {1}", s, ret);
                return ret;
        }

        public const int MaxNGramSearch = 300;
        public const int MinValidDistance = 90000;
        public int DistanceFrom(TextFingerprint fp)
        {
                int distance = 0;
                int found = 0, notfound = 0;

                int max_dist = (fingerprintList.Count > fp.fingerprintList.Count ?
                                fingerprintList.Count : fp.fingerprintList.Count);
                /*
                for (int j=0; j < 10; j++)
                {
                        Console.WriteLine ("Character {0}: {1}", j, (fp.fingerprintList.GetKey (j) as NGramDuple).ToString());
                }
                */

                for (int i=0; i < fingerprintList.Count && i < MaxNGramSearch; i++)
                {
                        NGramDuple ng = (fingerprintList.GetKey (i) as NGramDuple);
                        String current_ngram = ng.NGram;
                        
                        int fp_index = fp.GetIndexOfNGram (current_ngram);
                        if (fp_index < 0) {      // => Not found
                                notfound++;
                                distance += max_dist;
                                continue; 
                        }
                        found++;
                        int to_add = System.Math.Abs (i - fp_index); 
                        distance += to_add;
                }

                //Console.WriteLine ("Found = {0}, Notfound = {1}", found, notfound);
                //Console.WriteLine ("Distance = {0}", distance);

                // If distance isn't big enough, it probably means we don't have
                // enough text so we'll throw it out.
                return (distance < MinValidDistance ? -1 : distance);
        }

        public void Analyze(TextReader input)
        {
                string data;
                Clear ();
                while ( (data = input.ReadLine ()) != null )     { analyze (data); }
                analyze_finish ();
        }

        public void Analyze(string s) { Clear (); analyze (s); analyze_finish (); }
        private void analyze(string s)
        {
                // Ghetto tokenize; we'd use Regexes but I want it to be i18n 
                // friendly (ie, I want it to match against all letters, even
                // kana ones for example)

                int start = -1;
                for (int i=0; i < s.Length; i++)
                {
                        // implies we haven't found a letter or apostrophe yet
                        if (start < 0)
                        {
                                if (!isPartOfNGram (s [i]))        continue;
                                start = i;
                                continue;
                        }

                        // implies we have found a letter 
                        if (isPartOfNGram (s [i]))         continue;

                        // We've got a whole token! Winnar!
                        analyzeToken (s.Substring (start, i - start));
                        start = -1;
                }

                // Pick up the last one
                if (start > 0)
                        analyzeToken (s.Substring (start, s.Length - start));
        }

        private void analyze_finish()
        {
                foreach (DictionaryEntry de in fingerprintTable)
                {
                        // FIXME: We shouldn't need this here but we do. It has to do
                        // with how SortedList in Mono detects a duplicate key (it's 
                        // based on the sorting function)
                        if (fingerprintList.ContainsKey (de.Value))
                                continue;

                        //Console.WriteLine ("a_f: ({0}, {1})", de.Value.ToString (), de.Key.ToString ());
                        fingerprintList.Add (de.Value, null);
                }

                /*
                for (int j=0; j < 10; j++)
                {
                        Console.WriteLine ("Character {0}: {1}", j, (fingerprintList.GetKey (j) as NGramDuple).ToString ());
                }
                */
                //Console.WriteLine ("count: {0}", fingerprintList.Count);
        }

        private static bool isPartOfNGram(Char c)
        {
                return (Char.IsLetter (c) || c == '\'');
        }

        private void analyzeToken(string s)
        {
                // FIXME: This function is probably gonna thrash the GC. A lot.
                string buf = " " + s.ToLower () + " ";

                // First, iterate through the word and generate the letter count
                for (int i=0; i < buf.Length; i++)
                {
                        incrementNGram (buf [i].ToString ());
                }

                // FIXME: This algorithm calls Substring a whole lot, this should
                // be fixed
                for (int NGramLen = 2; NGramLen <= 5; NGramLen++)
                {
                        int max = buf.Length / NGramLen;
                        int start = 0;
                        for (int i=0; i < max; i++)
                        {
                                incrementNGram (buf.Substring (start, NGramLen));
                                start += NGramLen;
                        }
                }
        }

        private void incrementNGram(string name)
        {
                NGramDuple ng;
                //Console.WriteLine ("fingerprintList.Count = {0}", fingerprintList.Count);
                if (fingerprintTable.ContainsKey (name)) 
                { 
                        //Console.WriteLine ("Incrementing {0}...", name);
                        ng = (NGramDuple)fingerprintTable [name];
                        ng.Count = ng.Count + 1; 
                        return; 
                }

                //Console.WriteLine ("Adding {0}...", name);
                ng = new NGramDuple (name);
                fingerprintTable.Add (name, ng);
        }

        private void setNGram(string name, int count)
        {
                NGramDuple ng;
                if (fingerprintTable.ContainsKey (name)) 
                { 
                        ng = (NGramDuple)fingerprintTable [name];
                        ng.Count = count; 
                        return; 
                }

                ng = new NGramDuple (name);
                ng.Count = count;
                fingerprintTable.Add (name, ng);
        }
}

} // Namespace
