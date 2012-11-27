// 
// SummarySplitViewController.cs
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

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace FlightLog {
	public class SummarySplitViewController : UISplitViewController
	{
		FlightDetailsViewController details;
		SummaryViewController overview;
		UIViewController[] controllers;
		
		public SummarySplitViewController ()
		{
			TabBarItem.Image = UIImage.FromBundle ("Images/first");
			Title = "Summary";
			
			details = new FlightDetailsViewController ();
			overview = new SummaryViewController (details);
			
			controllers = new UIViewController[] {
				new UINavigationController (overview),
				new UINavigationController (details),
			};
			
			ViewControllers = controllers;
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			if (overview != null) {
				overview.Dispose ();
				overview = null;
			}
			
			if (details != null) {
				details.Dispose ();
				details = null;
			}
		}
	}
}
