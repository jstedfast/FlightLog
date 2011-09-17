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

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace FlightLog {
	public static class PhotoManager
	{
		static string Documents;
		
		static PhotoManager ()
		{
			Documents = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
		}
		
		public static void Delete (string tailNumber)
		{
			string filename = Path.Combine (Documents, tailNumber + ".jpg");
			
			if (File.Exists (filename))
				File.Delete (filename);
		}
		
		public static UIImage Load (string tailNumber)
		{
			return UIImage.FromFileUncached (Path.Combine (Documents, tailNumber + ".jpg"));
		}
		
		public static bool Save (string tailNumber, UIImage photo, out NSError error)
		{
			string filename = Path.Combine (Documents, tailNumber + ".jpg");
			
			return photo.AsJPEG ().Save (filename, true, out error);
		}
	}
}
