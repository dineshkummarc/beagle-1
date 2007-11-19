using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using NDesk.DBus;
using org.freedesktop.DBus;

using Beagle;
using Beagle.Util;

namespace Beagle.Daemon {

	class QueryManager : IQueryManager {

		public void Send (Query query)
		{
			Log.Debug ("Yipeeeee dbus works we rock!!!");
		}
	}

	class StatusManager : IStatusManager {
		
		public void Shutdown ()
		{
			//ExceptionHandlingThread.Start (new ThreadStart (delegate {
			Beagle.Daemon.Shutdown.BeginShutdown ();
			//}));
		}

		public void OptimizeIndexes ()
		{
			Log.Debug ("1");
			LuceneQueryable.OptimizeAll ();
		}

		public void ReloadConfiguration ()
		{
			Log.Debug ("2");
			Conf.Reload ();
		}

		public DaemonStatus GetDaemonStatus ()
		{
			return new DaemonStatus ();
		}
	}

	public class DBusServer {

		private const string bus_name = "org.freedesktop.Beagle";
		private const string object_root = "/org/freedesktop/Beagle";
		
		public static void Init ()
		{
			Log.Debug ("Starting D-Bus session server...");
			
			BusG.Init ();

			try {
				Bus.Session.RequestName (bus_name);
			} catch (Exception e) {
				Log.Error (e, "Could not connect to D-Bus");
			}

			RegisterObject (new QueryManager (), "QueryManager");
			RegisterObject (new StatusManager (), "StatusManager");
		}
		
		private static void RegisterObject (object o, string name)
		{
			ObjectPath path = new ObjectPath (object_root + "/" + name);
			
			if (Bus.Session != null) {
				Bus.Session.Register (bus_name, path, o);
			}

			Log.Debug ("Registered '{0}' on '{1}'", o, path);
		}
	}
}