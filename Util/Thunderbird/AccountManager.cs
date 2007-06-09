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
	
		public MessageIdentity (string key)
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
		
		public string IdentityName { 
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public string FullName { 
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		public string Email { 
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
	}

	public class IncomingServer : PropertyStore {
	
		public IncomingServer (string key)
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
		
		public string PrettyName { 
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public string HostName { 
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public string RealHostName { 
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public long Port { 
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public string Username { 
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public string RealUserName { 
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public string Type { 
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
	}
	
	public class Account {
		
		public Account()
		{
			throw new NotImplementedException ();
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
