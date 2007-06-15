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
		
		private PropertyValue (string str)
		{
			String = str;
			Bool = false;
			Integer = 0;
			Type = PropertyValueType.String;
		}
		
		private PropertyValue (bool predicate)
		{
			String = string.Empty;
			Bool = predicate;
			Integer = 0;
			Type = PropertyValueType.Boolean;
		}
		
		private PropertyValue (int integer)
		{
			String = string.Empty;
			Bool = false;
			Integer = integer;
			Type = PropertyValueType.Integer;
		}

		public static PropertyValue New (string str)
		{
			return new PropertyValue (str);
		}

		public static PropertyValue New (bool predicate)
		{
			return new PropertyValue (predicate);
		}

		public static PropertyValue New (int integer)
		{
			return new PropertyValue (integer);
		}
	}

	public class PropertyStore : Dictionary<string, PropertyValue> {
		
		public PropertyStore ()
		{
		}
	}
}
