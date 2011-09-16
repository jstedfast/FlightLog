// 
// EditAircraftDetailsViewController.cs
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

namespace FlightLog
{
	public class EditAircraftDetailsViewController : DialogViewController
	{
		RadioGroup classes = new RadioGroup ("AircraftClassification", 0);
		BooleanElement isComplex, isHighPerformance, isTailDragger;
		LimitedEntryElement make, model, notes;
		RootElement category, classification;
		AircraftEntryElement tailNumber;
		UIBarButtonItem cancel, save;
		int previousCategory;
		ImageElement photo;
		bool exists;
		
		public EditAircraftDetailsViewController (Aircraft aircraft, bool exists) : base (UITableViewStyle.Grouped, new RootElement (null))
		{
			this.exists = exists;
			Aircraft = aircraft;
			Autorotate = false;
		}
		
		public Aircraft Aircraft {
			get; private set;
		}
		
		public override UITableView MakeTableView (RectangleF bounds, UITableViewStyle style)
		{
			return base.MakeTableView (bounds, style);
		}
		
		AircraftCategory CategoryFromIndex (int index)
		{
			return (AircraftCategory) (index * Aircraft.CategoryStep);
		}
		
		int CategoryToIndex (AircraftCategory category)
		{
			return ((int) category) / Aircraft.CategoryStep;
		}
		
		RootElement CreateCategoryElement (AircraftCategory category)
		{
			RootElement root = new RootElement ("Category", new RadioGroup ("AircraftCategory", 0));
			Section section = new Section ();
			
			foreach (AircraftCategory value in Enum.GetValues (typeof (AircraftCategory)))
				section.Add (new RadioElement (value.ToHumanReadableName (), "AircraftCategory"));
			
			root.Add (section);
			
			root.RadioSelected = CategoryToIndex (category);
			
			return root;
		}
		
		AircraftClassification ClassificationFromIndexes (int category, int classification)
		{
			return (AircraftClassification) ((category * Aircraft.CategoryStep) + classification);
		}
		
		int ClassificationToIndex (AircraftClassification classification)
		{
			return ((int) classification) % Aircraft.CategoryStep;
		}
		
		RootElement CreateClassificationElement (AircraftCategory category)
		{
			RootElement root = new RootElement ("Class", classes);
			int next = (int) category + Aircraft.CategoryStep;
			Section section = new Section ();
			
			foreach (AircraftClassification value in Enum.GetValues (typeof (AircraftClassification))) {
				if ((int) value < (int) category)
					continue;
				
				if ((int) value >= next)
					break;
				
				section.Add (new RadioElement (value.ToHumanReadableName (), "AircraftClassification"));
			}
			
			root.Add (section);
			
			return root;
		}
		
		Section CreateMakeAndModelSection ()
		{
			return new Section ("Make & Model") {
				(make = new LimitedEntryElement ("Make", "Aircraft Manufacturer", Aircraft.Make, 30)),
				(model = new LimitedEntryElement ("Model", "Aircraft Model", Aircraft.Model, 20)),
			};
		}
		
		Section CreateAircraftTypeSection ()
		{
			Section section = new Section ("Type of Aircraft") {
				(category = CreateCategoryElement (Aircraft.Category)),
				(classification = CreateClassificationElement (Aircraft.Category)),
				(isComplex = new BooleanElement ("Complex", Aircraft.IsComplex)),
				(isHighPerformance = new BooleanElement ("High Performance", Aircraft.IsHighPerformance)),
				(isTailDragger = new BooleanElement ("Tail Dragger", Aircraft.IsTailDragger)),
			};
			
			classification.RadioSelected = ClassificationToIndex (Aircraft.Classification);
			
			return section;
		}
		
		public override void LoadView ()
		{
			if (Aircraft == null) {
				Aircraft = new Aircraft ();
				exists = false;
			}
			
			Title = exists ? Aircraft.TailNumber : "New Aircraft";
			
			// FIXME: the photo (and tail#, if not adding a new aircraft) should probably be in a header
			Section section = new Section ("");
			section.Add (photo = new ImageElement (null));
			section.Add (tailNumber = new AircraftEntryElement (Aircraft.TailNumber));
			Root.Add (section);
			
			Root.Add (CreateMakeAndModelSection ());
			Root.Add (CreateAircraftTypeSection ());
			Root.Add (new Section ("Notes") {
				(notes = new LimitedEntryElement (null, "Enter any additional notes about the aircraft here.",
					Aircraft.Notes, 140)),
			});
			
			cancel = new UIBarButtonItem (UIBarButtonSystemItem.Cancel, OnCancelClicked);
			NavigationItem.LeftBarButtonItem = cancel;
			
			save = new UIBarButtonItem (UIBarButtonSystemItem.Save, OnSaveClicked);
			NavigationItem.RightBarButtonItem = save;
			
			base.LoadView ();
		}
		
		void OnCancelClicked (object sender, EventArgs args)
		{
			this.NavigationController.PopViewControllerAnimated (true);
			
			OnEditorClosed ();
		}
		
		void FetchValues ()
		{
			// Make sure all entry elements sync their values from their UITextFields
			foreach (Section s in Root) {
				foreach (Element e in s) {
					if (e is EntryElement)
						((EntryElement) e).FetchValue ();
				}
			}
		}
		
		void OnSaveClicked (object sender, EventArgs args)
		{
			FetchValues ();
			
			if (tailNumber.Value == null || tailNumber.Value.Length < 5)
				return;
			
			// Save the values back to the Aircraft object
			Aircraft.TailNumber = tailNumber.Value;
			Aircraft.Make = make.Value;
			Aircraft.Model = model.Value;
			Aircraft.Classification = ClassificationFromIndexes (category.RadioSelected, classes.Selected);
			Aircraft.IsComplex = isComplex.Value;
			Aircraft.IsHighPerformance = isHighPerformance.Value;
			Aircraft.IsTailDragger = isTailDragger.Value;
			Aircraft.Notes = notes.Value;
			
			if (exists)
				LogBook.Update (Aircraft);
			else
				LogBook.Add (Aircraft);
			
			this.NavigationController.PopViewControllerAnimated (true);
			
			OnEditorClosed ();
		}
		
		public override void ViewWillAppear (bool animated)
		{
			if (category.RadioSelected != previousCategory) {
				var index = classification.IndexPath;
				
				classification = CreateClassificationElement (CategoryFromIndex (category.RadioSelected));
				classification.RadioSelected = 0;
				
				Root[index.Section].Remove (index.Row);
				Root[index.Section].Insert (index.Row, classification);
				classes.Selected = 0;
				
				Root.Reload (Root[index.Section], UITableViewRowAnimation.Right);
			}
			
			base.ViewWillAppear (animated);
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			previousCategory = category.RadioSelected;
			base.ViewWillDisappear (animated);
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
		
		public event EventHandler<EventArgs> EditorClosed;
		
		void OnEditorClosed ()
		{
			var handler = EditorClosed;
			
			if (handler != null)
				handler (this, EventArgs.Empty);
		}
	}
}
