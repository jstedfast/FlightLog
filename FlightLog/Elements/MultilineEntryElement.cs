// 
// MultilineEntryElement.cs
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
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class MultilineEntryElement : LimitedEntryElement, IElementSizing
	{
		static readonly NSString MultilineEntryElementCellKey = new NSString ("MultilineEntryElement");
		int nVisibleLines = 1;
		bool recalc = true;
		float height;
		
		public MultilineEntryElement (string placeholder, string value) : base ("", placeholder, value)
		{
		}
		
		public MultilineEntryElement (string placeholder, string value, int visibleLines) : base ("", placeholder, value)
		{
			VisibleLines = visibleLines;
		}
		
		public MultilineEntryElement (string placeholder, string value, int visibleLines, int maxLength) : base ("", placeholder, value, maxLength)
		{
			VisibleLines = visibleLines;
		}

		protected override NSString CellKey {
			get { return MultilineEntryElementCellKey; }
		}
		
		public int VisibleLines {
			get { return nVisibleLines; }
			set {
				if (value == nVisibleLines)
					return;
				
				nVisibleLines = value;
				recalc = true;
			}
		}
		
		protected override UITextField CreateTextField (RectangleF frame)
		{
			UITextField entry = base.CreateTextField (frame);
			
			entry.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			
			return entry;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			return base.GetCell (tv);
		}
		
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			if (recalc) {
				using (var font = UIFont.FromName ("Helvetica", 17f)) {
					var size = new SizeF (320f, float.MaxValue);
					var lines = new string[nVisibleLines];
					for (int i = 0; i < nVisibleLines; i++)
						lines[i] = "Mj";
					
					string text = string.Join ("\n", lines);
					
					height = tableView.StringSize (text, font, size, UILineBreakMode.WordWrap).Height + 10;
				}
			}
			
			return height;
		}
	}
}
