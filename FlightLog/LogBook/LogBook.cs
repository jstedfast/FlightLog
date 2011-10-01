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

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using SQLite;

namespace FlightLog {
	public static class LogBook
	{
		static SQLiteConnection sqlitedb;
		
		public static void Init ()
		{
			string docsDir = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			string logbook = Path.Combine (docsDir, "LogBook.sqlite");
			
			sqlitedb = new SQLiteConnection (logbook);
			sqlitedb.CreateTable<Flight> ();
			sqlitedb.CreateTable<Aircraft> ();
		}
		
		#region Aircraft
		/// <summary>
		/// Event that gets emitted when a new Aircraft is added to the LogBook.
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
		/// Delete the specified aircraft from the LogBook.
		/// </summary>
		/// <param name='aircraft'>
		/// The aircraft to delete from the LogBook.
		/// </param>
		public static bool Delete (Aircraft aircraft)
		{
			if (!CanDelete (aircraft))
				return false;
			
			PhotoManager.Delete (aircraft.TailNumber);
			
			return sqlitedb.Delete<Aircraft> (aircraft) > 0;
		}
		
		/// <summary>
		/// Update the specified aircraft.
		/// </summary>
		/// <param name='aircraft'>
		/// The aircraft to update.
		/// </param>
		public static bool Update (Aircraft aircraft)
		{
			if (sqlitedb.Update (aircraft) > 0) {
				aircraft.OnUpdated ();
				return true;
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
		/// Gets a list of aircraft of the specified classification.
		/// </summary>
		/// <returns>
		/// A list of aircraft.
		/// </returns>
		/// <param name='classification'>
		/// The classification of aircraft being requested.
		/// </param>
		public static List<Aircraft> GetAircraft (AircraftClassification classification)
		{
			return sqlitedb.Query<Aircraft> ("select * from Aircraft where Classification = ?", (int) classification);
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
		#endregion
		
		#region Flight Entries
		/// <summary>
		/// Event that gets emitted when a new Flight is added to the LogBook.
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
		/// Event that gets emitted when a Flight is deleted from the LogBook.
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
		/// Event that gets emitted when a Flight is updated in the LogBook.
		/// </summary>
		public static event EventHandler<FlightEventArgs> FlightUpdated;
		
		static void OnFlightUpdated (Flight flight)
		{
			var handler = FlightUpdated;
			
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
			if (sqlitedb.Update (flight) > 0) {
				OnFlightUpdated (flight);
				flight.OnUpdated ();
				return true;
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
		#endregion
	}
}
