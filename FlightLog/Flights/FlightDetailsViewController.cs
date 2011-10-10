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
		EditFlightDetailsViewController editor;
		UIBarButtonItem edit;
		Flight flight;
		
		public FlightDetailsViewController () : base (UITableViewStyle.Grouped, new RootElement (null))
		{
			Autorotate = true;
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
		
		static string FormatFlightTime (int seconds)
		{
			double time = Math.Round (seconds / 3600.0, 1);
			
			if (time > 0.9 && time < 1.1)
				return "1 hour";
			
			return time.ToString () + " hours";
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
			
			List<string> visited = new List<string> ();
			if (Flight.AirportVisited1 != null && Flight.AirportVisited1.Length > 0)
				visited.Add (Flight.AirportVisited1);
			if (Flight.AirportVisited2 != null && Flight.AirportVisited2.Length > 0)
				visited.Add (Flight.AirportVisited2);
			if (Flight.AirportVisited3 != null && Flight.AirportVisited3.Length > 0)
				visited.Add (Flight.AirportVisited3);
			
			bool reload = true;
			int s = 0, row = 0;
			
			SetCaptionAndValue (Root[s], row++, ref reload, "Date", Flight.Date.ToLongDateString ());
			SetCaptionAndValue (Root[s], row++, ref reload, "Aircraft", Flight.Aircraft);
			SetCaptionAndValue (Root[s], row++, ref reload, "Departed", Flight.AirportDeparted);
			if (visited.Count > 0)
				SetCaptionAndValue (Root[s], row++, ref reload, "Visited", string.Join (", ", visited.ToArray ()));
			SetCaptionAndValue (Root[s], row++, ref reload, "Arrived", Flight.AirportArrived);
			if (row < Root[s].Count)
				Root[s].RemoveRange (row, Root[s].Count - row, UITableViewRowAnimation.Fade);
			else if (reload)
				Root.Reload (Root[s], UITableViewRowAnimation.None);
			
			reload = true;
			row = 0;
			s++;
			
			SetCaptionAndValue (Root[s], row++, ref reload, "Flight Time", FormatFlightTime (Flight.FlightTime));
			if (Flight.CertifiedFlightInstructor > 0)
				SetCaptionAndValue (Root[s], row++, ref reload, "Certified Flight Instructor", FormatFlightTime (Flight.CertifiedFlightInstructor));
			if (Flight.PilotInCommand > 0)
				SetCaptionAndValue (Root[s], row++, ref reload, "Pilot in Command", FormatFlightTime (Flight.PilotInCommand));
			if (Flight.SecondInCommand > 0)
				SetCaptionAndValue (Root[s], row++, ref reload, "Second in Command", FormatFlightTime (Flight.SecondInCommand));
			if (Flight.DualReceived > 0)
				SetCaptionAndValue (Root[s], row++, ref reload, "Dual Received", FormatFlightTime (Flight.DualReceived));
			if (Flight.CrossCountry > 0)
				SetCaptionAndValue (Root[s], row++, ref reload, "Cross Country", FormatFlightTime (Flight.CrossCountry));
			if (Flight.Night > 0)
				SetCaptionAndValue (Root[s], row++, ref reload, "Night Flying", FormatFlightTime (Flight.Night));
			if (Flight.DayLandings > 0)
				SetCaptionAndValue (Root[s], row++, ref reload, "Day Landings", Flight.DayLandings.ToString ());
			if (Flight.NightLandings > 0)
				SetCaptionAndValue (Root[s], row++, ref reload, "Night Landings", Flight.NightLandings.ToString ());
			if (row < Root[s].Count)
				Root[s].RemoveRange (row, Root[s].Count - row, UITableViewRowAnimation.Fade);
			else if (reload)
				Root.Reload (Root[s], UITableViewRowAnimation.None);
			
			if (HasInstrumentExperience (Flight)) {
				row = 0;
				s++;
				
				if (s == Root.Count || Root[s].Caption == "Remarks") {
					Root.Insert (s, UITableViewRowAnimation.Fade, new Section ("Instrument Experience"));
					reload = false;
				} else {
					reload = true;
				}
				
				if (Flight.InstrumentActual > 0)
					SetCaptionAndValue (Root[s], row++, ref reload, "Actual Time", FormatFlightTime (Flight.InstrumentActual));
				if (Flight.InstrumentHood > 0)
					SetCaptionAndValue (Root[s], row++, ref reload, "Hood Time", FormatFlightTime (Flight.InstrumentHood));
				if (Flight.InstrumentSimulator > 0)
					SetCaptionAndValue (Root[s], row++, ref reload, "Simulator Time", FormatFlightTime (Flight.InstrumentSimulator));
				if (Flight.InstrumentApproaches > 0)
					SetCaptionAndValue (Root[s], row++, ref reload, "Approaches", Flight.InstrumentApproaches.ToString ());
				
				if (row < Root[s].Count)
					Root[s].RemoveRange (row, Root[s].Count - row, UITableViewRowAnimation.Fade);
				else if (reload)
					Root.Reload (Root[s], UITableViewRowAnimation.None);
			}
			
			reload = true;
			row = 0;
			s++;
			
			if (Flight.Remarks != null && Flight.Remarks.Length > 0) {
				while (s < Root.Count && Root[s].Caption != "Remarks") {
					if (Root[s].Count > 0)
						Root[s].RemoveRange (0, Root[s].Count, UITableViewRowAnimation.Fade);
					Root.RemoveAt (s, UITableViewRowAnimation.Fade);
				}
				
				if (s == Root.Count) {
					Root.Insert (s, UITableViewRowAnimation.Fade, new Section ("Remarks", Flight.Remarks));
				} else {
					Root[s].Footer = Flight.Remarks;
					Root.Reload (Root[s], UITableViewRowAnimation.None);
				}
				
				s++;
			}
			
			while (s < Root.Count) {
				if (Root[s].Count > 0)
					Root[s].RemoveRange (0, Root[s].Count, UITableViewRowAnimation.Fade);
				Root.RemoveAt (s, UITableViewRowAnimation.Fade);
			}
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (flight != null)
				UpdateDetails ();
		}
		
		void OnEditorClosed (object sender, EventArgs args)
		{
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
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}
