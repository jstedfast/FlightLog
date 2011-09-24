// 
// FlightDateEntryElement.cs
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
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class FlightDateEntryElement : Element
	{
		static NSString key = new NSString ("FlightDateEntryElement");
		DatePickerController picker;
		UIPopoverController popover;
		
		public FlightDateEntryElement (string caption, DateTime date) : base (caption)
		{
			DateValue = date;
		}
		
		public DateTime DateValue {
			get; set;
		}
		
		protected override NSString CellKey {
			get { return key; }
		}
		
		string FormatDate (DateTime date)
		{
			return date.ToShortDateString ();
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (key);
			
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Value1, key);
				cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
				cell.DetailTextLabel.TextAlignment = UITextAlignment.Left;
				cell.TextLabel.TextAlignment = UITextAlignment.Left;
			}
			
			cell.DetailTextLabel.Text = FormatDate (DateValue);
			cell.TextLabel.Text = Caption;
			
			return cell;
		}
		
		public override string Summary ()
		{
			return FormatDate (DateValue);
		}
		
		class DatePickerController : UIViewController {
			UIDatePicker picker;
			
			public DatePickerController ()
			{
				Title = "Pick a Date";
				
				View = picker = new UIDatePicker (RectangleF.Empty) {
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
					Mode = UIDatePickerMode.Date,
				};
			}
			
			public UIPopoverController Popover {
				get; set;
			}
			
			public DateTime DateValue {
				get { return (DateTime) picker.Date; }
				set { picker.Date = (NSDate) value; }
			}
			
			public override void ViewDidAppear (bool animated)
			{
				Popover.SetPopoverContentSize (picker.Frame.Size, animated);
				
				base.ViewDidAppear (animated);
			}
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			picker = new DatePickerController ();
			popover = new UIPopoverController (picker);
			//picker.DateValue = DateValue;
			picker.Popover = popover;
			
			var cell = GetActiveCell ();
			
			popover.DidDismiss += (sender, e) => {
				DateValue = picker.DateValue;
				
				GetImmediateRootElement ().Reload (this, UITableViewRowAnimation.None);
			};
			
			popover.PresentFromRect (cell.Frame, tableView, UIPopoverArrowDirection.Any, true);
		}
		
		public override bool Matches (string text)
		{
			return base.Matches (text);
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}
