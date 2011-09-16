// 
// EnumExtension.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2011 Jeffrey Stedfast
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 

using System;
using System.Reflection;

namespace FlightLog {
	[AttributeUsage (AttributeTargets.Field)]
	public class HumanReadableNameAttribute : Attribute {
		public HumanReadableNameAttribute (string name)
		{
			HumanReadableName = name;
		}
		
		public string HumanReadableName {
			get; protected set;
		}
	}
	
	public static class EnumExtension
	{
		public static string ToHumanReadableName (this Enum value)
		{
			string name = value.ToString ();
			Type type = value.GetType ();
			FieldInfo field = type.GetField (name);
			
			object[] attrs = field.GetCustomAttributes (typeof (HumanReadableNameAttribute), true);
			if (attrs == null || attrs.Length == 0)
				return name;
			
			return ((HumanReadableNameAttribute) attrs[0]).HumanReadableName;
		}
	}
}
