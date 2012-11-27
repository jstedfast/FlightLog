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
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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
		const string ManufacturerKey = "Manufacturer Name";
		const string ModelKey = "Model";

		static string Normalize (string name)
		{
			StringBuilder sb = new StringBuilder (name.Length);

			sb.Append (char.ToUpperInvariant (name[0]));
			for (int i = 1; i < name.Length; i++)
				sb.Append (char.ToLowerInvariant (name[i]));

			return sb.ToString ();
		}

		static AircraftDetails ParseAircraftDetails (Stream stream)
		{
			Dictionary<string, string> metadata = new Dictionary<string, string> (StringComparer.InvariantCultureIgnoreCase);
			HtmlDocument doc = new HtmlDocument ();
			HtmlNode description = null;
			string key = null;

			doc.Load (stream);

			foreach (var h3 in doc.DocumentNode.SelectNodes ("//h3")) {
				if (h3.InnerText == "Aircraft Description" && h3.ParentNode.Name == "div") {
					var table = h3.ParentNode.ChildNodes.Where (tag => tag.Name == "table").FirstOrDefault ();
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
						var span = td.ChildNodes.Where (tag => tag.Name == "span" || tag.Name == "strong").FirstOrDefault ();
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
				make = Normalize (make);
			else
				make = null;

			if (!metadata.TryGetValue (ModelKey, out model))
				model = null;

			return new AircraftDetails (make, model);
		}

		static AircraftDetails GetAircraftDetails (string tailNumber)
		{
			string url = "http://registry.faa.gov/aircraftinquiry/NNum_Results.aspx?NNumbertxt=" + tailNumber;
			HttpWebRequest request = (HttpWebRequest) WebRequest.Create (url);
			request.AllowAutoRedirect = true;

			var response = (HttpWebResponse) request.GetResponse ();

			return ParseAircraftDetails (response.GetResponseStream ());
		}

		public static Task<AircraftDetails> GetAircraftDetails (string tailNumber, CancellationToken cancelToken)
		{
			return Task.Factory.StartNew (() => {
				return GetAircraftDetails (tailNumber);
			}, cancelToken);
		}
	}
}
