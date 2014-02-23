//
// FlightAware.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc.
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
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MonoTouch.SystemConfiguration;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;

using HtmlAgilityPack;

namespace FlightLog
{
	public static class FlightAware
	{
		const string UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.7; rv:17.0) Gecko/20100101 Firefox/17.0";
		const string HostName = "flightaware.com";

		static NetworkReachability reachability = null;
		static NetworkReachabilityFlags flags;
		static bool haveFlags = false;

		static void ReachabilityChanged (NetworkReachabilityFlags flags)
		{
			FlightAware.flags = flags;
			haveFlags = true;
		}

		public static bool IsReachableViaWiFi {
			get {
				if (reachability == null) {
					reachability = new NetworkReachability (HostName);
					reachability.Schedule (CFRunLoop.Current, CFRunLoop.ModeDefault);
					reachability.SetNotification (ReachabilityChanged);

					haveFlags = reachability.TryGetFlags (out flags);
				}

				if (!haveFlags)
					return false;

				if (!flags.HasFlag (NetworkReachabilityFlags.Reachable))
					return false;

				if (flags.HasFlag (NetworkReachabilityFlags.IsWWAN))
					return false;

				return true;
			}
		}

		static string ScrapeHtmlForPhotoPageUrl (Stream stream)
		{
			HtmlDocument doc = new HtmlDocument ();

			doc.Load (stream);

			foreach (var div in doc.DocumentNode.SelectNodes ("//div").Where (tag => tag.HasAttributes)) {
				var attr = div.Attributes.FirstOrDefault (x => x.Name == "class");

				if (attr == null || attr.Value != "track-panel-header")
					continue;

				var a = div.ChildNodes.FirstOrDefault (tag => tag.Name == "a" && tag.HasChildNodes && tag.FirstChild.Name == "img");
				if (a == null || !a.HasAttributes)
					continue;

				var href = a.Attributes.FirstOrDefault (x => x.Name == "href");
				if (href == null)
					continue;

				var url = href.Value;
				if (url[0] == '/')
					url = "http://" + HostName + url;

				return url;
			}

			throw new Exception ("Aircraft photo page url not found.");
		}

		static string ScrapeHtmlForPhotoUrl (Stream stream)
		{
			var doc = new HtmlDocument ();

			doc.Load (stream);

			foreach (var img in doc.DocumentNode.SelectNodes ("//img").Where (tag => tag.HasAttributes)) {
				var attr = img.Attributes.FirstOrDefault (x => x.Name == "id");

				if (attr != null && attr.Value == "photo_main") {
					var src = img.Attributes.FirstOrDefault (x => x.Name == "src");
					if (src == null)
						continue;

					var url = src.Value;
					if (url[0] == '/')
						url = "http://" + HostName + url;

					return url;
				}
			}

			throw new Exception ("Aircraft photo url not found.");
		}

		static Stream RequestStream (string url, CancellationToken cancelToken, bool keepAlive)
		{
			var request = (HttpWebRequest) WebRequest.Create (url);

			request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
			request.AllowAutoRedirect = true;
			request.KeepAlive = keepAlive;
			request.UserAgent = UserAgent;

			if (cancelToken.IsCancellationRequested)
				throw new OperationCanceledException (cancelToken);

			using (var response = (HttpWebResponse) request.GetResponse ()) {
				var stream = response.GetResponseStream ();
				var mem = new MemoryStream ();
				var buf = new byte[4096];
				int nread;

				do {
					if (cancelToken.IsCancellationRequested)
						throw new OperationCanceledException (cancelToken);

					if ((nread = stream.Read (buf, 0, buf.Length)) > 0)
						mem.Write (buf, 0, nread);
				} while (nread > 0);

				mem.Seek (0, SeekOrigin.Begin);

				return mem;
			}
		}

		static NSData RequestAircraftPhoto (string tailNumber, CancellationToken cancelToken)
		{
			string url = "http://" + HostName + "/live/flight/" + tailNumber;
			Stream stream;

			using (stream = RequestStream (url, cancelToken, true)) {
				url = ScrapeHtmlForPhotoPageUrl (stream);
			}

			using (stream = RequestStream (url, cancelToken, true)) {
				url = ScrapeHtmlForPhotoUrl (stream);
			}

			using (stream = RequestStream (url, cancelToken, false)) {
				return NSData.FromStream (stream);
			}
		}

		public static Task<NSData> GetAircraftPhoto (string tailNumber, CancellationToken cancelToken)
		{
			return Task.Factory.StartNew (() => {
				return RequestAircraftPhoto (tailNumber, cancelToken);
			}, cancelToken);
		}
	}
}
