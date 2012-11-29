// 
// AircraftViewController.cs
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
using System.Drawing;

using MonoTouch.UIKit;

namespace FlightLog {
	public class AircraftSplitViewController : UISplitViewController
	{
		AircraftDetailsViewController details;
		AircraftViewController overview;
		UIViewController[] controllers;
		
		public AircraftSplitViewController ()
		{
			TabBarItem.Image = UIImage.FromBundle ("Images/aircraft");
			Title = "Aircraft";
			
			details = new AircraftDetailsViewController ();
			overview = new AircraftViewController ();

			overview.DetailsViewController = details;
			details.RootViewController = overview;
			
			controllers = new UIViewController[] {
				new UINavigationController (overview),
				new UINavigationController (details),
			};
			
			ViewControllers = controllers;
			WeakDelegate = details;
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (controllers != null)
					controllers = null;

				if (overview != null) {
					overview.Dispose ();
					overview = null;
				}

				if (details != null) {
					details.Dispose ();
					details = null;
				}
			}

			base.Dispose (disposing);
		}
	}
}
