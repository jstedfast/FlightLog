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
		BooleanElement cfi, aifr, hifr, lifr;
		LimitedEntryElement name;

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
			aifr = new BooleanElement ("Instrument Rated (Airplane)", Pilot.InstrumentRatings.HasFlag (InstrumentRating.Airplane));
			hifr = new BooleanElement ("Instrument Rated (Helicopter)", Pilot.InstrumentRatings.HasFlag (InstrumentRating.Helicopter));
			lifr = new BooleanElement ("Instrument Rated (Powered-Lift)", Pilot.InstrumentRatings.HasFlag (InstrumentRating.PoweredLift));
			certification = CreatePilotCertificationElement (Pilot.Certification);
			endorsements = CreateEndorsementsElement (Pilot.Endorsements);
			birthday = new DateEntryElement ("Date of Birth", Pilot.BirthDate);
			medical = new DateEntryElement ("Last Medical Exam", Pilot.LastMedicalExam);
			review = new DateEntryElement ("Last Flight Review", Pilot.LastFlightReview);

			base.LoadView ();

			Root.Add (new Section ("Pilot Information") { name, birthday, certification, endorsements, aifr, hifr, lifr, cfi });
			Root.Add (new Section ("Pilot Status") { medical, review });
		}

		void Save ()
		{
			PilotCertification cert = (PilotCertification) certification.RadioSelected;
			AircraftEndorsement endorsements = AircraftEndorsement.None;
			InstrumentRating ratings = InstrumentRating.None;
			bool changed = false;
			int flag = 1 << 0;

			foreach (var section in this.endorsements) {
				foreach (var element in section) {
					if (((BooleanElement) element).Value)
						endorsements |= (AircraftEndorsement) flag;

					flag <<= 1;
				}
			}

			if (aifr.Value)
				ratings |= InstrumentRating.Airplane;
			if (hifr.Value)
				ratings |= InstrumentRating.Helicopter;
			if (lifr.Value)
				ratings |= InstrumentRating.PoweredLift;

			if (Pilot.Certification != cert) {
				Pilot.Certification = cert;
				changed = true;
			}

			if (Pilot.Endorsements != endorsements) {
				Pilot.Endorsements = endorsements;
				changed = true;
			}

			if (Pilot.IsCertifiedFlightInstructor != cfi.Value) {
				Pilot.IsCertifiedFlightInstructor = cfi.Value;
				changed = true;
			}

			if (Pilot.InstrumentRatings != ratings) {
				Pilot.InstrumentRatings = ratings;
				changed = true;
			}

			if (Pilot.BirthDate != birthday.DateValue) {
				Pilot.BirthDate = birthday.DateValue;
				changed = true;
			}

			if (Pilot.Name != name.Value) {
				Pilot.Name = name.Value;
				changed = true;
			}

			if (Pilot.LastMedicalExam != medical.DateValue) {
				Pilot.LastMedicalExam = medical.DateValue;
				changed = true;
			}

			if (Pilot.LastFlightReview != review.DateValue) {
				Pilot.LastFlightReview = review.DateValue;
				changed = true;
			}

			if (changed)
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
