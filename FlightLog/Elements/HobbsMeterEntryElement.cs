// 
// HobbsMeterEntryElement.cs
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
	public class HobbsMeterEntryElement : LimitedEntryElement
	{
		public HobbsMeterEntryElement (string caption, string placeholder) : base (caption, placeholder, 4)
		{
			KeyboardType = UIKeyboardType.DecimalPad;
			base.Value = string.Empty;
		}
		
		public HobbsMeterEntryElement (string caption, string placeholder, int seconds) : base (caption, placeholder, 4)
		{
			KeyboardType = UIKeyboardType.DecimalPad;
			if (seconds <= 0)
				base.Value = string.Empty;
			else
				ValueAsSeconds = seconds;
		}
		
		public HobbsMeterEntryElement (string caption, string placeholder, float value) : base (caption, placeholder, 4)
		{
			KeyboardType = UIKeyboardType.DecimalPad;
			if (value < 0.1f)
				base.Value = string.Empty;
			else
				Value = value;
		}
		
		public new float Value {
			set {
				if (value < 0.1f) {
					base.Value = string.Empty;
					return;
				}
				
				ValueAsSeconds = (int) (value * 3600);
			}
			
			get {
				string str = base.Value;
				float value;
				
				if (str == null || str.Length == 0)
					return 0.0f;
				
				if (float.TryParse (str, out value)) {
					if (value < 0.0f)
						return 0.0f;
					
					if (value > 24.0f)
						return 24.0f;
					
					return value;
				}
				
				return 0.0f;
			}
		}
		
		public int ValueAsSeconds {
			set { base.Value = Math.Round (value / 3600.0, 1).ToString (); }
			get {
				string str = base.Value;
				int hours, tenths = 0;
				int dot;
				
				if (str == null || str.Length == 0)
					return 0;
				
				if ((dot = str.IndexOf ('.')) != -1) {
					if (dot > 0) {
						if (!Int32.TryParse (str.Substring (0, dot), out hours))
							return 0;
					} else {
						hours = 0;
					}
					
					str = str.Substring (dot + 1);
					if (str.Length > 0 && str[0] >= '0' && str[0] <= '9')
						tenths = (int) (str[0] - '0');
				} else {
					if (!Int32.TryParse (str, out hours))
						return 0;
				}
				
				return (hours * 3600) + (tenths * 360);
			}
		}
		
		static NSString hobbsKey = new NSString ("HobbsMeterEntryElement");
		protected override NSString EntryKey {
			get {
				return hobbsKey;
			}
		}
		
		protected override bool AllowTextChange (string currentText, NSRange changedRange, string replacementText)
		{
			int newLength = currentText.Length - changedRange.Length + replacementText.Length;
			StringBuilder result;
			int dot, hours = 0;
			int i;
			
			if (newLength > MaxLength)
				return false;
			
			if (newLength == 0)
				return true;
			
			// Validate that the replacement characters are all numeric
			for (i = 0; i < replacementText.Length; i++) {
				if ((replacementText[i] < '0' || replacementText[i] > '9') && replacementText[i] != '.')
					return false;
			}
			
			// Combine the currentText with the replacementText to get our resulting text
			result = new StringBuilder (newLength);
			for (i = 0; i < changedRange.Location; i++)
				result.Append (currentText[i]);
			for (i = 0; i < replacementText.Length; i++)
				result.Append (replacementText[i]);
			for (i = changedRange.Location + changedRange.Length; i < currentText.Length; i++)
				result.Append (currentText[i]);
			
			// Validate the value
			for (i = 0, dot = -1; i < result.Length; i++) {
				if (result[i] == '.') {
					dot = i;
					break;
				} else {
					hours = (hours * 10) + (result[i] - '0');
				}
			}
			
			// Make sure total entered time is <= 24 hours
			if (hours > 24)
				return false;
			
			if (dot != -1) {
				// Make sure we don't have more than 1 significant decimal point
				if ((dot + 2) < result.Length)
					return false;
				
				// Make sure the decimal value is in range
				if ((dot + 1) < result.Length && (result[dot + 1] < '0' || result[dot + 1] > '9'))
					return false;
				
				// Make sure the number of hours does not exceed 24
				if (hours > 23)
					return false;
			}
			
			return true;
		}
	}
}
