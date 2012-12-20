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
		public Pilot ()
		{
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

		public bool IsCertifiedFlightInstructor {
			get; set;
		}

		public bool IsInstrumentRated {
			get; set;
		}

		public DateTime LastMedical {
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
