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
		StringElement date, aircraft, departed, arrived, visited;
		StringElement total, cfi, pic, sic, dual, xc, night;
		StringElement actual, hood, simulator, approaches;
		StringElement dayLandings, nightLandings;
		EditFlightDetailsViewController editor;
		MultilineElement remarks;
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
			section.Add (date = new StringElement ("Date"));
			section.Add (aircraft = new StringElement ("Aircraft"));
			section.Add (departed = new StringElement ("Departed"));
			section.Add (visited = new StringElement ("Visited"));
			section.Add (arrived = new StringElement ("Arrived"));
			Root.Add (section);
			
			section = new Section ("Flight Experience");
			section.Add (total = new StringElement ("Flight Time"));
			section.Add (cfi = new StringElement ("C.F.I."));
			section.Add (pic = new StringElement ("P.I.C."));
			section.Add (sic = new StringElement ("S.I.C."));
			section.Add (dual = new StringElement ("Dual Received"));
			section.Add (xc = new StringElement ("Cross Country"));
			section.Add (night = new StringElement ("Night Flying"));
			section.Add (dayLandings = new StringElement ("Day Landings"));
			section.Add (nightLandings = new StringElement ("Night Landings"));
			Root.Add (section);
			
			section = new Section ("Instrument Experience");
			section.Add (actual = new StringElement ("Actual Time"));
			section.Add (hood = new StringElement ("Hood Time"));
			section.Add (simulator = new StringElement ("Simulator Time"));
			section.Add (approaches = new StringElement ("Approaches"));
			Root.Add (section);
			
			section = new Section ("Remarks");
			section.Add (remarks = new MultilineElement (""));
			Root.Add (section);
			
			edit = new UIBarButtonItem (UIBarButtonSystemItem.Edit, OnEditClicked);
			NavigationItem.LeftBarButtonItem = edit;
			
			base.LoadView ();
		}
		
		static string FormatFlightTime (int seconds)
		{
			return Math.Round (seconds / 3600.0, 1).ToString () + " hours";
		}
		
		void UpdateDetails ()
		{
			Title = string.Format ("{0} to {1} on {2}", Flight.AirportDeparted,
				Flight.AirportArrived, Flight.Date.ToShortDateString ());
			
			List<string> visitedList = new List<string> ();
			if (Flight.AirportVisited1 != null && Flight.AirportVisited1.Length > 0)
				visitedList.Add (Flight.AirportVisited1);
			if (Flight.AirportVisited2 != null && Flight.AirportVisited2.Length > 0)
				visitedList.Add (Flight.AirportVisited2);
			if (Flight.AirportVisited3 != null && Flight.AirportVisited3.Length > 0)
				visitedList.Add (Flight.AirportVisited3);
			
			date.Value = Flight.Date.ToLongDateString ();
			aircraft.Value = Flight.Aircraft;
			departed.Value = Flight.AirportDeparted;
			visited.Value = string.Join (", ", visitedList.ToArray ());
			arrived.Value = Flight.AirportArrived;
			
			total.Value = FormatFlightTime (Flight.FlightTime);
			cfi.Value = FormatFlightTime (Flight.CertifiedFlightInstructor);
			pic.Value = FormatFlightTime (Flight.PilotInCommand);
			sic.Value = FormatFlightTime (Flight.SecondInCommand);
			dual.Value = FormatFlightTime (Flight.DualReceived);
			xc.Value = FormatFlightTime (Flight.CrossCountry);
			night.Value = FormatFlightTime (Flight.Night);
			dayLandings.Value = Flight.DayLandings.ToString ();
			nightLandings.Value = Flight.NightLandings.ToString ();
			
			actual.Value = FormatFlightTime (Flight.InstrumentActual);
			hood.Value = FormatFlightTime (Flight.InstrumentHood);
			simulator.Value = FormatFlightTime (Flight.InstrumentSimulator);
			approaches.Value = Flight.InstrumentApproaches.ToString ();
			
			remarks.Value = Flight.Remarks;
			
			foreach (var section in Root)
				Root.Reload (section, UITableViewRowAnimation.None);
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
				return true;
			}
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}
