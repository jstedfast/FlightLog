//
// Settings.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2012 Jeffrey Stedfast
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

using MonoTouch.Foundation;

namespace FlightLog {
	public enum PilotCertification {
		[HumanReadableName ("Student Pilot")]
		Student,
		[HumanReadableName ("Sport Pilot")]
		Sport,
		[HumanReadableName ("Recreational Pilot")]
		Recreational,
		[HumanReadableName ("Private Pilot")]
		Private,
		[HumanReadableName ("Commercial Pilot")]
		Commercial,
		[HumanReadableName ("Airline Transport Pilot")]
		AirlineTransport
	}

	public enum FlightTimeFormat {
		[HumanReadableName ("Decimal Format")]
		Decimal,
		[HumanReadableName ("Standard Format (hh:mm)")]
		Standard
	}

	public static class Settings
	{
		static NSUserDefaults settings = NSUserDefaults.StandardUserDefaults;

		public static PilotCertification PilotCertification {
			get {
				string str = settings.StringForKey ("PilotCertification");
				PilotCertification value;

				if (!string.IsNullOrEmpty (str)) {
					if (Enum.TryParse<PilotCertification> (str, out value))
						return value;
				}

				// Assume most users will be Private Pilots
				return PilotCertification.Private;
			}
			set {
				settings.SetString (value.ToString (), "PilotCertification");
			}
		}

		public static string PilotName {
			get {
				return settings.StringForKey ("PilotName");
			}
			set {
				settings.SetString (value, "PilotName");
			}
		}

		public static bool IsCertifiedFlightInstructor {
			get {
				return settings.BoolForKey ("IsCertifiedFlightInstructor");
			}
			set {
				settings.SetBool (value, "IsCertifiedFlightInstructor");
			}
		}

		public static FlightTimeFormat FlightTimeFormat {
			get {
				string str = settings.StringForKey ("FlightTimeFormat");
				FlightTimeFormat value;

				if (!string.IsNullOrEmpty (str)) {
					if (Enum.TryParse<FlightTimeFormat> (str, out value))
						return value;
				}

				return FlightTimeFormat.Standard;
			}
			set {
				settings.SetString (value.ToString (), "FlightTimeFormat");
			}
		}

		public static bool ShowInstrumentExperience {
			get {
				return settings.BoolForKey ("ShowInstrumentExperience");
			}
			set {
				settings.SetBool (value, "ShowInstrumentExperience");
			}
		}
	}
}
