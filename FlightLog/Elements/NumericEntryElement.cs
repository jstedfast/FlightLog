// 
// NumericEntryElement.cs
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
using System.Text;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class NumericEntryElement : LimitedEntryElement
	{
		static readonly NSString NumericEntryElementCellKey = new NSString ("NumericEntryElement");

		public NumericEntryElement (string caption, string placeholder) : base (caption, placeholder, 10)
		{
			KeyboardType = UIKeyboardType.NumberPad;
			base.Value = string.Empty;
			MaxValue = Int32.MaxValue;
			MinValue = 0;
		}
		
		public NumericEntryElement (string caption, string placeholder, int value) : base (caption, placeholder, 10)
		{
			KeyboardType = UIKeyboardType.NumberPad;
			MaxValue = Int32.MaxValue;
			MinValue = 0;
			
			Value = value;
		}
		
		public NumericEntryElement (string caption, string placeholder, int value, int minValue, int maxValue) : base (caption, placeholder, 10)
		{
			KeyboardType = UIKeyboardType.NumberPad;
			MaxValue = maxValue;
			MinValue = minValue;
			Value = value;
		}

		protected override NSString CellKey {
			get { return NumericEntryElementCellKey; }
		}
		
		public new int Value {
			set {
				if (value > MinValue & value < MaxValue)
					base.Value = value.ToString ();
				else
					base.Value = string.Empty;
			}
			
			get {
				string str = base.Value;
				int value;
				
				if (str == null || str.Length == 0)
					return 0;
				
				if (Int32.TryParse (str, out value)) {
					if (value < MinValue)
						return MinValue;
					
					if (value > MaxValue)
						return MaxValue;
					
					return value;
				}
				
				return MinValue;
			}
		}
		
		public int MaxValue {
			get; set;
		}
		
		public int MinValue {
			get; set;
		}
		
		protected override bool AllowTextChange (string currentText, NSRange changedRange, string replacementText, string result)
		{
			int value;
			
			if (result.Length > MaxLength)
				return false;
			
			if (result.Length == 0)
				return true;
			
			// Validate that the replacement characters are all numeric
			for (int i = 0; i < replacementText.Length; i++) {
				if (replacementText[i] < '0' || replacementText[i] > '9')
					return false;
			}
			
			if (!Int32.TryParse (result, out value))
				return false;
			
			if (value < MinValue || value > MaxValue)
				return false;
			
			return true;
		}
	}
}
