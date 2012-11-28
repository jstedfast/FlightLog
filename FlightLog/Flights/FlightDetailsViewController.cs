// 
// FlightDetailsViewController.cs
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
using MonoTouch.SQLite;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class FlightDetailsViewController : AllInOneTableViewController
	{
		static readonly string[] SectionTitles = new string[] { "Flight", "Flight Experience", "Instrument Experience", "Remarks" };
		static readonly NSString TextFieldCellKey = new NSString ("FlightDetails.Text");
		static readonly List<List<FlightProperty>> PropertySections;

		enum SectionTitle {
			Flight,
			FlightExperience,
			InstrumentExperience,
			Remarks
		}

		List<List<FlightProperty>> sections = new List<List<FlightProperty>> ();
		List<SectionTitle> titles = new List<SectionTitle> ();
		EditFlightDetailsViewController editor;
		UIBarButtonItem edit;
		Flight flight;

		static FlightDetailsViewController ()
		{
			var sections = new List<List<FlightProperty>> ();

			sections.Add (new List<FlightProperty> () {
				FlightProperty.Date,
				FlightProperty.Aircraft,
				FlightProperty.AirportDeparted,
				FlightProperty.AirportVisited,
				FlightProperty.AirportArrived,
			});
			sections.Add (new List<FlightProperty> () {
				FlightProperty.FlightTime,
				FlightProperty.CertifiedFlightInstructor,
				FlightProperty.DualReceived,
				FlightProperty.PilotInCommand,
				FlightProperty.SecondInCommand,
				FlightProperty.Night,
				FlightProperty.DayLandings,
				FlightProperty.NightLandings,
			});
			sections.Add (new List<FlightProperty> () {
				FlightProperty.InstrumentActual,
				FlightProperty.InstrumentHood,
				FlightProperty.InstrumentSimulator,
				FlightProperty.InstrumentApproaches,
				FlightProperty.InstrumentHoldingProcedures,
				FlightProperty.ActingInstrumentSafetyPilot,
				FlightProperty.InstrumentSafetyPilot,
			});
			sections.Add (new List<FlightProperty> () {
				FlightProperty.Remarks
			});

			PropertySections = sections;
		}

		public FlightDetailsViewController () : base (UITableViewStyle.Grouped, false)
		{
		}

		public Flight Flight {
			get { return flight; }
			set {
				flight = value;
				if (value != null && IsViewLoaded)
					UpdateDetails ();
			}
		}
		
		public bool EditorEngaged {
			get { return editor != null; }
		}

		public override void LoadView ()
		{
			edit = new UIBarButtonItem (UIBarButtonSystemItem.Edit, OnEditClicked);
			NavigationItem.LeftBarButtonItem = edit;

			var section = new List<FlightProperty> () {
				FlightProperty.Date,
				FlightProperty.Aircraft,
				FlightProperty.AirportDeparted,
				FlightProperty.AirportArrived,
			};

			titles.Add (SectionTitle.Flight);
			sections.Add (section);
			
			section = new List<FlightProperty> () {
				FlightProperty.FlightTime,
			};

			titles.Add (SectionTitle.FlightExperience);
			sections.Add (section);

			base.LoadView ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			TableView.AllowsSelection = false;
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (flight != null)
				UpdateDetails ();
		}
		
		void OnEditorClosed (object sender, EventArgs args)
		{
			if (flight != null)
				UpdateDetails ();
			editor = null;
		}
		
		void OnEditClicked (object sender, EventArgs args)
		{
			Flight edit;
			bool exists;
			
			if (Flight == null) {
				edit = new Flight (DateTime.Today);
				exists = false;
			} else {
				edit = Flight;
				exists = true;
			}
			
			editor = new EditFlightDetailsViewController (edit, exists);
			editor.EditorClosed += OnEditorClosed;
			
			NavigationController.PushViewController (editor, true);
		}

		protected override int NumberOfSections (UITableView tableView)
		{
			return sections.Count;
		}

		protected override int RowsInSection (UITableView tableView, int section)
		{
			if (titles[section] == SectionTitle.Remarks)
				return 0;

			return sections[section].Count;
		}

		protected override string TitleForHeader (UITableView tableView, int section)
		{
			return SectionTitles[(int) titles[section]];
		}

		protected override string TitleForFooter (UITableView tableView, int section)
		{
			if (titles[section] == SectionTitle.Remarks)
				return Flight.GetValue (FlightProperty.Remarks);

			return null;
		}

		protected override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			var cell = tableView.DequeueReusableCell (TextFieldCellKey);

			if (cell == null)
				cell = new UITableViewCell (UITableViewCellStyle.Value1, TextFieldCellKey);

			var property = sections[indexPath.Section][indexPath.Row];
			cell.TextLabel.Text = property.ToHumanReadableName ();
			cell.DetailTextLabel.Text = Flight.GetValue (property);

			return cell;
		}
		
		void UpdateDetails ()
		{
			Title = string.Format ("{0} to {1} on {2}", Flight.AirportDeparted,
			                       Flight.AirportArrived, Flight.Date.ToShortDateString ());

			List<List<FlightProperty>> newSections = new List<List<FlightProperty>> ();
			List<SectionTitle> newTitles = new List<SectionTitle> ();

			// Create a new list of sections & properties
			for (int i = 0; i < PropertySections.Count; i++) {
				List<FlightProperty> section = null;

				foreach (var property in PropertySections[i]) {
					if (Flight.GetValue (property) != null) {
						if (section == null)
							section = new List<FlightProperty> ();
						section.Add (property);
					}
				}

				if (section != null) {
					newTitles.Add ((SectionTitle) i);
					newSections.Add (section);
				}
			}

#if ATTEMPT_TO_DO_FANCY_ANIMATIONS
			// Update sections & titles to match newSections and newTitles
			List<NSIndexPath> reload = new List<NSIndexPath> ();
			bool needEndUpdates = false;
			bool reloadRemarks = false;

			// Remove old rows and sections...
			for (int section = PropertySections.Count - 1; section >= 0; section--) {
				SectionTitle title = (SectionTitle) section;
				int oldSection = titles.IndexOf (title);

				if (oldSection == -1)
					continue;

				int newSection = newTitles.IndexOf (title);
				if (newSection != -1 && title != SectionTitle.Remarks) {
					// Remove old rows...
					List<NSIndexPath> rows = new List<NSIndexPath> ();

					for (int row = PropertySections[section].Count - 1; row >= 0; row--) {
						var property = PropertySections[section][row];
						int oldRow = sections[oldSection].IndexOf (property);

						if (oldRow == -1)
							continue;

						int newRow = newSections[newSection].IndexOf (property);
						if (newRow != -1)
							continue;

						//Console.WriteLine ("Removing {0} (row = {1}) from {2} (section = {3})", property, oldRow, title, oldSection);
						rows.Add (NSIndexPath.FromRowSection (oldRow, oldSection));
						sections[oldSection].RemoveAt (oldRow);
					}

					if (rows.Count > 0) {
						if (!needEndUpdates) {
							TableView.BeginUpdates ();
							needEndUpdates = true;
						}

						TableView.DeleteRows (rows.ToArray (), UITableViewRowAnimation.Automatic);
						foreach (var row in rows)
							row.Dispose ();
					}
				} else if (newSection == -1) {
					// Remove the entire section...
					//Console.WriteLine ("Removing {0} @ {1}", title, oldSection);
					if (!needEndUpdates) {
						TableView.BeginUpdates ();
						needEndUpdates = true;
					}

					var idx = NSIndexSet.FromIndex (oldSection);
					TableView.DeleteSections (idx, UITableViewRowAnimation.Automatic);
					sections.RemoveAt (oldSection);
					titles.RemoveAt (oldSection);
					idx.Dispose ();
				}
			}

			// Add new rows and sections while maintaining a list of rows which need to be reloaded
			for (int section = 0; section < PropertySections.Count; section++) {
				SectionTitle title = (SectionTitle) section;
				int newSection = newTitles.IndexOf (title);

				if (newSection == -1)
					continue;

				int oldSection = titles.IndexOf (title);
				if (oldSection != -1 && title != SectionTitle.Remarks) {
					// Add new rows...
					List<NSIndexPath> rows = new List<NSIndexPath> ();
					
					for (int row = 0; row < PropertySections[section].Count; row++) {
						var property = PropertySections[section][row];
						int newRow = newSections[newSection].IndexOf (property);

						if (newRow == -1)
							continue;
						
						int oldRow = sections[oldSection].IndexOf (property);
						if (oldRow != -1) {
							reload.Add (NSIndexPath.FromRowSection (newRow, newSection));
							continue;
						}

						//Console.WriteLine ("Inserting {0} (row = {1}) into {2} (section = {3})", property, newRow, title, oldSection);
						rows.Add (NSIndexPath.FromRowSection (newRow, oldSection));
						sections[oldSection].Insert (newRow, property);
					}
					
					if (rows.Count > 0) {
						if (!needEndUpdates) {
							TableView.BeginUpdates ();
							needEndUpdates = true;
						}

						TableView.InsertRows (rows.ToArray (), UITableViewRowAnimation.Automatic);
						foreach (var row in rows)
							row.Dispose ();
					}
				} else if (oldSection == -1) {
					// Add the entire section...
					if (!needEndUpdates) {
						TableView.BeginUpdates ();
						needEndUpdates = true;
					}

					//Console.WriteLine ("Inserting {0} @ {1}", title, newSection);
					var idx = NSIndexSet.FromIndex (newSection);
					TableView.InsertSections (idx, UITableViewRowAnimation.Automatic);
					sections.Insert (newSection, newSections[newSection]);
					titles.Insert (newSection, title);
					idx.Dispose ();
				} else if (title == SectionTitle.Remarks) {
					reloadRemarks = true;
				}
			}

			if (needEndUpdates)
				TableView.EndUpdates ();
			
			if (reload.Count > 0) {
				TableView.ReloadRows (reload.ToArray (), UITableViewRowAnimation.None);
				foreach (var row in reload)
					row.Dispose ();
			}

			if (reloadRemarks) {
				var remarks = NSIndexSet.FromIndex (sections.Count - 1);
				TableView.ReloadSections (remarks, UITableViewRowAnimation.None);
				remarks.Dispose ();
			}
#else
			sections = newSections;
			titles = newTitles;

			TableView.ReloadData ();
#endif
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}
