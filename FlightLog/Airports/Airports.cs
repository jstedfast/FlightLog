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

using MonoTouch.MapKit;
using SQLite;

namespace FlightLog {
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
		/// Gets the airport specified by the given by FAA code.
		/// </summary>
		/// <returns>
		/// The airport specified by the given FAA code.
		/// </returns>
		/// <param name="code">
		/// The FAA code of the desired airport.
		/// </param>
		public static Airport GetAirport (string code)
		{
			var results = sqlitedb.Query<Airport> ("select * from Airport where FAA = ?", code);
			
			return results.Count > 0 ? results[0] : null;
		}
		
		/// <summary>
		/// Gets a list of all airports.
		/// </summary>
		/// <returns>
		/// A list of all airports.
		/// </returns>
		public static List<Airport> GetAllAirports ()
		{
			return sqlitedb.Query<Airport> ("select * from Airport");
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
		public static List<Airport> GetAirports (MKCoordinateRegion region)
		{
			double longMin = region.Center.Longitude - region.Span.LongitudeDelta / 2.0;
			double longMax = region.Center.Longitude + region.Span.LongitudeDelta / 2.0;
			double latMin = region.Center.Latitude - region.Span.LatitudeDelta / 2.0;
			double latMax = region.Center.Latitude + region.Span.LatitudeDelta / 2.0;
			
			return sqlitedb.Query<Airport> ("select * from Airport where Latitude between ? and ? and Longitude between ? and ?",
				latMin, latMax, longMin, longMax);
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
	}
}
