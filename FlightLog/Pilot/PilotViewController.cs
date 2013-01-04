//
// PilotViewController.cs
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
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class PilotViewController : DialogViewController
	{
		DateEntryElement birthday, medical, review;
		RootElement certification, endorsements;
		LimitedEntryElement name;
		BooleanElement cfi, ifr;

		public PilotViewController (Pilot pilot) : base (UITableViewStyle.Grouped, new RootElement (null))
		{
			TabBarItem.Image = UIImage.FromBundle ("Images/user");
			Autorotate = true;
			Title = "Pilot";
			Pilot = pilot;
		}

		public Pilot Pilot {
			get; private set;
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

		static void PopulateEndorsementSection (Section section, AircraftEndorsement endorsements, AircraftEndorsement mask)
		{
			string caption;

			foreach (AircraftEndorsement endorsement in Enum.GetValues (typeof (AircraftEndorsement))) {
				if (endorsement == AircraftEndorsement.None || !mask.HasFlag (endorsement))
					continue;

				caption = endorsement.ToHumanReadableName ();
				section.Add (new BooleanElement (caption, endorsements.HasFlag (endorsement), endorsement.ToString ()));
			}
		}

		static RootElement CreateEndorsementsElement (AircraftEndorsement endorsements)
		{
			RootElement root = new RootElement ("Aircraft Ratings & Endorsements");
			Section section;

			foreach (AircraftCategory category in Enum.GetValues (typeof (AircraftCategory))) {
				section = new Section (category.ToHumanReadableName ());
				PopulateEndorsementSection (section, endorsements, Pilot.GetEndorsementMask (category));
				root.Add (section);
			}

			return root;
		}

		public override void LoadView ()
		{
			name = new LimitedEntryElement ("Name", "Enter the name of the pilot.", Pilot.Name);
			cfi = new BooleanElement ("Certified Flight Instructor", Pilot.IsCertifiedFlightInstructor);
			ifr = new BooleanElement ("Instrument Rated / In-Training", Pilot.IsInstrumentRated);
			certification = CreatePilotCertificationElement (Pilot.Certification);
			endorsements = CreateEndorsementsElement (Pilot.Endorsements);
			birthday = new DateEntryElement ("Date of Birth", Pilot.BirthDate);
			medical = new DateEntryElement ("Last Medical Exam", Pilot.LastMedicalExam);
			review = new DateEntryElement ("Last Flight Review", Pilot.LastFlightReview);

			base.LoadView ();

			Root.Add (new Section ("Pilot Information") { name, birthday, certification, endorsements, ifr, cfi });
			Root.Add (new Section ("Pilot Status") { medical, review });
		}

		void Save ()
		{
			AircraftEndorsement endorsements = AircraftEndorsement.None;
			int endorsement = 1 << 0;

			foreach (var section in this.endorsements) {
				foreach (var element in section) {
					if (((BooleanElement) element).Value)
						endorsements |= (AircraftEndorsement) endorsement;

					endorsement <<= 1;
				}
			}

			Pilot.Certification = (PilotCertification) certification.RadioSelected;
			Pilot.Endorsements = endorsements;
			Pilot.IsCertifiedFlightInstructor = cfi.Value;
			Pilot.IsInstrumentRated = ifr.Value;
			Pilot.BirthDate = birthday.DateValue;
			Pilot.Name = name.Value;

			Pilot.LastMedicalExam = medical.DateValue;
			Pilot.LastFlightReview = review.DateValue;

			LogBook.Update (Pilot);
		}

		public override void ViewWillAppear (bool animated)
		{
			Save ();

			base.ViewWillAppear (animated);
		}

		public override void ViewWillDisappear (bool animated)
		{
			Save ();

			base.ViewWillDisappear (animated);
		}
	}
}
