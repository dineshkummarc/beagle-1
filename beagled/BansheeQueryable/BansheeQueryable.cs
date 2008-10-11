//
// BansheeQueryable.cs
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
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

using Stopwatch = Beagle.Util.Stopwatch;

using Beagle.Daemon;
using Beagle.Util;

[assembly: Beagle.Daemon.IQueryableTypes (typeof (Beagle.Daemon.BansheeQueryable.BansheeQueryable))]

namespace Beagle.Daemon.BansheeQueryable
{
    [QueryableFlavor (Name = "Banshee", Domain=QueryDomain.Local, RequireInotify = false, DependsOn = "Files")]
    public class BansheeQueryable : ExternalMetadataQueryable
    {
        private FileSystemQueryable.FileSystemQueryable fs_queryable;
        private BansheeIndexer indexer;
        private BansheeIndexableGenerator indexable_generator;
        
        internal BansheeIndexer Indexer {
            get { return indexer; }
        }

        public override void Start () 
        {
            base.Start ();
            NDesk.DBus.BusG.Init ();
            fs_queryable = (FileSystemQueryable.FileSystemQueryable)QueryDriver.GetQueryable ("Files").IQueryable;
            ExceptionHandlingThread.Start (new ThreadStart (StartWorker));
        }

        private void StartWorker ()
        {
            indexer = new BansheeIndexer (this);
            indexer.Start ();
        
            Log.Info ("BansheeQueryable: Banshee metadata backend started");
        }
        
        internal void OnIndexablesReady ()
        {
            LaunchIndexable ();
        }
        
        private void LaunchIndexable ()
        {
            // Cancel running task before adding a new one
            CancelIndexable ();

            // Add the new indexable generator
            indexable_generator = new BansheeIndexableGenerator (this);

            Scheduler.Task task = fs_queryable.NewAddTask (indexable_generator);
            task.Tag = BansheeIndexableGenerator.Tag;
            fs_queryable.ThisScheduler.Add (task);
            
            Log.Info ("BansheeQueryable: Scheduled new BansheeIndexableGenerator ({0})", indexable_generator);
        }

        private void CancelIndexable ()
        {
            if (indexable_generator != null && fs_queryable.ThisScheduler.ContainsByTag (BansheeIndexableGenerator.Tag)) {
                Log.Info ("BansheeQueryable: Cancelling existing BansheeIndexableGenerator ({0})", indexable_generator);
                fs_queryable.ThisScheduler.GetByTag (BansheeIndexableGenerator.Tag).Cancel ();
            }
        }
    }
}
