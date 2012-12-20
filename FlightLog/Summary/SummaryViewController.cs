// 
// SummaryViewController.cs
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
using System.Collections;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class SummaryViewController : DialogViewController
	{
		public SummaryViewController () : base (UITableViewStyle.Grouped, new RootElement (null))
		{
			TabBarItem.Image = UIImage.FromBundle ("Images/ekg");
			EnableSearch = false;
			Title = "Summary";
		}

		static DateTime GetMonthsAgo (int months)
		{
			return DateTime.Today.AddMonths (-months);
		}

		void LoadFlightTimeTotals ()
		{
			DateTime twelveMonthsAgo = GetMonthsAgo (12);
			DateTime sixMonthsAgo = GetMonthsAgo (6);
			int last12months = 0;
			int last6months = 0;
			int total = 0;

			foreach (var flight in LogBook.GetAllFlights ()) {
				if (flight.Date >= sixMonthsAgo) {
					last12months += flight.FlightTime;
					last6months += flight.FlightTime;
				} if (flight.Date >= twelveMonthsAgo) {
					last12months += flight.FlightTime;
				}
				total += flight.FlightTime;
			}

			Section totals = new Section ("Totals") {
				new StringElement ("Total", FlightExtension.FormatFlightTime (total, true)),
				new StringElement ("12 Months", FlightExtension.FormatFlightTime (last12months, true)),
				new StringElement ("6 Months", FlightExtension.FormatFlightTime (last6months, true)),
			};

			Root.Add (totals);
		}
		
		void AddLandingCurrency (Section section, List<Aircraft> list, AircraftClassification @class, bool night, bool tailDragger)
		{
			string caption = string.Format ("{0} Current{1}", night ? "Night" : "Day", tailDragger ? " (TailDragger)" : "");
			DateTime oldestLanding = DateTime.Now;
			int landings = 0;
			
			foreach (var flight in LogBook.GetFlightsForPassengerCurrencyRequirements (list, night)) {
				landings += flight.NightLandings;
				
				if (!night)
					landings += flight.DayLandings;
				
				oldestLanding = flight.Date;
				
				if (landings >= 3) {
					section.Add (new CurrencyElement (caption, oldestLanding.AddDays (90)));
					return;
				}
			}
			
			// currency is out of date
			section.Add (new CurrencyElement (caption, DateTime.Now));
		}
		
		void LoadDayAndNightCurrency ()
		{
			// Day/Night currency is per-AircraftClassification and TailDragger vs not.
			foreach (var value in Enum.GetValues (typeof (AircraftClassification))) {
				AircraftClassification @class = (AircraftClassification) value;
				
				List<Aircraft> list = LogBook.GetAircraft (@class, false);
				if (list == null || list.Count == 0)
					continue;
				
				AircraftCategory category = Aircraft.GetCategoryFromClass (@class);
				Section section;
				string caption;
				
				if (category == AircraftCategory.Airplane)
					caption = string.Format ("{0} {1}", category.ToHumanReadableName (), @class.ToHumanReadableName ());
				else
					caption = @class.ToHumanReadableName ();
				
				section = new Section (caption);
				
				// Only Airplanes can be tail-draggers
				if (category == AircraftCategory.Airplane) {
					List<Aircraft> taildraggers = new List<Aircraft> ();
					foreach (var aircraft in list) {
						if (aircraft.IsTailDragger)
							taildraggers.Add (aircraft);
					}
					
					if (taildraggers.Count > 0) {
						AddLandingCurrency (section, taildraggers, @class, false, true);
						AddLandingCurrency (section, taildraggers, @class, true, true);
					}
				}
				
				AddLandingCurrency (section, list, @class, false, false);
				AddLandingCurrency (section, list, @class, true, false);
				
				Root.Add (section);
			}
		}
		
		static DateTime GetInstrumentCurrencyExipirationDate (DateTime oldest)
		{
			TimeSpan offset = new TimeSpan (oldest.Day, oldest.Hour, oldest.Minute, oldest.Second, oldest.Millisecond);
			DateTime expires = oldest.Subtract (offset).AddMonths (7);
			
			return expires;
		}
		
		void AddInstrumentCurrency (Section section, List<Aircraft> list, AircraftCategory category)
		{
			string caption = "Instrument Current";
			DateTime oldestApproach = DateTime.Now;
			int approaches = 0;
			
			foreach (var flight in LogBook.GetFlightsForInstrumentCurrencyRequirements (list)) {
				approaches += flight.InstrumentApproaches;
				oldestApproach = flight.Date;
				
				if (approaches >= 6) {
					DateTime expires = GetInstrumentCurrencyExipirationDate (oldestApproach);
					section.Add (new CurrencyElement (caption, expires));
					return;
				}
			}

			// currency is out of date
			section.Add (new CurrencyElement (caption, DateTime.Today));
		}
		
		void LoadInstrumentCurrency ()
		{
			// Instrument currency is per-AircraftCategory
			foreach (var value in Enum.GetValues (typeof (AircraftCategory))) {
				AircraftCategory category = (AircraftCategory) value;
				List<Aircraft> list = LogBook.GetAircraft (category, false);
				if (list == null || list.Count == 0)
					continue;
				
				Section section = new Section (category.ToHumanReadableName ());
				AddInstrumentCurrency (section, list, category);
				Root.Add (section);
			}
		}
		
		void LoadSummary ()
		{
			LoadFlightTimeTotals ();
			LoadDayAndNightCurrency ();
			LoadInstrumentCurrency ();
		}
		
		public override void ViewWillAppear (bool animated)
		{
			Root.Clear ();
			LoadSummary ();
			
			base.ViewWillAppear (animated);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}
