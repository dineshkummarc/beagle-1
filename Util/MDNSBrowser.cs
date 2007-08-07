//
// MDNSBrowser.cs
//
// Copyright (C) 2006 Kyle Ambroff <kwa@icculus.org>
//

//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//


using System;
using System.Collections;
using System.Xml.Serialization;

using GLib;
using Avahi;

namespace Beagle.Util
{
        public delegate void MDNSEventHandler (object sender, MDNSEventArgs args);
        
        public class MDNSBrowser : IDisposable
        {
                public event MDNSEventHandler HostFound;
                public event MDNSEventHandler HostRemoved;
                
                private Avahi.Client avahi_client = null;
                private ServiceBrowser browser = null;
                private ServiceResolver resolver = null;
                
                private Hashtable hosts = null;
                
                public MDNSBrowser ()
                {
                        hosts = new Hashtable ();
                        avahi_client = new Avahi.Client ();
                }

                public void Dispose ()
                {
                        browser.Dispose ();
                        browser = null;
                }

                public void Start ()
                {
                        browser = new ServiceBrowser (avahi_client, "_beagle._tcp");
                        browser.ServiceAdded += OnServiceAdded;
                        browser.ServiceRemoved += OnServiceRemoved;
                }
                
                private void OnServiceAdded (object sender, ServiceInfoArgs args)
                {
                        if ((args.Service.Flags & LookupResultFlags.Local) > 0)
                                return;
                        
                        resolver = new ServiceResolver (avahi_client, args.Service);
                        resolver.Found += OnServiceResolved;
                        resolver.Timeout += OnServiceTimeout;
                }
                
                private void OnServiceResolved (object sender, ServiceInfoArgs args)
                {
                        // GC the service resolver.
                        (sender as ServiceResolver).Dispose ();

			string url = String.Format ("http://{0}:{1}", args.Service.HostName, args.Service.Port);
                        Uri uri = new Uri (url);

                        bool password_required = false;
                        string cookie = null;

                        foreach (byte[] b in args.Service.Text) {
                                string text = System.Text.Encoding.UTF8.GetString (b);
                                string [] split = text.Split ('=');
                                
                                if (split.Length < 2)
                                        continue;

                                if (split [0].ToLower () == "password")
                                        password_required = (split [1].ToLower () == "true");
                                else if (split [0].ToLower () == "org.freedesktop.avahi.cookie")
                                        cookie = split [1];
                        }

                        MDNSService service = new MDNSService (args.Service.Name, uri, password_required, cookie);
                        hosts [args.Service.Name] = service;

                        MDNSEventArgs event_args = new MDNSEventArgs (service);

                        if (HostFound != null)
                                HostFound (null, event_args);
                        
                        resolver.Dispose ();
                }

                private void OnServiceTimeout (object sender, EventArgs args)
                {
                        (sender as ServiceResolver).Dispose ();
                        Logger.Log.Info ("Failed to resolve service.");
                }

                private void OnServiceRemoved (object sender, ServiceInfoArgs args)
                {
                        MDNSService service = (MDNSService)hosts [args.Service.Name];
                        
			if (service != null)
                                hosts.Remove (service.Name);
                        
                        MDNSEventArgs event_args = new MDNSEventArgs (service);

                        if (HostRemoved != null)
                                HostRemoved (this, event_args);
                }

                public Hashtable AvailableHosts {
                        get { return hosts; }
                }
        }
       
        public class MDNSEventArgs : EventArgs
        {
                private MDNSService service;

                public MDNSEventArgs (MDNSService service) : base ()
                {
                        this.service = service;
                }

                public MDNSService Service {
                        get { return service; }
                }
                
                public Uri Address {
                        get { return service.GetUri (); }
                }
                
                public string Name {
                        get { return service.Name; }
                }
        }
}
