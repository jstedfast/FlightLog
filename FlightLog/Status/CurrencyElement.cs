// 
// CurrencyElement.cs
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
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class CurrencyTableViewCell : UITableViewCell
	{
		readonly UIColor defaultColor;

		public CurrencyTableViewCell (UITableViewCellStyle style, NSString key) : base (style, key)
		{
			DetailTextLabel.TextAlignment = UITextAlignment.Right;
			DetailTextLabel.HighlightedTextColor = UIColor.White;
			defaultColor = DetailTextLabel.TextColor;
		}
		
		public DateTime ExpirationDate {
			set {
				DateTime now = DateTime.Now;
				
				if (value > now) {
					if (value.AddMonths (-1) > now)
						DetailTextLabel.TextColor = defaultColor; //UIColor.FromRGB (64, 176, 16);
					else
						DetailTextLabel.TextColor = UIColor.Orange;
					DetailTextLabel.Text = string.Format ("Current until {0}", value.ToShortDateString ());
				} else {
					DetailTextLabel.TextColor = UIColor.Red;
					DetailTextLabel.Text = "Expired";
				}
			}
		}
		
		public string Caption {
			get { return TextLabel.Text; }
			set { TextLabel.Text = value; }
		}
	}
	
	public class CurrencyElement : Element
	{
		static readonly NSString CurrencyElementCellKey = new NSString ("CurrencyElement");
		
		public CurrencyElement (string caption, DateTime expires) : base (caption)
		{
			ExpirationDate = expires;
		}
		
		public DateTime ExpirationDate {
			get; set;
		}
		
		protected override NSString CellKey {
			get { return CurrencyElementCellKey; }
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (CurrencyElementCellKey) as CurrencyTableViewCell;
			
			if (cell == null)
				cell = new CurrencyTableViewCell (UITableViewCellStyle.Value1, CurrencyElementCellKey);
			
			cell.ExpirationDate = ExpirationDate;
			cell.Caption = Caption;
			
			return cell;
		}
	}
}
