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
		/// Gets all of the registered aircraft.
		/// </summary>
		/// <returns>
		/// All of the registered aircraft.
		/// </returns>
		public static IEnumerable<Aircraft> GetAllAircraft ()
		{
			return sqlitedb.Table<Aircraft> ();
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
		/// Delete the specified Flight entry.
		/// </summary>
		/// <param name='flight'>
		/// The Flight entry to delete.
		/// </param>
		public static bool Delete (Flight flight)
		{
			return sqlitedb.Delete<Flight> (flight) > 0;
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
				flight.OnUpdated ();
				return true;
			}
			
			return false;
		}
		
		/// <summary>
		/// Gets all of the logged flights.
		/// </summary>
		/// <returns>
		/// All of the logged flights.
		/// </returns>
		public static IEnumerable<Flight> GetAllFlights ()
		{
			return from flight in sqlitedb.Table<Flight> ()
				   orderby flight.Date descending
				   select flight;
		}
		
		/// <summary>
		/// Gets the logged flight between the specified dates.
		/// </summary>
		/// <returns>
		/// The logged flights between the specified dates.
		/// </returns>
		/// <param name='start'>
		/// The start date.
		/// </param>
		/// <param name='end'>
		/// The end date.
		/// </param>
		public static IEnumerable<Flight> GetFlights (DateTime start, DateTime end)
		{
			return from flight in sqlitedb.Table<Flight> ()
				   where (flight.Date >= start && flight.Date <= end)
				   orderby flight.Date
				   select flight;
		}
		
		/// <summary>
		/// Gets the logged flights since the specified date.
		/// </summary>
		/// <returns>
		/// The logged flights since the specified date.
		/// </returns>
		/// <param name='since'>
		/// The start date.
		/// </param>
		public static IEnumerable<Flight> GetFlights (DateTime since)
		{
			return from flight in sqlitedb.Table<Flight> ()
				   where flight.Date >= since
				   orderby flight.Date
				   select flight;
		}
		
		/// <summary>
		/// Gets the logged flights flown with the specified aircraft.
		/// </summary>
		/// <returns>
		/// The logged flights flown with the specified aircraft.
		/// </returns>
		/// <param name='tailNumber'>
		/// The tail number of the aircraft.
		/// </param>
		public static IEnumerable<Flight> GetFlights (string tailNumber)
		{
			return from flight in sqlitedb.Table<Flight> ()
				   where flight.Aircraft == tailNumber
				   orderby flight.Date
				   select flight;
		}
		
		/// <summary>
		/// Gets the logged flights flown with the specified aircraft since the specified date.
		/// </summary>
		/// <returns>
		/// The logged flights flown with the specified aircraft since the specified date.
		/// </returns>
		/// <param name='tailNumber'>
		/// The tail number of the aircraft.
		/// </param>
		/// <param name='since'>
		/// The start date.
		/// </param>
		public static IEnumerable<Flight> GetFlights (string tailNumber, DateTime since)
		{
			return from flight in sqlitedb.Table<Flight> ()
				   where flight.Aircraft == tailNumber
				   where flight.Date >= since
				   orderby flight.Date
				   select flight;
		}
		#endregion
	}
}
