// 
// AirportEntryElement.cs
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

using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace FlightLog {
	public class AirportEntryElement : LimitedEntryElement
	{
		static readonly NSString AirportEntryElementCellKey = new NSString ("AirportEntryElement");

		public AirportEntryElement (string caption, string value) : base (caption, "Enter the airport's FAA, ICAO, or IATA code.", value, 4)
		{
			KeyboardType = UIKeyboardType.Default;
		}
		
		public new string Value {
			set { base.Value = value; }
			get {
				string code = base.Value;
				
				if (code == null)
					return null;
				
				return code.ToUpperInvariant ();
			}
		}

		protected override NSString CellKey {
			get { return AirportEntryElementCellKey; }
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
			// Base method checks against the max length for us...
			if (!base.AllowTextChange (currentText, changedRange, replacementText, result))
				return false;
			
			// Validate that all of the characters are legal for an airport code
			for (int i = 0; i < replacementText.Length; i++) {
				if ((replacementText[i] >= 'A' && replacementText[i] <= 'Z') ||
					(replacementText[i] >= 'a' && replacementText[i] <= 'z') ||
					(replacementText[i] >= '0' && replacementText[i] <= '9'))
					continue;
				
				return false;
			}
			
			return true;
		}
	}
}
