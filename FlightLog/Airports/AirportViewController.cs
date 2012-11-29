// 
// MapViewController.cs
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
		
		//loads the MapViewController.xib file and connects it to this object
		public AirportViewController () : base ("AirportViewController", null)
		{
			TabBarItem.Image = UIImage.FromBundle ("Images/first");
			Title = "Airports";
			
			annotations = new Dictionary<string, AirportAnnotation> ();
		}
		
		MKAnnotationView GetAirportAnnotationView (MKMapView map, NSObject annotation)
		{
			MKAnnotationView annotationView = mapView.DequeueReusableAnnotation ("annotationViewID");
			
			if (annotationView == null)
				annotationView = new MKPinAnnotationView (annotation, "annotationViewID");
			
			annotationView.Annotation = (MKAnnotation) annotation;
			
			return annotationView;
		}
		
		void MapRegionChanged (object sender, MKMapViewChangeEventArgs e)
		{
			Dictionary<string, AirportAnnotation> current = new Dictionary<string, AirportAnnotation> ();
			List<AirportAnnotation> added = new List<AirportAnnotation> ();
			
			// Query for the list of airports in the new region
			foreach (var airport in Airports.GetAirports (mapView.Region)) {
				AirportAnnotation aa;
				
				if (annotations.ContainsKey (airport.FAA)) {
					// This airport is already in view...
					aa = annotations[airport.FAA];
					annotations.Remove (airport.FAA);
				} else {
					// This is a new annotation, keep track of it in 'added'
					aa = new AirportAnnotation (airport);
					added.Add (aa);
				}
				
				current.Add (aa.Airport.FAA, aa);
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
			
			// Get the region for North America
			MKCoordinateRegion region = new MKCoordinateRegion (
				new CLLocationCoordinate2D (37.37, -96.24),
				new MKCoordinateSpan (28.49, 31.025)
			);
			
			mapView.SetRegion (region, false);
			
			mapView.GetViewForAnnotation += GetAirportAnnotationView;
			mapView.RegionChanged += MapRegionChanged;
			mapType.ValueChanged += MapTypeChanged;
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
