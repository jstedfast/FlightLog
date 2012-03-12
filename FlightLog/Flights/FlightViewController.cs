// 
// FlightViewController.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2012 Jeffrey Stedfast
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
using MonoTouch.UIKit;

namespace FlightLog {
	public class FlightViewController : TableViewController
	{
		static SQLiteOrderBy orderBy = new SQLiteOrderBy ("Date", SQLiteOrderByDirection.Descending);
		static string sectionExpr = "strftime ('%Y', Date)";
		static NSString key = new NSString ("Flight");
		
		SQLiteTableModel<Flight> searchModel = new SQLiteTableModel<Flight> (LogBook.SQLiteDB, 10, orderBy, sectionExpr);
		SQLiteTableModel<Flight> model = new SQLiteTableModel<Flight> (LogBook.SQLiteDB, 10, orderBy, sectionExpr);
		EditFlightDetailsViewController editor;
		NSIndexPath selected, searchSelected;
		FlightDetailsViewController details;
		UIBarButtonItem addFlight;
		bool searching;
		
		public FlightViewController (FlightDetailsViewController details) :
			base (UITableViewStyle.Plain)
		{
			
			SearchPlaceholder = "Search Flights";
			AutoHideSearch = true;
			Title = "Flights";
			
			this.details = details;
		}
		
		void OnFlightAdded (object sender, FlightEventArgs added)
		{
			searchModel.Refresh ();
			model.Refresh ();
			
			if (searching)
				SearchDisplayController.SearchResultsTableView.ReloadData ();
			
			TableView.ReloadData ();
		}
		
		void OnEditorClosed (object sender, EventArgs args)
		{
			editor = null;
		}
		
		void OnAddClicked (object sender, EventArgs args)
		{
			if (editor != null || details.EditorEngaged)
				return;
			
			editor = new EditFlightDetailsViewController (new Flight (DateTime.Today), false);
			editor.EditorClosed += OnEditorClosed;
			
			details.NavigationController.PushViewController (editor, true);
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			addFlight = new UIBarButtonItem (UIBarButtonSystemItem.Add, OnAddClicked);
			NavigationItem.LeftBarButtonItem = addFlight;
			
			SearchDisplayController.SearchResultsTableView.RowHeight = FlightTableViewCell.CellHeight;
			SearchDisplayController.SearchResultsTableView.AllowsMultipleSelection = false;
			SearchDisplayController.SearchResultsTableView.SectionFooterHeight = 0;
			SearchDisplayController.SearchResultsTableView.AllowsSelection = true;
			
			TableView.RowHeight = FlightTableViewCell.CellHeight;
			TableView.AllowsMultipleSelection = false;
			TableView.SectionFooterHeight = 0;
			TableView.AllowsSelection = true;
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			
			if (selected == null) {
				// Try to select the first aircraft. If that fails, add a new one.
				//selected = NSIndexPath.FromRowSection (0, 0);
				//SelectOrAdd (0);
			}
		}
		
		protected override int NumberOfSections (UITableView tableView)
		{
			if (tableView == SearchDisplayController.SearchResultsTableView)
				return searchModel.SectionCount;
			else
				return model.SectionCount;
		}
		
		protected override string TitleForHeader (UITableView tableView, int section)
		{
			if (tableView == SearchDisplayController.SearchResultsTableView)
				return searchModel.SectionTitles[section];
			else
				return model.SectionTitles[section];
		}
		
		protected override int RowsInSection (UITableView tableView, int section)
		{
			if (tableView == SearchDisplayController.SearchResultsTableView)
				return searchModel.GetRowCount (section);
			else
				return model.GetRowCount (section);
		}
		
		Flight GetFlight (UITableView tableView, NSIndexPath indexPath)
		{
			if (tableView == SearchDisplayController.SearchResultsTableView)
				return searchModel.GetItem (indexPath.Section, indexPath.Row);
			else
				return model.GetItem (indexPath.Section, indexPath.Row);
		}
		
		protected override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			FlightTableViewCell cell = tableView.DequeueReusableCell (key) as FlightTableViewCell;
			if (cell == null)
				cell = new FlightTableViewCell (key);
			
			cell.Flight = GetFlight (tableView, indexPath);
			
			return cell;
		}
		
		protected override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
		{
			return true;
		}
		
		protected override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			if (editingStyle != UITableViewCellEditingStyle.Delete)
				return;
			
			Flight flight = GetFlight (tableView, indexPath);
			
			if (LogBook.Delete (flight)) {
				searchModel.Refresh ();
				model.Refresh ();
				
				tableView.ReloadData ();
			}
		}
		
		protected override void DidBeginSearch (UISearchDisplayController controller)
		{
			searching = true;
		}
		
		protected override void DidEndSearch (UISearchDisplayController controller)
		{
			searching = false;
		}
		
		protected override bool ShouldReloadForSearchString (UISearchDisplayController controller, string search)
		{
			searchModel.SearchText = search;
			return true;
		}
		
		static bool PathsEqual (NSIndexPath path0, NSIndexPath path1)
		{
			return path0.Section == path1.Section && path0.Row == path1.Row;
		}
		
		protected override NSIndexPath WillSelectRow (UITableView tableView, NSIndexPath indexPath)
		{
			if (tableView == SearchDisplayController.SearchResultsTableView) {
				if (searchSelected != null)
					tableView.DeselectRow (searchSelected, false);
			} else {
				if (selected != null)
					tableView.DeselectRow (selected, false);
			}
			
			return indexPath;
		}
		
		protected override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{
			details.Flight = GetFlight (tableView, indexPath);
			
			if (tableView == SearchDisplayController.SearchResultsTableView) {
				searchSelected = indexPath;
			} else {
				selected = indexPath;
			}
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			if (searchModel != null) {
				searchModel.Dispose ();
				searchModel = null;
			}
			
			if (model != null) {
				model.Dispose ();
				model = null;
			}
		}
	}
}
