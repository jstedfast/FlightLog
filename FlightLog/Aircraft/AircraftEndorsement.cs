//
// AircraftEndorsement.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
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

namespace FlightLog {
	[Flags]
	public enum AircraftEndorsement {
		None                           = 0,

		#region Airplane
		[HumanReadableName ("Single-Engine Land")]
		SingleEngineLand               = 1 << 0,
		[HumanReadableName ("Multi-Engine Land")]
		MultiEngineLand                = 1 << 1,
		[HumanReadableName ("Single-Engine Sea")]
		SingleEngineSea                = 1 << 2,
		[HumanReadableName ("Multi-Engine Sea")]
		MultiEngineSea                 = 1 << 3,

		[HumanReadableName ("Complex")]
		Complex                        = 1 << 4,
		[HumanReadableName ("High-Performance")]
		HighPerformance                = 1 << 5,
		[HumanReadableName ("Taildragger")]
		TailDragger                    = 1 << 6,
		#endregion

		#region Rotorcraft
		[HumanReadableName ("Helicoptor")]
		Helicoptor                     = 1 << 7,
		[HumanReadableName ("Gyroplane")]
		Gryoplane                      = 1 << 8,
		#endregion

		#region Glider
		Glider                         = 1 << 9,
		#endregion

		#region LighterThanAir
		[HumanReadableName ("Airship")]
		Airship                        = 1 << 10,
		[HumanReadableName ("Balloon")]
		Balloon                        = 1 << 11,
		#endregion

		#region PoweredLift
		[HumanReadableName ("Powered-Lift")]
		PoweredLift                    = 1 << 12,
		#endregion

		#region PoweredParachute
		[HumanReadableName ("Powered-Parachute Land")]
		PoweredParachuteLand           = 1 << 13,
		[HumanReadableName ("Powered-Parachute Sea")]
		PoweredParachuteSea            = 1 << 14,
		#endregion

		#region WeightShiftControl
		[HumanReadableName ("Weight-Shift-Control Land")]
		WeightShiftControlLand         = 1 << 15,
		[HumanReadableName ("Weight-Shift-Control Sea")]
		WeightShiftControlSea          = 1 << 16,
		#endregion
	}
}
