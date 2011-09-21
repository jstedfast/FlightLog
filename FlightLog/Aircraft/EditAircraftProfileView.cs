// 
// EditAircraftProfileView.cs
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

using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class EditAircraftProfileView : UIView
	{
		enum TableSections {
			TailNumber,
			MakeAndModel
		}
		
		const float AddPhotoFontSize = 24.0f;
		const float XBorderPadding = 43.0f;
		const float YBorderPadding = 20.0f;
		const float PhotoHeight = 210.0f;
		const float PhotoWidth = 280.0f;
		
		const float AddPhotoYOffset = YBorderPadding + (PhotoHeight / 2.0f) - (AddPhotoFontSize / 2.0f) - 1.0f;
		const float TableViewOffset = XBorderPadding + PhotoWidth + 20.0f;
		const float ProfileHeight = PhotoHeight + YBorderPadding * 2;
		
		static RectangleF PhotoRect = new RectangleF (XBorderPadding, YBorderPadding, PhotoWidth, PhotoHeight);
		static CGPath PhotoBorder = GraphicsUtil.MakeRoundedRectPath (PhotoRect, 12.0f);
		static UIFont AddPhotoFont = UIFont.BoldSystemFontOfSize (AddPhotoFontSize);
		static UIColor AddPhotoTextColor = UIColor.FromRGB (76, 86, 108);
		static UIColor HighlightedButtonColor = UIColor.LightGray;
		static UIColor NormalButtonColor = UIColor.White;
		
		LimitedEntryElement make, model;
		AircraftEntryElement tailNumber;
		UIImagePickerController picker;
		UIPopoverController popover;
		UIActionSheet sheet;
		UIImage photograph;
		DialogView dialog;
		bool pressed;
		
		public EditAircraftProfileView (float width) : this (new RectangleF (0.0f, 0.0f, width, ProfileHeight)) { }
		
		public EditAircraftProfileView (RectangleF frame) : base (frame)
		{
			BackgroundColor = UIColor.Clear;
			
			float dialogWidth = frame.Width - TableViewOffset - XBorderPadding - 55.0f;
			RectangleF dialogFrame = new RectangleF (TableViewOffset, YBorderPadding, dialogWidth, PhotoHeight);
			RootElement root = new RootElement (null);
			root.Add (CreateTailNumberSection ());
			root.Add (CreateMakeAndModelSection ());
			dialog = new DialogView (dialogFrame, root);
			AddSubview (dialog);
		}
		
		public UIImage Photograph {
			get { return photograph; }
			set {
				photograph = value;
				SetNeedsDisplay ();
			}
		}
		
		public string Make {
			get { return make.Value; }
			set { make.Value = value; }
		}
		
		public string Model {
			get { return model.Value; }
			set { model.Value = value; }
		}
		
		public string TailNumber {
			get { return tailNumber.Value; }
			set { tailNumber.Value = value; }
		}
		
		Section CreateTailNumberSection ()
		{
			return new Section () {
				(tailNumber = new AircraftEntryElement ("")),
			};
		}
		
		Section CreateMakeAndModelSection ()
		{
			return new Section ("Make & Model") {
				(make = new LimitedEntryElement ("Make", "Aircraft Manufacturer", "", 30)),
				(model = new LimitedEntryElement ("Model", "Aircraft Model", "", 20)),
			};
		}
		
		void DrawAddPhotoButton (CGContext ctx)
		{
			UIColor background = pressed ? HighlightedButtonColor : NormalButtonColor;
			RectangleF bounds = PhotoRect;
			float alpha = 1.0f;
			
			ctx.SaveState ();
			ctx.AddPath (PhotoBorder);
			ctx.Clip ();
			
			using (var cs = CGColorSpace.CreateDeviceRGB ()) {
				var bottomCenter = new PointF (bounds.GetMidX (), bounds.GetMaxY ());
				var topCenter = new PointF (bounds.GetMidX (), bounds.Y);
				float[] gradColors;
				CGPath container;
				
				gradColors = new float [] { 0.23f, 0.23f, 0.23f, alpha, 0.67f, 0.67f, 0.67f, alpha };
				using (var gradient = new CGGradient (cs, gradColors, new float [] { 0.0f, 1.0f })) {
					ctx.DrawLinearGradient (gradient, topCenter, bottomCenter, 0);
				}
				
				var bg = bounds.Inset (1.0f, 1.0f);
				container = GraphicsUtil.MakeRoundedRectPath (bg, 13.5f);
				ctx.AddPath (container);
				ctx.Clip ();
				
				background.SetFill ();
				ctx.FillRect (bg);
				
				gradColors = new float [] {
					0.0f, 0.0f, 0.0f, 0.75f,
					0.0f, 0.0f, 0.0f, 0.65f,
					0.0f, 0.0f, 0.0f, 0.35f,
					0.0f, 0.0f, 0.0f, 0.05f
				};
				
				using (var gradient = new CGGradient (cs, gradColors, new float [] { 0.0f, 0.1f, 0.4f, 1.0f })) {
					ctx.DrawLinearGradient (gradient, topCenter, bottomCenter, 0);
				}
			}
			
			//ctx.AddPath (PhotoBorder);
			//ctx.SetStrokeColor (0.5f, 0.5f, 0.5f, 1.0f);
			//ctx.SetLineWidth (0.5f);
			//ctx.StrokePath ();
			
			ctx.RestoreState ();
		}
		
		void DrawPhoto (CGContext ctx)
		{
			ctx.SaveState ();
			
			ctx.AddPath (PhotoBorder);
			ctx.Clip ();
			
			Photograph.Draw (PhotoRect);
			
			ctx.AddPath (PhotoBorder);
			ctx.SetStrokeColor (0.5f, 0.5f, 0.5f, 1.0f);
			ctx.SetLineWidth (0.5f);
			ctx.StrokePath ();
			
			ctx.RestoreState ();
		}
		
		public override void Draw (RectangleF rect)
		{
			CGContext ctx = UIGraphics.GetCurrentContext ();
			
			if (Photograph == null) {
				DrawAddPhotoButton (ctx);
				
				RectangleF addPhotoRect = new RectangleF (XBorderPadding, AddPhotoYOffset, PhotoWidth, AddPhotoFontSize);
				AddPhotoTextColor.SetColor ();
				
				DrawString ("Add Photo", addPhotoRect, AddPhotoFont, UILineBreakMode.WordWrap, UITextAlignment.Center);
			} else {
				DrawPhoto (ctx);
			}
		}
		
		class PhotoPickerDelegate : UIImagePickerControllerDelegate {
			EditAircraftProfileView profile;
			
			public PhotoPickerDelegate (EditAircraftProfileView profile)
			{
				this.profile = profile;
			}
			
			public override void FinishedPickingImage (UIImagePickerController picker, UIImage image, NSDictionary editingInfo)
			{
				profile.OnPhotoChosen (picker, image);
			}
		}
		
		class PhotoActionSheetDelegate : UIActionSheetDelegate {
			EditAircraftProfileView profile;
			
			public PhotoActionSheetDelegate (EditAircraftProfileView profile)
			{
				this.profile = profile;
			}
			
			public override void Dismissed (UIActionSheet actionSheet, int buttonIndex)
			{
				profile.OnActionSheetDismissed (actionSheet, buttonIndex);
			}
		}
		
		void OnActionSheetDismissed (UIActionSheet sheet, int buttonIndex)
		{
			if (sheet != null) {
				sheet.Dispose ();
				sheet = null;
			}
			
			picker = new UIImagePickerController ();
			picker.Delegate = new PhotoPickerDelegate (this);
			
			switch (buttonIndex) {
			case 0: // Choose Photo
				picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
				picker.AllowsEditing = true;
				break;
			case 1: // Take Photo
				picker.SourceType = UIImagePickerControllerSourceType.Camera;
				picker.CameraDevice = UIImagePickerControllerCameraDevice.Rear;
				picker.CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Photo;
				picker.CameraFlashMode = UIImagePickerControllerCameraFlashMode.Auto;
				picker.ShowsCameraControls = true;
				picker.AllowsEditing = true;
				break;
			}
			
			popover = new UIPopoverController (picker);
			popover.DidDismiss += OnPopoverDismissed;
			
			popover.PresentFromRect (PhotoRect, this, UIPopoverArrowDirection.Any, true);
		}
		
		void ShowPhotoPickerOptions ()
		{
			// If the device (such as the simulator) doesn't have a camera, don't present that option.
			if (!UIImagePickerController.IsSourceTypeAvailable (UIImagePickerControllerSourceType.Camera)) {
				OnActionSheetDismissed (null, 0);
				return;
			}
			
			sheet = new UIActionSheet ("Aircraft Photo");
			sheet.AddButton ("Choose Photo");
			sheet.AddButton ("Take Photo");
			
			sheet.Delegate = new PhotoActionSheetDelegate (this);
			
			sheet.ShowFrom (PhotoRect, this, true);
		}
		
		void OnPhotoSaved (UIImage photo, NSError error)
		{
			// dispose of the full-size photograph
			photo.Dispose ();
		}
		
		void OnPhotoChosen (UIImagePickerController picker, UIImage photo)
		{
			Photograph = PhotoManager.ScaleToSize (photo, (int) PhotoWidth, (int) PhotoHeight);
			
			if (picker.SourceType == UIImagePickerControllerSourceType.Camera)
				photo.SaveToPhotosAlbum (OnPhotoSaved);
			else
				photo.Dispose ();
			
			popover.Dismiss (true);
		}
		
		void OnPopoverDismissed (object sender, EventArgs args)
		{
			popover.Dispose ();
			popover = null;
			
			picker.Dispose ();
			picker = null;
		}
		
		bool IsInsidePhotoButton (NSSet touches)
		{
			var touched = touches.AnyObject as UITouch;
			var location = touched.LocationInView (this);
			
			return PhotoRect.Contains (location);
		}
		
		public override void TouchesBegan (NSSet touches, UIEvent uievent)
		{
			base.TouchesBegan (touches, uievent);
			
			if (uievent.Type != UIEventType.Touches)
				return;
			
			if (IsInsidePhotoButton (touches)) {
				SetNeedsDisplay ();
				pressed = true;
			}
		}
		
		public override void TouchesCancelled (NSSet touches, UIEvent uievent)
		{
			base.TouchesCancelled (touches, uievent);
			
			if (pressed) {
				SetNeedsDisplay ();
				pressed = false;
			}
		}
		
		public override void TouchesMoved (NSSet touches, UIEvent uievent)
		{
			base.TouchesMoved (touches, uievent);
			
			if (pressed && !IsInsidePhotoButton (touches)) {
				SetNeedsDisplay ();
				pressed = false;
			}
		}
		
		public override void TouchesEnded (NSSet touches, UIEvent uievent)
		{
			base.TouchesEnded (touches, uievent);
			
			if (!pressed)
				return;
			
			ShowPhotoPickerOptions ();
			SetNeedsDisplay ();
			pressed = false;
		}
		
		protected override void Dispose (bool disposing)
		{
			if (photograph != null) {
				photograph.Dispose ();
				photograph = null;
			}
			
			if (dialog != null) {
				dialog.Dispose ();
				dialog = null;
			}
		}
	}
}
