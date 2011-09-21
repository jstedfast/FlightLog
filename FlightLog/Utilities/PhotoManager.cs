// 
// PhotoManager.cs
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
using System.IO;
using System.Drawing;

using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace FlightLog {
	public static class PhotoManager
	{
		static LRUCache<string, UIImage> ThumbnailCache;
		static string PhotosDir;
		
		static PhotoManager ()
		{
			string Documents = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			PhotosDir = Path.Combine (Path.GetDirectoryName (Documents), "Library", "Photos");
			if (!Directory.Exists (PhotosDir))
				Directory.CreateDirectory (PhotosDir);
			
			ThumbnailCache = new LRUCache<string, UIImage> (12);
		}
		
		public static void Delete (string tailNumber)
		{
			string filename = Path.Combine (PhotosDir, tailNumber + ".jpg");
			
			if (File.Exists (filename))
				File.Delete (filename);
			
			filename = Path.Combine (PhotosDir, tailNumber + "-thumb.jpg");
			
			if (File.Exists (filename))
				File.Delete (filename);
		}
		
		public static UIImage ScaleToSize (UIImage image, int width, int height)
		{
			UIGraphics.BeginImageContext (new SizeF (width, height));
			CGContext ctx = UIGraphics.GetCurrentContext ();
			float ratio = (float) width / (float) height;
			
			ctx.AddRect (new RectangleF (0.0f, 0.0f, width, height));
			ctx.Clip ();
			
			var cg = image.CGImage;
			float h = cg.Height;
			float w = cg.Width;
			float ar = w / h;
			
			if (ar != ratio) {
				// Image's aspect ratio is wrong so we'll need to crop
				float scaleY = height / h;
				float scaleX = width / w;
				PointF offset;
				SizeF crop;
				float size;
				
				if (scaleX >= scaleY) {
					size = h * (w / width);
					offset = new PointF (0.0f, h / 2.0f - size / 2.0f);
					crop = new SizeF (w, size);
				} else {
					size = w * (h / height);
					offset = new PointF (w / 2.0f - size / 2.0f, 0.0f);
					crop = new SizeF (size, h);
				}
				
				ctx.ScaleCTM (1.0f, -1.0f);
				using (var copy = cg.WithImageInRect (new RectangleF (offset, crop))) {
					ctx.DrawImage (new RectangleF (0.0f, 0.0f, width, -height), copy);
				}
			} else {
				image.Draw (new RectangleF (0.0f, 0.0f, width, height));
			}
			
			UIImage scaled = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			
			return scaled;
		}
		
		public static UIImage Load (string tailNumber, bool thumbnail)
		{
			if (thumbnail && ThumbnailCache.Contains (tailNumber))
				return ThumbnailCache[tailNumber];
			
			bool resize = false;
			string path;
			
			if (thumbnail) {
				path = Path.Combine (PhotosDir, tailNumber + "-thumb.jpg");
				if (!File.Exists (path)) {
					path = Path.Combine (PhotosDir, tailNumber + ".jpg");
					resize = true;
				}
			} else
				path = Path.Combine (PhotosDir, tailNumber + ".jpg");
			
			if (!File.Exists (path))
				return null;
			
			UIImage image = UIImage.FromFileUncached (path);
			
			if (image == null)
				return null;
			
			if (resize) {
				UIImage scaled = ScaleToSize (image, 96, 72);
				NSError error;
				
				scaled.AsJPEG ().Save (Path.Combine (PhotosDir, tailNumber + "-thumb.jpg"), true, out error);
				
				image.Dispose ();
				image = scaled;
			}
			
			if (thumbnail)
				ThumbnailCache[tailNumber] = image;
			
			return image;
		}
		
		public static bool Save (string tailNumber, UIImage photo, bool thumbnail, out NSError error)
		{
			string path = Path.Combine (PhotosDir, string.Format ("{0}{1}.jpg", tailNumber, thumbnail ? "-thumb" : ""));
			
			if (thumbnail)
				ThumbnailCache[tailNumber] = photo;
			
			if (!photo.AsJPEG ().Save (path, true, out error))
				return false;
			
			return true;
		}
	}
}
