// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace FlightLog
{
	[Register ("MapViewController")]
	partial class AirportViewController
	{
		[Outlet]
		MonoTouch.UIKit.UISegmentedControl mapType { get; set; }

		[Outlet]
		MonoTouch.MapKit.MKMapView mapView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (mapType != null) {
				mapType.Dispose ();
				mapType = null;
			}

			if (mapView != null) {
				mapView.Dispose ();
				mapView = null;
			}
		}
	}
}
