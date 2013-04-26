// 
// AirportViewController.cs
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
using System.Collections;
using System.Collections.Generic;

using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;

namespace FlightLog {
	public partial class AirportViewController : UIViewController
	{
		Dictionary<string, AirportAnnotation> annotations;
		CLLocationCoordinate2D coordinates;
		CLLocationManager manager;
		bool visited = true;

		public AirportViewController () : base ("AirportViewController", null)
		{
			TabBarItem.Image = UIImage.FromBundle ("Images/map");
			Title = "Airports";
			
			annotations = new Dictionary<string, AirportAnnotation> ();
		}
		
		MKAnnotationView GetAirportAnnotationView (MKMapView map, NSObject annotation)
		{
			MKAnnotationView view = mapView.DequeueReusableAnnotation ("AirportAnnotation");
			
			if (view == null)
				view = new MKPinAnnotationView (annotation, "AirportAnnotation");
			
			view.Annotation = (MKAnnotation) annotation;
			view.CanShowCallout = true;
			
			return view;
		}

		IEnumerable<Airport> GetAirports (MKCoordinateRegion region)
		{
			if (visited) {
				foreach (var code in LogBook.GetVisitedAirports ()) {
					Airport airport = Airports.GetAirport (code, AirportCode.FAA);
					if (airport != null)
						yield return airport;
				}
			} else {
				foreach (var airport in Airports.GetAirports (region))
					yield return airport;
			}

			yield break;
		}
		
		void MapRegionChanged (object sender, MKMapViewChangeEventArgs e)
		{
			Dictionary<string, AirportAnnotation> current = new Dictionary<string, AirportAnnotation> ();
			List<AirportAnnotation> added = new List<AirportAnnotation> ();
			
			// Query for the list of airports in the new region
			foreach (var airport in GetAirports (mapView.Region)) {
				AirportAnnotation annotation;
				
				if (annotations.ContainsKey (airport.FAA)) {
					// This airport is already in view...
					annotation = annotations[airport.FAA];
					annotation.UserCoordinates = coordinates;
					annotations.Remove (airport.FAA);
				} else {
					// This is a new annotation, keep track of it in 'added'
					annotation = new AirportAnnotation (airport, coordinates);
					added.Add (annotation);
				}
				
				current.Add (annotation.Airport.FAA, annotation);
			}
			
			if (annotations.Count > 0) {
				// Remove annotations that are no longer in view
				mapView.RemoveAnnotations (annotations.Values.ToArray ());
				foreach (var annotation in annotations.Values)
					annotation.Dispose ();
				annotations.Clear ();
			}
			
			if (added.Count > 0) {
				mapView.AddAnnotations (added.ToArray ());
				foreach (var annotation in added)
					annotation.Dispose ();
			}
			
			annotations = current;
		}
		
		void MapTypeChanged (object sender, EventArgs args)
		{
			switch (mapType.SelectedSegment) {
			case 0: mapView.MapType = MKMapType.Standard; break;
			case 1: mapView.MapType = MKMapType.Satellite; break;
			case 2: mapView.MapType = MKMapType.Hybrid; break;
			}
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			manager = new CLLocationManager ();

			// handle the updated location method and update the UI
			if (UIDevice.CurrentDevice.CheckSystemVersion (6, 0)) {
				manager.LocationsUpdated += LocationsUpdated;
			} else {
				// this won't be called on iOS 6 (deprecated)
				manager.UpdatedLocation += UpdatedLocation;
			}

			if (CLLocationManager.LocationServicesEnabled)
				manager.StartUpdatingLocation ();

			//if (CLLocationManager.HeadingAvailable)
			//	manager.StartUpdatingHeading ();

			mapView.GetViewForAnnotation += GetAirportAnnotationView;
			mapView.RegionChanged += MapRegionChanged;
			mapType.ValueChanged += MapTypeChanged;
			MapTypeChanged (null, null);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			// Default to the region for North America
			double minLatitude = 37.37 - (28.49 / 2);
			double maxLatitude = 37.37 + (28.49 / 2);
			double minLongitide = -96.24 - (31.025 / 2);
			double maxLongitude = -96.24 + (31.025 / 2);
			bool initialized = false;

			foreach (var code in LogBook.GetVisitedAirports ()) {
				Airport airport = Airports.GetAirport (code, AirportCode.FAA);
				if (airport == null)
					continue;

				if (!initialized) {
					minLongitide = maxLongitude = airport.Longitude;
					minLatitude = maxLatitude = airport.Latitude;
					initialized = true;
				} else {
					minLongitide = Math.Min (minLongitide, airport.Longitude);
					maxLongitude = Math.Max (maxLongitude, airport.Longitude);
					minLatitude = Math.Min (minLatitude, airport.Latitude);
					maxLatitude = Math.Max (maxLatitude, airport.Latitude);
				}
			}

			coordinates = new CLLocationCoordinate2D ((minLatitude + maxLatitude) / 2, (minLongitide + maxLongitude) / 2);
			double spanLongitude = Math.Abs (maxLongitude - minLongitide);
			double spanLatitude = Math.Abs (maxLatitude - minLatitude);

			if (initialized) {
				spanLongitude = Math.Max (spanLongitude, 1.0) * 1.25;
				spanLatitude = Math.Max (spanLatitude, 1.0) * 1.25;
			}

			// Get the region for North America
			MKCoordinateRegion region = new MKCoordinateRegion (
				coordinates, new MKCoordinateSpan (spanLatitude, spanLongitude)
			);

			mapView.SetRegion (region, animated);
		}

		void UpdateLocation (CLLocation location)
		{
			MKCoordinateRegion region = new MKCoordinateRegion (
				location.Coordinate, new MKCoordinateSpan (1.0, 1.0)
			);

			coordinates = location.Coordinate;

			if (!visited)
				mapView.SetRegion (region, true);
		}

		void UpdatedLocation (object sender, CLLocationUpdatedEventArgs e)
		{
			UpdateLocation (e.NewLocation);
		}

		void LocationsUpdated (object sender, CLLocationsUpdatedEventArgs e)
		{
			UpdateLocation (e.Locations[e.Locations.Length - 1]);
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();

			foreach (var annotation in annotations.Values)
				annotation.Dispose ();
			annotations.Clear ();
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (annotations != null) {
					annotations.Clear ();
					annotations = null;
				}
			}
			
			base.Dispose (disposing);
		}
	}
}
