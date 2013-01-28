// 
// AirportAnnotation.cs
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

using MonoTouch.CoreLocation;
using MonoTouch.MapKit;

namespace FlightLog {
	public class AirportAnnotation : MKAnnotation
	{
		public AirportAnnotation (Airport airport, CLLocationCoordinate2D location)
		{
			CurrentLocation = location;
			Airport = airport;
		}
		
		public Airport Airport {
			get; private set;
		}
		
		public override string Title {
			get {
				return Airport.Name;
			}
		}

		public CLLocationCoordinate2D CurrentLocation {
			get; set;
		}
		
		public override string Subtitle {
			get {
				var distance = Airports.GetDistanceFrom (Airport, CurrentLocation);

				return string.Format ("{0} ({1}nm away)", Airport.FAA, distance.ToString ("F1"));
			}
		}
		
		public override CLLocationCoordinate2D Coordinate {
			get {
				return new CLLocationCoordinate2D (Airport.Latitude, Airport.Longitude);
			}
			
			set {
				throw new NotSupportedException ();
			}
		}
	}
}
