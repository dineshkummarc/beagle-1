using System;
using System.Collections;
using System.Collections.Generic;

using NDesk.DBus;
using org.freedesktop.DBus;

namespace Beagle {

	[Interface ("org.freedesktop.Beagle.QueryManager")]
	public interface IQueryManager {

		void Send (Query query);
	}
}