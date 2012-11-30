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
		static readonly NSString FlightDateEntryElementCellKey = new NSString ("FlightDateEntryElement");
		static SizeF DatePickerSize = new SizeF (316.0f, 216.0f);
		DatePickerController picker;
		UIPopoverController popover;
		UINavigationController nav;
		
		public FlightDateEntryElement (string caption, DateTime date) : base (caption)
		{
			DateValue = date;
		}
		
		public DateTime DateValue {
			get; set;
		}
		
		protected override NSString CellKey {
			get { return FlightDateEntryElementCellKey; }
		}
		
		string FormatDateTime (DateTime date)
		{
			//return date.ToString ("f");
			return date.ToLongDateString ();
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (FlightDateEntryElementCellKey);
			
			if (cell == null) {
				cell = new UITableViewCell (UITableViewCellStyle.Value1, FlightDateEntryElementCellKey);
				cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
				cell.DetailTextLabel.TextAlignment = UITextAlignment.Left;
				cell.TextLabel.TextAlignment = UITextAlignment.Left;
			}
			
			cell.DetailTextLabel.Text = FormatDateTime (DateValue);
			cell.TextLabel.Text = Caption;
			
			return cell;
		}
		
#if false
		// Note: Need to figure out how users want to deal with time pickers as far as timezones go.
		// e.g. do they want to enter local time or zulu time? Should I have an option for that?
		class TimePickerController : UIViewController {
			UIBarButtonItem done;
			UIDatePicker picker;
			
			public TimePickerController (EventHandler doneClicked)
			{
				Title = "Pick a Time";
				
				View = picker = new UIDatePicker (new RectangleF (PointF.Empty, DatePickerSize)) {
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
					Mode = UIDatePickerMode.Time,
					MinuteInterval = 5,
				};
				
				done = new UIBarButtonItem (UIBarButtonSystemItem.Done);
				NavigationItem.RightBarButtonItem = done;
				done.Clicked += doneClicked;
			}
			
			public DateTime DateValue {
				get { return (DateTime) picker.Date; }
				set { picker.Date = (NSDate) value; }
			}
			
			protected override void Dispose (bool disposing)
			{
				if (picker != null) {
					picker.Dispose ();
					picker = null;
				}
				
				if (done != null) {
					done.Dispose ();
					done = null;
				}
				
				base.Dispose (disposing);
			}
		}
#endif
		
		class DatePickerController : UIViewController {
			//TimePickerController timePicker;
			//UIBarButtonItem editTime;
			UIBarButtonItem cancel;
			UIBarButtonItem done;
			UIDatePicker picker;
			
			public DatePickerController (DateTime date)
			{
				Title = "Pick a Date";
				
				View = picker = new UIDatePicker (new RectangleF (PointF.Empty, DatePickerSize)) {
					AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
					Mode = UIDatePickerMode.Date,
					Date = date,
				};
				
				cancel = new UIBarButtonItem (UIBarButtonSystemItem.Cancel);
				NavigationItem.LeftBarButtonItem = cancel;
				cancel.Clicked += (sender, e) => {
					Popover.Dismiss (true);
				};
				
				//editTime = new UIBarButtonItem ("Time", UIBarButtonItemStyle.Plain, OnEditClicked);
				//NavigationItem.RightBarButtonItem = editTime;
				
				done = new UIBarButtonItem (UIBarButtonSystemItem.Done);
				NavigationItem.RightBarButtonItem = done;
				done.Clicked += OnDoneClicked;
			}
			
			public UIPopoverController Popover {
				get; set;
			}
			
			public DateTime DateValue {
				get { return (DateTime) picker.Date; }
				set { picker.Date = (NSDate) value; }
			}
			
			public override void ViewWillAppear (bool animated)
			{
				//if (timePicker != null)
				//	DateValue = timePicker.DateValue;
				
				base.ViewWillAppear (animated);
			}
			
#if false
			void OnEditClicked (object sender, EventArgs args)
			{
				if (timePicker == null)
					timePicker = new TimePickerController (OnDoneClicked);
				
				timePicker.ContentSizeForViewInPopover = DatePickerSize;
				timePicker.DateValue = DateValue;
				
				NavigationController.PushViewController (timePicker, true);
			}
#endif
			
			public event EventHandler DatePicked;
			
			public void OnDoneClicked (object sender, EventArgs args)
			{
				//DateValue = timePicker.DateValue;
				Popover.Dismiss (true);
				
				if (DatePicked != null)
					DatePicked (this, EventArgs.Empty);
			}
			
			protected override void Dispose (bool disposing)
			{
#if false
				if (timePicker != null) {
					timePicker.Dispose ();
					timePicker = null;
				}
				
				if (editTime != null) {
					editTime.Dispose ();
					editTime = null;
				}
#endif
				
				if (cancel != null) {
					cancel.Dispose ();
					cancel = null;
				}
				
				if (done != null) {
					done.Dispose ();
					done = null;
				}
				
				if (picker != null) {
					picker.Dispose ();
					picker = null;
				}
				
				base.Dispose (disposing);
			}
		}
		
		class DatePickerNavigationDelegate : UINavigationControllerDelegate {
			public DatePickerNavigationDelegate ()
			{
			}
			
			public override void WillShowViewController (UINavigationController navController, UIViewController viewController, bool animated)
			{
				viewController.ContentSizeForViewInPopover = DatePickerSize;
			}
		}
		
		void OnDatePicked (object sender, EventArgs args)
		{
			DateValue = ((DatePickerController) sender).DateValue;
			
			GetImmediateRootElement ().Reload (this, UITableViewRowAnimation.None);
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			picker = new DatePickerController (DateValue);
			nav = new UINavigationController (picker);
			popover = new UIPopoverController (nav);
			
			picker.ContentSizeForViewInPopover = DatePickerSize;
			nav.ContentSizeForViewInPopover = DatePickerSize;
			popover.PopoverContentSize = DatePickerSize;
			
			nav.Delegate = new DatePickerNavigationDelegate ();
			picker.DatePicked += OnDatePicked;
			picker.Popover = popover;
			
			var cell = GetActiveCell ();
			
			//popover.DidDismiss += (sender, e) => {
			//	popover.Dispose ();
			//	popover = null;
			//	picker.Dispose ();
			//	picker = null;
			//};
			
			popover.PresentFromRect (cell.Frame, tableView, UIPopoverArrowDirection.Up, true);
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
