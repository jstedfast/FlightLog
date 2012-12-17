//
// SettingsViewController.cs
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

using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog
{
	public class SettingsViewController : DialogViewController
	{
		RootElement certification, format;
		LimitedEntryElement name;
		BooleanElement cfi;

		public SettingsViewController () : base (UITableViewStyle.Grouped, new RootElement (null))
		{
			TabBarItem.Image = UIImage.FromBundle ("Images/sliders");
			Title = "Settings";
			Autorotate = true;
		}

		static RootElement CreatePilotCertificationElement (PilotCertification certification)
		{
			RootElement root = new RootElement ("Pilot Certification", new RadioGroup ("PilotCertification", 0));
			Section section = new Section ();

			foreach (PilotCertification value in Enum.GetValues (typeof (PilotCertification)))
				section.Add (new RadioElement (value.ToHumanReadableName (), "PilotCertification"));

			root.Add (section);

			root.RadioSelected = (int) certification;

			return root;
		}

		static RootElement CreateFlightTimeFormatElement (FlightTimeFormat format)
		{
			RootElement root = new RootElement ("Flight Time Format", new RadioGroup ("FlightTimeFormat", 0));
			Section section = new Section ();

			foreach (FlightTimeFormat value in Enum.GetValues (typeof (FlightTimeFormat)))
				section.Add (new RadioElement (value.ToHumanReadableName (), "FlightTimeFormat"));

			root.Add (section);

			root.RadioSelected = (int) format;

			return root;
		}

		public override void LoadView ()
		{
			name = new LimitedEntryElement ("Pilot's Name", "Enter the name of the pilot.", Settings.PilotName);
			cfi = new BooleanElement ("Certified Flight Instructor", Settings.IsCertifiedFlightInstructor);
			certification = CreatePilotCertificationElement (Settings.PilotCertification);
			format = CreateFlightTimeFormatElement (Settings.FlightTimeFormat);

			base.LoadView ();

			Root.Add (new Section ("Pilot Information") { name, certification, cfi });
			Root.Add (new Section ("LogBook Entry") { format });
		}

		public override void ViewWillDisappear (bool animated)
		{
			Settings.PilotCertification = (PilotCertification) certification.RadioSelected;
			Settings.IsCertifiedFlightInstructor = cfi.Value;
			Settings.PilotName = name.Value;

			Settings.FlightTimeFormat = (FlightTimeFormat) format.RadioSelected;

			base.ViewWillDisappear (animated);
		}
	}
}
