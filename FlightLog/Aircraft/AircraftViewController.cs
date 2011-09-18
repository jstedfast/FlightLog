// 
// AircraftOverviewViewController.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc.
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
	public class AircraftViewController : DialogViewController
	{
		EditAircraftDetailsViewController editor;
		AircraftDetailsViewController details;
		UIBarButtonItem addAircraft;
		UITableView tableView;
		int selected = 0;
		
		public AircraftViewController (AircraftDetailsViewController details) :
			base (UITableViewStyle.Plain, new RootElement (null))
		{
			Title = "Aircraft";
			
			SearchPlaceholder = "Search Aircraft";
			AutoHideSearch = true;
			EnableSearch = true;
			Autorotate = false;
			
			this.details = details;
		}
		
		void LoadAircraft ()
		{
			Section section = new Section ();
			AircraftElement element;
			
			foreach (var aircraft in LogBook.GetAllAircraft ()) {
				element = new AircraftElement (aircraft);
				element.Changed += OnElementChanged;
				section.Add (element);
			}
			
			Root.Add (section);
			
			LogBook.AircraftAdded += OnAircraftAdded;
		}
		
		public override UITableView MakeTableView (RectangleF bounds, UITableViewStyle style)
		{
			tableView = base.MakeTableView (bounds, style);
			
			tableView.RowHeight = AircraftCell.CellHeight;
			tableView.AllowsSelection = true;
			
			return tableView;
		}
		
		void SelectRow (NSIndexPath path, bool animated, UITableViewScrollPosition scroll)
		{
			tableView.SelectRow (path, animated, scroll);
			
			// Note: Calling SelectRow() programatically will not cause the
			// TableSource's RowSelected method to be called, so we have to
			// do it ourselves.
			tableView.Source.RowSelected (tableView, path);
		}
		
		void OnElementChanged (object sender, EventArgs args)
		{
			AircraftElement element = (AircraftElement) sender;
			
			Root.Reload (element, UITableViewRowAnimation.None);
		}
		
		void OnAircraftAdded (object sender, AircraftEventArgs added)
		{
			AircraftElement element = new AircraftElement (added.Aircraft);
			
			element.Changed += OnElementChanged;
			
			// Disengage search before adding to the list
			FinishSearch ();
			
			Root[0].Add (element);
			Root.Reload (Root[0], UITableViewRowAnimation.Fade);
			
			// Select the aircraft we just added
			NSIndexPath path = element.IndexPath;
			SelectRow (path, true, UITableViewScrollPosition.Bottom);
		}
		
		void OnEditorClosed (object sender, EventArgs args)
		{
			editor = null;
		}
		
		void OnAddClicked (object sender, EventArgs args)
		{
			if (editor != null && !details.EditorEngaged)
				return;
			
			editor = new EditAircraftDetailsViewController (new Aircraft (), false);
			editor.EditorClosed += OnEditorClosed;
			
			details.NavigationController.PushViewController (editor, true);
		}
		
		public override void LoadView ()
		{
			addAircraft = new UIBarButtonItem (UIBarButtonSystemItem.Add, OnAddClicked);
			
			NavigationItem.LeftBarButtonItem = addAircraft;
			
			LoadAircraft ();
			
			base.LoadView ();
		}
		
		bool CanDeleteAircraftElement (Element element)
		{
			AircraftElement aircraft = element as AircraftElement;
			
			return aircraft != null && LogBook.CanDelete (aircraft.Aircraft);
		}
		
		public override Source CreateSizingSource (bool unevenRows)
		{
			SwipeToDeleteTableSource source = new SwipeToDeleteTableSource (this, CanDeleteAircraftElement);
			source.ElementDeleted += OnElementDeleted;
			return source;
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			
			SelectOrAdd (selected);
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			switch (toInterfaceOrientation) {
			case UIInterfaceOrientation.LandscapeRight:
			case UIInterfaceOrientation.LandscapeLeft:
				return true;
			default:
				return false;
			}
		}
		
		void OnElementDeleted (object sender, ElementEventArgs args)
		{
			AircraftElement deleted = args.Element as AircraftElement;
			NSIndexPath path = deleted.IndexPath;
			
			if (LogBook.Delete (deleted.Aircraft)) {
				deleted.Changed -= OnElementChanged;
				Root[0].Remove (path.Row);
				SelectOrAdd (path.Row);
			}
		}
		
		protected virtual void OnAircraftSelected (Aircraft aircraft)
		{
			details.Aircraft = aircraft;
		}
		
		void SelectOrAdd (int row)
		{
			UITableViewScrollPosition scroll = UITableViewScrollPosition.None;
			NSIndexPath path;
			
			// Calculate the row to select
			if (row >= Root[0].Count) {
				// Looks like the last element was selected and no longer exists...
				if (Root[0].Count > 0) {
					// Get the path of the current last element
					path = NSIndexPath.FromRowSection (Root[0].Count - 1, 0);
					scroll = UITableViewScrollPosition.Bottom;
				} else {
					// No elements exist, can't select anything
					path = null;
				}
			} else {
				path = NSIndexPath.FromRowSection (row, 0);
			}
			
			if (path != null) {
				// Select the most appropriate element
				SelectRow (path, true, scroll);
				return;
			}
			
			OnAddClicked (null, EventArgs.Empty);
		}
		
		public override void Deselected (NSIndexPath indexPath)
		{
			base.Deselected (indexPath);
		}
		
		public override void Selected (NSIndexPath indexPath)
		{
			var element = Root[indexPath.Section][indexPath.Row] as AircraftElement;
			
			OnAircraftSelected (element.Aircraft);
			selected = indexPath.Row;
			
			base.Selected (indexPath);
		}
	}
}
