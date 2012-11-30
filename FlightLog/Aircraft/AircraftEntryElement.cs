// 
// TailNumberElement.cs
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
using System.Collections;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class AircraftEntryElement : LimitedEntryElement
	{
		static readonly NSString AircraftEntryElementCellKey = new NSString ("AircraftEntryElement");
		static char[] NotAllowedInTheUS = new char[] { 'I', 'O', 'i', 'o' };
#if ENABLE_GLOBAL_SUPPORT
		static string[] PrefixesStartingWithDigits = new string[] {
			"4K", "8P", "9A", "5B", "4L", "9G", "3X", "8R", "6Y", "5Y", "9K", "7P",
			"5A", "5R", "7Q", "9M", "8Q", "9H", "5T", "3B", "3A", "9N", "5U", "5N",
			"9XR", "6V", "9L", "9V", "6O", "4R", "3D", "5H", "5V", "9Y", "5X", "9J"
		};
		static Dictionary<string, int> MaxLengthMapping;
		
		static AircraftEntryElement ()
		{
			MaxLengthMapping = new Dictionary<string, int> ();
			MaxLengthMapping.Add ("EK", 7);
			MaxLengthMapping.Add ("4K", 7);
			MaxLengthMapping.Add ("EW", 7);
			MaxLengthMapping.Add ("HJ", 7);
			MaxLengthMapping.Add ("HK", 7);
			MaxLengthMapping.Add ("CU", 7);
			MaxLengthMapping.Add ("HI", 7);
			MaxLengthMapping.Add ("4L", 7);
			MaxLengthMapping.Add ("EX", 7);
			MaxLengthMapping.Add ("RD", 9); // prefix is actually RDPL
			MaxLengthMapping.Add ("ER", 7);
			MaxLengthMapping.Add ("HP", 8);
			MaxLengthMapping.Add ("RA", 7);
			MaxLengthMapping.Add ("RF", 7);
			MaxLengthMapping.Add ("EY", 7);
			MaxLengthMapping.Add ("UR", 7);
			MaxLengthMapping.Add ("UK", 7);
			MaxLengthMapping.Add ("YV", 7);
		}
#endif
		
		static int GetMaxLength (char cc0, char cc1)
		{
#if ENABLE_GLOBAL_SUPPORT
			string code = string.Concat (cc0, cc1);
			int max;
			
			if (MaxLengthMapping.TryGetValue (code, out max))
				return max;
#endif
			return 6;
		}
		
		public AircraftEntryElement (string value) : base ("Aircraft", "Aircraft Registration Number", value)
		{
			KeyboardType = UIKeyboardType.Default;
		}
		
		public bool AutoComplete {
			get; set;
		}
		
		public new string Value {
			set { base.Value = value; }
			get {
				string value = base.Value;
				
				if (value == null || value.Length == 0)
					return null;
				
				return value;
			}
		}

		protected override NSString CellKey {
			get { return AircraftEntryElementCellKey; }
		}
		
		protected override UITextField CreateTextField (RectangleF frame)
		{
			UITextField entry = base.CreateTextField (frame);
			
			entry.AutocapitalizationType = UITextAutocapitalizationType.AllCharacters;
			entry.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
			entry.AutocorrectionType = UITextAutocorrectionType.No;
			
			return entry;
		}
		
		protected override bool AllowTextChange (string currentText, NSRange changedRange, string replacementText, string result)
		{
			int i;
			
			if (result.Length == 0)
				return true;
			
			// If the user backspaced, allow the change to go through.
			if (replacementText.Length == 0)
				return true;
			
			// Validate according to http://en.wikipedia.org/wiki/Aircraft_registration
			
			// First step is to validate that all characters are ASCII AlphaNumeric.
			for (i = 0; i < replacementText.Length; i++) {
				if ((replacementText[i] >= 'A' && replacementText[i] <= 'Z') ||
					(replacementText[i] >= 'a' && replacementText[i] <= 'z') ||
					(replacementText[i] >= '0' && replacementText[i] <= '9'))
					continue;
				
				return false;
			}
			
#if ENABLE_GLOBAL_SUPPORT
			// If the resulting tail number begins with a digit, make sure it is valid.
			if (result[0] >= '0' && result[0] <= '9') {
				bool matched = false;
				
				Console.WriteLine ("Validating {0}...", result.ToString ());
				foreach (var prefix in PrefixesStartingWithDigits) {
					if (result[0] != prefix[0]) {
						Console.WriteLine ("0: {0} does not match {1}", result[0], prefix[0]);
						continue;
					}
					
					if (result.Length > 1 && result[1] != prefix[1]) {
						Console.WriteLine ("1: {0} does not match {1}", result[1], prefix[1]);
						continue;
					}
					
					matched = true;
					break;
				}
				
				if (!matched)
					return false;
			}
#endif
			
			if (result.Length == 1)
				return true;
			
			// Verify that the text length does not exceed the max length for a tail number.
			if (result.Length > GetMaxLength (result[0], result[1]))
				return false;
			
			// If this is a U.S. tail number, verify that it doesn't contain an I or O.
			if (result[0] == 'N') {
				if (result.IndexOfAny (NotAllowedInTheUS) != -1)
					return false;
			}
			
			if (AutoComplete && result.Length > 2) {
				// Try to auto-complete the registration number from the database of known aircraft.
				var matches = LogBook.GetMatchingAircraft (result);
				
				// If we've only got 1 match, auto-complete for the user.
				if (matches != null && matches.Count == 1) {
					Value = matches[0].TailNumber;
					return false;
				}
			}
			
			return true;
		}
	}
}
