//
// FederalAviationAdministration.cs
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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using MonoTouch.SystemConfiguration;
using MonoTouch.CoreFoundation;

using HtmlAgilityPack;

namespace FlightLog
{
	public class AircraftDetails
	{
		internal AircraftDetails (string make, string model)
		{
			Make = make;
			Model = model;
		}

		public string Make {
			get; private set;
		}

		public string Model {
			get; private set;
		}
	}

	public static class FAARegistry
	{
		const string UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.7; rv:17.0) Gecko/20100101 Firefox/17.0";
		const string ManufacturerKey = "Manufacturer Name";
		const string ModelKey = "Model";
		const string HostName = "registry.faa.gov";

		static readonly Dictionary<string, string> manufacturers;
		static NetworkReachability reachability = null;
		static NetworkReachabilityFlags flags;
		static bool haveFlags = false;

		static FAARegistry ()
		{
			manufacturers = new Dictionary<string, string> ();
			manufacturers.Add ("BEECH", "Beechcraft");
			manufacturers.Add ("BELLANCA", "Bellanca Aircraft Company");
			manufacturers.Add ("CESSNA", "Cessna Aircraft Company");
			manufacturers.Add ("PIPER", "Piper Aircraft");
		}

		static void ReachabilityChanged (NetworkReachabilityFlags flags)
		{
			FAARegistry.flags = flags;
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

		static string Normalize (string name)
		{
			var builder = new StringBuilder (name.Length);
			bool upper = false;

			builder.Append (char.ToUpperInvariant (name[0]));
			for (int i = 1; i < name.Length; i++) {
				if (char.IsWhiteSpace (name[i])) {
					builder.Append (name[i]);
					upper = true;
					continue;
				}

				if (upper) {
					builder.Append (char.ToUpperInvariant (name[i]));
					upper = false;
				} else
					builder.Append (char.ToLowerInvariant (name[i]));
			}

			return builder.ToString ();
		}

		static string GetManufacturer (string name)
		{
			string make;

			if (manufacturers.TryGetValue (name, out make))
				return make;

			return Normalize (name);
		}

		static AircraftDetails ParseAircraftDetails (Stream stream)
		{
			var metadata = new Dictionary<string, string> (StringComparer.InvariantCultureIgnoreCase);
			var doc = new HtmlDocument ();
			HtmlNode description = null;
			string key = null;

			doc.Load (stream);

			foreach (var h3 in doc.DocumentNode.SelectNodes ("//h3")) {
				if (h3.InnerText == "Aircraft Description" && h3.ParentNode.Name == "div") {
					var table = h3.ParentNode.ChildNodes.FirstOrDefault (tag => tag.Name == "table");
					if (table == null)
						continue;

					description = table;
					break;
				}
			}

			if (description == null)
				throw new Exception ("Could not locate Aircraft Description table.");

			foreach (var tr in description.ChildNodes.Where (tag => tag.Name == "tr")) {
				foreach (var td in tr.ChildNodes.Where (tag => tag.Name == "td")) {
					string value;

					if (td.HasChildNodes) {
						var span = td.ChildNodes.FirstOrDefault (tag => tag.Name == "span" || tag.Name == "strong");
						value = span != null ? span.InnerText.Trim () : td.InnerText.Trim ();
					} else {
						value = td.InnerText.Trim ();
					}

					if (key != null) {
						metadata.Add (key, value);
						key = null;
					} else {
						key = value;
					}
				}
			}

			string make, model;

			if (metadata.TryGetValue (ManufacturerKey, out make))
				make = GetManufacturer (make);
			else
				make = null;

			if (!metadata.TryGetValue (ModelKey, out model))
				model = null;

			return new AircraftDetails (make, model);
		}

		static AircraftDetails RequestAircraftDetails (string tailNumber, CancellationToken cancelToken)
		{
			string url = "http://" + HostName + "/aircraftinquiry/NNum_Results.aspx?NNumbertxt=" + tailNumber;
			var request = (HttpWebRequest) WebRequest.Create (url);

			request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
			request.AllowAutoRedirect = true;
			request.UserAgent = UserAgent;
			request.KeepAlive = false;

			if (cancelToken.IsCancellationRequested)
				throw new OperationCanceledException (cancelToken);

			using (var response = (HttpWebResponse) request.GetResponse ()) {
				var stream = response.GetResponseStream ();
				using (var mem = new MemoryStream ()) {
					var buf = new byte[4096];
					int nread;

					do {
						if (cancelToken.IsCancellationRequested)
							throw new OperationCanceledException (cancelToken);

						if ((nread = stream.Read (buf, 0, buf.Length)) > 0)
							mem.Write (buf, 0, nread);
					} while (nread > 0);

					mem.Seek (0, SeekOrigin.Begin);

					return ParseAircraftDetails (mem);
				}
			}
		}

		public static Task<AircraftDetails> GetAircraftDetails (string tailNumber, CancellationToken cancelToken)
		{
			return Task.Factory.StartNew (() => {
				return RequestAircraftDetails (tailNumber, cancelToken);
			}, cancelToken);
		}
	}
}
