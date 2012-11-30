// 
// AircraftDetailsViewController.cs
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
using System.Collections;
using System.Collections.Generic;

using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class AircraftDetailsViewController : DialogViewController
	{
		UIPopoverController masterPopoverController;
		EditAircraftDetailsViewController editor;
		StringElement isComplex, isHighPerformance, isTailDragger, isSimulator;
		StringElement category, classification;
		AircraftProfileView profile;
		UIBarButtonItem edit;
		Aircraft aircraft;
		
		public AircraftDetailsViewController () : base (UITableViewStyle.Grouped, new RootElement (null))
		{
			Autorotate = true;
		}

		public AircraftViewController RootViewController {
			get; set;
		}

		static bool AircraftEqual (Aircraft aircraft1, Aircraft aircraft2)
		{
			if (aircraft1 == aircraft2)
				return true;

			if (aircraft1 != null && aircraft2 != null)
				return aircraft1.Id == aircraft2.Id;

			return false;
		}
		
		public Aircraft Aircraft {
			get { return aircraft; }
			set {
				if (AircraftEqual (aircraft, value))
					return;

				aircraft = value;
				if (value != null && IsViewLoaded)
					UpdateDetails ();

				if (masterPopoverController != null)
					masterPopoverController.Dismiss (true);
			}
		}
		
		public bool EditorEngaged {
			get { return editor != null; }
		}
		
		public override void LoadView ()
		{
			base.LoadView ();
			
			profile = new AircraftProfileView (View.Bounds.Width);
			Root.Add (new Section (profile));
			
			Section section = new Section ("Type of Aircraft");
			section.Add (category = new StringElement ("Category"));
			section.Add (classification = new StringElement ("Classification"));
			section.Add (isComplex = new StringElement ("Complex"));
			section.Add (isHighPerformance = new StringElement ("High Performance"));
			section.Add (isTailDragger = new StringElement ("Tail Dragger"));
			section.Add (isSimulator = new StringElement ("Simulator"));
			Root.Add (section);
			
			edit = new UIBarButtonItem (UIBarButtonSystemItem.Edit, OnEditClicked);
			NavigationItem.RightBarButtonItem = edit;
		}
		
		void UpdateDetails ()
		{
			Title = Aircraft.TailNumber;
			
			profile.Photograph = PhotoManager.Load (Aircraft.TailNumber, false);
			profile.Make = Aircraft.Make;
			profile.Model = Aircraft.Model;
			profile.Remarks = Aircraft.Notes;
			
			category.Value = Aircraft.Category.ToHumanReadableName ();
			classification.Value = Aircraft.Classification.ToHumanReadableName ();
			isComplex.Value = Aircraft.IsComplex ? "Yes" : "No";
			isHighPerformance.Value = Aircraft.IsHighPerformance ? "Yes" : "No";
			isTailDragger.Value = Aircraft.IsTailDragger ? "Yes" : "No";
			isSimulator.Value = Aircraft.IsSimulator ? "Yes" : "No";
			
			foreach (var section in Root)
				Root.Reload (section, UITableViewRowAnimation.None);
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			if (InterfaceOrientation == UIInterfaceOrientation.Portrait || InterfaceOrientation == UIInterfaceOrientation.PortraitUpsideDown) {
				if (aircraft == null)
					Aircraft = RootViewController.FirstOrSelected;

				if (aircraft == null)
					Edit (new Aircraft (), false);
			} else {
				if (aircraft != null)
					UpdateDetails ();
			}
		}
		
		void OnEditorClosed (object sender, EventArgs args)
		{
			if (aircraft != null)
				UpdateDetails ();
			editor = null;
		}
		
		void OnEditClicked (object sender, EventArgs args)
		{
			if (Aircraft == null) {
				Edit (new Aircraft (), false);
			} else {
				Edit (Aircraft, true);
			}
		}

		public void Edit (Aircraft aircraft, bool exists)
		{
			if (masterPopoverController != null)
				masterPopoverController.Dismiss (true);
			
			editor = new EditAircraftDetailsViewController (aircraft, exists);
			editor.EditorClosed += OnEditorClosed;
			
			NavigationController.PushViewController (editor, true);
		}

		[Export ("splitViewController:willHideViewController:withBarButtonItem:forPopoverController:")]
		public void WillHideMasterViewController (UISplitViewController splitViewController, UIViewController masterViewController, UIBarButtonItem barButtonItem, UIPopoverController popoverController)
		{
			barButtonItem.Title = masterViewController.Title;
			NavigationItem.SetLeftBarButtonItem (barButtonItem, true);
			masterPopoverController = popoverController;
		}

		[Export ("splitViewController:willShowViewController:invalidatingBarButtonItem:")]
		public void WillShowMasterViewController (UISplitViewController splitViewController, UIViewController masterViewController, UIBarButtonItem barButtonItem)
		{
			NavigationItem.SetLeftBarButtonItem (null, true);
			masterPopoverController = null;
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}
