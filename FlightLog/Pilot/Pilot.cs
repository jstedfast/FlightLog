//
// Pilot.cs
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

using MonoTouch.SQLite;

namespace FlightLog {
	public class Pilot
	{
		public static readonly DateTime WrightBrosFirstFlight = new DateTime (1903, 12, 17, 0, 0, 0, DateTimeKind.Local);

		public Pilot ()
		{
			BirthDate = WrightBrosFirstFlight;
			LastMedicalExam = WrightBrosFirstFlight;
			LastFlightReview = WrightBrosFirstFlight;
		}

		public static AircraftEndorsement GetEndorsementMask (AircraftCategory category)
		{
			AircraftEndorsement mask = AircraftEndorsement.None;

			switch (category) {
			case AircraftCategory.Airplane:
				mask |= AircraftEndorsement.SingleEngineLand;
				mask |= AircraftEndorsement.SingleEngineSea;
				mask |= AircraftEndorsement.MultiEngineLand;
				mask |= AircraftEndorsement.MultiEngineSea;

				mask |= AircraftEndorsement.Complex;
				mask |= AircraftEndorsement.HighPerformance;
				mask |= AircraftEndorsement.TailDragger;
				break;
			case AircraftCategory.Rotorcraft:
				mask |= AircraftEndorsement.Helicoptor;
				mask |= AircraftEndorsement.Gryoplane;
				break;
			case AircraftCategory.Glider:
				mask = AircraftEndorsement.Glider;
				break;
			case AircraftCategory.LighterThanAir:
				mask |= AircraftEndorsement.Airship;
				mask |= AircraftEndorsement.Balloon;
				break;
			case AircraftCategory.PoweredLift:
				mask = AircraftEndorsement.PoweredLift;
				break;
			case AircraftCategory.PoweredParachute:
				mask |= AircraftEndorsement.PoweredParachuteLand;
				mask |= AircraftEndorsement.PoweredParachuteSea;
				break;
			case AircraftCategory.WeightShiftControl:
				mask |= AircraftEndorsement.WeightShiftControlLand;
				mask |= AircraftEndorsement.WeightShiftControlSea;
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}

			return mask;
		}

		[PrimaryKey][AutoIncrement]
		public int Id {
			get; set;
		}

		[MaxLength (40)]
		public string Name {
			get; set;
		}

		public DateTime BirthDate {
			get; set;
		}

		public PilotCertification Certification {
			get; set;
		}

		public AircraftEndorsement Endorsements {
			get; set;
		}

		public InstrumentRating InstrumentRatings {
			get; set;
		}

		public bool IsCertifiedFlightInstructor {
			get; set;
		}

		public DateTime LastMedicalExam {
			get; set;
		}

		public DateTime LastFlightReview {
			get; set;
		}

		/// <summary>
		/// Event that gets emitted when the Pilot gets updated.
		/// </summary>
		public event EventHandler<EventArgs> Updated;

		internal void OnUpdated ()
		{
			var handler = Updated;

			if (handler != null)
				handler (this, EventArgs.Empty);
		}
	}
}
