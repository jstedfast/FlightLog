//
// FlightExtensions.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc.
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
using System.Collections.Generic;

namespace FlightLog
{
	public enum FlightProperty {
		Date,
		Aircraft,
		[HumanReadableName ("Departed")]
		AirportDeparted,
		[HumanReadableName ("Visited")]
		AirportVisited,
		[HumanReadableName ("Arrived")]
		AirportArrived,

		[HumanReadableName ("Flight Time")]
		FlightTime,
		[HumanReadableName ("Certified Flight Instructor")]
		CertifiedFlightInstructor,
		[HumanReadableName ("Dual Received")]
		DualReceived,
		[HumanReadableName ("Pilot In Command")]
		PilotInCommand,
		[HumanReadableName ("Second In Command")]
		SecondInCommand,
		[HumanReadableName ("Night Flying")]
		Night,
		[HumanReadableName ("Day Landings")]
		DayLandings,
		[HumanReadableName ("Night Landings")]
		NightLandings,

		[HumanReadableName ("Actual Time")]
		InstrumentActual,
		[HumanReadableName ("Hood Time")]
		InstrumentHood,
		[HumanReadableName ("Simulator Time")]
		InstrumentSimulator,
		[HumanReadableName ("Approaches")]
		InstrumentApproaches,
		[HumanReadableName ("Performed Holding Procedures")]
		InstrumentHoldingProcedures,
		[HumanReadableName ("Acted as Safety Pilot")]
		ActingInstrumentSafetyPilot,
		[HumanReadableName ("Safety Pilot")]
		InstrumentSafetyPilot,

		Remarks,
	}

	public static class FlightExtension
	{
		static string GetFlightAirportsVisited (Flight flight)
		{
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

		static string FormatFlightTime (int seconds, bool force)
		{
			if (seconds == 0 && !force)
				return null;

			double time = Math.Round (seconds / 3600.0, 1);

			if (time > 0.9 && time < 1.1)
				return "1 hour";

			return time.ToString () + " hours";
		}

		public static string GetValue (this Flight flight, FlightProperty property)
		{
			switch (property) {
			case FlightProperty.Date:
				return flight != null ? flight.Date.ToLongDateString () : string.Empty;
			case FlightProperty.Aircraft:
				return flight != null ? flight.Aircraft : string.Empty;
			case FlightProperty.AirportDeparted:
				return flight != null ? flight.AirportDeparted : string.Empty;
			case FlightProperty.AirportVisited:
				return flight != null ? GetFlightAirportsVisited (flight) : null;
			case FlightProperty.AirportArrived:
				return flight != null ? flight.AirportArrived : string.Empty;
			case FlightProperty.FlightTime:
				return flight != null ? FormatFlightTime (flight.FlightTime, true) : string.Empty;
			case FlightProperty.CertifiedFlightInstructor:
				return flight != null ? FormatFlightTime (flight.CertifiedFlightInstructor, false) : null;
			case FlightProperty.DualReceived:
				return flight != null ? FormatFlightTime (flight.DualReceived, false) : null;
			case FlightProperty.PilotInCommand:
				return flight != null ? FormatFlightTime (flight.PilotInCommand, false) : null;
			case FlightProperty.SecondInCommand:
				return flight != null ? FormatFlightTime (flight.SecondInCommand, false) : null;
			case FlightProperty.Night:
				return flight != null ? FormatFlightTime (flight.Night, false) : null;
			case FlightProperty.DayLandings:
				return flight != null && flight.DayLandings > 0 ? flight.DayLandings.ToString () : null;
			case FlightProperty.NightLandings:
				return flight != null && flight.NightLandings > 0 ? flight.NightLandings.ToString () : null;
			case FlightProperty.InstrumentActual:
				return flight != null ? FormatFlightTime (flight.InstrumentActual, false) : null;
			case FlightProperty.InstrumentHood:
				return flight != null ? FormatFlightTime (flight.InstrumentHood, false) : null;
			case FlightProperty.InstrumentSimulator:
				return flight != null ? FormatFlightTime (flight.InstrumentSimulator, false) : null;
			case FlightProperty.InstrumentApproaches:
				return flight != null && flight.InstrumentApproaches > 0 ? flight.InstrumentApproaches.ToString () : null;
			case FlightProperty.InstrumentHoldingProcedures:
				return flight != null && flight.InstrumentHoldingProcedures ? "yes" : null;
			case FlightProperty.ActingInstrumentSafetyPilot:
				return flight != null && flight.ActingInstrumentSafetyPilot ? "yes" : null;
			case FlightProperty.InstrumentSafetyPilot:
				return flight != null && !string.IsNullOrEmpty (flight.InstrumentSafetyPilot) ? flight.InstrumentSafetyPilot : null;
			case FlightProperty.Remarks:
				return flight != null && !string.IsNullOrEmpty (flight.Remarks) ? flight.Remarks : null;
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}
	}
}
