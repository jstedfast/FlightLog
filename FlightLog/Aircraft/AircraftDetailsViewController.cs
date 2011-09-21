// 
// AircraftDetailsViewController.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc.
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

using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class AircraftDetailsViewController : DialogViewController
	{
		EditAircraftDetailsViewController editor;
		StringElement isComplex, isHighPerformance, isTailDragger;
		StringElement category, classification;
		AircraftProfileView profile;
		UIBarButtonItem edit;
		Aircraft aircraft;
		
		public AircraftDetailsViewController () : base (UITableViewStyle.Grouped, new RootElement (null))
		{
			Autorotate = true;
		}
		
		public Aircraft Aircraft {
			get { return aircraft; }
			set {
				aircraft = value;
				if (value != null && IsViewLoaded)
					UpdateDetails ();
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
			Root.Add (section);
			
			edit = new UIBarButtonItem (UIBarButtonSystemItem.Edit, OnEditClicked);
			NavigationItem.LeftBarButtonItem = edit;
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
			
			foreach (var section in Root)
				Root.Reload (section, UITableViewRowAnimation.None);
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			if (aircraft != null)
				UpdateDetails ();
		}
		
		void OnEditorClosed (object sender, EventArgs args)
		{
			UpdateDetails ();
			editor = null;
		}
		
		void OnEditClicked (object sender, EventArgs args)
		{
			Aircraft edit;
			bool exists;
			
			if (Aircraft == null) {
				edit = new Aircraft ();
				exists = false;
			} else {
				edit = Aircraft;
				exists = true;
			}
			
			editor = new EditAircraftDetailsViewController (edit, exists);
			editor.EditorClosed += OnEditorClosed;
			
			NavigationController.PushViewController (editor, true);
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			switch (toInterfaceOrientation) {
			case UIInterfaceOrientation.LandscapeRight:
			case UIInterfaceOrientation.LandscapeLeft:
				return true;
			default:
				return false;
			}
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}
