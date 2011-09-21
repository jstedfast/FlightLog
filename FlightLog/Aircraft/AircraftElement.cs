// 
// AircraftElement.cs
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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class AircraftCell : UITableViewCell
	{
		const float AircraftModelFontSize = 18.0f;
		const float AircraftMakeFontSize = 14.0f;
		const float TailNumberFontSize = 18.0f;
		const float FlightTimeFontSize = 12.0f;
		const float TextPadding = 4.0f;
		const float PhotoHeight = 72.0f;
		const float PhotoWidth = 96.0f;
		const float PhotoXPad = 0.0f;
		const float PhotoYPad = 0.0f;
		
		const float PhotoAreaWidth = 2 * PhotoXPad + PhotoWidth;
		
		const float AircraftModelYOffset = TextPadding;
		const float AircraftMakeYOffset = AircraftModelYOffset + AircraftModelFontSize + TextPadding + 4;
		const float FlightTimeYOffset = AircraftMakeYOffset + AircraftMakeFontSize + TextPadding + 4;
		const float TailNumberYOffset = TextPadding;
		const float TailNumberWidth = 90.0f; // "MMMMMM".Width @ size 18
		
		static UIFont TailNumberFont = UIFont.BoldSystemFontOfSize (TailNumberFontSize);
		static UIFont AircraftModelFont = UIFont.BoldSystemFontOfSize (AircraftModelFontSize);
		static UIFont AircraftMakeFont = UIFont.SystemFontOfSize (AircraftMakeFontSize);
		static UIFont FlightTimeFont = UIFont.SystemFontOfSize (FlightTimeFontSize);
		static UIColor TailNumberColor = UIColor.FromRGB (56, 84, 135); //(147, 170, 204);
		static CGGradient BottomGradient, TopGradient;
		static UIImage DefaultPhoto;
		
		public static float CellHeight = PhotoHeight + 2 * PhotoYPad;
		
		static AircraftCell ()
		{
			DefaultPhoto = UIImage.FromResource (typeof (AircraftCell).Assembly, "FlightLog.Images.mini-plane72.png");
			
			//NSString ns = new NSString ("MMMMMM");
			//Console.WriteLine ("Width needed is: {0}", ns.DrawString (new RectangleF (0, 0, 200, 30), TailNumberFont).Width);
			//TailNumberWidth = ns.DrawString (new RectangleF (0, 0, 200, TailNumberSize), TailNumberFont).Width;
			
			using (var rgb = CGColorSpace.CreateDeviceRGB ()) {
				float [] colorsBottom = {
					1.00f, 1.00f, 1.00f, 0.5f,
					0.93f, 0.93f, 0.93f, 0.5f
				};
				
				BottomGradient = new CGGradient (rgb, colorsBottom, null);
				
				float [] colorsTop = {
					0.93f, 0.93f, 0.93f, 0.5f,
					1.00f, 1.00f, 1.00f, 0.5f
				};
				
				TopGradient = new CGGradient (rgb, colorsTop, null);
			}
		}
		
		public class AircraftCellView : UIView
		{
			Aircraft aircraft;
			
			public AircraftCellView ()
			{
			}
			
			/// <summary>
			/// Gets or sets the aircraft being rendered by this cell view.
			/// </summary>
			/// <value>
			/// The aircraft.
			/// </value>
			public Aircraft Aircraft {
				get { return aircraft; }
				set {
					aircraft = value;
					SetNeedsDisplay ();
				}
			}
			
			/// <summary>
			/// Formats the flight time into a more friendly format.
			/// </summary>
			/// <returns>
			/// A friendly formatted string for the given flight time.
			/// </returns>
			/// <param name='flightTime'>
			/// Flight time in seconds.
			/// </param>
			string FormatFlightTime (int flightTime)
			{
				if (flightTime == 0)
					return "No time logged for this aircraft.";
				
				int minutes = (flightTime / 60) % 60;
				int hours = flightTime / 3600;
				
				if (hours > 1000) {
					// get rid of insiginificant digits
					hours = (hours / 10) * 10;
					minutes = -1;
				} else if (hours > 10) {
					// get rid of insignificant digits
					minutes = -1;
				}
				
				if (minutes < 10) {
					if (minutes > 0 || minutes == -1)
						return string.Format ("Over {0} hours logged in this aircraft.", hours);
					else
						return string.Format ("{0} hours logged in this aircraft.", hours);
				}
				
				return string.Format ("{0} hours and {1} minutes logged in this aircraft.", hours, minutes);
			}
			
			int FlightTime (int hours, int minutes)
			{
				return (hours * 3600) + (minutes * 60);
			}
			
			public override void Draw (RectangleF rect)
			{
				CGContext ctx = UIGraphics.GetCurrentContext ();
				
				// Superview is the container, its superview is the UITableViewCell
				bool highlighted = (Superview.Superview as UITableViewCell).Selected;
				UIColor textColor, tailColor;
				
				var bounds = Bounds;
				var midx = bounds.Width / 2;
				
				if (highlighted) {
					UIColor.FromRGB (4, 0x79, 0xef).SetColor ();
					ctx.FillRect (bounds);
					//Images.MenuShadow.Draw (bounds, CGBlendMode.Normal, 0.5f);
					tailColor = textColor = UIColor.White;
				} else {
					UIColor.White.SetColor ();
					ctx.FillRect (bounds);
					ctx.DrawLinearGradient (BottomGradient, new PointF (midx, bounds.Height - 17), new PointF (midx, bounds.Height), 0);
					ctx.DrawLinearGradient (TopGradient, new PointF (midx, 1), new PointF (midx, 3), 0);
					tailColor = TailNumberColor;
					textColor = UIColor.Black;
				}
				
				// Compute the bounds for each line of text...
				var tailXOffset = bounds.X + bounds.Width - TailNumberWidth - TextPadding;
				var textXOffset = PhotoAreaWidth + TextPadding;
				
				var modelWidth = bounds.Width - PhotoAreaWidth - TailNumberWidth - (TextPadding * 2);
				var makeWidth = bounds.Width - PhotoAreaWidth - (TextPadding * 2);
				var timeWidth = makeWidth;
				
				var modelBounds = new RectangleF (textXOffset, bounds.Y + AircraftModelYOffset, modelWidth, AircraftModelFontSize);
				var makeBounds = new RectangleF (textXOffset, bounds.Y + AircraftMakeYOffset, makeWidth, AircraftModelFontSize);
				var tailBounds = new RectangleF (tailXOffset, bounds.Y + TailNumberYOffset, TailNumberWidth, TailNumberFontSize);
				var timeBounds = new RectangleF (textXOffset, bounds.Y + FlightTimeYOffset, timeWidth, FlightTimeFontSize);
				
				tailColor.SetColor ();
				DrawString (aircraft.TailNumber, tailBounds, TailNumberFont, UILineBreakMode.Clip, UITextAlignment.Left);
				
				textColor.SetColor ();
				DrawString (aircraft.Model ?? "", modelBounds, AircraftModelFont, UILineBreakMode.TailTruncation, UITextAlignment.Left);
				DrawString (aircraft.Make ?? "", makeBounds, AircraftMakeFont, UILineBreakMode.TailTruncation, UITextAlignment.Left);
				
				// FIXME: get the actual flight time
				DrawString (FormatFlightTime (FlightTime (9, 52)), timeBounds, FlightTimeFont, UILineBreakMode.TailTruncation);
				
				UIImage photo = PhotoManager.Load (aircraft.TailNumber, true);
				if (photo == null)
					photo = DefaultPhoto;
				
				photo.Draw (new RectangleF (PhotoXPad, PhotoYPad, PhotoWidth, PhotoHeight));
			}
		}
		
		AircraftCellView view;
		
		public AircraftCell (Aircraft aircraft, NSString key) : base (UITableViewCellStyle.Default, key)
		{
			SelectionStyle = UITableViewCellSelectionStyle.Blue;
			ContentView.ClipsToBounds = true;
			view = new AircraftCellView ();
			view.Aircraft = aircraft;
			ContentView.Add (view);
		}
		
		/// <summary>
		/// Gets or sets the aircraft being rendered by this cell.
		/// </summary>
		/// <value>
		/// The aircraft.
		/// </value>
		public Aircraft Aircraft {
			get { return view.Aircraft; }
			set {
				view.Aircraft = value;
				SetNeedsDisplay ();
			}
		}
		
		//public override UITableViewCellEditingStyle EditingStyle {
		//	get {
		//		// Our only supported editing operation is delete.
		//		return UITableViewCellEditingStyle.Delete;
		//	}
		//}
		
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			view.Frame = ContentView.Bounds;
			view.SetNeedsDisplay ();
		}
	}
	
	public class AircraftElement : Element, IElementSizing
	{
		static NSString key = new NSString ("AircraftElement");
		
		public AircraftElement (Aircraft aircraft) : base (null)
		{
			aircraft.Updated += OnAircraftUpdated;
			Aircraft = aircraft;
		}
		
		/// <summary>
		/// Gets or sets the aircraft for this table element.
		/// </summary>
		/// <value>
		/// The aircraft.
		/// </value>
		public Aircraft Aircraft {
			get; private set;
		}
		
		public event EventHandler<EventArgs> Changed;
		
		void OnAircraftUpdated (object sender, EventArgs args)
		{
			var handler = Changed;
			
			if (handler != null)
				handler (this, EventArgs.Empty);
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			AircraftCell cell = tv.DequeueReusableCell (key) as AircraftCell;
			
			if (cell == null)
				cell = new AircraftCell (Aircraft, key);
			else
				cell.Aircraft = Aircraft;
			
			return cell;
		}
		
		public override bool Matches (string text)
		{
			return (Aircraft.TailNumber.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1) || 
				(Aircraft.Make.IndexOf (text, StringComparison.InvariantCultureIgnoreCase) != -1) || 
					(Aircraft.Model.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1);
		}
		
		#region IElementSizing implementation
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return AircraftCell.CellHeight;
		}
		#endregion
		
		protected override void Dispose (bool disposing)
		{
			if (Aircraft != null) {
				Aircraft.Updated -= OnAircraftUpdated;
				Aircraft = null;
			}
			
			base.Dispose (disposing);
		}
	}
}
