//
// AccountManager.cs: All account types and operations are defined here
//
// Copyright (C) 2007 Pierre Östlund
//

//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Beagle.Util.Thunderbird.Preferences;

namespace Beagle.Util.Thunderbird {
	
	public class MessageIdentity : PropertyStore {
		private int id;
	
		public MessageIdentity (int id)
		{
			this.id = id;
		}
		
		public int Id { 
			get {
				return id;
			}
			set {
				id = value;
			}
		}
		
		public string IdentityName { 
			get {
				try {
					GetString ("identityName");
				} catch {
				}
				
				return string.Empty;
			}
			set {
				Set ("identityName", value);
			}
		}
		
		public string FullName { 
			get {
				try {
					GetString ("fullName");
				} catch {
				}
				
				return string.Empty;
			}
			set {
				Set ("fullName", value);
			}
		}
		public string Email { 
			get {
				try {
					GetString ("useremail");
				} catch {
				}
				
				return string.Empty;
			}
			set {
				Set ("useremail", value);
			}
		}
	}

	public class IncomingServer : PropertyStore {
		private int key;
	
		public IncomingServer (int key)
		{
			this.key = key;
		}
		
		public int Key { 
			get {
				return key;
			}
			set {
				key = value;
			}
		}
		
		public string PrettyName { 
			get {
				try {
					return GetString ("prettyName");
				} catch {
				}
				
				return string.Empty;
			}
			set {
				Set ("prettyName", value);
			}
		}
		
		public string HostName { 
			get {
				try {
					return GetString ("hostname");
				} catch {
				}
				
				return string.Empty;
			}
			set {
				Set ("hostname", value);
			}
		}
		
		public string RealHostName { 
			get {
				try {
					return GetString ("realHostName");
				} catch {
				}
				
				return string.Empty;
			}
			set {
				Set ("realHostName", value);
			}
		}
		
		public int Port { 
			get {
				try {
					return GetInt ("port");
				} catch {
				}
				
				return -1;
			}
			set {
				Set ("port", value);
			}
		}
		
		public string Username { 
			get {
				try {
					return GetString ("username");
				} catch {
				}
				
				return string.Empty;
			}
			set {
				Set ("username", value);
			}
		}
		
		public string RealUserName { 
			get {
				try {
					return GetString ("realUsername");
				} catch {
				}
				
				return string.Empty;
			}
			set {
				Set ("realUsername", value);
			}
		}
		
		public string Type { 
			get {
				try {
					return GetString ("type");
				} catch {
				}
				
				return string.Empty;
			}
			set {
				Set ("type", value);
			}
		}
	}
	
	public class Account {
		private int key;
		private List<MessageIdentity> identities;
		private IncomingServer incoming_server = null;
		
		public Account(int key)
		{
			this.key = key;
			this.identities = new List<MessageIdentity> ();
		}

		public void Add (MessageIdentity id)
		{
			if (id == null)
				throw new ArgumentNullException ("id");
				
			// We don't add duplicates
			foreach (MessageIdentity ident in identities) {
				if (ident.Id == id.Id)
					return;
			}
			
			// Add identity
			identities.Add (id);
		}
		
		public void Remove (MessageIdentity id)
		{
			if (id == null)
				throw new ArgumentNullException ("id");
			
			// Remove identity if it exists
			foreach (MessageIdentity ident in identities) {
				if (ident.Id == id.Id) {
					identities.Remove (ident);
					return;
				}
			}
		}
		
		public void Clear ()
		{
			identities.Clear ();
			incoming_server = null;
		}
		
		public void SetNewKey (int key)
		{
			this.key = key;
			
			if (incoming_server != null)
				incoming_server.Key = key;
		}
		
		public override string ToString ()
		{
			int count = 0;
			StringBuilder builder = new StringBuilder ();
			
			foreach (MessageIdentity ident in identities) {
				if (count++ != identities.Count-1)
					builder.AppendLine (ident.ToString ());
				else
					builder.Append (ident.ToString ());
			}
			
			return String.Format ("Account: {0}\nIncoming server:\n{1}\nIdentities:\n{2}", 
				key, incoming_server, builder.ToString ());
		}
		
		public int Key { 
			get {
				return key;
			}
			set {
				Clear ();
				key = value;
			}
		}
		
		public IncomingServer Server {
			get {
				return incoming_server;
			}
			set {
				if (value.Key != this.key)
					throw new ArgumentException ("key mismatch");
				
				incoming_server = value;
			}
		}
		
		public MessageIdentity Default {
			get {
				if (identities.Count == 0)
					return null;
				
				return identities [0];
			}
		}
		
		public MessageIdentity[] Identities {
			get {
				return identities.ToArray ();
			}
		}
		
		public override int GetHashCode ()
		{
			// They key should always be unique within an instance, so it should be ok to use
			// it as as hash code
			return Key;
		}
		
		public override bool Equals (object o)
		{
			if (o == null || GetType () != o.GetType ())
				return false;
			
			Account acc = (Account) o;
			
			// Check some basic conditions
			if (acc.Key != Key || Identities.Length != acc.Identities.Length)
				return false;
			if ((acc.Server == null && Server != null) || (acc.Server != null && Server == null))
				return false;
			if (Identities.Length == 0 && acc.Identities.Length == 0)
				return true;
			
			// Make sure all identities match
			foreach (MessageIdentity ident in Identities) {
				bool found = false;
				foreach (MessageIdentity new_ident in acc.Identities) {	
					if (PropertyStore.Equals (ident, new_ident)) {
						found = true;
						break;
					}
				}
				
				if (!found)
					return false;
			}
			
			return true;
		}
	}
	
	public class AccountEventArgs : EventArgs {
		private Account account;
		
		public AccountEventArgs (Account account)
		{
			this.account = account;
		}
		
		public Account Account {
			get {
				return account;
			}
		}
	}
	
	public class AccountChangedEventArgs : AccountEventArgs {
		private Account old_account;
		
		public AccountChangedEventArgs (Account new_account, Account old_account) 
			: base (new_account)
		{
			this.old_account = old_account;
		}
		
		public Account OldAccount {
			get {
				return old_account;
			}
		}
	}
	
	public class AccountManager : IManager<Account, AccountEventArgs> {
		private string filename = null;
		private List<Account> accounts;
		private bool debug = false;
		
		public AccountManager (string filename)
		{
			debug = (Environment.GetEnvironmentVariable ("THUNDERBIRD_ACCOUNTMANAGER_DEBUG") != null);
			this.filename = filename;
		}
		
		public void Load ()
		{
			if (Loaded) {
				Reload ();
				return;
			}
			
			accounts = new List<Account> ();
			
			// Add accounts
			foreach (Account acc in GetAccounts (filename))
				Add (acc);
		}
		
		private List<Account> GetAccounts (string file)
		{
			string account_string = null;
			PreferenceStore store = new PreferenceStore ();
			new PreferenceParser (store, filename);
			List<Account> list = new List<Account> ();
			
			try {
				account_string = store.GetString ("mail.accountmanager.accounts");
			} catch (InvalidCastException) {
				throw new AccountException ("No accounts to load");
			}
			
			// Add all available accounts
			foreach (string account in account_string.Split (',')) {
				try {
					list.Add (BuildAccount (store, account));
				} catch (Exception e) {
					if (debug)
						Logger.Log.Debug (e, "Failed to load account");
				}
			}
			
			return list;
		}
		
		private Account BuildAccount (PreferenceStore store, string account_str)
		{
			Account acc = new Account (GetNumber (account_str, 7));
			acc.Server = BuildIncomingServer (store, acc.Key);
			
			string key_str = String.Format ("mail.account.account{0}.identities", acc.Key);
			try {
				// There might exist account without identities (the "local" account does not
				// have an identity by default for instance).
				foreach (string id_str in store.GetString (key_str).Split (',')) {
					acc.Add (BuildMessageIdentity (store, GetNumber (id_str, 2)));
				}
					
			} catch (Exception e) {
				if (debug)
					Logger.Log.Debug (e, "No identities or invalid identity found"); 
			}
			
			return acc;
		}
		
		private int GetNumber (string str, int preamble)
		{
			if (String.IsNullOrEmpty (str))
				throw new ArgumentNullException ("str");
			if (str.Length < preamble+1)
				throw new ArgumentNullException ("too short string");
			
			return System.Convert.ToInt32 (str.Substring (preamble));;
		}
		
		private IncomingServer BuildIncomingServer (PreferenceStore store, int key)
		{
			IncomingServer server = new IncomingServer (key);
			ExtractProperties (store, server, String.Format ("mail.server.server{0}.", key));
			return server;
		}
		
		private MessageIdentity BuildMessageIdentity (PreferenceStore store, int id)
		{
			MessageIdentity identity = new MessageIdentity (id);
			ExtractProperties (store, identity, String.Format ("mail.identity.id{0}.", id));
			return identity;
		}
		
		private void ExtractProperties (PreferenceStore in_store, 
							PropertyStore out_store, string base_key)
		{
			if (in_store == null)
				throw new ArgumentNullException ("in_store");
			if (out_store == null)
				throw new ArgumentNullException ("out_store");
			if (String.IsNullOrEmpty (base_key))
				throw new ArgumentNullException ("base_key");
				
			foreach (string key_val in in_store.Keys) {
				if (!key_val.StartsWith (base_key))
					continue;
				
				try {
					out_store.Add (key_val.Substring (base_key.Length), in_store [key_val]);
				} catch (Exception e) {
					if (debug)
						Logger.Log.Debug (e, "Failed to add properties for {0}", base_key);
				}
			}
		}
		
		public void Reload ()
		{
			CheckLoaded ();
			
			List<Account> new_accounts = GetAccounts (filename),
					removals = new List<Account> ();
			
			// Add new accounts and update old ones
			foreach (Account acc in new_accounts)
				Add (acc);
			
			// Create a list of accounts that doesn't exist anymore
			foreach (Account acc in accounts) {
				bool found = false;
				
				foreach (Account new_acc in new_accounts) {
					if (new_acc.Key == acc.Key) {
						found = true;
						break;
					}
				}
				
				if (!found)
					removals.Add (acc);
			}
			
			// Remove accounts that doesn't exist
			foreach (Account acc in removals)
				Remove (acc);
		}
		
		public void Unload ()
		{
			Clear ();
			accounts = null;
		}
		
		public void Add (Account account)
		{
			CheckLoaded ();
			if (account == null)
				throw new ArgumentNullException ("account");
			
			foreach (Account acc in accounts) {
				if (acc.Key != account.Key)
					continue;
				else if (Update (acc, account) || acc.Equals (account))
					return;
			}
			
			// Add new account
			accounts.Add (account);
			OnAdded (new AccountEventArgs (account));
		}
		
		private bool Update (Account old_account, Account new_account)
		{
			CheckLoaded ();
			if (old_account == null)
				throw new ArgumentNullException ("old_account");
			if (new_account == null)
				throw new ArgumentNullException ("new_account");
			if (old_account.Key != new_account.Key) 
				throw new ArgumentException ("key mismatch");
			
			// No need to update if they are the same object. We shouldn't update in case they
			// have different keys either (how could they ever be the same account?).
			if (old_account.Equals (new_account) || old_account.Key != new_account.Key)
				return false;
			
			// Update with new account
			accounts.Remove (old_account);
			accounts.Add (new_account);
			OnChanged (new AccountChangedEventArgs (new_account, old_account));
			return true;
		}
		
		public bool Remove (Account account)
		{
			CheckLoaded ();
			if (account == null)
				throw new ArgumentNullException ("account");
			
			foreach (Account acc in accounts) {
				if (acc.Equals (account)) {
					accounts.Remove (acc);
					OnRemoved (new AccountEventArgs (acc));
					return true;
				}
			}
			
			return false;
		}
		
		public bool Contains (Account account)
		{
			CheckLoaded ();
			return accounts.Contains (account);
		}
		
		public void Clear ()
		{
			CheckLoaded ();
			while (accounts.Count > 0)
				Remove (accounts [0]);
		}
		
		public void CopyTo (Account[] accounts, int index)
		{
			CheckLoaded ();
			this.accounts.CopyTo (accounts, index);
		}
		
		public IEnumerator<Account> GetEnumerator ()
		{
			CheckLoaded ();
			return accounts.GetEnumerator ();
		}
		
		/* public */ IEnumerator IEnumerable.GetEnumerator ()
		{
			CheckLoaded ();
			return accounts.GetEnumerator ();
		}
		
		private void CheckLoaded ()
		{
			if (!Loaded)
				throw new InvalidOperationException ("not loaded");
		}
		
		public bool Loaded {
			get {
				return (accounts != null);
			}
		}
		
		public int Count {
			get {
				return (Loaded ? accounts.Count : 0); 
			}
		}
		
		public bool IsReadOnly {
			get {
				return false;
			}
		}
		
		protected virtual void OnAdded (AccountEventArgs args)
		{
			if (Added != null)
				Added (this, args);
		}
		
		protected virtual void OnRemoved (AccountEventArgs args)
		{
			if (Removed != null)
				Removed (this, args);
		}
		
		protected virtual void OnChanged (AccountChangedEventArgs args)
		{
			if (Changed != null)
				Changed (this, args);
		}
		
		public event EventHandler<AccountEventArgs> Added;
		public event EventHandler<AccountEventArgs> Removed;
		public event EventHandler<AccountEventArgs> Changed;
	}
	
	public class AccountException : Exception {
	
		public AccountException (string message) : base (message) { }
	}
}
