// 
// FlightTableViewCell.cs
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

using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class FlightTableViewCell : UITableViewCell
	{
		const float AirportFontSize = 17.0f;
		const float AircraftFontSize = 14.0f;
		const float RemarksFontSize = 11.0f;
		const float ViaFontSize = 13.0f;
		const float ImagePadding = 3.0f;
		const float TextPadding = 4.0f;
		
		const float AirportYOffset = TextPadding;
		const float ViaYOffset = TextPadding + (AirportFontSize - ViaFontSize);
		const float AircraftYOffset = AirportYOffset + AirportFontSize + TextPadding / 2;
		const float RemarksYOffset = AircraftYOffset + AircraftFontSize + TextPadding;
		
		static UIFont AirportFont = UIFont.BoldSystemFontOfSize (AirportFontSize);
		static UIFont AircraftFont = UIFont.BoldSystemFontOfSize (AircraftFontSize);
		static UIFont RemarksFont = UIFont.ItalicSystemFontOfSize (RemarksFontSize);
		static UIFont ViaBoldFont = UIFont.BoldSystemFontOfSize (ViaFontSize);
		static UIFont ModelFont = UIFont.SystemFontOfSize (AircraftFontSize);
		static UIFont ViaFont = UIFont.SystemFontOfSize (ViaFontSize);
		static UIColor AirportColor = UIColor.FromRGB (56, 84, 135);
		static UIColor AircraftColor = UIColor.FromRGB (56, 84, 135);
		static UIColor RemarksColor = UIColor.DarkGray;
		static CGGradient BottomGradient, TopGradient;
		static UIImage Calendar;
		
		public static float CellHeight;
		
		static FlightTableViewCell ()
		{
			Calendar = UIImage.FromResource (typeof (FlightTableViewCell).Assembly, "FlightLog.Images.calendar.png");
			
			CellHeight = Calendar.Size.Height + ImagePadding * 2;
			
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
		
		class FlightCellView : UIView
		{
			Flight flight;
			
			public FlightCellView ()
			{
			}
			
			/// <summary>
			/// Gets or sets the flight data being rendered by this cell view.
			/// </summary>
			/// <value>
			/// The flight data.
			/// </value>
			public Flight Flight {
				get { return flight; }
				set {
					flight = value;
					SetNeedsDisplay ();
				}
			}
			
			static UIImage CreateCalendarImage (DateTime date)
			{
				string month = date.ToString ("MMMM");
				string day = date.Day.ToString ();
				
				using (var cs = CGColorSpace.CreateDeviceRGB ()) {
					using (var context = new CGBitmapContext (IntPtr.Zero, 57, 57, 8, 57 * 4, cs, CGImageAlphaInfo.PremultipliedLast)) {
						//context.ScaleCTM (0.5f, -1);
						context.TranslateCTM (0, 0);
						context.DrawImage (new RectangleF (0, 0, 57, 57), Calendar.CGImage);
						context.SetFillColor (1.0f, 1.0f, 1.0f, 1.0f);
						
						context.SelectFont ("Helvetica", 10f, CGTextEncoding.MacRoman);
						
						// Pretty lame way of measuring strings, as documented:
						var start = context.TextPosition.X;					
						context.SetTextDrawingMode (CGTextDrawingMode.Invisible);
						context.ShowText (month);
						var width = context.TextPosition.X - start;
						
						context.SetTextDrawingMode (CGTextDrawingMode.Fill);
						context.ShowTextAtPoint ((57 - width) / 2, 46, month);
						
						// The big string
						context.SelectFont ("Helvetica-Bold", 32, CGTextEncoding.MacRoman);					
						start = context.TextPosition.X;
						context.SetTextDrawingMode (CGTextDrawingMode.Invisible);
						context.ShowText (day);
						width = context.TextPosition.X - start;
						
						context.SetFillColor (0.0f, 0.0f, 0.0f, 1.0f);
						context.SetTextDrawingMode (CGTextDrawingMode.Fill);
						context.ShowTextAtPoint ((57 - width) / 2, 9, day);
						
						context.StrokePath ();
						
						return UIImage.FromImage (context.ToImage ());
					}
				}
			}
			
			static Dictionary<int, UIImage> CalendarImages = new Dictionary<int, UIImage> ();
			static UIImage CalendarImageForDate (DateTime date)
			{
				int key = (date.Month << 5) | date.Day;
				UIImage image;
				
				if (CalendarImages.ContainsKey (key))
					return CalendarImages[key];
				
				image = CreateCalendarImage (date);
				CalendarImages.Add (key, image);
				
				return image;
			}
			
			static string FormatFlightTime (int seconds)
			{
				double time = Math.Round (seconds / 3600.0, 1);
				
				if (time > 0.9 && time < 1.1)
					return "1 hour";
				
				return time.ToString () + " hours";
			}
			
			public override void Draw (RectangleF area)
			{
				CGContext ctx = UIGraphics.GetCurrentContext ();
				
				// Superview is the container, its superview is the UITableViewCell
				bool highlighted = (Superview.Superview as UITableViewCell).Selected;
				UIColor textColor, airportColor, aircraftColor, remarksColor;
				
				var bounds = Bounds;
				var midx = bounds.Width / 2;
				
				if (highlighted) {
					UIColor.FromRGB (4, 0x79, 0xef).SetColor ();
					ctx.FillRect (bounds);
					//Images.MenuShadow.Draw (bounds, CGBlendMode.Normal, 0.5f);
					aircraftColor = UIColor.White;
					airportColor = UIColor.White;
					remarksColor = UIColor.White;
					textColor = UIColor.White;
				} else {
					UIColor.White.SetColor ();
					ctx.FillRect (bounds);
					ctx.DrawLinearGradient (BottomGradient, new PointF (midx, bounds.Height - 17), new PointF (midx, bounds.Height), 0);
					ctx.DrawLinearGradient (TopGradient, new PointF (midx, 1), new PointF (midx, 3), 0);
					aircraftColor = AircraftColor;
					airportColor = AirportColor;
					remarksColor = RemarksColor;
					textColor = UIColor.Black;
				}
				
				UIImage image = CalendarImageForDate (Flight.Date);
				image.Draw (new RectangleF (new PointF (bounds.X + ImagePadding, bounds.Y + ImagePadding), image.Size));
				
				float width = bounds.Width - (ImagePadding + image.Size.Width + TextPadding * 2);
				float x = bounds.X + ImagePadding + image.Size.Width + TextPadding;
				float y = bounds.Y + AirportYOffset;
				RectangleF rect;
				SizeF size;
				
				if (flight.AirportDeparted != null || flight.AirportArrived != null) {
					// Render the departed airport
					airportColor.SetColor ();
					rect = new RectangleF (x, y, width, AirportFontSize);
					if (flight.AirportDeparted != null)
						size = DrawString (Flight.AirportDeparted, rect, AirportFont, UILineBreakMode.TailTruncation, UITextAlignment.Left);
					else
						size = DrawString (Flight.AirportArrived, rect, AirportFont, UILineBreakMode.TailTruncation, UITextAlignment.Left);
					width -= size.Width;
					x += size.Width;
					
					if (flight.AirportArrived != null) {
						// Render the '-' between the departed and arrived airports
						textColor.SetColor ();
						rect = new RectangleF (x, y, width, AirportFontSize);
						size = DrawString ("-", rect, AirportFont, UILineBreakMode.TailTruncation, UITextAlignment.Left);
						width -= size.Width;
						x += size.Width;
						
						// Render the arrived airport
						airportColor.SetColor ();
						rect = new RectangleF (x, y, width, AirportFontSize);
						size = DrawString (Flight.AirportArrived, rect, AirportFont, UILineBreakMode.TailTruncation, UITextAlignment.Left);
						width -= size.Width;
						x += size.Width;
					}
					
					// Render any additional airports visited
					List<string> visited = new List<string> ();
					
					if (flight.AirportVisited1 != null && flight.AirportVisited1.Length > 0)
						visited.Add (flight.AirportVisited1);
					if (flight.AirportVisited2 != null && flight.AirportVisited2.Length > 0)
						visited.Add (flight.AirportVisited2);
					if (flight.AirportVisited3 != null && flight.AirportVisited3.Length > 0)
						visited.Add (flight.AirportVisited3);
					
					string[] prefix = new string[] { " via ", ", ", ", " };
					for (int i = 0; i < visited.Count; i++) {
						textColor.SetColor ();
						rect = new RectangleF (x, bounds.Y + ViaYOffset, width, ViaFontSize);
						size = DrawString (prefix[i], rect, ViaFont, UILineBreakMode.TailTruncation, UITextAlignment.Left);
						width -= size.Width;
						x += size.Width;
						
						airportColor.SetColor ();
						rect = new RectangleF (x, bounds.Y + ViaYOffset, width, ViaFontSize);
						size = DrawString (visited[i], rect, ViaBoldFont, UILineBreakMode.TailTruncation, UITextAlignment.Left);
						width -= size.Width;
						x += size.Width;
					}
				}
				
				// Move down onto the next line (to render the aircraft info)
				width = bounds.Width - (ImagePadding + image.Size.Width + TextPadding * 2);
				x = bounds.X + ImagePadding + image.Size.Width + TextPadding;
				y = bounds.Y + AircraftYOffset;
				
				// Render the Aircraft tail number
				aircraftColor.SetColor ();
				rect = new RectangleF (x, y, width, AircraftFontSize);
				size = DrawString (Flight.Aircraft, rect, AircraftFont, UILineBreakMode.TailTruncation, UITextAlignment.Left);
				width -= size.Width;
				x += size.Width;
				
				// Render the Aircraft model
				Aircraft aircraft = LogBook.GetAircraft (Flight.Aircraft);
				if (aircraft != null && aircraft.Model != null) {
					width -= TextPadding;
					x += TextPadding;
					textColor.SetColor ();
					rect = new RectangleF (x, y, width, AircraftFontSize);
					size = DrawString (aircraft.Model, rect, ModelFont, UILineBreakMode.TailTruncation, UITextAlignment.Left);
					width -= size.Width;
					x += size.Width;
				}
				
				// Render the Flight Time
				textColor.SetColor ();
				width -= TextPadding;
				x += TextPadding;
				rect = new RectangleF (x, y, width, AircraftFontSize);
				size = DrawString (FormatFlightTime (Flight.FlightTime), rect, ModelFont, UILineBreakMode.TailTruncation,
					UITextAlignment.Right);
				width -= size.Width;
				x += size.Width;
				
				// Move down onto the next line (to render the remarks)
				width = bounds.Width - (ImagePadding + image.Size.Width + TextPadding * 2);
				x = bounds.X + ImagePadding + image.Size.Width + TextPadding;
				y = bounds.Y + RemarksYOffset;
				
				// Render the remarks
				if (Flight.Remarks != null) {
					remarksColor.SetColor ();
					rect = new RectangleF (x, y, width, RemarksFontSize);
					size = DrawString (Flight.Remarks, rect, RemarksFont, UILineBreakMode.TailTruncation, UITextAlignment.Left);
					width -= size.Width;
					x += size.Width;
				}
			}
		}
		
		FlightCellView view;
		
		public FlightTableViewCell (NSString key) : base (UITableViewCellStyle.Default, key)
		{
			SelectionStyle = UITableViewCellSelectionStyle.Blue;
			Accessory = UITableViewCellAccessory.None;
			ContentMode = UIViewContentMode.Left;
			ContentView.ClipsToBounds = true;
			view = new FlightCellView ();
			
			ContentView.Add (view);
		}
		
		/// <summary>
		/// Gets or sets the flight data being rendered by this cell.
		/// </summary>
		/// <value>
		/// The flight data.
		/// </value>
		public Flight Flight {
			get { return view.Flight; }
			set {
				view.Flight = value;
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
}
