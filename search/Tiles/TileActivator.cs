using System;
using System.Collections;
using System.Reflection;
using Mono.Posix;

namespace Search.Tiles {

	// FIXME: Should we call this something else?
	public abstract class TileActivator : IComparable {
		
		private ArrayList flavors;

		protected TileActivator ()
		{
			this.flavors = new ArrayList ();
		}

		public abstract Tile BuildTile (Beagle.Hit hit, Beagle.Query query);

		public int Weight = 0;

		protected void AddSupportedFlavor (HitFlavor flavor)
		{
			flavors.Add (flavor);
		}

		public virtual bool Validate (Beagle.Hit hit)
		{
			if (flavors.Count < 1)
				return false;

			Weight = 0;

			HitFlavor best = null;

			foreach (HitFlavor flavor in flavors) {
				if (! flavor.IsMatch (hit))
					continue;

				if (best == null) {
					best = flavor;
					continue;
				}

				if (flavor.Weight > best.Weight) {
					best = flavor;
				}
			}

			if (best != null) {
				Weight += best.Weight;
				return true;
			}

			return false;
		}

		public int CompareTo (object o)
		{
			TileActivator other = (TileActivator)o;

			if (other == null)
				return 1;

			return (this.Weight - other.Weight);
		}
	}

	// FIXME: Rename this and move out of this file
	public static class TileActivatorOrg {

		private static ArrayList activators;

		static TileActivatorOrg ()
		{
			activators = new ArrayList ();

			foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies ()) {
				
				foreach (Type type in assembly.GetTypes ()) {
					
					if (! type.IsSubclassOf (typeof (TileActivator)) || type.IsAbstract)
						continue;
					
					try {
						activators.Add ((TileActivator) Activator.CreateInstance (type));
					} catch (Exception e) {
						Console.WriteLine ("Caught exception while instantiating tile.");
					}
				}
			}
		}

		public static Tile MakeTile (Beagle.Hit hit, Beagle.Query query)
		{
			TileActivator best = null;

			foreach (TileActivator activator in activators) {
				if (! activator.Validate (hit))
					continue;

				if (best == null) {
					best = activator;
					continue;
				}

				if (activator.Weight > best.Weight)
					best = activator;
			}

			if (best != null)
				return best.BuildTile (hit, query);

			return null;
		}
	}
}