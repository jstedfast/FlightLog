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
using System.Drawing;
using System.Collections;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class FlightDetailsViewController : DialogViewController
	{
		static string[] SectionNames = new string[] { "Flight", "Flight Experience", "Instrument Experience" };
		
		delegate string FlightDetailGetValue (Flight flight);
		
		class FlightDetails {
			public string Detail;
			public FlightDetailGetValue GetValue;
			
			public FlightDetails (string detail, FlightDetailGetValue getValue)
			{
				Detail = detail;
				GetValue = getValue;
			}
		}
		
		EditFlightDetailsViewController editor;
		List<List<FlightDetails>> sections;
		UIBarButtonItem edit;
		Flight flight;
		
		public FlightDetailsViewController () : base (UITableViewStyle.Grouped, new RootElement (null))
		{
			Autorotate = true;
			
			List<FlightDetails> basic = new List<FlightDetails> ();
			basic.Add (new FlightDetails ("Date", GetFlightDate));
			basic.Add (new FlightDetails ("Aircraft", GetFlightAircraft));
			basic.Add (new FlightDetails ("Departed", GetFlightAirportDeparted));
			basic.Add (new FlightDetails ("Visited", GetFlightAirportVisited));
			basic.Add (new FlightDetails ("Arrived", GetFlightAirportArrived));
			
			List<FlightDetails> xp = new List<FlightDetails> ();
			xp.Add (new FlightDetails ("Flight Time", GetFlightFlightTime));
			xp.Add (new FlightDetails ("Certified Flight Instructor", GetFlightCertifiedFlightInstructor));
			xp.Add (new FlightDetails ("Dual Received", GetFlightDualReceived));
			xp.Add (new FlightDetails ("Pilot in Command", GetFlightPilotInCommand));
			xp.Add (new FlightDetails ("Second in Command", GetFlightSecondInCommand));
			xp.Add (new FlightDetails ("Cross Country", GetFlightCrossCountry));
			xp.Add (new FlightDetails ("Night Flying", GetFlightNight));
			xp.Add (new FlightDetails ("Day Landings", GetFlightDayLandings));
			xp.Add (new FlightDetails ("Night Landings", GetFlightNightLandings));
			
			List<FlightDetails> inst = new List<FlightDetails> ();
			inst.Add (new FlightDetails ("Actual Time", GetFlightInstrumentActual));
			inst.Add (new FlightDetails ("Hood Time", GetFlightInstrumentHood));
			inst.Add (new FlightDetails ("Simulator Time", GetFlightInstrumentSimulator));
			inst.Add (new FlightDetails ("Approaches", GetFlightInstrumentApproaches));
			
			sections = new List<List<FlightDetails>> ();
			sections.Add (basic);
			sections.Add (xp);
			sections.Add (inst);
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
			Section section;
			
			section = new Section ("Flight");
			section.Add (new StringElement ("Date"));
			section.Add (new StringElement ("Aircraft"));
			section.Add (new StringElement ("Departed"));
			section.Add (new StringElement ("Arrived"));
			Root.Add (section);
			
			section = new Section ("Flight Experience");
			section.Add (new StringElement ("Flight Time"));
			Root.Add (section);
			
			edit = new UIBarButtonItem (UIBarButtonSystemItem.Edit, OnEditClicked);
			NavigationItem.LeftBarButtonItem = edit;
			
			base.LoadView ();
		}
		
		static string FormatFlightTime (int seconds, bool force)
		{
			if (seconds == 0 && !force)
				return null;
			
			double time = Math.Round (seconds / 3600.0, 1);
			
			if (time > 0.9 && time < 1.1)
				return "1 hour";
			
			return time.ToString () + " hours";
		}
		
		static string GetFlightDate (Flight flight)
		{
			return flight != null ? flight.Date.ToLongDateString () : string.Empty;
		}
		
		static string GetFlightAircraft (Flight flight)
		{
			return flight != null ? flight.Aircraft : string.Empty;
		}
		
		static string GetFlightAirportDeparted (Flight flight)
		{
			return flight != null ? flight.AirportDeparted : string.Empty;
		}
		
		static string GetFlightAirportVisited (Flight flight)
		{
			if (flight == null)
				return null;
			
			List<string> visited = new List<string> ();
			if (flight.AirportVisited1 != null && flight.AirportVisited1.Length > 0)
				visited.Add (flight.AirportVisited1);
			if (flight.AirportVisited2 != null && flight.AirportVisited2.Length > 0)
				visited.Add (flight.AirportVisited2);
			if (flight.AirportVisited3 != null && flight.AirportVisited3.Length > 0)
				visited.Add (flight.AirportVisited3);
			
			if (visited.Count == 0)
				return null;
			
			return string.Join (", ", visited.ToArray ());
		}
		
		static string GetFlightAirportArrived (Flight flight)
		{
			return flight != null ? flight.AirportArrived : string.Empty;
		}
		
		static string GetFlightFlightTime (Flight flight)
		{
			return flight != null ? FormatFlightTime (flight.FlightTime, true) : string.Empty;
		}
		
		static string GetFlightCertifiedFlightInstructor (Flight flight)
		{
			return flight != null ? FormatFlightTime (flight.CertifiedFlightInstructor, false) : null;
		}
		
		static string GetFlightDualReceived (Flight flight)
		{
			return flight != null ? FormatFlightTime (flight.DualReceived, false) : null;
		}
		
		static string GetFlightPilotInCommand (Flight flight)
		{
			return flight != null ? FormatFlightTime (flight.PilotInCommand, false) : null;
		}
		
		static string GetFlightSecondInCommand (Flight flight)
		{
			return flight != null ? FormatFlightTime (flight.SecondInCommand, false) : null;
		}
		
		static string GetFlightCrossCountry (Flight flight)
		{
			return flight != null && flight.IsCrossCountry ? "true" : "false";
		}
		
		static string GetFlightNight (Flight flight)
		{
			return flight != null ? FormatFlightTime (flight.Night, false) : null;
		}
		
		static string GetFlightNightLandings (Flight flight)
		{
			return flight != null && flight.NightLandings > 0 ? flight.NightLandings.ToString () : null;
		}
		
		static string GetFlightDayLandings (Flight flight)
		{
			return flight != null && flight.DayLandings > 0 ? flight.DayLandings.ToString () : null;
		}
		
		static string GetFlightInstrumentActual (Flight flight)
		{
			return flight != null ? FormatFlightTime (flight.InstrumentActual, false) : null;
		}
		
		static string GetFlightInstrumentHood (Flight flight)
		{
			return flight != null ? FormatFlightTime (flight.InstrumentHood, false) : null;
		}
		
		static string GetFlightInstrumentSimulator (Flight flight)
		{
			return flight != null ? FormatFlightTime (flight.InstrumentSimulator, false) : null;
		}
		
		static string GetFlightInstrumentApproaches (Flight flight)
		{
			return flight != null && flight.InstrumentApproaches > 0 ? flight.InstrumentApproaches.ToString () : null;
		}
		
		void SetCaptionAndValue (Section section, int index, ref bool reload, string caption, string value)
		{
			if (index >= section.Count) {
				section.Insert (index, UITableViewRowAnimation.None, new StringElement (caption, value));
				reload = false;
			} else {
				StringElement element = section[index] as StringElement;
				element.Caption = caption;
				element.Value = value;
				Root.Reload (element, UITableViewRowAnimation.None);
			}
		}
		
		bool HasInstrumentExperience (Flight flight)
		{
			return flight.InstrumentActual > 0 || flight.InstrumentHood > 0 || flight.InstrumentSimulator > 0 || flight.InstrumentApproaches > 0;
		}
		
		void UpdateDetails ()
		{
			Title = string.Format ("{0} to {1} on {2}", Flight.AirportDeparted,
				Flight.AirportArrived, Flight.Date.ToShortDateString ());
			
			List<NSIndexPath> updated = new List<NSIndexPath> ();
			Section section;
			int reload = -1;
			int sect = 0;
			
			//TableView.BeginUpdates ();
			
			for (int s = 0; s < sections.Count; s++) {
				List<FlightDetails> details = sections[s];
				string[] values = new string[details.Count];
				int first = -1, last = -1;
				int n = 0;
				
				for (int i = 0; i < details.Count; i++) {
					values[i] = details[i].GetValue (Flight);
					if (values[i] != null) {
						if (first == -1)
							first = i;
						last = i;
						n++;
					}
				}
				
				if (n > 0) {
					if (sect >= Root.Count) {
						section = new Section (SectionNames[s]);
						Root.Insert (sect, UITableViewRowAnimation.Top, section);
						Console.WriteLine ("Adding section @ {0} {1}", sect, section.Caption);
					} else {
						section = Root[sect];
						if (section.Caption != SectionNames[s]) {
							section = new Section (SectionNames[s]);
							Root.Insert (sect, UITableViewRowAnimation.Top, section);
							Console.WriteLine ("Inserting section @ {0} {1}", sect, section.Caption);
						} else {
							Console.WriteLine ("Updating section @ {0} {1}", sect, section.Caption);
						}
					}
					
					for (int i = 0, row = 0; i < values.Length; i++) {
						StringElement se;
						
						if (values[i] != null) {
							if (row >= section.Count) {
								se = new StringElement (details[i].Detail, values[i]);
								section.Insert (row, UITableViewRowAnimation.Fade, se);
								Console.WriteLine ("\tAdding row @ {0} {1}", row, se.Caption);
							} else {
								se = section[row] as StringElement;
								if (se.Caption != details[i].Detail) {
									se = new StringElement (details[i].Detail, values[i]);
									section.Insert (row, UITableViewRowAnimation.Middle, se);
									Console.WriteLine ("\tInserting row @ {0} {1}", row, se.Caption);
								} else {
									Console.WriteLine ("\tUpdating row @ {0} {1}", row, se.Caption);
									updated.Add (NSIndexPath.FromRowSection (row, sect));
									se.Value = values[i];
								}
							}
							
							row++;
						} else if (row < section.Count) {
							se = section[row] as StringElement;
							if (se.Caption == details[i].Detail) {
								//if (i > last) {
								//	int count = section.Count - row;
								//	section.RemoveRange (row, count, UITableViewRowAnimation.Automatic);
								//	Console.WriteLine ("\tDeleting rows @ {0}-{1} {2}", row, section.Count - 1, se.Caption);
								//	break;
								//}
								
								section.RemoveRange (row, 1, UITableViewRowAnimation.Bottom);
								Console.WriteLine ("\tDeleting row @ {0} {1}", row, se.Caption);
							}
						}
					}
					
					sect++;
				} else if (sect < Root.Count) {
					section = Root[sect];
					if (section.Caption == SectionNames[s]) {
						Root.RemoveAt (sect, UITableViewRowAnimation.Middle);
						Console.WriteLine ("Deleting section @ {0} {1}", sect, section.Caption);
					}
				}
			}
			
			if (Flight.Remarks != null && Flight.Remarks.Length > 0) {
				if (sect >= Root.Count) {
					section = new Section ("Remarks", Flight.Remarks);
					Root.Insert (sect, UITableViewRowAnimation.Top, section);
				} else {
					section = Root[sect];
					section.Footer = Flight.Remarks;
					reload = sect;
				}
			} else if (sect < Root.Count) {
				Root.RemoveAt (sect, UITableViewRowAnimation.Middle);
			}
			
			//TableView.EndUpdates ();
			
			if (updated.Count > 0)
				TableView.ReloadRows (updated.ToArray (), UITableViewRowAnimation.None);
			
			if (reload != -1)
				TableView.ReloadSections (NSIndexSet.FromIndex (reload), UITableViewRowAnimation.None);
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
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}
