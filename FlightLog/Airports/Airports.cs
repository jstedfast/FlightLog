// 
// Airports.cs
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
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using MonoTouch.CoreLocation;
using MonoTouch.MapKit;
using MonoTouch.SQLite;

namespace FlightLog {
	public enum AirportCode {
		FAA,
		IATA,
		ICAO,
		Any
	}
	
	public static class Airports
	{
		static SQLiteConnection sqlitedb;
		
		static Airports () { }
		
		public static void Init ()
		{
			sqlitedb = new SQLiteConnection ("Airports.sqlite");
			sqlitedb.CreateTable<Airport> ();
		}
		
		public static int Count {
			get {
				return sqlitedb.Table<Airport> ().Count ();
			}
		}
		
		/// <summary>
		/// Gets the airport specified by the given airport code.
		/// </summary>
		/// <returns>
		/// The airport specified by the given airport code.
		/// </returns>
		/// <param name="code">
		/// The airport code of the desired airport.
		/// </param>
		/// <param name="type">
		/// The type of airport code given.
		/// </param>
		public static Airport GetAirport (string code, AirportCode type)
		{
			List<Airport> results;
			
			switch (type) {
			case AirportCode.FAA:
				results = sqlitedb.Query<Airport> ("select * from Airport where FAA = ?", code);
				break;
			case AirportCode.IATA:
				results = sqlitedb.Query<Airport> ("select * from Airport where IATA = ?", code);
				break;
			case AirportCode.ICAO:
				results = sqlitedb.Query<Airport> ("select * from Airport where ICAO = ?", code);
				break;
			case AirportCode.Any:
				results = sqlitedb.Query<Airport> ("select * from Airport where FAA = ? or IATA = ? or ICAO = ?", code, code, code);
				break;
			default:
				return null;
			}
			
			return results.Count > 0 ? results[0] : null;
		}
		
		/// <summary>
		/// Gets the airport specified by the given name.
		/// </summary>
		/// <returns>
		/// The airport by name.
		/// </returns>
		/// <param name='name'>
		/// The name of the airport.
		/// </param>
		public static Airport GetAirportByName (string name)
		{
			var results = sqlitedb.Query<Airport> ("select * from Airport where Name = ?", name);
			
			return results.Count > 0 ? results[0] : null;
		}

		static IEnumerable<Airport> EnumerateAirports (string query, params object[] args)
		{
			var cmd = sqlitedb.CreateCommand (query.Replace ("*", "count (*)"), args);
			int count = cmd.ExecuteScalar<int> ();
			List<Airport> airports;
			int offset = 0;
			int limit;

			while (offset < count) {
				limit = Math.Min (count - offset, 64);
				cmd = sqlitedb.CreateCommand (query + " limit " + limit + " offset " + offset, args);
				airports = cmd.ExecuteQuery<Airport> ();

				foreach (var airport in airports)
					yield return airport;

				offset += limit;
			}

			yield break;
		}
		
		/// <summary>
		/// Gets a list of all airports.
		/// </summary>
		/// <returns>
		/// A list of all airports.
		/// </returns>
		public static IEnumerable<Airport> GetAllAirports ()
		{
			return EnumerateAirports ("select * from Airport");
		}
		
		/// <summary>
		/// Gets a list of all airports in the specified map region.
		/// </summary>
		/// <returns>
		/// A list of all airports in the specified map region.
		/// </returns>
		/// <param name='region'>
		/// The map region.
		/// </param>
		public static IEnumerable<Airport> GetAirports (MKCoordinateRegion region)
		{
			string query = "select * from Airport where Latitude between ? and ? and Longitude between ? and ?";
			double longMin = region.Center.Longitude - region.Span.LongitudeDelta / 2.0;
			double longMax = region.Center.Longitude + region.Span.LongitudeDelta / 2.0;
			double latMin = region.Center.Latitude - region.Span.LatitudeDelta / 2.0;
			double latMax = region.Center.Latitude + region.Span.LatitudeDelta / 2.0;

			return EnumerateAirports (query, latMin, latMax, longMin, longMax);
		}
		
		static bool IsPossibleCode (string query)
		{
			for (int i = 0; i < query.Length; i++) {
				if ((query[i] < 'A' || query[i] > 'Z') &&
					(query[i] < '0' || query[i] > '9'))
					return false;
			}
			
			return true;
		}
		
		static char[] LikeSpecials = new char[] { '\\', '_', '%' };
		
		static string EscapeTextForLike (string text)
		{
			int first = text.IndexOfAny (LikeSpecials);
			
			if (first == -1)
				return text;
			
			var sb = new StringBuilder (text, 0, first, text.Length + 1);
			
			for (int i = first; i < text.Length; i++) {
				switch (text[i]) {
				case '\\': // escape character
				case '_': // matches any single character
				case '%': // matches any sequence of zero or more characters
					sb.Append ('\\');
					break;
				default:
					break;
				}
				
				sb.Append (text[i]);
			}
			
			return sb.ToString ();
		}
		
		/// <summary>
		/// Gets a list of all airports containing the specified substring
		/// in the ICAO code, the IATA code or the full name of the airport.
		/// </summary>
		/// <returns>
		/// A list of all airports containing the specified substring.
		/// </returns>
		/// <param name='contains'>
		/// The substring to search for.
		/// </param>
		public static List<Airport> GetAirports (string contains)
		{
			string escaped = EscapeTextForLike (contains);
			string pattern = string.Format ("%{0}%", escaped);
			string code = null;
			
			if (contains.Length <= 4) {
				// ICAO, IATA and FAA codes are capitalized
				code = contains.ToUpper ();
			}
			
			if (code != null && IsPossibleCode (code)) {
				string codeLike = code + '%';
				
				switch (contains.Length) {
				case 4: return sqlitedb.Query<Airport> ("select * from Airport where FAA = ? or ICAO = ? or Name like ?", code, code, pattern);
				case 3: return sqlitedb.Query<Airport> ("select * from Airport where FAA like ? or ICAO like ? or IATA = ? or Name like ?",
						codeLike, codeLike, code, pattern);
				default:
					return sqlitedb.Query<Airport> ("select * from Airport where FAA like ? or ICAO like ? or IATA like ?",
						codeLike, codeLike, codeLike, pattern);
				}
			} else if (escaped.Length > contains.Length) {
				return sqlitedb.Query<Airport> ("select * from Airport where Name like ? escape ?", pattern, '\\');
			} else {
				return sqlitedb.Query<Airport> ("select * from Airport where Name like ?", pattern);
			}
		}
		
		const double MetersPerNauticalMile = 1852;
		const double MeanEarthRadius = 6371009;
		
		static double ToRadians (double degrees)
		{
			return degrees * Math.PI / 180;
		}

		public static double GetDistanceFrom (Airport airport, CLLocationCoordinate2D location)
		{
			// Calculates distance between 2 locations on Earth using the Haversine formula.
			// http://www.movable-type.co.uk/scripts/latlong.html
			double distLongitude = ToRadians (airport.Longitude - location.Longitude);
			double distLatitude = ToRadians (airport.Latitude - location.Latitude);

			double a = Math.Pow (Math.Sin (distLatitude / 2), 2) +
				Math.Pow (Math.Sin (distLongitude / 2), 2) *
					Math.Cos (ToRadians (location.Latitude)) *
					Math.Cos (ToRadians (airport.Latitude));

			double c = 2 * Math.Atan2 (Math.Sqrt (a), Math.Sqrt (1 - a));

			return c * MeanEarthRadius / MetersPerNauticalMile;
		}
		
		public static double GetDistanceBetween (Airport depart, Airport arrive)
		{
			// Calculates distance between 2 locations on Earth using the Haversine formula.
			// http://www.movable-type.co.uk/scripts/latlong.html
			double distLongitude = ToRadians (arrive.Longitude - depart.Longitude);
			double distLatitude = ToRadians (arrive.Latitude - depart.Latitude);
			
			double a = Math.Pow (Math.Sin (distLatitude / 2), 2) +
				Math.Pow (Math.Sin (distLongitude / 2), 2) *
				Math.Cos (ToRadians (depart.Latitude)) *
				Math.Cos (ToRadians (arrive.Latitude));
			
			double c = 2 * Math.Atan2 (Math.Sqrt (a), Math.Sqrt (1 - a));
			
			return c * MeanEarthRadius / MetersPerNauticalMile;
		}
		
#if false
		// MapKit alternative
		public static double MKGetDistanceBetween (Airport airport1, Airport airport2)
		{
			CLLocationCoordinate2D loc1 = new CLLocationCoordinate2D (airport1.Latitude, airport1.Longitude);
			CLLocationCoordinate2D loc2 = new CLLocationCoordinate2D (airport2.Latitude, airport2.Longitude);
			MKMapPoint point1 = MKMapPoint.FromCoordinate (loc1);
			MKMapPoint point2 = MKMapPoint.FromCoordinate (loc2);
			
			return MKGeometry.MetersBetweenMapPoints (point1, point2) / MetersPerNauticalMile;
		}
#endif
	}
}
