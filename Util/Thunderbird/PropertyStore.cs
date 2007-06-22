//
// PropertyStore.cs: A key-value based system with string, integer or bool as key type
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
using System.Collections.Generic;

namespace Beagle.Util.Thunderbird {

	public enum PropertyValueType { 
		String, 
		Boolean, 
		Integer
	}
	
	public struct PropertyValue {		
		public readonly string String;
		public readonly bool Bool;
		public readonly int Integer;
		public readonly PropertyValueType Type;
		private readonly bool IsNull;
		
		public static readonly PropertyValue Null = 
			new PropertyValue (null, false, 0, PropertyValueType.String, true);
		
		private PropertyValue (string str, bool pred, 
				int integer, PropertyValueType type, bool is_null)
		{
			String = str;
			Bool = pred;
			Integer = integer;
			Type = type;
			IsNull = is_null;
		}

		public static PropertyValue New (string str)
		{
			return new PropertyValue (str, false, 0, PropertyValueType.String, false);
		}

		public static PropertyValue New (bool predicate)
		{
			return new PropertyValue (null, predicate, 0, PropertyValueType.Boolean, false);
		}

		public static PropertyValue New (int integer)
		{
			return new PropertyValue (null, false, integer, PropertyValueType.Integer, false);
		}
		
		public override string ToString ()
		{
			switch (Type) {
			case PropertyValueType.String:
				return String;
			case PropertyValueType.Integer:
				return Convert.ToString (Integer);
			case PropertyValueType.Boolean:
				return Convert.ToString (Bool);
			}
			
			return null;
		}
	}

	public class PropertyStore : Dictionary<string, PropertyValue> {
		
		public PropertyStore ()
		{
		}
		
		private PropertyValue Get (string key)
		{
			PropertyValue property = PropertyValue.Null;
			
			if (TryGetValue (key, out property))
				return property;
			
			return PropertyValue.Null;
		}
		
		public void Set (string key, string val)
		{
			PropertyValue pv = PropertyValue.New (val);
			
			if (ContainsKey (key)) {
				this [key] = pv;
			} else {
				Add (key, pv);
			}
		}
		
		public void Set (string key, int val)
		{
			PropertyValue pv = PropertyValue.New (val);
			
			if (ContainsKey (key)) {
				this [key] = pv;
			} else {
				Add (key, pv);
			}
		}
		
		public void Set (string key, bool val)
		{
			PropertyValue pv = PropertyValue.New (val);
			
			if (ContainsKey (key)) {
				this [key] = pv;
			} else {
				Add (key, pv);
			}
		}
		
		public string GetString (string key)
		{
			PropertyValue val = Get (key);
			
			if (!val.Type.Equals (PropertyValueType.String))
				throw new InvalidCastException ("invalid cast");
			
			return val.String;
		}
		
		public int GetInt (string key)
		{
			PropertyValue val = Get (key);
			
			if (!val.Type.Equals (PropertyValueType.Integer))
				throw new InvalidCastException ("invalid cast");
			
			return val.Integer;
		}
		
		public bool GetBoolean (string key)
		{
			PropertyValue val = Get (key);
			
			if (!val.Type.Equals (PropertyValueType.Boolean))
				throw new InvalidCastException ("invalid cast");
			
			return val.Bool;
		}
	}
}
