// 
// StatusViewController.cs
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
	public class Totals
	{
		public object Property { get; private set; }
		public string Title { get; private set; }
		public int Last12Months;
		public int Last6Months;
		public int Total;

		public Totals (string title, object property)
		{
			Property = property;
			Title = title;
		}
	}

	public class StatusViewController : DialogViewController
	{
		bool dirty = true;
		bool disposed;

		public StatusViewController () : base (UITableViewStyle.Grouped, new RootElement (null))
		{
			TabBarItem.Image = UIImage.FromBundle ("Images/ekg");
			LogBook.PilotUpdated += PilotChanged;
			LogBook.AircraftAdded += AircraftChanged;
			LogBook.AircraftDeleted += AircraftChanged;
			LogBook.AircraftUpdated += AircraftChanged;
			LogBook.FlightAdded += FlightChanged;
			LogBook.FlightDeleted += FlightChanged;
			LogBook.FlightUpdated += FlightChanged;
			EnableSearch = false;
			Title = "Status";
		}

		void PilotChanged (object sender, EventArgs e)
		{
			dirty = true;
		}

		void AircraftChanged (object sender, AircraftEventArgs e)
		{
			dirty = true;
		}

		void FlightChanged (object sender, FlightEventArgs e)
		{
			dirty = true;
		}

		static DateTime GetMonthsAgo (int months)
		{
			return DateTime.Today.AddMonths (-months);
		}

		void LoadFlightTimeTotals ()
		{
			List<Totals> totals = new List<Totals> ();
			DateTime twelveMonthsAgo = GetMonthsAgo (12);
			DateTime sixMonthsAgo = GetMonthsAgo (6);
			Dictionary<string, Aircraft> dict;
			Aircraft aircraft;
			int time;

			dict = new Dictionary<string, Aircraft> ();
			foreach (var craft in LogBook.GetAllAircraft ())
				dict.Add (craft.TailNumber, craft);

			totals.Add (new Totals ("Flight Time Totals", FlightProperty.FlightTime));
			totals.Add (new Totals ("Pilot-in-Command Totals", FlightProperty.PilotInCommand));
			totals.Add (new Totals ("Certified Flight Instructor Totals", FlightProperty.CertifiedFlightInstructor));
			totals.Add (new Totals ("Cross-Country Totals", FlightProperty.CrossCountry));
			totals.Add (new Totals ("Cross-Country (PIC) Totals", FlightProperty.CrossCountryPIC));
			totals.Add (new Totals ("Night Totals", FlightProperty.Night));
			totals.Add (new Totals ("Instrument (Actual) Totals", FlightProperty.InstrumentActual));
			totals.Add (new Totals ("Instrument (Hood) Totals", FlightProperty.InstrumentHood));

			if (LogBook.Pilot.Endorsements.HasFlag (AircraftEndorsement.Complex))
				totals.Add (new Totals ("Complex Totals", AircraftProperty.IsComplex));
			if (LogBook.Pilot.Endorsements.HasFlag (AircraftEndorsement.HighPerformance))
				totals.Add (new Totals ("High-Performance Totals", AircraftProperty.IsHighPerformance));
			if (LogBook.Pilot.Endorsements.HasFlag (AircraftEndorsement.TailDragger))
				totals.Add (new Totals ("Taildragger Totals", AircraftProperty.IsTailDragger));

			foreach (AircraftClassification @class in Enum.GetValues (typeof (AircraftClassification))) {
				AircraftCategory category = Aircraft.GetCategoryFromClass (@class);
				string title = @class.ToHumanReadableName () + " Totals";

				if (category == AircraftCategory.Airplane)
					title = "Airplane " + title;

				totals.Add (new Totals (title, @class));
			}

			foreach (var flight in LogBook.GetAllFlights ()) {
				if (!dict.TryGetValue (flight.Aircraft, out aircraft))
					continue;

				if (aircraft.IsSimulator)
					continue;

				foreach (var total in totals) {
					if (total.Property is FlightProperty) {
						time = flight.GetFlightTime ((FlightProperty) total.Property);
					} else if (total.Property is AircraftProperty) {
						if (!((bool) aircraft.GetValue ((AircraftProperty) total.Property)))
							continue;

						time = flight.FlightTime;
					} else {
						if (aircraft.Classification != (AircraftClassification) total.Property)
							continue;

						time = flight.FlightTime;
					}

					if (flight.Date >= sixMonthsAgo) {
						total.Last12Months += time;
						total.Last6Months += time;
					} else if (flight.Date >= twelveMonthsAgo) {
						total.Last12Months += time;
					}

					total.Total += time;
				}
			}

			var aircraftTotals = new RootElement ("By Aircraft Category...");
			var otherTotals = new RootElement ("Other Totals");
			for (int i = 1; i < totals.Count; i++) {
				if (totals[i].Total == 0)
					continue;

				if (totals[i].Property is FlightProperty) {
					otherTotals.Add (new Section (totals[i].Title) {
						new StringElement ("Total", FlightExtension.FormatFlightTime (totals[i].Total, true)),
						new StringElement ("12 Months", FlightExtension.FormatFlightTime (totals[i].Last12Months, true)),
						new StringElement ("6 Months", FlightExtension.FormatFlightTime (totals[i].Last6Months, true)),
					});
				} else {
					aircraftTotals.Add (new Section (totals[i].Title) {
						new StringElement ("Total", FlightExtension.FormatFlightTime (totals[i].Total, true)),
						new StringElement ("12 Months", FlightExtension.FormatFlightTime (totals[i].Last12Months, true)),
						new StringElement ("6 Months", FlightExtension.FormatFlightTime (totals[i].Last6Months, true)),
					});
				}
			}

			Root.Add (new Section (totals[0].Title) {
				new StringElement ("Total", FlightExtension.FormatFlightTime (totals[0].Total, true)),
				new StringElement ("12 Months", FlightExtension.FormatFlightTime (totals[0].Last12Months, true)),
				new StringElement ("6 Months", FlightExtension.FormatFlightTime (totals[0].Last6Months, true)),
				aircraftTotals,
				otherTotals
			});
		}

		void AddLandingCurrency (Section section, List<Aircraft> aircraft, bool night)
		{
			string caption = string.Format ("{0} Current", night ? "Night" : "Day");
			DateTime oldestLanding = DateTime.Now;
			int landings = 0;

			if (aircraft != null && aircraft.Count > 0) {
				foreach (var flight in LogBook.GetFlightsForPassengerCurrencyRequirements (aircraft, night)) {
					landings += flight.NightLandings;

					if (!night)
						landings += flight.DayLandings;

					oldestLanding = flight.Date;

					if (landings >= 3) {
						section.Add (new CurrencyElement (caption, oldestLanding.AddDays (90)));
						return;
					}
				}
			}
			
			// currency is out of date
			section.Add (new CurrencyElement (caption, DateTime.Now));
		}
		
		void LoadDayAndNightCurrency ()
		{
			if (LogBook.Pilot.Endorsements.HasFlag (AircraftEndorsement.TailDragger)) {
				var list = LogBook.GetAircraft (AircraftCategory.Airplane, false);
				List<Aircraft> taildraggers = new List<Aircraft> ();

				foreach (var aircraft in list) {
					if (aircraft.IsTailDragger)
						taildraggers.Add (aircraft);
				}

				var section = new Section ("Taildragger Currency");
				AddLandingCurrency (section, taildraggers, false);
				AddLandingCurrency (section, taildraggers, true);
				Root.Add (section);
			}

			// Day/Night currency is per-AircraftClassification
			foreach (AircraftClassification @class in Enum.GetValues (typeof (AircraftClassification))) {
				AircraftCategory category = Aircraft.GetCategoryFromClass (@class);
				AircraftEndorsement endorsement;

				if (!Enum.TryParse<AircraftEndorsement> (@class.ToString (), out endorsement))
					continue;

				if (!LogBook.Pilot.Endorsements.HasFlag (endorsement))
					continue;

				var list = LogBook.GetAircraft (@class, false);
				string caption;
				
				if (category == AircraftCategory.Airplane)
					caption = "Airplane " + @class.ToHumanReadableName ();
				else
					caption = @class.ToHumanReadableName ();
				
				var section = new Section (string.Format ("{0} Currency", caption));
				AddLandingCurrency (section, list, false);
				AddLandingCurrency (section, list, true);
				Root.Add (section);
			}
		}
		
		static DateTime GetInstrumentCurrencyExipirationDate (DateTime oldest)
		{
			TimeSpan offset = new TimeSpan (oldest.Day, oldest.Hour, oldest.Minute, oldest.Second, oldest.Millisecond);
			DateTime expires = oldest.Subtract (offset).AddMonths (7);
			
			return expires;
		}
		
		void AddInstrumentCurrency (Section section, string caption, List<Aircraft> aircraft)
		{
			DateTime oldestApproach = DateTime.Now;
			int approaches = 0;
			int holds = 0;

			if (aircraft != null && aircraft.Count > 0) {
				foreach (var flight in LogBook.GetFlightsForInstrumentCurrencyRequirements (aircraft)) {
					approaches += flight.InstrumentApproaches;
					if (flight.InstrumentHoldingProcedures)
						holds++;

					oldestApproach = flight.Date;

					if (approaches >= 6 && holds > 0) {
						DateTime expires = GetInstrumentCurrencyExipirationDate (oldestApproach);
						section.Add (new CurrencyElement (caption, expires));
						return;
					}
				}
			}

			// currency is out of date
			section.Add (new CurrencyElement (caption, DateTime.Today));
		}
		
		void LoadInstrumentCurrency ()
		{
			Section section = new Section ("Instrument Currency");
			List<Aircraft> list;

			// Instrument currency is per-AircraftCategory
			if (LogBook.Pilot.InstrumentRatings.HasFlag (InstrumentRating.Airplane)) {
				list = LogBook.GetAircraft (AircraftCategory.Airplane, false);
				AddInstrumentCurrency (section, "Airplane", list);
			}

			if (LogBook.Pilot.InstrumentRatings.HasFlag (InstrumentRating.Helicopter)) {
				list = LogBook.GetAircraft (AircraftClassification.Helicoptor, false);
				AddInstrumentCurrency (section, "Helicopter", list);
			}

			if (LogBook.Pilot.InstrumentRatings.HasFlag (InstrumentRating.PoweredLift)) {
				list = LogBook.GetAircraft (AircraftClassification.PoweredLift, false);
				AddInstrumentCurrency (section, "Powered-Lift", list);
			}

			if (section.Count > 0)
				Root.Add (section);
		}
		
		void LoadSummary ()
		{
			LoadFlightTimeTotals ();
			LoadDayAndNightCurrency ();
			LoadInstrumentCurrency ();
		}

		public override void ViewWillAppear (bool animated)
		{
			if (dirty) {
				Root.Clear ();
				LoadSummary ();
				dirty = false;
			}
			
			base.ViewWillAppear (animated);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing && !disposed) {
				LogBook.PilotUpdated -= PilotChanged;
				LogBook.AircraftAdded -= AircraftChanged;
				LogBook.AircraftDeleted -= AircraftChanged;
				LogBook.AircraftUpdated -= AircraftChanged;
				LogBook.FlightAdded -= FlightChanged;
				LogBook.FlightDeleted -= FlightChanged;
				LogBook.FlightUpdated -= FlightChanged;
				disposed = true;
			}

			base.Dispose (disposing);
		}
	}
}
