using System;
using System.Collections;
using System.Collections.Generic;

using NDesk.DBus;
using org.freedesktop.DBus;

namespace Beagle {

	[Interface ("org.freedesktop.Beagle.StatusManager")]
	public interface IStatusManager {

		void Shutdown ();
		void OptimizeIndexes ();
		void ReloadConfiguration ();
		DaemonStatus GetDaemonStatus ();
	}
}