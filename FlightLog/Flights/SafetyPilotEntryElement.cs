//
// SafetyPilotEntryElement.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc.
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
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace FlightLog
{
	public class SafetyPilotEntryElement : LimitedEntryElement
	{
		static readonly NSString SafetyPilotEntryElementCellKey = new NSString ("SafetyPilotEntryElement");
		bool autocompleted;
		bool backspaced;

		public SafetyPilotEntryElement (string value) : base ("Safety Pilot", "The name of your safety pilot.", value, 40)
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

				if (string.IsNullOrEmpty (value))
					return null;

				return value;
			}
		}

		protected override NSString CellKey {
			get { return SafetyPilotEntryElementCellKey; }
		}

		protected override bool AllowTextChange (string currentText, NSRange changedRange, string replacementText, string result)
		{
			if (result.Length == 0)
				return true;

			// If the user backspaced, allow the change to go through.
			if (replacementText.Length == 0) {
				if (autocompleted) {
					// If we've auto-completed and the user backspaces, then he/she is probably entering a name we haven't seen before.
					backspaced = true;
				}

				return true;
			}

			for (int i = 0; i < replacementText.Length; i++) {
				if (replacementText[i] == '%' || replacementText[i] == '*')
					return false;
			}

			if (AutoComplete && result.Length > 0 && !backspaced) {
				// Try to auto-complete the safety pilot from the list of known safety pilots matching the provided text
				var matches = LogBook.GetMatchingSafetyPilots (result);

				if (matches != null) {
					// If we've only got 1 match, auto-complete for the user.
					if (matches.Count == 1) {
						autocompleted = true;
						Value = matches[0];
						return false;
					}

					// Figure out the maximum amount of matching text so that we can complete up to that far...
					int maxLength = matches[0].Length;
					for (int i = 1; i < matches.Count; i++) {
						int n;

						for (n = 0; n < Math.Min (matches[i].Length, maxLength); n++) {
							if (matches[0][n] != matches[i][n])
								break;
						}

						if (n < maxLength)
							maxLength = n;
					}

					Value = matches[0].Substring (0, maxLength);
					autocompleted = true;
					return false;
				}
			}

			return base.AllowTextChange (currentText, changedRange, replacementText, result);
		}
	}
}
