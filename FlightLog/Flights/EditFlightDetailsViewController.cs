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

using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.MapKit;
using MonoTouch.UIKit;

namespace FlightLog {
	public class EditFlightDetailsViewController : DialogViewController
	{
		HobbsMeterEntryElement total, dual, night, pic, sic, cfi, actual, hood, simulator;
		NumericEntryElement landDay, landNight, approaches;
		AirportEntryElement visited1, visited2, visited3;
		LimitedEntryElement remarks, safetyPilot;
		AirportEntryElement departed, arrived;
		BooleanElement holdingProcedures;
		AircraftEntryElement aircraft;
		UIBarButtonItem cancel, save;
		FlightDateEntryElement date;
		bool autoFlightTimes = true;
		UIAlertViewDelegate del;
		UIAlertView alert;
		bool exists;
		
		public EditFlightDetailsViewController (Flight flight, bool exists) : base (UITableViewStyle.Grouped, new RootElement (null))
		{
			this.exists = exists;
			Flight = flight;
		}
		
		public Flight Flight {
			get; private set;
		}
		
		public override UITableView MakeTableView (RectangleF bounds, UITableViewStyle style)
		{
			return base.MakeTableView (bounds, style);
		}
		
		Section CreateFlightSection ()
		{
			date = new FlightDateEntryElement ("Date", Flight.Date);
			aircraft = new AircraftEntryElement (Flight.Aircraft) { AutoComplete = true };
			departed = new AirportEntryElement ("Departed", Flight.AirportDeparted);
			visited1 = new AirportEntryElement ("Visited", Flight.AirportVisited1);
			visited2 = new AirportEntryElement ("Visited", Flight.AirportVisited2);
			visited3 = new AirportEntryElement ("Visited", Flight.AirportVisited3);
			arrived = new AirportEntryElement ("Arrived", Flight.AirportArrived);

			return new Section ("Flight") {
				date, aircraft, departed, visited1, visited2, visited3, arrived
			};
		}

		Section CreateFlightTimeBreakdownSection ()
		{
			cfi = new HobbsMeterEntryElement ("C.F.I.", "Time spent sweating only on the right side of your face.", Flight.CertifiedFlightInstructor);
			dual = new HobbsMeterEntryElement ("Dual Received", "Time spent in training with an instructor.", Flight.DualReceived);
			pic = new HobbsMeterEntryElement ("P.I.C.", "Time spent as Pilot in Command.", Flight.PilotInCommand);
			sic = new HobbsMeterEntryElement ("S.I.C.", "Time spent as Second in Command.", Flight.SecondInCommand);
			night = new HobbsMeterEntryElement ("Night Flying", "Time spent flying after dark.", Flight.Night);

			dual.EditingCompleted += DisableAutoFlightTimes;
			cfi.EditingCompleted += DisableAutoFlightTimes;
			pic.EditingCompleted += DisableAutoFlightTimes;
			sic.EditingCompleted += DisableAutoFlightTimes;

			return new Section ("Flight Time Breakdown") {
				cfi, pic, sic, dual, night
			};
		}
		
		Section CreateExperienceSection ()
		{
			total = new HobbsMeterEntryElement ("Flight Time", "Total flight time, as measured on the Hobbs Meter.", Flight.FlightTime);
			var breakdown = new RootElement ("Flight Time Breakdown") {
				CreateFlightTimeBreakdownSection (),
			};

			// When the user enters the total flight time, default PIC, CFI, and/or DualReceived times as appropriate.
			total.EditingCompleted += OnFlightTimeEntered;

			return new Section ("Flight Experience") {
				total, breakdown
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
		
		Section CreateInstrumentSection ()
		{
			simulator = new HobbsMeterEntryElement ("Simulator Time", "Time spent practicing in a simulator.", Flight.InstrumentSimulator);
			actual = new HobbsMeterEntryElement ("Actual Time", "Time spent flying by instrumentation only.", Flight.InstrumentActual);
			hood = new HobbsMeterEntryElement ("Hood Time", "Time spent flying under a hood.", Flight.InstrumentHood);

			approaches = new NumericEntryElement ("Approaches", "The number of approaches made.", Flight.InstrumentApproaches, 1, 99);
			holdingProcedures = new BooleanElement ("Performed Holding Procedures", Flight.InstrumentHoldingProcedures);
			safetyPilot = new SafetyPilotEntryElement (Flight.InstrumentSafetyPilot) { AutoComplete = true };

			var instrument = new RootElement ("Instrument Experience") {
				new Section ("Instrument Experience") {
					actual, hood, simulator, approaches, holdingProcedures, safetyPilot
				}
			};

			return new Section () {
				instrument
			};
		}

		void DisableAutoFlightTimes (object sender, EventArgs e)
		{
			autoFlightTimes = false;
		}

		void OnFlightTimeEntered (object sender, EventArgs e)
		{
			if (autoFlightTimes) {
				if (false /* IsStudentPilot */) {
					dual.ValueAsSeconds = total.ValueAsSeconds;
				} else if (false /* IsFlightInstructor */) {
					cfi.ValueAsSeconds = total.ValueAsSeconds;
				} else {
					pic.ValueAsSeconds = total.ValueAsSeconds;
				}
			}

			// Cap the time limit for each of the time-based entry elements to the total time.
			int seconds = total.ValueAsSeconds;

			actual.MaxValueAsSeconds = seconds;
			hood.MaxValueAsSeconds = seconds;
			night.MaxValueAsSeconds = seconds;
			dual.MaxValueAsSeconds = seconds;
			cfi.MaxValueAsSeconds = seconds;
			pic.MaxValueAsSeconds = seconds;
			sic.MaxValueAsSeconds = seconds;
		}
		
		public override void LoadView ()
		{
			Title = exists ? Flight.Date.ToShortDateString () : "New Flight Entry";
			
			Root.Add (CreateFlightSection ());
			Root.Add (CreateLandingsSection ());
			Root.Add (CreateExperienceSection ());
			Root.Add (CreateInstrumentSection ());
			
			Root.Add (new Section ("Remarks") {
				(remarks = new LimitedEntryElement (null, "Enter any remarks here.", Flight.Remarks, 140)),
			});
			
			cancel = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, OnCancelClicked);
			NavigationItem.LeftBarButtonItem = cancel;
			
			save = new UIBarButtonItem (UIBarButtonSystemItem.Save, OnSaveClicked);
			NavigationItem.RightBarButtonItem = save;
			
			base.LoadView ();
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
		
		static bool IsCrossCountry (List<Airport> airports)
		{
			if (airports.Count < 2)
				return false;
			
			Airport origin = airports[0];
			
			for (int i = 1; i < airports.Count; i++) {
				double distance = Airports.GetDistanceBetween (origin, airports[i]);
				
				// The FAA only classifies flights where the pilot touches down
				// at one or more airports that are greater than 50 nautical miles
				// from the point of origin.
				if (distance > 50.0)
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
				if (buttonIndex == 1 /* Yes */)
					editor.Flight.IsCrossCountry = true;
				
				editor.SaveAndClose ();
			}
		}
		
		void ShowCrossCountryAlert (HashSet<string> missing)
		{
			string title = missing.Count > 1 ? "Missing Airports" : "Missing Airport";
			StringBuilder message = new StringBuilder ();
			int i = 0;
			
			if (missing.Count > 1)
				message.Append ("The following airports are unknown: ");
			else
				message.Append ("The following airport is unknown: ");
			
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
			
			message.AppendLine ("Was this flight a Cross-Country flight?");
			
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
			string airport;
			
			if ((airport = GetAirportCode (departed.Value, airports, missing)) == null)
				return;
			
			// Save the values back to the Flight record
			Flight.Date = date.DateValue;
			Flight.Aircraft = aircraft.Value;
			Flight.AirportDeparted = airport;
			Flight.AirportVisited1 = GetAirportCode (visited1.Value, airports, missing);
			Flight.AirportVisited2 = GetAirportCode (visited2.Value, airports, missing);
			Flight.AirportVisited3 = GetAirportCode (visited3.Value, airports, missing);
			Flight.AirportArrived = GetAirportCode (arrived.Value, airports, missing);
			
			if (Flight.AirportArrived == null)
				Flight.AirportArrived = Flight.AirportDeparted;
			
			// Flight Time values
			Flight.FlightTime = total.ValueAsSeconds;
			Flight.CertifiedFlightInstructor = cfi.ValueAsSeconds;
			Flight.InstrumentSimulator = simulator.ValueAsSeconds;
			Flight.InstrumentActual = actual.ValueAsSeconds;
			Flight.InstrumentHood = hood.ValueAsSeconds;
			Flight.SecondInCommand = sic.ValueAsSeconds;
			Flight.PilotInCommand = pic.ValueAsSeconds;
			Flight.DualReceived = dual.ValueAsSeconds;
			Flight.Night = night.ValueAsSeconds;
			
			Flight.Day = Flight.FlightTime - Flight.Night;
			
			// Landings and Approaches
			Flight.InstrumentHoldingProcedures = holdingProcedures.Value;
			Flight.InstrumentApproaches = approaches.Value;
			Flight.NightLandings = landNight.Value;
			Flight.DayLandings = landDay.Value;
			
			// Safety Pilot info
			Flight.InstrumentSafetyPilot = safetyPilot.Value;
			
			// Remarks
			Flight.Remarks = remarks.Value;
			
			// Note: Cross-Country needs to be done last in case we are forced to pop up
			// an alert dialog.
			if (missing.Count > 0) {
				ShowCrossCountryAlert (missing);
				return;
			}

			// FIXME: Doesn't count as Cross-Country if you are the acting Safety-Pilot
			// for another pilot who is under the hood.
			Flight.IsCrossCountry = IsCrossCountry (airports);
			
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
