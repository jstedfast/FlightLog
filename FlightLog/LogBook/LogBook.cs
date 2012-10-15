// 
// LogBook.cs
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
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using MonoTouch.SQLite;

namespace FlightLog {
	public static class LogBook
	{
		static SQLiteConnection sqlitedb;
		
		public static void Init ()
		{
			string docsDir = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			string logbook = Path.Combine (docsDir, "LogBook.sqlite");
			
			sqlitedb = new SQLiteConnection (logbook, true);
			sqlitedb.CreateTable<Flight> ();
			sqlitedb.CreateTable<Aircraft> ();
		}
		
		public static SQLiteConnection SQLiteDB {
			get { return sqlitedb; }
		}
		
		#region Aircraft
		/// <summary>
		/// Event that occurs when a new Aircraft is added to the LogBook.
		/// </summary>
		public static event EventHandler<AircraftEventArgs> AircraftAdded;
		
		static void OnAircraftAdded (Aircraft aircraft)
		{
			var handler = AircraftAdded;
			
			if (handler != null)
				handler (null, new AircraftEventArgs (aircraft));
		}
		
		/// <summary>
		/// Add the specified aircraft to the LogBook.
		/// </summary>
		/// <param name='aircraft'>
		/// The aircraft to add to the LogBook.
		/// </param>
		public static bool Add (Aircraft aircraft)
		{
			if (Contains (aircraft))
				return false;
			
			if (sqlitedb.Insert (aircraft) > 0) {
				OnAircraftAdded (aircraft);
				return true;
			}
			
			return false;
		}
		
		/// <summary>
		/// Determines whether the specified aircraft can be deleted.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the specified aircraft can be deleted; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='aircraft'>
		/// The aircraft to to delete.
		/// </param>
		public static bool CanDelete (Aircraft aircraft)
		{
			return sqlitedb.Query<Flight> ("select 1 from Flight where Aircraft = ?", aircraft.TailNumber).Count == 0;
		}
		
		/// <summary>
		/// Checks whether or not the LogBook contains the specified aircraft.
		/// </summary>
		/// <returns>
		/// <c>true</c> if the specified aircraft is already in the LogBook; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='aircraft'>
		/// The aircraft to check for.
		/// </param>
		public static bool Contains (Aircraft aircraft)
		{
			return sqlitedb.Query<Flight> ("select 1 from Aircraft where TailNumber = ?", aircraft.TailNumber).Count == 1;
		}
		
		/// <summary>
		/// Event that occurs when an Aircraft is deleted from the LogBook.
		/// </summary>
		public static event EventHandler<AircraftEventArgs> AircraftDeleted;
		
		static void OnAircraftDeleted (Aircraft aircraft)
		{
			var handler = AircraftDeleted;
			
			if (handler != null)
				handler (null, new AircraftEventArgs (aircraft));
		}
		
		/// <summary>
		/// Delete the specified aircraft from the LogBook.
		/// </summary>
		/// <param name='aircraft'>
		/// The aircraft to delete from the LogBook.
		/// </param>
		public static bool Delete (Aircraft aircraft)
		{
			if (!CanDelete (aircraft))
				return false;
			
			if (sqlitedb.Delete<Aircraft> (aircraft) > 0) {
				PhotoManager.Delete (aircraft.TailNumber);
				OnAircraftDeleted (aircraft);
				return true;
			}
			
			return false;
		}
		
		/// <summary>
		/// Event that occurs when an Aircraft will update. Always followed by either
		/// <see cref="AircraftUpdated"/> or <see cref="AircraftUpdateFailed"/>.
		/// </summary>
		public static event EventHandler<AircraftEventArgs> AircraftWillUpdate;
		
		static void OnAircraftWillUpdate (Aircraft aircraft)
		{
			var handler = AircraftWillUpdate;
			
			if (handler != null)
				handler (null, new AircraftEventArgs (aircraft));
		}
		
		/// <summary>
		/// Event that occurs when an Aircraft is updated in the LogBook.
		/// </summary>
		public static event EventHandler<AircraftEventArgs> AircraftUpdated;
		
		static void OnAircraftUpdated (Aircraft aircraft)
		{
			var handler = AircraftUpdated;
			
			if (handler != null)
				handler (null, new AircraftEventArgs (aircraft));
		}
		
		/// <summary>
		/// Event that occurs when an Aircraft update fails.
		/// </summary>
		public static event EventHandler<AircraftEventArgs> AircraftUpdateFailed;
		
		static void OnAircraftUpdateFailed (Aircraft aircraft)
		{
			var handler = AircraftUpdateFailed;
			
			if (handler != null)
				handler (null, new AircraftEventArgs (aircraft));
		}
		
		/// <summary>
		/// Update the specified aircraft.
		/// </summary>
		/// <param name='aircraft'>
		/// The aircraft to update.
		/// </param>
		public static bool Update (Aircraft aircraft)
		{
			OnAircraftWillUpdate (aircraft);
			
			if (sqlitedb.Update (aircraft) > 0) {
				OnAircraftUpdated (aircraft);
				aircraft.OnUpdated ();
				return true;
			} else {
				OnAircraftUpdateFailed (aircraft);
			}
			
			return false;
		}
		
		/// <summary>
		/// Gets a list of all of the registered aircraft.
		/// </summary>
		/// <returns>
		/// A list of all of the registered aircraft.
		/// </returns>
		public static List<Aircraft> GetAllAircraft ()
		{
			return sqlitedb.Query<Aircraft> ("select * from Aircraft");
		}
		
		/// <summary>
		/// Gets a list of all of the registered aircraft.
		/// </summary>
		/// <returns>
		/// A list of all of the registered aircraft.
		/// </returns>
		/// <param name='includeSimulators'>
		/// Specifies whether or not simulators should be included.
		/// </param>
		public static List<Aircraft> GetAllAircraft (bool includeSimulators)
		{
			if (includeSimulators)
				return GetAllAircraft ();
			
			return sqlitedb.Query<Aircraft> ("select * from Aircraft where IsSimulator = ?", false);
		}
		
		/// <summary>
		/// Gets a list of aircraft up to a specified limit.
		/// </summary>
		/// <returns>
		/// A list of aircraft, up to the specified limit.
		/// </returns>
		/// <param name='limit'>
		/// The number of aircraft to limit the results to.
		/// </param>
		public static List<Aircraft> GetAircraft (int limit)
		{
			return sqlitedb.Query<Aircraft> ("select * from Aircraft limit ?", limit);
		}
		
		/// <summary>
		/// Gets a list of all of the aircraft of the specified category.
		/// </summary>
		/// <returns>
		/// A list of all of the aircraft of the specified category
		/// </returns>
		/// <param name='category'>
		/// The category of aircraft requested.
		/// </param>
		public static List<Aircraft> GetAircraft (AircraftCategory category)
		{
			AircraftClassification firstClass = (AircraftClassification) (int) category;
			AircraftClassification lastClass = firstClass + Aircraft.CategoryStep;
			
			return sqlitedb.Query<Aircraft> ("select * from Aircraft where Classification between ? and ?",
				firstClass, lastClass);
		}
		
		/// <summary>
		/// Gets a list of all of the aircraft of the specified category.
		/// </summary>
		/// <returns>
		/// A list of all of the aircraft of the specified category
		/// </returns>
		/// <param name='category'>
		/// The category of aircraft requested.
		/// </param>
		/// <param name='includeSimulators'>
		/// Specifies whether or not simulators should be included.
		/// </param>
		public static List<Aircraft> GetAircraft (AircraftCategory category, bool includeSimulators)
		{
			if (includeSimulators)
				return GetAircraft (category);
			
			AircraftClassification firstClass = (AircraftClassification) (int) category;
			AircraftClassification lastClass = firstClass + Aircraft.CategoryStep;
			
			return sqlitedb.Query<Aircraft> ("select * from Aircraft where Classification between ? and ? and IsSimulator = ?",
				firstClass, lastClass, false);
		}
		
		/// <summary>
		/// Gets a list of aircraft of the specified classification.
		/// </summary>
		/// <returns>
		/// A list of aircraft.
		/// </returns>
		/// <param name='classification'>
		/// The classification of aircraft requested.
		/// </param>
		public static List<Aircraft> GetAircraft (AircraftClassification classification)
		{
			return sqlitedb.Query<Aircraft> ("select * from Aircraft where Classification = ?", classification);
		}
		
		/// <summary>
		/// Gets a list of aircraft of the specified classification.
		/// </summary>
		/// <returns>
		/// A list of aircraft.
		/// </returns>
		/// <param name='classification'>
		/// The classification of aircraft requested.
		/// </param>
		/// <param name='includeSimulators'>
		/// Specifies whether or not simulators should be included.
		/// </param>
		public static List<Aircraft> GetAircraft (AircraftClassification classification, bool includeSimulators)
		{
			if (includeSimulators)
				return GetAircraft (classification);
			
			return sqlitedb.Query<Aircraft> ("select * from Aircraft where Classification = ? and IsSimulator = ?",
				classification, false);
		}
		
		/// <summary>
		/// Gets the aircraft specified by the given tail number.
		/// </summary>
		/// <returns>
		/// The aircraft specified by the given tail number.
		/// </returns>
		/// <param name='tailNumber'>
		/// The tail number of the desired aircraft.
		/// </param>
		public static Aircraft GetAircraft (string tailNumber)
		{
			var results = sqlitedb.Query<Aircraft> ("select * from Aircraft where TailNumber = ?", tailNumber);
			
			return results.Count > 0 ? results[0] : null;
		}
		
		/// <summary>
		/// Gets the aircraft matching the provided string.
		/// </summary>
		/// <returns>
		/// The list of matching aircraft.
		/// </returns>
		/// <param name='text'>
		/// The text to match against.
		/// </param>
		public static List<Aircraft> GetMatchingAircraft (string text)
		{
			return sqlitedb.Query<Aircraft> ("select * from Aircraft where TailNumber like ?", "%" + text + "%");
		}
		#endregion
		
		#region Flight Entries
		/// <summary>
		/// Event that occurs when a new Flight is added to the LogBook.
		/// </summary>
		public static event EventHandler<FlightEventArgs> FlightAdded;
		
		static void OnFlightAdded (Flight flight)
		{
			var handler = FlightAdded;
			
			if (handler != null)
				handler (null, new FlightEventArgs (flight));
		}
		
		/// <summary>
		/// Add the specified Flight entry.
		/// </summary>
		/// <param name='flight'>
		/// The Flight entry to add.
		/// </param>
		public static bool Add (Flight flight)
		{
			if (sqlitedb.Insert (flight) > 0) {
				OnFlightAdded (flight);
				return true;
			}
			
			return false;
		}
		
		/// <summary>
		/// Event that occurs when a Flight is deleted from the LogBook.
		/// </summary>
		public static event EventHandler<FlightEventArgs> FlightDeleted;
		
		static void OnFlightDeleted (Flight flight)
		{
			var handler = FlightDeleted;
			
			if (handler != null)
				handler (null, new FlightEventArgs (flight));
		}
		
		/// <summary>
		/// Delete the specified Flight entry.
		/// </summary>
		/// <param name='flight'>
		/// The Flight entry to delete.
		/// </param>
		public static bool Delete (Flight flight)
		{
			if (sqlitedb.Delete<Flight> (flight) > 0) {
				OnFlightDeleted (flight);
				return true;
			}
			
			return false;
		}
		
		/// <summary>
		/// Event that occurs when a Flight will update. Always followed by either
		/// <see cref="FlightUpdated"/> or <see cref="FlightUpdateFailed"/>.
		/// </summary>
		public static event EventHandler<FlightEventArgs> FlightWillUpdate;
		
		static void OnFlightWillUpdate (Flight flight)
		{
			var handler = FlightWillUpdate;
			
			if (handler != null)
				handler (null, new FlightEventArgs (flight));
		}
		
		/// <summary>
		/// Event that occurs when a Flight is updated in the LogBook.
		/// </summary>
		public static event EventHandler<FlightEventArgs> FlightUpdated;
		
		static void OnFlightUpdated (Flight flight)
		{
			var handler = FlightUpdated;
			
			if (handler != null)
				handler (null, new FlightEventArgs (flight));
		}
		
		/// <summary>
		/// Event that occurs when a Flight update fails.
		/// </summary>
		public static event EventHandler<FlightEventArgs> FlightUpdateFailed;
		
		static void OnFlightUpdateFailed (Flight flight)
		{
			var handler = FlightUpdateFailed;
			
			if (handler != null)
				handler (null, new FlightEventArgs (flight));
		}
		
		/// <summary>
		/// Update the specified Flight entry.
		/// </summary>
		/// <param name='entry'>
		/// The Flight entry to update.
		/// </param>
		public static bool Update (Flight flight)
		{
			OnFlightWillUpdate (flight);
			
			if (sqlitedb.Update (flight) > 0) {
				OnFlightUpdated (flight);
				flight.OnUpdated ();
				return true;
			} else {
				OnFlightUpdateFailed (flight);
			}
			
			return false;
		}
		
		/// <summary>
		/// Gets a list of all of the logged flights.
		/// </summary>
		/// <returns>
		/// A list of all of the logged flights.
		/// </returns>
		public static List<Flight> GetAllFlights ()
		{
			return sqlitedb.Query<Flight> ("select * from Flight order by Date desc");
		}
		
		/// <summary>
		/// Gets a list of logged flights, up to the specified limit.
		/// </summary>
		/// <returns>
		/// A list of logged flights, up to the specified limit.
		/// </returns>
		/// <param name='limit'>
		/// The limit.
		/// </param>
		public static List<Flight> GetFlights (int limit)
		{
			return sqlitedb.Query<Flight> ("select * from Flight order by Date desc limit ?", limit);
		}
		
		/// <summary>
		/// Gets a list of the logged flights between the specified dates.
		/// </summary>
		/// <returns>
		/// A list of the logged flights between the specified dates.
		/// </returns>
		/// <param name='start'>
		/// The start date.
		/// </param>
		/// <param name='end'>
		/// The end date.
		/// </param>
		public static List<Flight> GetFlights (DateTime start, DateTime end)
		{
			return sqlitedb.Query<Flight> ("select * from Flight where Date between ? and ? order by Date desc", start, end);
		}
		
		/// <summary>
		/// Gets a list of the logged flights since the specified date.
		/// </summary>
		/// <returns>
		/// A list of the logged flights since the specified date.
		/// </returns>
		/// <param name='since'>
		/// The start date.
		/// </param>
		public static List<Flight> GetFlights (DateTime since)
		{
			return sqlitedb.Query<Flight> ("select * from Flight where Date >= ? order by Date desc", since);
		}
		
		/// <summary>
		/// Gets a list of the logged flights flown with the specified aircraft.
		/// </summary>
		/// <returns>
		/// A list of the logged flights flown with the specified aircraft.
		/// </returns>
		/// <param name='aircraft'>
		/// The aircraft of interest.
		/// </param>
		public static List<Flight> GetFlights (Aircraft aircraft)
		{
			return sqlitedb.Query<Flight> ("select * from Flight where Aircraft = ? order by Date desc", aircraft.TailNumber);
		}
		
		/// <summary>
		/// Gets a list of the logged flights flown with the specified aircraft since the specified date.
		/// </summary>
		/// <returns>
		/// A list of the logged flights flown with the specified aircraft since the specified date.
		/// </returns>
		/// <param name='aircraft'>
		/// The aircraft of interest.
		/// </param>
		/// <param name='since'>
		/// The start date.
		/// </param>
		public static List<Flight> GetFlights (Aircraft aircraft, DateTime since)
		{
			return sqlitedb.Query<Flight> ("select * from Flight where Aircraft = ? and Date >= ? order by Date desc",
				aircraft.TailNumber, since);
		}
		
		/// <summary>
		/// Gets the most recent flights flown with the specified list of aircraft,
		/// going back no farther than the specified date.
		/// </summary>
		/// <returns>
		/// The flights matching the criterian provided.
		/// </returns>
		/// <param name='aircraft'>
		/// The list of aircraft.
		/// </param>
		/// <param name='earliest'>
		/// The earliest date to match back to.
		/// </param>
		/// <param name='limit'>
		/// The maximum number of matches to return.
		/// </param>
		public static List<Flight> GetFlights (List<Aircraft> aircraft, DateTime earliest, int limit)
		{
			if (aircraft.Count == 0)
				return new List<Flight> ();
			
			StringBuilder query = new StringBuilder ("select * from Flight where ");
			int i;
			
			if (aircraft.Count > 1) {
				query.Append ("Aircraft in (?");
				for (i = 1; i < aircraft.Count; i++)
					query.Append (", ?");
				query.Append (")");
			} else {
				query.Append ("Aircraft = ?");
			}
			
			query.Append (" and Date >= ? order by Date desc limit ?");
			
			object[] args = new object [aircraft.Count + 2];
			for (i = 0; i < aircraft.Count; i++)
				args[i] = aircraft[i].TailNumber;
			
			args[i++] = earliest;
			args[i++] = limit;
			
			return sqlitedb.Query<Flight> (query.ToString (), args);
		}
		
		static DateTime GetNinetyDaysAgo ()
		{
			DateTime today = DateTime.Today;
			
			return today.Subtract (new TimeSpan (90, 0, 0, 0, 0));
		}
		
		/// <summary>
		/// Gets a list of flights useful for checking FAA passenger currency requirements.
		/// </summary>
		/// <returns>
		/// The flights for passenger currency requirements.
		/// </returns>
		/// <param name='aircraft'>
		/// The list of aircraft that can count towards the requirements.
		/// </param>
		/// <param name='night'>
		/// Specifies whether or not the query should only include night landings.
		/// </param>
		public static List<Flight> GetFlightsForPassengerCurrencyRequirements (List<Aircraft> aircraft, bool night)
		{
			StringBuilder query = new StringBuilder ("select * from Flight where ");
			object[] args = new object [aircraft.Count + 3 + (night ? 0 : 1)];
			DateTime ninetyDaysAgo = GetNinetyDaysAgo ();
			int i = 1;
			
			args[0] = aircraft[0].TailNumber;
			
			if (aircraft.Count > 1) {
				query.Append ("Aircraft in (?");
				for (i = 1; i < aircraft.Count; i++) {
					args[i] = aircraft[i].TailNumber;
					query.Append (", ?");
				}
				query.Append (")");
			} else {
				query.Append ("Aircraft = ?");
			}
			
			query.Append (" and Date >= ?");
			args[i++] = ninetyDaysAgo;
			
			query.Append (" and (NightLandings > ?");
			args[i++] = 0;
			
			if (!night) {
				query.Append (" or DayLandings > ?");
				args[i++] = 0;
			}
			
			query.Append (") order by Date desc limit ?");
			// We only need 3 landings, which means at most 3 flights with 1 or more landings
			args[i++] = 3;
			
			return sqlitedb.Query<Flight> (query.ToString (), args);
		}
		
		// This actually gets the start of the month six months prior to the current month
		static DateTime GetSixMonthsAgo ()
		{
			DateTime today = DateTime.Today;
			TimeSpan thisMonth = new TimeSpan (today.Day - 1, 0, 0, 0, 0);
			TimeSpan oneDay = new TimeSpan (24, 0, 0);
			int months = 0;
			
			DateTime date = today.Subtract (thisMonth);
			
			while (months < 6) {
				date = date.Subtract (oneDay);
				thisMonth = new TimeSpan (date.Day - 1, 0, 0, 0, 0);
				date = date.Subtract (thisMonth);
				months++;
			}
			
			return date;
		}
		
		/// <summary>
		/// Gets a list of flights useful for checking FAA instrument currency requirements.
		/// </summary>
		/// <returns>
		/// The flights for instrument currency requirements.
		/// </returns>
		/// <param name='aircraft'>
		/// The list of aircraft that can count toward the requirements.
		/// </param>
		public static List<Flight> GetFlightsForInstrumentCurrencyRequirements (List<Aircraft> aircraft)
		{
			StringBuilder query = new StringBuilder ("select * from Flight where ");
			object[] args = new object [aircraft.Count + 3];
			DateTime sixMonthsAgo = GetSixMonthsAgo ();
			int i = 1;
			
			args[0] = aircraft[0].TailNumber;
			
			if (aircraft.Count > 1) {
				query.Append ("Aircraft in (?");
				for (i = 1; i < aircraft.Count; i++) {
					args[i] = aircraft[i].TailNumber;
					query.Append (", ?");
				}
				query.Append (")");
			} else {
				query.Append ("Aircraft = ?");
			}
			
			query.Append (" and Date >= ?");
			args[i++] = sixMonthsAgo;
			
			query.Append (" and InstrumentApproaches > ?");
			args[i++] = 0;
			
			query.Append (" order by Date desc limit ?");
			// We only need 6 approaches, which means at most 6 flights
			args[i++] = 6;
			
			return sqlitedb.Query<Flight> (query.ToString (), args);
		}
		#endregion
	}
}
