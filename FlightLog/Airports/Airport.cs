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

using MonoTouch.SQLite;

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
	}
}
