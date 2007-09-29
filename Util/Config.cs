using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Beagle.Util {

	/**
	 * Manages the configuration files for beagle.
	 * Each configuration file is defined by the specific class that needs it and is stored in
	 * an XML format. The file is a list of Options, where each Option is a BoolOption, a StringOption
	 * or a ListOption.
	 *
	 * Most of beagle components are supposed to read values from the files. The easiest way to do that
	 * is:
	 * Config config = ConfigManager.Get (<name of config>);
	 * - To get a bool or a string option:
	 * bool/string opt_val = ConfigManager.GetOption (config, <name of option>, default value);
	 * - To get the parameters of a list option:
	 * string[] params = ConfigManager.GetListOptionParams (config, <name of option>);
	 * - To get the list of values for a list option:
	 * List<string[]> values = ConfigManager.GetListOptionValues (config, <name of option>);
	 *
	 * Both params and values can be null if the option is not found in the files.
	 * Some standard config names and option names are listed in ConfigManager.Names to avoid ambiguity.
	 * If ConfigManager.WatchForUpdates() is called, subsequent ConfigManager.Get returns the
	 * latest copy of the configuration and also caches the copy for future
	 * use. It uses inotify to refresh the cached copy if the config file is modified.
	 *
	 * Classes can also listen to changes by saying,
	 * ConfigManager.WatchForUpdates ();
	 * and then subscribing to a particular config for changes,
	 * ConfigManager.Subscribe (<name of config>, ConfigUpdateHandler);
	 *
	 * Of course, if classes do not need such sophisticated behaviour, it can just call
	 * Config config = ConfigManager.Load (<name of config>);
	 * This will return the respective config (can be null if not present) but will not monitor the
	 * config file for changes.
	 *
	 * To save the config, call ConfigManager.Save (config);
	 */
	public static class ConfigManager {

		// A list of names to eliminate typos
		// Every name need not be here, this is just for convenience
		public static class Names {

			internal const int NumConfig = 4;

			// DO NOT change these 4 names - the names should be same as Config.Name and file names
			public const string FilesQueryableConfig = "FilesQueryable";
			public const string BeagleSearchConfig = "BeagleSearch";
			public const string DaemonConfig = "Daemon";
			public const string NetworkingConfig = "Networking";

			// Dont change these names either, otherwise old option values cant be read

			// Options for FilesQueryableConfig
			// boolean
			public const string IndexHomeDir = "IndexHomeDir"; // default true
			// list (1 param)
			public const string Roots = "Roots";
			public const string ExcludeSubdirectory = "ExcludeSubdirectory";
			public const string ExcludePattern = "ExcludePattern";

			// Options for SearchingConfig
			// boolean
			public const string KeyBinding_Ctrl = "KeyBinding_Ctrl"; // default false
			public const string KeyBinding_Alt = "KeyBinding_Alt"; // default false
			public const bool BeagleSearchAutoSearch = "BeagleSearchAutoSearch"; // default true
			// string
			public const string KeyBinding_Key = "KeyBinding_Key"; // default F12
			public const string BeaglePosX = "BeaglePosX";
			public const string BeaglePosY = "BeaglePosY";
			public const string BeagleSearchWidth = "BeagleSearchWidth";
			public const string BeagleSearchHeight = "BeagleSearchHeight";
			// list (1 param)
			public const string SearchHistory = "SearchHistory";

			// Options for DaemonConfig
			// bool
			public const string AllowStaticBackend = "AllowStaticBackend"; // default true
			public const string IndexSynchronization = "IndexSynchronization"; // default true
			public const string AllowRoot = "AllowRoot"; // default false
			public const string IndexOnBattery = "IndexOnBattery"; // default false
			public const string IndexFasterOnScreensaver = "IndexFasterOnScreensaver"; // default true
			// list (1 param)
			public const string StaticQueryables = "StaticQueryables";
			public const string DeniedBackends = "DeniedBackends";
			public const string Maildirs = "Maildirs";
			public const string ExcludeMailfolder = "ExcludeMailfolder";

			// Options for NetworkingConfig
			// bool
			public const string ServiceEnabled = "ServiceEnabled";
			public const string PasswordRequired = "PasswordRequired";
			// string
			public const string ServiceName = "ServiceName";
			public const string ServicePassword = "ServicePassword";
			// list (4 params)
			public const string NetworkServices = "NetworkServices";
		}

		private static string configs_dir;
		private static Hashtable configs;
		private static Hashtable mtimes;
		private static Hashtable subscriptions;

		private static bool watching_for_updates = false;
		private static bool update_watch_present = false;

		public delegate void ConfigUpdateHandler (Config config);

		static ConfigManager ()
		{
			configs = new Hashtable (Names.NumConfig);
			mtimes = new Hashtable (Names.NumConfig);
			subscriptions = new Hashtable (Names.NumConfig);

			configs_dir = Path.Combine (PathFinder.StorageDir, "config");
		}

		public static void WatchForUpdates ()
		{
			// Make sure we don't try and watch for updates more than once
			if (update_watch_present)
				return;

			if (Inotify.Enabled) {
				Inotify.Subscribe (configs_dir, OnInotifyEvent, Inotify.EventType.Create | Inotify.EventType.CloseWrite);
			} else {
				// Poll for updates every 60 secs
				GLib.Timeout.Add (60000, new GLib.TimeoutHandler (CheckForUpdates));
			}

			update_watch_present = true;
			watching_for_updates = true;
		}

		private static void OnInotifyEvent (Inotify.Watch watch, string path, string subitem, string srcpath, Inotify.EventType type)
		{
			if (subitem == "" || watching_for_updates == false)
				return;

			Reload ();
		}

		private static bool CheckForUpdates ()
		{
			if (watching_for_updates)
				Reload ();
			return true;
		}

		public static void Subscribe (string name, ConfigUpdateHandler callback)
		{
			if (! update_watch_present)
				WatchForUpdates ();

			if (!subscriptions.ContainsKey (name))
				subscriptions.Add (name, new ArrayList (1));

			ArrayList callbacks = (ArrayList) subscriptions [name];
			callbacks.Add (callback);
		}

		private static void NotifySubscribers (Config config, string name)
		{
			ArrayList callbacks = (ArrayList) subscriptions [name];

			if (callbacks == null)
				return;

			foreach (ConfigUpdateHandler callback in callbacks)
				callback (config);
		}

		// Convenience routine to reload only the subscribed-to configs
		// Apps/Filters/Backends can always load config of their own
		public static void Reload ()
		{
			Config config;

			foreach (string name in subscriptions.Keys) {
				string filename = name + ".xml";
				string filepath = Path.Combine (configs_dir, filename);

				if (! File.Exists (filepath))
					continue;

				// If current_config is loaded and not modified, skip this one
				if (mtimes.ContainsKey (name) &&
				    File.GetLastWriteTimeUtc (filepath).CompareTo ((DateTime) mtimes [name]) <= 0)
					continue;

				config = Load (name);
				NotifySubscribers (config, name);
				mtimes [name] = DateTime.UtcNow;
				configs [name] = config;
			}
		}

		// Returns the config if present or a default config otherwise
		// Caches the config between subsequent calls and refreshes cache if config file is modified
		// Use this instead of Load() if you do not want to read the actual file everytime you fetch
		// the config.
		public static Config Get (string name)
		{
			Config config = (Config) configs [name];
			if (config != null)
				return config;

			config = Load (name);
			if (config != null) {
				configs [name] = config;
				mtimes [name] = DateTime.UtcNow;
				return config;
			}

			return Config.LoadNew (name);
		}

		private static string global_dir = Path.Combine (
						Path.Combine (ExternalStringsHack.SysConfDir, "beagle"),
						"config-files");

		// This is the core function to load, merge and return a config based on the local
		// and the global config file.
		// This should never return null since at least the global config file should be present
		// Still, its never hurts to check
		public static Config Load (string name)
		{
			string filename = (name + ".xml");

			string global_file = Path.Combine (global_dir, filename);
			string local_file = Path.Combine (configs_dir, filename);

			Config merge_from = LoadFrom (local_file);
			Config merge_to = LoadFrom (global_file);

			if (merge_to == null)
				return merge_from;

			foreach (Option option in merge_to.Options.Values)
				option.Global = true;

			if (merge_from == null)
				return merge_to;

			foreach (Option option in merge_from.Options.Values) {
				option.Global = false;
				merge_to.Options [option.Name] = option;
			}

			return merge_to;
		}

		private static XmlSerializer conf_ser = new XmlSerializer (typeof (Config));

		private static Config LoadFrom (string path)
		{
			if (! File.Exists (path))
				return null;

			Config config = null;

			using (StreamReader reader = new StreamReader (path)) {
				config = (Config) conf_ser.Deserialize (reader);
				Console.WriteLine ("Done reading conf from " + path);
			}

			return config;
		}

		public static void Save (Config config)
		{
			if (config == null)
				return;

			bool to_save = false;
			foreach (Option option in config.Options.Values)
				if (! option.Global)
					to_save = true;

			if (! to_save)
				return;

			bool watching_for_updates_current = watching_for_updates;
			watching_for_updates = false;

			string filename = (config.Name + ".xml");
			string configs_dir = Path.Combine (PathFinder.StorageDir, "config");
			if (!Directory.Exists (configs_dir))
				Directory.CreateDirectory (configs_dir);

			string local_file = Path.Combine (configs_dir, filename);

			using (StreamWriter writer = new StreamWriter (local_file)) {
				conf_ser.Serialize (writer, config);
				Console.WriteLine ("Done writing to " + local_file);
			}

			watching_for_updates = watching_for_updates_current;
		}

		///////////// Utility Methods : Use Them /////////////

		// FIXME: Use generics to reduce the next 4 methods to 2

		public static bool GetOption (Config config, string name, bool default_value)
		{
			if (config == null)
				throw new ArgumentException ("Null config", "config");

			BoolOption option = config [name] as BoolOption;
			if (option == null)
				return default_value;

			return option.Value;
		}

		public static bool SetOption (Config config, string name, bool value)
		{
			if (config == null)
				throw new ArgumentException ("Null config", "config");

			BoolOption option = config [name] as BoolOption;
			if (option == null)
				return false;

			option.Value = value;
			return true;
		}

		public static string GetOption (Config config, string name, string default_value)
		{
			if (config == null)
				throw new ArgumentException ("Null config", "config");

			StringOption option = config [name] as StringOption;
			if (option == null)
				return default_value;

			return option.Value;
		}

		public static bool SetOption (Config config, string name, string value)
		{
			if (config == null)
				throw new ArgumentException ("Null config", "config");

			StringOption option = config [name] as StringOption;
			if (option == null)
				return false;

			option.Value = value;
			return true;
		}

		public static string[] GetListOptionParams (Config config, string name)
		{
			if (config == null)
				throw new ArgumentException ("Null config", "config");

			ListOption option = config [name] as ListOption;
			if (option == null)
				return null;

			return option.ParamNames;
		}

		/*
		public static Option ListOptionCopy (Config config, Option opt)
		{
			return ListOptionCopy (config, opt.Name);
		}

		public static Option ListOptionCopy (Config config, string name)
		{
			ListOption option = config [name] as ListOption;
			if (option == null)
				return null;

			ListOption new_option = new ListOption (option);
			config.Options [name] = new_option;

			return new_option;
		}

		public static Option ListOptionNew (Config config, string name, string description, char separator, params string[] param_names)
		{
			ListOption option = new ListOption ();
			option.Name = name;
			option.Description = description;
			option.Separator = separator;
			option.Parameter_String = String.Join (separator.ToString (), param_names);
			option.Global = false;

			config.Options [name] = option;

			return option;
		}
		*/

		public static List<string[]> GetListOptionValues (Config config, string name)
		{
			if (config == null)
				throw new ArgumentException ("Null config", "config");

			ListOption option = config [name] as ListOption;
			if (option == null)
				return null;

			return option.Values;
		}

		public static bool SetListOptionValues (Config config, string name, List<string[]> values)
		{
			if (config == null)
				throw new ArgumentException ("Null config", "config");

			ListOption option = config [name] as ListOption;
			if (option == null)
				return false;

			option.Values = values;
			return true;
		}

		public static bool AddListOptionValue (Config config, string name, string[] values)
		{
			if (config == null)
				throw new ArgumentException ("Null config", "config");

			ListOption option = config [name] as ListOption;
			if (option == null)
				return false;

			int num_params = option.NumParams;
			// verify the number of values
			if (values == null || values.Length != num_params)
				throw new ArgumentException (String.Format ("Must be an array of {0} strings", num_params), "values");

			Array.Resize (ref option.Values_String, option.Values_String.Length + 1);
			option.Values_String [option.Values_String.Length - 1] = String.Join (option.Separator.ToString (), values);

			option.Global = false;
			return true;
		}

		public static bool RemoveListOptionValue (Config config, string name, string[] values)
		{
			if (config == null)
				throw new ArgumentException ("Null config", "config");

			ListOption option = config [name] as ListOption;
			if (option == null)
				return false;

			int num_params = option.NumParams;
			// verify the number of values
			if (values == null || values.Length != num_params)
				throw new ArgumentException (String.Format ("Must be an array of {0} strings", num_params), "values");

			string value = String.Join (option.Separator.ToString (), values);

			bool found = false;
			for (int i = 0; i < option.Values_String.Length; ++i) {
				if (found) {
					// FIXME: Assuming no duplicates
					option.Values_String [i-1] = option.Values_String [i];
					continue;
				}

				if (option.Values_String [i] == value)
					found = true;
			}

			if (found) {
				Array.Resize (ref option.Values_String, option.Values_String.Length - 1);
				option.Global = false;
			}

			return found;
		}
	}

	[XmlRoot ("BeagleConf")]
	public class Config {
		[XmlAttribute]
		public string Name = String.Empty;

		internal static Config LoadNew (string name)
		{
			Config config = new Config ();
			config.Name = name;
			return config;
		}

		private Hashtable options = new Hashtable ();

		[XmlIgnore]
		public Hashtable Options {
			get { return options; }
		}

		/* Exposed only for serialization. Do not use. */
		public class HashtableEnumerator : IEnumerable {
			public Hashtable options;

			public HashtableEnumerator (Hashtable options)
			{
				this.options = options;
			}

			public void Add (object o)
			{
				Option option = (Option) o;
				options [option.Name] = option;
			}

			public IEnumerator GetEnumerator ()
			{
				ArrayList local_options = new ArrayList (options.Count);
				foreach (Option option in options.Values)
					if (! option.Global)
						local_options.Add (option);

				return local_options.GetEnumerator ();
			}
		}

		[XmlArrayItem (ElementName="BoolOption", Type=typeof (BoolOption))]
		[XmlArrayItem (ElementName="StringOption", Type=typeof (StringOption))]
		[XmlArrayItem (ElementName="ListOption", Type=typeof (ListOption))]
		[XmlElement (ElementName="BoolOption", Type=typeof (BoolOption))]
		[XmlElement (ElementName="StringOption", Type=typeof (StringOption))]
		[XmlElement (ElementName="ListOption", Type=typeof (ListOption))]
		/* Exposed only for serialization. Do not use. */
		public HashtableEnumerator options_enumerator {
			get { return new HashtableEnumerator (options); }
			set { options = value.options; }
		}

		public Option this [string option_name] {
			get { return (Option) options [option_name]; }
		}
	}

	public enum OptionType {
		Bool = 0,
		String = 1,
		List = 2
	};

	public class Option {
		[XmlAttribute]
		public string Name = String.Empty;

		[XmlAttribute]
		public string Description = String.Empty;

		/* When saving, only the non-global (aka local) options are written to the disk */
		[XmlIgnore]
		internal bool Global = false;

		[XmlIgnore]
		public OptionType Type = OptionType.Bool;
	}

	//////////////////////////////////////////////////////////////////////////////////////////
	/* The classes below are exposed only for serialization. Use responsibly or do not use. */
	//////////////////////////////////////////////////////////////////////////////////////////

	public class BoolOption : Option {
		[XmlText]
		public string Value_String = String.Empty;

		[XmlIgnore]
		public bool Value {
			get {
				if (Value_String == String.Empty)
					return true; // default value
				return Convert.ToBoolean (Value_String);
			}
			set {
				Value_String = value.ToString ();
				Global = false;
			}
		}

		public BoolOption () : base ()
		{
			Type = OptionType.Bool;
		}
	}

	public class StringOption : Option {
		[XmlText]
		public string Value_String = String.Empty;

		[XmlIgnore]
		public string Value {
			get {
				if (Value_String == String.Empty)
					return null;
				return Value_String;
			}
			set {
				Value_String = value;
				Global = false;
			}
		}

		public StringOption () : base ()
		{
			Type = OptionType.String;
		}
	}

	public class ListOption : Option {

		[XmlIgnore]
		public char Separator = ',';

		[XmlAttribute (AttributeName = "Separator")]
		public string Separator_String {
			get { return Separator.ToString (); }
			set { if (value != null) Separator = value [0]; }
		}

		[XmlAttribute (AttributeName = "Params")]
		// Separated by "Separator"
		public string Parameter_String = String.Empty;

		public ListOption () : base ()
		{
			Type = OptionType.List;
		}

		public ListOption (ListOption old_option)
		{
			this.Name = old_option.Name;
			this.Description = old_option.Description;
			this.Values_String = old_option.Values_String;
			this.Type = OptionType.List;
			this.Global = false;
		}

		[XmlElement (ElementName = "Value", Type = typeof (string))]
		[XmlArrayItem (ElementName = "Value", Type = typeof (string))]
		// Each value is separated by "Separator"
		public string[] Values_String = new string [0];

		[XmlIgnore]
		public string[] ParamNames {
			get { return Parameter_String.Split (new char [] {Separator}); }
		}

		[XmlIgnore]
		public int NumParams {
			get { return ParamNames.Length; }
		}

		[XmlIgnore]
		public List<string[]> Values {
			get {
				if (Values_String == null)
					return null;

				List<string[]> list = new List<string[]> (Values_String.Length);
				int num_params = NumParams;

				foreach (string value in Values_String) {
					// Skip the bad values
					string[] values = value.Split (new char [] {Separator});	
					if (values == null || values.Length != num_params)
						continue;

					list.Add (values);
				}

				return list;
			}
			set {
				int num_params = NumParams;

				string[] values_string = new string[value.Count];

				// Verify that each string[] has num_params values
				for (int i = 0; i < value.Count; ++ i)  {
					string[] list_value = value [i];
					if (list_value == null || list_value.Length != num_params)
						throw new ArgumentException (String.Format ("Each list entry must be arrays of {0} strings", num_params), "values");
					values_string [i] = String.Join (Separator.ToString (), list_value);
				}

				Values_String = values_string;

				Global = false;
			}
		}
	}
}
