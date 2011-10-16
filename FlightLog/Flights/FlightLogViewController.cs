// 
// FlightLogViewController.cs
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
	public class FlightLogViewController : DialogViewController
	{
		EditFlightDetailsViewController editor;
		FlightDetailsViewController details;
		UIBarButtonItem addFlight;
		FlightElement selected;
		UITableView tableView;
		
		public FlightLogViewController (FlightDetailsViewController details) :
			base (UITableViewStyle.Plain, new RootElement (null))
		{
			SearchPlaceholder = "Search Flights";
			AutoHideSearch = true;
			EnableSearch = true;
			Autorotate = true;
			Title = "Flights";
			
			this.details = details;
		}
		
		void LoadFlightLog ()
		{
			int year = Int32.MaxValue;
			Section section = null;
			FlightElement element;
			
			foreach (var flight in LogBook.GetAllFlights ()) {
				if (flight.Date.Year != year) {
					year = flight.Date.Year;
					section = new YearSection (year);
					Root.Add (section);
				}
				
				element = new FlightElement (flight);
				element.Changed += OnFlightElementChanged;
				section.Add (element);
			}
			
			LogBook.FlightAdded += OnFlightAdded;
		}
		
		public override UITableView MakeTableView (RectangleF bounds, UITableViewStyle style)
		{
			tableView = base.MakeTableView (bounds, style);
			
			tableView.RowHeight = FlightTableViewCell.CellHeight;
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
		
		YearSection GetSectionForYear (int year)
		{
			int lo = 0, hi = Root.Count;
			YearSection section;
			int mid = 0;
			
			if (hi > 0) {
				do {
					mid = lo + (hi - lo) / 2;
					
					section = Root[mid] as YearSection;
					
					if (year == section.Year)
						return section;
					
					// Note: Sections are in reverse chronological order
					if (year < section.Year) {
						lo = mid + 1;
						mid++;
					} else {
						hi = mid;
					}
				} while (lo < hi);
			}
			
			section = new YearSection (year);
			Root.Insert (mid, UITableViewRowAnimation.Automatic, section);
			
			return section;
		}
		
		void InsertFlightElement (FlightElement element)
		{
			Flight flight = element.Flight;
			int year = flight.Date.Year;
			YearSection section;
			FlightElement fe;
			int lo, hi, mid;
			
			// Scan for the appropriate section to add this flight to...
			section = GetSectionForYear (year);
			hi = section.Count;
			mid = 0;
			lo = 0;
			
			// Scan for the right place to insert this flight
			if (hi > 0) {
				do {
					mid = lo + (hi - lo) / 2;
					
					fe = section[mid] as FlightElement;
					
					if (flight.Date == fe.Flight.Date) {
						// Note: Flights marked with the same date appear in reverse order
						while (mid > 0) {
							fe = section[mid] as FlightElement;
							if (fe.Flight.Date > flight.Date)
								break;
							mid--;
						}
						break;
					}
					
					// Note: flights are in reverse chronological order
					if (flight.Date < fe.Flight.Date) {
						lo = mid + 1;
						mid++;
					} else {
						hi = mid;
					}
				} while (lo < hi);
			}
			
			section.Insert (mid, UITableViewRowAnimation.Automatic, element);
			
			// Select the flight we just added
			SelectRow (element.IndexPath, true, UITableViewScrollPosition.Middle);
		}
		
		void OnFlightElementChanged (object sender, FlightElementChangedEventArgs args)
		{
			FlightElement element = (FlightElement) sender;
			NSIndexPath path = element.IndexPath;
			
			if (path == null || path.Section >= Root.Count || path.Row >= Root[path.Section].Count)
				return;
			
			if (args.DateChanged) {
				Root[path.Section].Remove (path.Row);
				if (Root[path.Section].Count == 0)
					Root.RemoveAt (path.Section, UITableViewRowAnimation.Fade);
				InsertFlightElement (element);
			} else {
				Root.Reload (element, UITableViewRowAnimation.None);
			}
		}
		
		void OnFlightAdded (object sender, FlightEventArgs added)
		{
			FlightElement element = new FlightElement (added.Flight);
			
			element.Changed += OnFlightElementChanged;
			
			InsertFlightElement (element);
		}
		
		void OnEditorClosed (object sender, EventArgs args)
		{
			editor = null;
		}
		
		void OnAddClicked (object sender, EventArgs args)
		{
			if (editor != null && !details.EditorEngaged)
				return;
			
			editor = new EditFlightDetailsViewController (new Flight (DateTime.Today), false);
			editor.EditorClosed += OnEditorClosed;
			
			details.NavigationController.PushViewController (editor, true);
		}
		
		public override void LoadView ()
		{
			addFlight = new UIBarButtonItem (UIBarButtonSystemItem.Add, OnAddClicked);
			
			NavigationItem.LeftBarButtonItem = addFlight;
			
			LoadFlightLog ();
			
			base.LoadView ();
		}
		
		bool CanDeleteFlightElement (Element element)
		{
			return true;
		}
		
		public override Source CreateSizingSource (bool unevenRows)
		{
			SwipeToDeleteTableSource source = new SwipeToDeleteTableSource (this, CanDeleteFlightElement);
			source.ElementDeleted += OnElementDeleted;
			return source;
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			
			if (selected == null) {
				// Try to select the first aircraft. If that fails, add a new one.
				SelectOrAdd (0);
			}
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
		
		int GetElementOffsetFromPath (NSIndexPath path)
		{
			int n = 0;
			
			for (int i = 0; i < path.Section; i++)
				n += Root[i].Count;
			
			return n + path.Row;
		}
		
		NSIndexPath GetElementPathFromOffset (int offset)
		{
			int n = 0, s = 0;
			
			if (Root.Count == 0)
				return null;
			
			while (s < Root.Count) {
				if (n + Root[s].Count > offset)
					break;
				
				n += Root[s].Count;
				s++;
			}
			
			return NSIndexPath.FromRowSection (Math.Min (offset - n, Root[s].Count - 1), s);
		}
		
		void OnElementDeleted (object sender, ElementEventArgs args)
		{
			FlightElement deleted = args.Element as FlightElement;
			NSIndexPath path = deleted.IndexPath;
			int n = GetElementOffsetFromPath (path);
			
			if (LogBook.Delete (deleted.Flight)) {
				deleted.Changed -= OnFlightElementChanged;
				Root[path.Section].Remove (path.Row);
				if (Root[path.Section].Count == 0)
					Root.RemoveAt (path.Section, UITableViewRowAnimation.Fade);
				
				SelectOrAdd (n);
			}
		}
		
		void SelectOrAdd (int nth)
		{
			NSIndexPath path = GetElementPathFromOffset (nth);
			
			if (path != null) {
				// Select the most appropriate element
				SelectRow (path, true, UITableViewScrollPosition.None);
				return;
			}
			
			OnAddClicked (null, EventArgs.Empty);
		}
		
		public override void Deselected (NSIndexPath indexPath)
		{
			base.Deselected (indexPath);
			selected = null;
		}
		
		public override void Selected (NSIndexPath indexPath)
		{
			selected = Root[indexPath.Section][indexPath.Row] as FlightElement;
			
			OnFlightSelected (selected.Flight);
			
			base.Selected (indexPath);
		}
		
		void OnFlightSelected (Flight flight)
		{
			details.Flight = flight;
		}
	}
}
