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
using System.Collections;
using System.Collections.Generic;

namespace Beagle.Util.Thunderbird {
	
	public class MessageIdentity : PropertyStore {
		private string key;
	
		public MessageIdentity (string key)
		{
			this.key = key;
		}
		
		public string Key { 
			get {
				return key;
			}
			set {
				key = value;
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
		private string key;
	
		public IncomingServer (string key)
		{
			this.key = key;
		}
		
		public string Key { 
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
		private List<MessageIdentity> identities;
		
		public Account()
		{
			this.identities = new List<MessageIdentity> ();
		}

		public void Add (MessageIdentity id)
		{
			throw new NotImplementedException ();
		}
		
		public void Remove (MessageIdentity id)
		{
			throw new NotImplementedException ();
		}
		
		public void Clear ()
		{
			throw new NotImplementedException ();
		}
		
		public string Key { 
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		public IncomingServer Server { 
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		public MessageIdentity Default { 
			get {
				throw new NotImplementedException ();
			}
		}
		public MessageIdentity[] Identities { 
			get {
				throw new NotImplementedException ();
			}
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
	
	public class AccountManager : IManager<Account, AccountEventArgs> {
		
		public AccountManager ()
		{
			throw new NotImplementedException ();
		}
		
		public void Load ()
		{
			throw new NotImplementedException ();
		}
		
		public void Unload ()
		{
			throw new NotImplementedException ();
		}
		
		public void Add (Account account)
		{
			throw new NotImplementedException ();
		}
		
		public bool Remove (Account account)
		{
			throw new NotImplementedException ();
		}
		
		public bool Contains (Account account)
		{
			throw new NotImplementedException ();
		}
		
		public void Clear ()
		{
			throw new NotImplementedException ();
		}
		
		public void CopyTo (Account[] accounts, int index)
		{
			throw new NotImplementedException ();
		}
		
		public IEnumerator<Account> GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		
		/* public */ IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
		
		public bool Loaded {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public int Count {
			get {
				throw new NotImplementedException ();
			}
		}
		
		public bool IsReadOnly {
			get {
				throw new NotImplementedException ();
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
		
		protected virtual void OnChanged (AccountEventArgs args)
		{
			if (Changed != null)
				Changed (this, args);
		}
		
		public event EventHandler<AccountEventArgs> Added;
		public event EventHandler<AccountEventArgs> Removed;
		public event EventHandler<AccountEventArgs> Changed;
	}
}
