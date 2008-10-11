//
// BansheeIndexer.cs
//
// Author:
//   Aaron Bockover <abockover@novell.com>
//
// Copyright (C) 2008 Novell, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;

using Banshee.Collection.Indexer.RemoteHelper;

using Beagle.Util;
using Beagle.Daemon;
using Stopwatch = Beagle.Util.Stopwatch;

namespace Beagle.Daemon.BansheeQueryable
{
    // Field dump from GetExportFields in Banshee 1.3.2
    //
    // bit-rate, artist, rating, skip-count, artist-music-brainz-id, conductor, mime-type, 
    // file-size, length, URI, music-brainz-id, name, play-count, license-uri, track-number, 
    // disc-count, grouping, date-added, album-artist, last-played, genre, release-date, 
    // local-path, is-compilation, media-attributes, more-info-uri, disc-number, comment, 
    // year, copyright, bpm, composer, last-skipped, track-count, album, album-music-brainz-id
    
    public class BansheeIndexer : SimpleIndexerClient
    {
        private List<IDictionary<string, object>> indexed_items = new List<IDictionary<string, object>> ();
        private BansheeQueryable queryable;
        private Stopwatch stopwatch; 
        private bool should_commit;
    
        public BansheeIndexer (BansheeQueryable queryable)
        {
            this.queryable = queryable;
        }
        
        public IDictionary<string, object> [] CopyResults ()
        {
            return indexed_items.ToArray ();
        }
        
#region SimpleIndexerClient Implementation Hooks
        
        protected override void OnStarted ()
        {   
            foreach (string field in GetAvailableExportFields ()) {
                if (!ShouldSkipField (field, SkipType.Exclude)) {
                    AddExportField (field);
                }
            }
        }
        
        protected override void OnBeginUpdateIndex ()
        {
            Log.Info ("BansheeQueryable: Reading indexer results from DBus");
            
            stopwatch = new Stopwatch ();
            stopwatch.Start ();
            
            indexed_items.Clear ();
            should_commit = true;
        }
        
        protected override void OnEndUpdateIndex ()
        {
            stopwatch.Stop ();
            Log.Info ("BansheeQueryable: Finished reading {0} results in {1} over DBus", indexed_items.Count, stopwatch);
            
            // The indexing server terminated our read, probably because a user requested
            // that Banshee start up as an application; in this case we discard any index
            // results collected, and reset - the index process will trigger automatically
            // when the application starts per the user's request
            if (!should_commit) {
                indexed_items.Clear ();
                Log.Info ("BansheeQueryable: Discarding index results, OnShutdownWhileIndexing called");
                return;
            }
            
            queryable.OnIndexablesReady ();
        }
    
        protected override void IndexResult (IDictionary<string, object> result)
        {
            indexed_items.Add (result);
        }
        
        protected override void OnShutdownWhileIndexing ()
        {
            Log.Info ("BansheeQueryable: OnShutdownWhileIndexing - server has terminated the index operation");
            should_commit = false;
        }
        
#endregion

#region SimpleIndexerClient Index Trigger Properties

        // FIXME: These need implementing, but I have no idea how to store and read
        // this data in Beagle. With these returning nothing, the entire index process
        // will trigger every time!
                                
        protected override int CollectionCount { 
            get { return 0; }
        }
        
        protected override DateTime CollectionLastModified { 
            get { return DateTime.MinValue; }
        }
        
#endregion
        
#region Convert the remote a{sv} result into a Beagle Indexable

        public static Indexable GetIndexable (IDictionary<string, object> result)
        {
            Uri uri = result.ContainsKey ("local-path") 
                ? UriFu.PathToFileUri ((string)result["local-path"])
                : UriFu.EscapedStringToUri ((string)result["URI"]);
            
            Indexable indexable = new Indexable (IndexableType.PropertyChange, uri);
            
            foreach (KeyValuePair<string, object> item in result) {
                FieldAction action = GetFieldAction (item.Key);
                if (action.Skip == SkipType.Ignore) {
                    continue;
                }
                
                string banshee_name = item.Key;
                string beagle_name = action.BeagleName ?? MakeBeagleName (banshee_name);
                Property property;
                
                switch (action.PropertyKind) {
                    case PropertyKind.String:
                        property = Beagle.Property.New (beagle_name, item.Value.ToString ());
                        break;
                    case PropertyKind.Date:
                        property = Beagle.Property.NewDateFromString (beagle_name, item.Value.ToString ());
                        break;
                    case PropertyKind.Unsearched:
                    default:
                        property = Beagle.Property.NewUnsearched (beagle_name, item.Value);
                        break;
                }
                
                property.IsMutable = true;
                property.IsPersistent = true;
                indexable.AddProperty (property);
            }
            
            return indexable;
        }
        
#endregion

#region Crazy Static stuff for magically translating, ignoring, and excluding fields from Banshee to Beagle        
    
        private enum PropertyKind
        {
            Unsearched,
            String,
            Date
        }
        
        private enum SkipType
        {
            None,
            Ignore,
            Exclude
        }
    
        private struct FieldAction
        {
            public static FieldAction Zero = new FieldAction ();
        
            public static FieldAction NewIgnore (string bansheeName)
            {
                FieldAction action = new FieldAction ();
                action.BansheeName = bansheeName;
                action.Skip = SkipType.Ignore;
                return action;
            }
            
            public static FieldAction NewExclude (string bansheeName)
            {
                FieldAction action = new FieldAction ();
                action.BansheeName = bansheeName;
                action.Skip = SkipType.Exclude;
                return action;
            }
            
            public static FieldAction NewMap (string bansheeName, string beagleName)
            {
                return NewMap (bansheeName, beagleName, PropertyKind.Unsearched);
            }
            
            public static FieldAction NewMap (string bansheeName, string beagleName, PropertyKind kind)
            {
                FieldAction action = new FieldAction ();
                action.BansheeName = bansheeName;
                action.BeagleName = beagleName;
                action.PropertyKind = kind;
                return action;
            }
            
            public static FieldAction NewOverride (string bansheeName, PropertyKind kind)
            {
                FieldAction action = new FieldAction ();
                action.BansheeName = bansheeName;
                action.PropertyKind = kind;
                return action;
            }
        
            public string BansheeName;
            public string BeagleName;
        
            public SkipType Skip;
            public PropertyKind PropertyKind;
        }
        
        private static List<FieldAction> field_actions = new List<FieldAction> ();
        
        static BansheeIndexer ()
        {
            field_actions.Add (FieldAction.NewMap ("copyright", "dc:copyright", PropertyKind.String));
            field_actions.Add (FieldAction.NewMap ("name", "dc:title", PropertyKind.String));
            
            field_actions.Add (FieldAction.NewOverride ("artist", PropertyKind.String));
            field_actions.Add (FieldAction.NewOverride ("album-artist", PropertyKind.String));
            field_actions.Add (FieldAction.NewOverride ("composer", PropertyKind.String));
            field_actions.Add (FieldAction.NewOverride ("album", PropertyKind.String));
            field_actions.Add (FieldAction.NewOverride ("conductor", PropertyKind.String));
            field_actions.Add (FieldAction.NewOverride ("genre", PropertyKind.String));
            field_actions.Add (FieldAction.NewOverride ("date-added", PropertyKind.Date));
            field_actions.Add (FieldAction.NewOverride ("release-date", PropertyKind.Date));
            field_actions.Add (FieldAction.NewOverride ("last-played", PropertyKind.Date));
            field_actions.Add (FieldAction.NewOverride ("last-skipped", PropertyKind.Date));
            
            field_actions.Add (FieldAction.NewIgnore ("URI"));
            field_actions.Add (FieldAction.NewIgnore ("local-path"));
            
            field_actions.Add (FieldAction.NewExclude ("file-size"));
            field_actions.Add (FieldAction.NewExclude ("media-attributes"));
            field_actions.Add (FieldAction.NewExclude ("mime-type"));
        }
        
        private static bool ShouldSkipField (string field, SkipType skip)
        {
            foreach (FieldAction action in field_actions) {
                if (action.Skip == skip && action.BansheeName == field) {
                    return true;
                }
            }
            
            return false;
        }
        
        private static FieldAction GetFieldAction (string field)
        {
            foreach (FieldAction action in field_actions) {
                if (action.BansheeName == field) {
                    return action;
                }
            }
            
            return FieldAction.Zero;
        }
        
        private static string MakeBeagleName (string bansheeName)
        {
            return String.Format ("fixme:{0}", bansheeName.ToLower ().Replace ("-", String.Empty));
        }
        
#endregion

    }
}
