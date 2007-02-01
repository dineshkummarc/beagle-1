//
// TextFingerprint.cs: Loads and manages sample fingerprints
//
// Paul Betts <paul.betts@gmail.com>
// Licenced under the Lesser General Public Licence (LGPL)
// Based on TextCat by Thomas Hammerl but with much less silly design

using System;
using System.Collections;
using System.Text;
using System.IO;

namespace Beagle.Util {

public class TextCategorizer
{
        private Hashtable languageSamples = new Hashtable ();

        public TextCategorizer() { Reset (); }

        public void Reset() { languageSamples.Clear (); }

        public void LoadAll(string path)
        {
                DirectoryInfo di = new DirectoryInfo (path);
                foreach (FileInfo current_fi in di.GetFiles ())
                {
                        string current = current_fi.FullName;

                        TextFingerprint fp = new TextFingerprint ();
                        try
                        {
                                //Console.WriteLine ("Reading {0}", current);
                                StreamReader sr = new StreamReader (current);
                                fp.Read (sr);   sr.Close ();
                        }
                        catch (Exception e)
                        {
                                Logger.Log.Warn ("Died: '{0}' reading {1}\nStack: {2}", e.Message, current, e.StackTrace);
                                continue;
                        }
                        languageSamples.Add (System.IO.Path.GetFileNameWithoutExtension (current), fp);
                }

                if (languageSamples.Count == 0)
                        throw new ApplicationException ("Couldn't load samples!");
        }

        // This returns the closest locale code corresponding to the language the 
        // text in the string is written in, or null on failure (assuming your file
        // names are locale codes, which they should be)
        public string GetTextLanguage(string s)
        {
                // Calculate the fingerprint of the sample text
                TextFingerprint fp = new TextFingerprint ();
                try { fp.Analyze (s); } catch (Exception) { return null; }

                return closestMatch (fp);
        }

        public string GetTextLanguage(TextReader input)
        {
                // Calculate the fingerprint of the sample text
                // FIXME: Should we log this somehow?
                TextFingerprint fp = new TextFingerprint ();
                try { 
                        fp.Analyze (input); 
                } 
                catch (Exception ex) { 
                        Logger.Log.Debug ("Error: {0}\nStack: {1}", ex.Message, ex.StackTrace);
                        return null; 
                }

                return closestMatch (fp);
        }

        // Sets how much lower the match has to be than the average
        private float MatchEpsilon = 0.22f;             // 22%

        // Sets how much different the top two have to be, in terms of 
        // the average
        private float DistEpsilon = 0.05f;              // 5%

        // Sets how different the two percentages must be, as an absolute
        // value
        private float DiffEpsilon = 0.035f;

        private string closestMatch(TextFingerprint fp)
        {
                string closest_name = null;
                int closest_dist = Int32.MaxValue;
                int average = 0, count = 0;
                int second_closest_dist = Int32.MaxValue;
                string second_closest_name = null;

                foreach (DictionaryEntry de in languageSamples)
                {
                        int current_dist;
                        try {
                                current_dist = fp.DistanceFrom (de.Value as TextFingerprint);
                                //Logger.Log.Debug ("Distance to {0}: {1}", de.Key.ToString (), current_dist);
                                if (current_dist <= 0)
                                        continue;
                                count++;
                                average += current_dist;

                                if (current_dist < closest_dist)
                                {
                                        second_closest_dist = closest_dist;
                                        second_closest_name = closest_name;
                                        closest_dist = current_dist;
                                        closest_name = de.Key.ToString ();
                                        continue;
                                }
                                if (current_dist < second_closest_dist)
                                {
                                        second_closest_dist = current_dist;
                                        second_closest_name = de.Key.ToString ();
                                }
                        }
                        catch (Exception e) 
                        { 
                                Logger.Log.Debug ("Error: {0}\nStack: {1}", e.Message, e.StackTrace);
                                continue; 
                        }
                }

                if (count == 0 || average == 0)
                        return null; 
                average /= count;

                // FIXME: There may be some unnecessary casts here
                float top_two_percentage = (float) (second_closest_dist - closest_dist) / (float)average;
                float top_vs_average = ((float) (average - closest_dist) / (float)average);

                // Print some debugging statistics
                Logger.Log.Debug ("closest = {0}, 2nd place = {1}, average = {2}", 
                                closest_dist, second_closest_dist, average);
                Logger.Log.Debug ("% between top two: {0}, % from top to average: {1}\nWould've picked {2}, 2nd is {3}", 
                                 top_two_percentage, top_vs_average, closest_name, second_closest_name);

                // Compare the top two items; if they're too close, 
                // we don't know so return null
                if (top_two_percentage < DistEpsilon)
                        return null;

                // Compare the closest with the average; if they're too close,
                // we don't know so return null
                if (top_vs_average < MatchEpsilon)
                        return null;

                // Generally if these two statistics are very close, we will
                // guess pretty wrong (Yiddish for some reason?), so avoid that too
                if (Math.Abs (top_vs_average - top_two_percentage) < DiffEpsilon)
                        return null;

                return closest_name;
        }

        static Hashtable locale_table;
        static Hashtable reverse_locale_table;
        private static void buildLocaleTables()
        {
                // Load the locale table if we don't have it
                // FIXME: I'm not sure the correct way to discern where this file is
                // Is there a sane system that _doesn't_ keep this here?
                StreamReader input = new StreamReader ("/usr/share/locale/all_languages");

                string s;
                string current = "";

                // Pick up everything before the encoding and get the locale
                string this_language = Environment.GetEnvironmentVariable ("LANG").Split ('.') [0];
                bool use_c_locale = (this_language == null || 
                                     this_language.Length == 0 || 
                                     this_language.Substring (0,2) == "en");
                if (!use_c_locale)
                        Console.WriteLine ("LANG = {0}", this_language);

                locale_table = new Hashtable ();
                reverse_locale_table = new Hashtable ();
                while ( (s = input.ReadLine ()) != null )
                {
                        // We do sloppy parsing here so that this goes faster
                        if (s [0] == '[') {
                                current = s.Substring (1,2);     // " [en]" => "en"
                                continue;
                        }

                        if (locale_table.ContainsKey (current) || s.Length < 3)
                                continue;

                        if (use_c_locale && s.Length > 5 && s [4] == '=') {
                                // "Name=English" => "English"
                                string name = s.Substring (5, s.Length - 5);
                                if (current == null || name == null) {
                                        Logger.Log.Debug ("Something is null! current = {0}, name = {1}",
                                                         current, name);
                                }
                                locale_table.Add (current, name);
                                reverse_locale_table.Add (name.ToLower (), current);
                                continue;
                        }
                        int right_bracket = s.IndexOf (']');
                        if (right_bracket < 1)   continue;
                        if (this_language != s.Substring (5, right_bracket-5))
                                continue;
                        
                        //Console.WriteLine ("Adding {0} - {1}", current, s.Substring (right_bracket+2, s.Length - right_bracket-2));
                        locale_table.Add (current, s.Substring (right_bracket+2, s.Length - right_bracket-2));
                }
                input.Close ();

                if (locale_table.Count == 0)
                        throw new ApplicationException ("No entries in locale table!");
        }
        public static string GetLocaleName(string locale_code)
        {
                if (locale_table == null) {
                        try { buildLocaleTables (); }
                        catch (Exception ex) {
                                Logger.Log.Error (ex, "Can't load locale table!");
                                return locale_code;
                        }
                }

                return (locale_table.ContainsKey (locale_code) && (string)locale_table [locale_code] != null ? 
                        (string)locale_table [locale_code] : locale_code);
        }

        public static string GetLocaleCode(string name)
        {
                if (reverse_locale_table == null) {
                        try { buildLocaleTables (); }
                        catch (Exception ex) {
                                Logger.Log.Error (ex, "Can't load locale table!");
                                return name;
                        }
                }

                string s = name.ToLower ();
                return (reverse_locale_table.ContainsKey (s) && reverse_locale_table [s] != null ? 
                        (string)reverse_locale_table [s] : name);
        }

}

} // Namespace
