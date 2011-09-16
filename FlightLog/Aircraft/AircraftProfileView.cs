// 
// AircraftProfileView.cs
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

using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class AircraftProfileView : UIView
	{
		const float AircraftMakeFontSize = 18.0f;
		const float AircraftModelFontSize = 16.0f;
		const float RemarksFontSize = 13.0f;
		const float XBorderPadding = 42.0f;
		const float YBorderPadding = 20.0f;
		const float ImageTextPadding = 20.0f;
		const float TextPadding = 8.0f;
		const float PhotoSize = 128.0f;
		
		const float TextOffset = XBorderPadding + PhotoSize + ImageTextPadding;
		const float MakeYOffset = YBorderPadding + TextPadding;
		const float ModelYOffset = MakeYOffset + AircraftMakeFontSize + TextPadding;
		const float RemarksYOffset = ModelYOffset + AircraftModelFontSize + TextPadding * 3;
		const float ProfileHeight = PhotoSize + YBorderPadding * 2;
		
		static UIFont AircraftModelFont = UIFont.BoldSystemFontOfSize (AircraftModelFontSize);
		static UIFont AircraftMakeFont = UIFont.BoldSystemFontOfSize (AircraftMakeFontSize);
		static UIFont RemarksFont = UIFont.ItalicSystemFontOfSize (RemarksFontSize);
		static CGPath photoBorder = GraphicsUtil.MakeRoundedPath (PhotoSize, 8.0f);
		static UIColor TextColor = UIColor.FromRGB (76, 86, 108);
		static UIImage DefaultPhoto;
		
		UIImageView photoView;
		
		static AircraftProfileView ()
		{
			DefaultPhoto = UIImage.FromResource (typeof (AircraftProfileView).Assembly, "FlightLog.Images.mini-plane128.png");
		}
		
		public AircraftProfileView (float width) : this (new RectangleF (0.0f, 0.0f, width, ProfileHeight)) { }
		
		public AircraftProfileView (RectangleF frame) : base (frame)
		{
			BackgroundColor = UIColor.Clear;
			
			// Add a subview for the aircraft's photo
			photoView = new UIImageView (new RectangleF (XBorderPadding, YBorderPadding, PhotoSize, PhotoSize));
			photoView.BackgroundColor = UIColor.Clear;
			AddSubview (photoView);
		}
		
		public UIImage Photograph {
			get { return photoView.Image; }
			set {
				if (value == null)
					photoView.Image = DefaultPhoto;
				else
					photoView.Image = value;
			}
		}
		
		public string Make {
			get; set;
		}
		
		public string Model {
			get; set;
		}
		
		public string Remarks {
			get; set;
		}
		
		public override void Draw (RectangleF rect)
		{
			float textWidth = rect.Width - TextOffset - XBorderPadding;
			CGContext ctx = UIGraphics.GetCurrentContext ();
			float y = rect.Y;
			float x = rect.X;
			
			TextColor.SetColor ();
			
			DrawString (Make ?? "Unknown Make", new RectangleF (x + TextOffset, y + MakeYOffset, textWidth, AircraftMakeFontSize), AircraftMakeFont);
			DrawString (Model ?? "Unknown Model", new RectangleF (x + TextOffset, y + ModelYOffset, textWidth, AircraftModelFontSize), AircraftModelFont);
			DrawString (Remarks ?? "", new RectangleF (x + TextOffset, y + RemarksYOffset, textWidth, RemarksFontSize * 3), RemarksFont, UILineBreakMode.WordWrap);
			
			ctx.TranslateCTM (XBorderPadding, YBorderPadding);
			ctx.AddPath (photoBorder);
			ctx.SetStrokeColor (0.5f, 0.5f, 0.5f, 1.0f);
			ctx.SetLineWidth (1.0f);
			ctx.StrokePath ();
		}
	}
}
