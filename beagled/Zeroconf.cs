//
// Zeroconf.cs
//
// Copyright (C) 2006 Kyle Ambroff <kambroff@csus.edu>
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

using Avahi;

using Beagle.Util;

namespace Beagle.Network
{        
        public class Zeroconf
        {
                public static string DOMAIN = "local";
                public static string PROTOCOL = "_beagle._tcp";

                // Service name, such as "Kyle's Beagle Index"
                private static string name;
                private static ushort port;
                private static bool enabled = false;

                public static string Name {
                        get { return name; }
                }

                public static ushort Port {
                        get { return port; }
                }

                public static bool Enabled {
                        get { return enabled; }
                }

                private static Publisher publisher;
                
                // no instantiation
                private Zeroconf () {}
                
                public static void ConfigurationChanged (Conf.Section section)
                {
                        Logger.Log.Debug ("Networking Configuration Changed");
                        Conf.NetworkingConfig net_conf = (Conf.NetworkingConfig) section;

                        if (enabled != net_conf.ShareIndex) {
                                enabled = net_conf.ShareIndex;
                                
                                if (enabled)
                                        publisher.Shutdown ();
                                else                                        
                                        Publish (port);
                                
                                Logger.Log.Info("Networking: Index sharing over the network {0}", 
                                                enabled ? "Enabled" : "Disabled");
                        }

                        // handle index name changes
                        if (String.Compare (name, net_conf.IndexName) != 0) {
                                publisher.Update ();
                        }
                }

                public static void Publish (ushort p)
                {
                        port = p;
                        Publish ();
                }

                public static void Publish ()
                {                        
                        publisher = new Publisher ();
                        publisher.Publish ();
                        
                        Conf.Subscribe (typeof (Conf.NetworkingConfig), new Conf.ConfigUpdateHandler (ConfigurationChanged));
                }

                public static void Stop ()
                {
                        publisher.Shutdown ();
                        publisher = null;
                }
        }

        public class Publisher
        {
                EntryGroup entry_group;
                Client client;

                private int collisions;
                
                public void Publish ()
                {
                        if (client == null)
                                client = new Client ();
                        
                        try {
                                if (entry_group != null)
                                        entry_group.Reset ();
                                else {
                                        entry_group = new EntryGroup (client);
                                        entry_group.StateChanged += OnEntryGroupStateChanged;
                                        
                                        string [] args = new string [] {
                                                "Password=" + (Conf.Networking.PasswordRequired ? "true" : "false")
                                        };
                                        
                                        string name = Conf.Networking.IndexName;
                                        if (collisions > 0)
                                                name += String.Format (" ({0})", collisions);
                                        
                                        entry_group.AddService (name,
                                                                Zeroconf.PROTOCOL,
                                                                Zeroconf.DOMAIN,
                                                                Zeroconf.Port,
                                                                args);
                                        entry_group.Commit ();                               
                                }
                        } catch (ClientException e) {
                                if (e.ErrorCode == ErrorCode.Collision) {
                                        HandleCollision ();
                                        return;
                                } else {
                                        throw;
                                }
                        } catch (Exception e) {
                                Logger.Log.Error ("Failed to publish\nException {0}\n{1}",
                                                  e.Message,
                                                  e.StackTrace);
                                // FIXME: Shutdown or unpublish
                        }
                }

                public void UnPublish ()
                {
                        if (entry_group == null)
                                return;
                        
                        entry_group.Reset ();
                        entry_group.Dispose ();
                        entry_group = null;
                }

                public void Update ()
                {
                        //entry_group.UpdateService (Conf.Networking.IndexName,
                        //                           Zeroconf.PROTOCOL,
                        //                           Zeroconf.DOMAIN);
                        // Do this to get around a bug in avahi-sharp ?
                        entry_group.Dispose ();
                        entry_group = null;
                        Publish ();
                }

                private void HandleCollision ()
                {
                        Logger.Log.Info ("(MDNS) IndexName collision.");
                        UnPublish ();
                        collisions++;
                        Publish ();       
                }
                
                private void OnEntryGroupStateChanged (object sender, EntryGroupStateArgs args)
                {
                        Logger.Log.Debug ("Zeroconf Publisher state changed: ({0})", args.State);
                        if (args.State == EntryGroupState.Collision) {
                                HandleCollision ();
                        }
                }

                public void Shutdown ()
                {
                        entry_group.Reset ();
                        entry_group.Dispose ();
                        //Logger.Log.Debug ("Zeroconf Publisher shutdown.");
                }
        }
}
