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

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace FlightLog {
	public class HobbsMeterEntryElement : LimitedEntryElement
	{
		static readonly NSString HobbsMeterEntryElementCellKey = new NSString ("HobbsMeterEntryElement");
		const int OneDayInSeconds = 24 * 3600;

		public HobbsMeterEntryElement (string caption, string placeholder) : this (caption, placeholder, 0)
		{
		}
		
		public HobbsMeterEntryElement (string caption, string placeholder, int seconds) : base (caption, placeholder, 4)
		{
			KeyboardType = UIKeyboardType.DecimalPad;
			ValueAsSeconds = Math.Max (0, seconds);
			MaxValueAsSeconds = OneDayInSeconds;
		}

		protected override NSString CellKey {
			get { return HobbsMeterEntryElementCellKey; }
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
				
				if (string.IsNullOrEmpty (str))
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

		public int MaxValueAsSeconds {
			get; set;
		}
		
		public int ValueAsSeconds {
			set { base.Value = value > 0 ? Math.Round (value / 3600.0, 1).ToString ("F1") : string.Empty; }
			get {
				string str = base.Value;
				int hours, tenths = 0;
				int dot;
				
				if (string.IsNullOrEmpty (str))
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
						tenths = str[0] - '0';
				} else {
					if (!Int32.TryParse (str, out hours))
						return 0;
				}
				
				return (hours * 3600) + (tenths * 360);
			}
		}
		
		protected override bool AllowTextChange (string currentText, NSRange changedRange, string replacementText, string result)
		{
			int maxHours = MaxValueAsSeconds / 3600;
			int tenths = -1;
			int hours = 0;
			int dot = -1;

			if (replacementText.Length == 0)
				return true;

			if (result.Length > MaxLength)
				return false;
			
			// Validate that the replacement characters are all numeric
			for (int i = 0; i < replacementText.Length; i++) {
				if ((replacementText[i] < '0' || replacementText[i] > '9') && replacementText[i] != '.')
					return false;
			}
			
			// Validate the value
			for (int i = 0; i < result.Length; i++) {
				if (result[i] == '.') {
					if (dot != -1)
						return false;
					dot = i;
				} else if (dot == -1) {
					hours = (hours * 10) + (result[i] - '0');
				} else if (tenths == -1) {
					tenths = result[i] - '0';
				} else {
					return false;
				}
			}

			if (tenths == -1)
				tenths = 0;

			// Make sure total entered time is <= the max number of hours
			if (hours > maxHours)
				return false;

			// Make sure the value does not exceed the maximum number of seconds
			int seconds = (hours * 3600) + (tenths * 360);
			if (seconds > MaxValueAsSeconds)
				return false;
			
			return true;
		}
	}
}
