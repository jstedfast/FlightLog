// 
// Airport.cs
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

using SQLite;

namespace FlightLog {
	public class Airport
	{
		public Airport ()
		{
		}
		
		/// <summary>
		/// Gets or sets the FAA airport code.
		/// 
		/// Note: There is an FAA code for all aiports in the U.S.
		/// If the ICAO code exists, it will typically be identical
		/// to the FAA code with a prefix of 'K'.
		/// </summary>
		/// <value>
		/// The FAA airport code.
		/// </value>
		[PrimaryKey][Indexed][MaxLength (4)]
		public string FAA {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the IATA airport code.
		/// </summary>
		/// <value>
		/// The IATA airport code.
		/// </value>
		[Indexed][MaxLength (3)]
		public string IATA {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the ICAO airport code.
		/// </summary>
		/// <value>
		/// The ICAO airport code.
		/// </value>
		[Indexed][MaxLength (4)]
		public string ICAO {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the name of the airport.
		/// </summary>
		/// <value>
		/// The name of the airport.
		/// </value>
		public string Name {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the name of the city where the airport is located.
		/// </summary>
		/// <value>
		/// The city where the airport is located.
		/// </value>
		public string City {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the state where the airport is located.
		/// 
		/// Note: Only available for airports in the United States.
		/// </summary>
		/// <value>
		/// The 2-character state abbreviation where the airport is located.
		/// </value>
		[MaxLength (2)]
		public string State {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the name of the country where the airport is located.
		/// </summary>
		/// <value>
		/// The country where the airport is located.
		/// </value>
		public string Country {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the latitude coordinate for the airport.
		/// </summary>
		/// <value>
		/// The latitude coordinate.
		/// </value>
		public double Latitude {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the longitude coordinate for the airport.
		/// </summary>
		/// <value>
		/// The longitude coordinate.
		/// </value>
		public double Longitude {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the elevation of the airport.
		/// </summary>
		/// <value>
		/// The elevation of the airport.
		/// </value>
		public int Elevation {
			get; set;
		}
		
		static int NumFields = Enum.GetNames (typeof (AirportField)).Length;
		
		static bool IsICAO (string field)
		{
			if (field.Length > 4)
				return false;
			
			for (int i = 0; i < field.Length; i++) {
				if (!(field[i] >= 'A' && field[i] <= 'Z'))
					return false;
			}
			
			return true;
		}
		
		static bool IsIATA (string field)
		{
			if (field.Length != 3)
				return false;
			
			if (field == "N/A")
				return true;
			
			for (int i = 0; i < field.Length; i++) {
				if (!(field[i] >= 'A' && field[i] <= 'Z') && !(field[i] >= '0' && field[i] <= '9'))
					return false;
			}
			
			return true;
		}
		
		static string FixCapitalization (string text)
		{
			if (text == string.Empty)
				return string.Empty;
			
			string[] words = text.Split (new char[] { ' ' });
			
			for (int i = 0; i < words.Length; i++) {
				if (words[i].Length > 0)
					words[i] = words[i][0] + words[i].Substring (1).ToLower ();
			}
			
			return string.Join (" ", words);
		}
		
		static bool TryParse (string entry, out Airport airport, ref AirportError error, ref AirportField field, ref string message)
		{
			string[] fields = entry.Split (new char[] { ':' });
			int degrees, minutes, seconds, elevation;
			string alt;
			int neg;
			
			if (fields.Length != NumFields) {
				if (fields.Length < NumFields) {
					message = string.Format ("Airport entry is missing {0} fields.", NumFields - fields.Length);
					field = (AirportField) (NumFields - fields.Length);
					error = AirportError.NotEnoughFields;
				} else {
					message = string.Format ("Airport entry has too many fields.");
					error = AirportError.TooManyFields;
					field = AirportField.Elevation;
				}
				
				airport = null;
				return false;
			}
			
			airport = new Airport ();
			
			// Parse ICAO code
			if (!IsICAO (fields[0])) {
				message = string.Format ("Airport ICAO '{0}' code is invalid.", fields[0]);
				field = AirportField.ICAO;
				airport = null;
				return false;
			} else {
				airport.ICAO = fields[0];
			}
			
			// Parse IATA code
			if (!IsIATA (fields[1])) {
				message = string.Format ("Airport IATA code '{0}' is invalid.", fields[1]);
				field = AirportField.IATA;
				airport = null;
				return false;
			} else if (fields[1] != "N/A") {
				airport.IATA = fields[1];
			} else {
				airport.IATA = "";
			}
			
			airport.Name = FixCapitalization (fields[2]);
			airport.City = FixCapitalization (fields[3]);
			airport.Country = fields[4] == "USA" ? "USA" : FixCapitalization (fields[4]);
			
			if (!Int32.TryParse (fields[5], out degrees) || degrees >= 180) {
				message = string.Format ("Airport Latitude (degrees) is invalid.");
				field = AirportField.LatitudeDegrees;
				airport = null;
				return false;
			} else if (!Int32.TryParse (fields[6], out minutes) || minutes >= 60) {
				message = string.Format ("Airport Latitude (minutes) is invalid.");
				field = AirportField.LatitudeMinutes;
				airport = null;
				return false;
			} else if (!Int32.TryParse (fields[7], out seconds) || seconds >= 60) {
				message = string.Format ("Airport Latitude (seconds) is invalid.");
				field = AirportField.LatitudeSeconds;
				airport = null;
				return false;
			} else if (!(fields[8] == "N" || fields[8] == "S" || fields[8] == "U")) {
				message = string.Format ("Airport Latitude direction is invalid.");
				field = AirportField.LatitudeDirection;
				airport = null;
				return false;
			} else {
				int sign = fields[8][0] == 'N' ? 1 : -1;
				
				airport.Latitude = (sign * (((degrees * 60) + minutes) * 60 + seconds)) / 3600.0;
			}
			
			if (!Int32.TryParse (fields[9], out degrees) || degrees >= 180) {
				message = string.Format ("Airport Longitude (degrees) is invalid.");
				field = AirportField.LongitudeDegrees;
				airport = null;
				return false;
			} else if (!Int32.TryParse (fields[10], out minutes) || minutes >= 60) {
				message = string.Format ("Airport Longitude (minutes) is invalid.");
				field = AirportField.LongitudeMinutes;
				airport = null;
				return false;
			} else if (!Int32.TryParse (fields[11], out seconds) || seconds >= 60) {
				message = string.Format ("Airport Longitude (seconds) is invalid.");
				field = AirportField.LongitudeSeconds;
				airport = null;
				return false;
			} else if (!(fields[12] == "E" || fields[12] == "W" || fields[12] == "U")) {
				message = string.Format ("Airport Longitude direction is invalid.");
				field = AirportField.LongitudeDirection;
				airport = null;
				return false;
			} else {
				int sign = fields[12][0] == 'E' ? 1 : -1;
				airport.Longitude = (sign * (((degrees * 60) + minutes) * 60 + seconds)) / 3600.0;
			}
			
			// Altitude can be below sealevel...
			if ((neg = fields[13].IndexOf ('-')) != -1)
				alt = fields[13].Substring (neg);
			else
				alt = fields[13];
			
			if (!Int32.TryParse (alt, out elevation)) {
				message = string.Format ("Airport Altitude '{0}' is invalid.", fields[13]);
				field = AirportField.Elevation;
				airport = null;
				return false;
			} else {
				airport.Elevation = elevation;
			}
			
			return true;
		}
		
		public static bool TryParse (string entry, out Airport airport)
		{
			AirportError error = AirportError.NotEnoughFields;
			AirportField field = AirportField.Elevation;
			string message = null;
			
			if (entry == null) {
				airport = null;
				return false;
			}
			
			return TryParse (entry, out airport, ref error, ref field, ref message);
		}
		
		public static Airport Parse (string entry)
		{
			AirportError error = AirportError.UnexpectedValue;
			AirportField field = AirportField.Elevation;
			string message = null;
			Airport airport;
			
			if (entry == null)
				throw new ArgumentNullException ("entry");
			
			if (TryParse (entry, out airport, ref error, ref field, ref message))
				return airport;
			
			throw new AirportParseException (entry, error, field, message);
		}
	}
}
