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
using System.Drawing;
using System.Collections;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class EditFlightDetailsViewController : DialogViewController
	{
		HobbsMeterEntryElement total, dual, xc, night, pic, sic, cfi, actual, hood, simulator;
		NumericEntryElement landDay, landNight, approaches;
		AirportEntryElement visited1, visited2, visited3;
		AirportEntryElement departed, arrived;
		AircraftEntryElement aircraft;
		LimitedEntryElement remarks;
		UIBarButtonItem cancel, save;
		FlightDateEntryElement date;
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
			return new Section ("Flight") {
				(date = new FlightDateEntryElement ("Date", Flight.Date)),
				(aircraft = new AircraftEntryElement (Flight.Aircraft)),
				(departed = new AirportEntryElement ("Departed", Flight.AirportDeparted)),
				(visited1 = new AirportEntryElement ("Visited", Flight.AirportVisited1)),
				(visited2 = new AirportEntryElement ("Visited", Flight.AirportVisited2)),
				(visited3 = new AirportEntryElement ("Visited", Flight.AirportVisited3)),
				(arrived = new AirportEntryElement ("Arrived", Flight.AirportArrived)),
			};
		}
		
		Section CreateExperienceSection ()
		{
			return new Section ("Flight Experience") {
				(total = new HobbsMeterEntryElement ("Flight Time", "Total flight time, as measured on the Hobbs Meter.", Flight.FlightTime)),
				(cfi = new HobbsMeterEntryElement ("C.F.I.", "Time spent sweating only on the right side of your face.", Flight.CertifiedFlightInstructor)),
				(pic = new HobbsMeterEntryElement ("P.I.C.", "Time spent as Pilot in Command.", Flight.PilotInCommand)),
				(sic = new HobbsMeterEntryElement ("S.I.C.", "Time spent as Second in Command.", Flight.SecondInCommand)),
				(dual = new HobbsMeterEntryElement ("Dual Received", "Time spent in training with an instructor.", Flight.DualReceived)),
				(xc = new HobbsMeterEntryElement ("Cross-Country", "Time spent flying cross-country.", Flight.CrossCountry)),
				(night = new HobbsMeterEntryElement ("Night Flying", "Time spent flying after dark.", Flight.Night)),
				(landDay = new NumericEntryElement ("Day Landings", "Number of landings made during daylight hours.", Flight.DayLandings, 1, 99)),
				(landNight = new NumericEntryElement ("Night Landings", "Number of landings made after dark.", Flight.NightLandings, 1, 99))
			};
		}
		
		Section CreateInstrumentSection ()
		{
			return new Section ("Instrument Experience") {
				(actual = new HobbsMeterEntryElement ("Actual Time", "Time spent flying by instrumentation only.", Flight.InstrumentActual)),
				(hood = new HobbsMeterEntryElement ("Hood Time", "Time spent flying under a hood.", Flight.InstrumentHood)),
				(simulator = new HobbsMeterEntryElement ("Simulator Time", "Time spent practicing in a simulator.", Flight.InstrumentSimulator)),
				(approaches = new NumericEntryElement ("Approaches", "The number of approaches made.", Flight.InstrumentApproaches, 1, 99)),
			};
		}
		
		public override void LoadView ()
		{
			Title = exists ? Flight.Date.ToShortDateString () : "New Flight Entry";
			
			Root.Add (CreateFlightSection ());
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
			this.NavigationController.PopViewControllerAnimated (true);
			
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
		
		void OnSaveClicked (object sender, EventArgs args)
		{
			FetchValues ();
			
			// Don't let the user save if the info is incomplete
			if (aircraft.Value == null || aircraft.Value.Length < 6)
				return;
			
			if (departed.Value == null || arrived == null)
				return;
			
			if (total.ValueAsSeconds == 0)
				return;
			
			// Save the values back to the Flight record
			Flight.Date = date.DateValue;
			Flight.Aircraft = aircraft.Value;
			Flight.AirportDeparted = departed.Value;
			Flight.AirportVisited1 = visited1.Value;
			Flight.AirportVisited2 = visited2.Value;
			Flight.AirportVisited3 = visited3.Value;
			Flight.AirportArrived = arrived.Value;
			
			// Flight Time values
			Flight.FlightTime = total.ValueAsSeconds;
			Flight.CertifiedFlightInstructor = cfi.ValueAsSeconds;
			Flight.InstrumentSimulator = simulator.ValueAsSeconds;
			Flight.InstrumentActual = actual.ValueAsSeconds;
			Flight.InstrumentHood = hood.ValueAsSeconds;
			Flight.SecondInCommand = sic.ValueAsSeconds;
			Flight.PilotInCommand = pic.ValueAsSeconds;
			Flight.DualReceived = dual.ValueAsSeconds;
			Flight.CrossCountry = xc.ValueAsSeconds;
			Flight.Night = night.ValueAsSeconds;
			
			Flight.Day = Flight.FlightTime - Flight.Night;
			
			// Landings and Approaches
			Flight.InstrumentApproaches = approaches.Value;
			Flight.NightLandings = landNight.Value;
			Flight.DayLandings = landDay.Value;
			
			// Remarks
			Flight.Remarks = remarks.Value;
			
			if (exists)
				LogBook.Update (Flight);
			else
				LogBook.Add (Flight);
			
			this.NavigationController.PopViewControllerAnimated (true);
			
			OnEditorClosed ();
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
		
		public event EventHandler<EventArgs> EditorClosed;
		
		void OnEditorClosed ()
		{
			var handler = EditorClosed;
			
			if (handler != null)
				handler (this, EventArgs.Empty);
		}
	}
}
