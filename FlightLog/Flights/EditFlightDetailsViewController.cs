// 
// EditFlightDetailsViewController.cs
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
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class EditFlightDetailsViewController : DialogViewController
	{
		HobbsMeterEntryElement total, dual, night, pic, sic, cfi, xc, actual, hood, simulator;
		NumericEntryElement landDay, landNight, approaches;
		LimitedEntryElement remarks, safetyPilot;
		AirportEntryElement departed, arrived;
		List<AirportEntryElement> visited;
		BooleanElement holdingProcedures;
		PilotCertification certification;
		AircraftEntryElement aircraft;
		UIBarButtonItem cancel, save;
		DateEntryElement date;
		bool autoFlightTimes = true;
		UIAlertViewDelegate del;
		UIAlertView alert;
		bool exists;
		
		public EditFlightDetailsViewController (Flight flight, bool exists) : base (UITableViewStyle.Grouped, new RootElement (null))
		{
			certification = LogBook.Pilot.Certification;
			this.exists = exists;
			Flight = flight;
		}
		
		public Flight Flight {
			get; private set;
		}

		bool IsStudentPilot {
			get { return certification == PilotCertification.Student; }
		}

		bool IsFlightInstructor {
			get { return LogBook.Pilot.IsCertifiedFlightInstructor; }
		}
		
		public override UITableView MakeTableView (RectangleF bounds, UITableViewStyle style)
		{
			return base.MakeTableView (bounds, style);
		}

#if false
		class EditFlightDetailsTableViewSource : DialogViewController.Source {
			public EditFlightDetailsTableViewSource (EditFlightDetailsViewController editor) : base (editor)
			{
			}

			EditFlightDetailsViewController Editor {
				get { return (EditFlightDetailsViewController) Container; }
			}

			List<AirportEntryElement> Visited {
				get { return Editor.visited; }
			}

			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
			{
				var element = Root[indexPath.Section][indexPath.Row];

				if (element == Visited[0])
					return UITableViewCellEditingStyle.Insert;

				for (int i = 1; i < Visited.Count; i++) {
					if (element == Visited[i])
						return UITableViewCellEditingStyle.Delete;
				}

				return UITableViewCellEditingStyle.None;
			}

			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				var element = Root[indexPath.Section][indexPath.Row];

				foreach (var airport in Visited) {
					if (element == airport)
						return true;
				}

				return false;
			}

			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				var section = Root[indexPath.Section];
				var element = section[indexPath.Row];
				AirportEntryElement airport;
				int row = indexPath.Row;

				switch (editingStyle) {
				case UITableViewCellEditingStyle.Insert:
					if (element == Visited[0]) {
						if (Visited.Count >= 3)
							return;

						row += Visited.Count;

						airport = new AirportEntryElement ("Visited", "");
						Visited.Add (airport);

						section.Insert (row, UITableViewRowAnimation.Automatic, airport);
					}
					break;
				case UITableViewCellEditingStyle.Delete:
					if (element is AirportEntryElement) {
						airport = (AirportEntryElement) element;
						if (Visited.Contains (airport)) {
							section.RemoveRange (row, 1, UITableViewRowAnimation.Automatic);
							Visited.Remove (airport);
						}
					}
					break;
				default:
					break;
				}
			}

			public override bool ShouldIndentWhileEditing (UITableView tableView, NSIndexPath indexPath)
			{
				return false;
			}
		}

		public override Source CreateSizingSource (bool unevenRows)
		{
			return new EditFlightDetailsTableViewSource (this);
		}
#endif
		
		Section CreateFlightSection ()
		{
			AirportEntryElement airport;

			date = new DateEntryElement ("Date", Flight.Date);
			aircraft = new AircraftEntryElement (Flight.Aircraft) { AutoComplete = true };
			departed = new AirportEntryElement ("Departed", Flight.AirportDeparted);
			arrived = new AirportEntryElement ("Arrived", Flight.AirportArrived);

			visited = new List<AirportEntryElement> (3);

			var section = new Section ("Flight");
			section.Add (date);
			section.Add (aircraft);
			section.Add (departed);

			string str = Flight.AirportVisited;
			if (!string.IsNullOrEmpty (str)) {
				foreach (var via in str.Split (new char[] { ',' })) {
					airport = new AirportEntryElement ("Visited", via.Trim ());
					airport.EditingCompleted += OnVisitedAirportEdited;
					section.Add (airport);
					visited.Add (airport);
				}
			}

			airport = new AirportEntryElement ("Visited", string.Empty);
			airport.EditingCompleted += OnVisitedAirportEdited;
			section.Add (airport);
			visited.Add (airport);

			section.Add (arrived);

			return section;
		}

		RootElement CreateFlightTimeDetailsElement ()
		{
			total = new HobbsMeterEntryElement ("Flight Time", "Total flight time, as measured on the Hobbs Meter.", Flight.FlightTime);
			cfi = new HobbsMeterEntryElement ("C.F.I.", "Time spent sweating only on the right side of your face.", Flight.CertifiedFlightInstructor);
			dual = new HobbsMeterEntryElement ("Dual Received", "Time spent in training with an instructor.", Flight.DualReceived);
			if (IsStudentPilot)
				pic = new HobbsMeterEntryElement ("Solo", "Time spent flying solo.", Flight.PilotInCommand);
			else
				pic = new HobbsMeterEntryElement ("P.I.C.", "Time spent flying as Pilot in Command.", Flight.PilotInCommand);
			sic = new HobbsMeterEntryElement ("S.I.C.", "Time spent flying as Second in Command.", Flight.SecondInCommand);
			night = new HobbsMeterEntryElement ("Night Flying", "Time spent flying after dark.", Flight.Night);
			xc = new HobbsMeterEntryElement ("Cross-Country", "Time spent flying cross-country.", Flight.CrossCountry);

			// When the user enters the total flight time, default XC, PIC, CFI, and/or DualReceived times as appropriate.
			total.EditingCompleted += OnFlightTimeEntered;

			// Disable auto-setting of the breakdown times if any of them are manually set.
			dual.EditingCompleted += DisableAutoFlightTimes;
			cfi.EditingCompleted += DisableAutoFlightTimes;
			pic.EditingCompleted += DisableAutoFlightTimes;
			sic.EditingCompleted += DisableAutoFlightTimes;
			xc.EditingCompleted += DisableAutoFlightTimes;

			var section = new Section ("Flight Time");
			section.Add (total);
			if (IsFlightInstructor || Flight.CertifiedFlightInstructor > 0)
				section.Add (cfi);
			section.Add (dual);
			section.Add (pic);
			if (certification >= PilotCertification.Private || Flight.SecondInCommand > 0)
				section.Add (sic);
			section.Add (night);
			section.Add (xc);

			return new RootElement ("Flight Time", 0, 0) { section };
		}

		RootElement CreateInstrumentTimeDetailsElement ()
		{
			simulator = new HobbsMeterEntryElement ("Simulator Time", "Time spent practicing in a simulator.", Flight.InstrumentSimulator);
			actual = new HobbsMeterEntryElement ("Actual Time", "Time spent flying by instrumentation only.", Flight.InstrumentActual);
			hood = new HobbsMeterEntryElement ("Hood Time", "Time spent flying under a hood.", Flight.InstrumentHood);

			return new RootElement ("Instrument Time") {
				new Section ("Instrument Time") {
					actual, hood, simulator
				}
			};
		}

		bool EditInstrumentExperience {
			get {
				return Flight.InstrumentActual > 0 || Flight.InstrumentHood > 0 || Flight.InstrumentSimulator > 0 ||
					Flight.InstrumentApproaches > 0 || Flight.InstrumentHoldingProcedures ||
						LogBook.Pilot.IsInstrumentRated;
			}
		}

		Section CreateExperienceSection ()
		{
			var section = new Section ("Flight Experience");

			section.Add (CreateFlightTimeDetailsElement ());
			if (EditInstrumentExperience)
				section.Add (CreateInstrumentTimeDetailsElement ());

			return section;
		}

		Section CreateInstrumentSection ()
		{
			approaches = new NumericEntryElement ("Approaches", "The number of approaches made.", Flight.InstrumentApproaches, 1, 99);
			holdingProcedures = new BooleanElement ("Holding Procedures", Flight.InstrumentHoldingProcedures);
			safetyPilot = new SafetyPilotEntryElement (Flight.InstrumentSafetyPilot) { AutoComplete = true };

			return new Section ("Instrument Experience") {
				approaches, holdingProcedures, safetyPilot
			};
		}

		Section CreateLandingsSection ()
		{
			landDay = new NumericEntryElement ("Day Landings", "Number of landings made during daylight hours.", Flight.DayLandings, 1, 99);
			landNight = new NumericEntryElement ("Night Landings", "Number of landings made after dark.", Flight.NightLandings, 1, 99);

			return new Section ("Landings") {
				landDay, landNight
			};
		}

		Section CreateRemarksSection ()
		{
			remarks = new LimitedEntryElement (null, "Enter any remarks here.", Flight.Remarks, 140);

			return new Section ("Remarks") { remarks };
		}

		List<Airport> GetAirports ()
		{
			HashSet<string> missing = new HashSet<string> ();
			List<Airport> airports = new List<Airport> ();

			if (GetAirportCode (departed.Value, airports, missing) == null)
				return null;

			foreach (var via in visited)
				GetAirportCode (via.Value, airports, missing);

			GetAirportCode (arrived.Value, airports, missing);

			return airports;
		}

		void OnVisitedAirportEdited (object sender, EventArgs e)
		{
			AirportEntryElement airport = (AirportEntryElement) sender;
			int index = visited.IndexOf (airport);
			int last = visited.Count - 1;

			using (var path = airport.IndexPath) {
				if (!string.IsNullOrEmpty (airport.Value) && index == last) {
					// Add another Visited entry element...
					airport = new AirportEntryElement ("Visited", string.Empty);
					airport.EditingCompleted += OnVisitedAirportEdited;
					Root[path.Section].Insert (path.Row + 1, UITableViewRowAnimation.Automatic, new Element[] { airport });
					visited.Add (airport);
				}
			}
		}

		void DisableAutoFlightTimes (object sender, EventArgs e)
		{
			autoFlightTimes = false;
		}

		void OnFlightTimeEntered (object sender, EventArgs e)
		{
			int seconds = total.ValueAsSeconds;

			if (autoFlightTimes) {
				if (certification == PilotCertification.Student) {
					dual.ValueAsSeconds = seconds;
				} else {
					pic.ValueAsSeconds = seconds;
				}

				var craft = LogBook.GetAircraft (aircraft.Value);
				if (craft != null) {
					double minimum = GetMinimumCrossCountryDistance (craft);
					var airports = GetAirports ();

					if (airports != null && IsCrossCountry (airports, minimum))
						xc.ValueAsSeconds = seconds;
				}
			}

			// Cap the time limit for each of the time-based entry elements to the total time.
			simulator.MaxValueAsSeconds = seconds;
			actual.MaxValueAsSeconds = seconds;
			hood.MaxValueAsSeconds = seconds;
			night.MaxValueAsSeconds = seconds;
			dual.MaxValueAsSeconds = seconds;
			cfi.MaxValueAsSeconds = seconds;
			pic.MaxValueAsSeconds = seconds;
			sic.MaxValueAsSeconds = seconds;
			xc.MaxValueAsSeconds = seconds;
		}
		
		public override void LoadView ()
		{
			Title = exists ? Flight.Date.ToShortDateString () : "New Flight Entry";
			
			Root.Add (CreateFlightSection ());
			Root.Add (CreateLandingsSection ());
			Root.Add (CreateExperienceSection ());
			if (EditInstrumentExperience)
				Root.Add (CreateInstrumentSection ());
			Root.Add (CreateRemarksSection ());
			
			cancel = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, OnCancelClicked);
			NavigationItem.LeftBarButtonItem = cancel;
			
			save = new UIBarButtonItem (UIBarButtonSystemItem.Save, OnSaveClicked);
			NavigationItem.RightBarButtonItem = save;
			
			base.LoadView ();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			//SetEditing (true, animated);
		}
		
		void OnCancelClicked (object sender, EventArgs args)
		{
			NavigationController.PopViewControllerAnimated (true);
			
			OnEditorClosed ();
		}
		
		void FetchValues ()
		{
			// Make sure all entry elements sync their values from their UITextFields
			foreach (Section s in Root) {
				foreach (Element e in s) {
					if (e is EntryElement)
						((EntryElement) e).FetchValue ();
				}
			}
		}

		static double GetMinimumCrossCountryDistance (Aircraft aircraft)
		{
			// SportPilot + PoweredParachute = 15nm
			if (aircraft.Category == AircraftCategory.PoweredParachute)
				return 15.0;

			// SportPilot = 25nm

			// Rotorcraft = 25nm
			if (aircraft.Category == AircraftCategory.Rotorcraft)
				return 25.0;

			return 50.0;
		}
		
		static bool IsCrossCountry (List<Airport> airports, double minimum)
		{
			if (airports.Count < 2)
				return false;
			
			Airport origin = airports[0];
			
			for (int i = 1; i < airports.Count; i++) {
				double distance = Airports.GetDistanceBetween (origin, airports[i]);
				
				// The FAA only classifies flights where the pilot touches down
				// at one or more airports that are greater than 50 nautical miles
				// from the point of origin.
				if (distance > minimum)
					return true;
			}
			
			return false;
		}
		
		static string GetAirportCode (string value, List<Airport> airports, HashSet<string> missing)
		{
			Airport airport;
			
			if (string.IsNullOrEmpty (value))
				return null;
			
			// First, try looking up the airport by (any) airport code.
			if ((airport = Airports.GetAirport (value, AirportCode.Any)) != null) {
				airports.Add (airport);
				return airport.FAA;
			}
			
			// Next, try matching the common name of the airport.
			if ((airport = Airports.GetAirportByName (value)) != null) {
				airports.Add (airport);
				return airport.FAA;
			}
			
			// Unknown airport... just return the provided string.
			missing.Add (value);
			
			return value;
		}
		
		class CrossCountryAlertDelegate : UIAlertViewDelegate {
			EditFlightDetailsViewController editor;
			
			public CrossCountryAlertDelegate (EditFlightDetailsViewController editor)
			{
				this.editor = editor;
			}
			
			public override void Clicked (UIAlertView alertview, int buttonIndex)
			{
				if (buttonIndex == 0 /* No */)
					editor.Flight.CrossCountry = 0;
				
				editor.SaveAndClose ();
			}
		}
		
		void ShowCrossCountryAlert (Aircraft aircraft, string origin, HashSet<string> missing, double minimum)
		{
			StringBuilder message = new StringBuilder ();
			string title = "Cross-Country Alert";
			int i = 0;

			if (missing.Count > 0) {
				message.Append ("Could not verify that this flight was Cross-Country according to FAA regulations because ");

				if (missing.Count > 1)
					message.Append ("the following airports are unknown: ");
				else
					message.Append ("the following airport is unknown: ");

				foreach (var airport in missing) {
					if (i > 0) {
						if (i + 1 == missing.Count)
							message.Append (" and ");
						else
							message.Append (", ");
					}

					message.Append (airport);
				}

				message.AppendLine (".");
				message.AppendLine ();

				if (missing.Count > 1)
					message.AppendFormat ("Are any of these airports greater than {0} nautical miles from {1}?", minimum, origin);
				else
					message.AppendFormat ("Is this airport greater than {0} nautical miles from {1}?", minimum, origin);

				message.AppendLine ();
			} else {
				message.AppendFormat ("None of the airports listed are greater than {0} nautical miles from {1}", minimum, origin);
				message.AppendLine ();
				message.AppendLine ();
				message.AppendLine ("Are you sure that this was a Cross-Country flight according to FAA regulations?");
			}
			
			del = new CrossCountryAlertDelegate (this);
			alert = new UIAlertView (title, message.ToString (), del, "No", "Yes");
			alert.Show ();
		}
		
		void SaveAndClose ()
		{
			if (exists)
				LogBook.Update (Flight);
			else
				LogBook.Add (Flight);
			
			NavigationController.PopViewControllerAnimated (true);
			
			OnEditorClosed ();
		}
		
		void OnSaveClicked (object sender, EventArgs args)
		{
			FetchValues ();
			
			// Don't let the user save if the info is incomplete
			if (aircraft.Value == null || aircraft.Value.Length < 2)
				return;
			
			// We need at least a departure airport
			HashSet<string> missing = new HashSet<string> ();
			List<Airport> airports = new List<Airport> ();
			List<string> via = new List<string> ();
			string code;
			
			if ((code = GetAirportCode (departed.Value, airports, missing)) == null)
				return;

			// Save the values back to the Flight record
			Flight.AirportDeparted = code;

			foreach (var airport in visited) {
				if (string.IsNullOrEmpty (airport.Value))
					continue;

				code = GetAirportCode (airport.Value, airports, missing);
				via.Add (code);
			}

			if (via.Count > 0)
				Flight.AirportVisited = string.Join (", ", via);
			else
				Flight.AirportVisited = null;

			Flight.AirportArrived = GetAirportCode (arrived.Value, airports, missing);
			if (Flight.AirportArrived == null)
				Flight.AirportArrived = Flight.AirportDeparted;

			Flight.Date = date.DateValue;
			Flight.Aircraft = aircraft.Value;
			
			// Flight Time values
			Flight.FlightTime = total.ValueAsSeconds;
			Flight.CertifiedFlightInstructor = cfi.ValueAsSeconds;
			Flight.SecondInCommand = sic.ValueAsSeconds;
			Flight.PilotInCommand = pic.ValueAsSeconds;
			Flight.DualReceived = dual.ValueAsSeconds;
			Flight.CrossCountry = xc.ValueAsSeconds;
			Flight.Night = night.ValueAsSeconds;
			
			// Landings
			Flight.NightLandings = landNight.Value;
			Flight.DayLandings = landDay.Value;

			if (EditInstrumentExperience) {
				// Flight Time values
				Flight.InstrumentSimulator = simulator.ValueAsSeconds;
				Flight.InstrumentActual = actual.ValueAsSeconds;
				Flight.InstrumentHood = hood.ValueAsSeconds;

				// Holding Procedures and Approaches
				Flight.InstrumentHoldingProcedures = holdingProcedures.Value;
				Flight.InstrumentApproaches = approaches.Value;

				// Safety Pilot info
				Flight.InstrumentSafetyPilot = safetyPilot.Value;
			}

			if (Flight.InstrumentSimulator == 0)
				Flight.Day = Flight.FlightTime - Flight.Night;
			else
				Flight.Day = 0;

			// Remarks
			Flight.Remarks = remarks.Value;
			
			// Verify that the flight was really cross-country.
			var craft = LogBook.GetAircraft (Flight.Aircraft);
			double minimum = GetMinimumCrossCountryDistance (craft);
			if (Flight.CrossCountry > 0 && !IsCrossCountry (airports, minimum)) {
				ShowCrossCountryAlert (craft, Flight.AirportDeparted, missing, minimum);
				return;
			}
			
			SaveAndClose ();
		}
		
		public event EventHandler<EventArgs> EditorClosed;
		
		void OnEditorClosed ()
		{
			var handler = EditorClosed;
			
			if (handler != null)
				handler (this, EventArgs.Empty);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}
