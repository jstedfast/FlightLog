// 
// Aircraft.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2011 Jeffrey Stedfast
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

using SQLite;

namespace FlightLog {
#if false
	// List from http://www.highwayhome.com/corporate/aircraftmanufacturers.html
	public enum AircraftManufacturer {
		[HumanReadableName ("AAR Corporation")]
		AARCorporation,
		[HumanReadableName ("Advanced Aero & Structure")]
		AdvancedAeroAndStructure,
		[HumanReadableName ("Aerospatiale Matra")]
		AerospatialeMatra,
		Aerotech,
		[HumanReadableName ("Aero Technologies")]
		AeroTechnologies,
		[HumanReadableName ("Airbus Industrie")]
		AirbusIndustrie,
		[HumanReadableName ("Atlantic Aero Marine")]
		AtlanticAeroMarine,
		[HumanReadableName ("ATR Aircraft")]
		ATRAircraft,
		[HumanReadableName ("Aurora Flight Sciences")]
		AuroraFlightSciences,
		[HumanReadableName ("Autek Systems")]
		AutekSystems,
		[HumanReadableName ("Avtec Systems")]
		AvtecSystems,
		[HumanReadableName ("Ayres Corporation")]
		AyresCorporation,
		[HumanReadableName ("BAE Systems")]
		BAESystems,
		[HumanReadableName ("BBA Group")]
		BBAGroup,
		[HumanReadableName ("Beta Air")]
		BetaAir,
		Boeing,
		[HumanReadableName ("Bombardier Aerospace")]
		BombardierAerospace,
		[HumanReadableName ("Century Flight System")]
		CenturyFlightSystem,
		[HumanReadableName ("Cessna Aircraft Company")]
		CessnaAircraftCompany,
		[HumanReadableName ("Cub Crafters")]
		CubCrafters,
		[HumanReadableName ("Diamond Aircraft Industries")]
		DiamondAircraftIndustries,
		[HumanReadableName ("Dragon Aero")]
		DragonAero,
		Embraer,
		[HumanReadableName ("Fairchild Aerospace")]
		FairchildAerospace,
		[HumanReadableName ("Fairchild Controls")]
		FairchildControls,
		[HumanReadableName ("Galaxy Aerospace")]
		GalaxyAerospace,
		[HumanReadableName ("GE Aircraft Engines")]
		GEAircraftEngines,
		GenCorp,
		[HumanReadableName ("General Dynamics")]
		GeneralDynamics,
		[HumanReadableName ("Golden Circle Air")]
		GoldenCircleAir,
		Gulfstream,
		[HumanReadableName ("Hardman Air Specialties")]
		HardmanAirSpecialties,
		Honeywell,
		[HumanReadableName ("Hughes Electronics")]
		HughesElectronics,
		[HumanReadableName ("Inland Aero")]
		InlandAero,
		[HumanReadableName ("JetProp DLX")]
		JetPropDLX,
		[HumanReadableName ("Lockheed Martin")]
		LockheedMartin,
		[HumanReadableName ("Lockheed Martin Aero")]
		LockheedMartinAero,
		[HumanReadableName ("LoPresti Fury")]
		LoPrestiFury,
		[HumanReadableName ("Luscombe Aircraft")]
		LuscombeAircraft,
		[HumanReadableName ("Mid-Continent Aircraft")]
		MidContinentAircraft,
		[HumanReadableName ("Middle River Aircraft System")]
		MiddleRiverAircraftSystem,
		[HumanReadableName ("Moeller Aircraft")]
		MoellerAircraft,
		[HumanReadableName ("Mooney Aircraft")]
		MooneyAircraft,
		[HumanReadableName ("Morrow Aircraft")]
		MorrowAircraft,
		[HumanReadableName ("New Piper Aircraft")]
		NewPiperAircraft,
		[HumanReadableName ("NH Aviation")]
		NHAviation,
		[HumanReadableName ("Nizhny Novgotod")]
		NizhnyNovgorod,
		[HumanReadableName ("Pilatus Aircraft")]
		PilatusAircraft,
		[HumanReadableName ("Piper Aircraft")]
		PiperAircraft,
		[HumanReadableName ("Raytheon Aircraft Company")]
		RaytheonAircraftCompany,
		[HumanReadableName ("Rocket Engineering")]
		RocketEngineering,
		[HumanReadableName ("Safire Aircraft Company")]
		SafireAircraftCompany,
		[HumanReadableName ("Samsung Aerospace")]
		SamsungAerospace,
		[HumanReadableName ("Scweizer Aircraft")]
		SchweizerAircraft,
		[HumanReadableName ("Sino Swearingen Aircraft")]
		SinoSwearingenAircraft,
		[HumanReadableName ("Socata Aircraft")]
		SocataAircraft,
		[HumanReadableName ("Soloy Corporation")]
		SoloyCorporation,
		Superskyrocket,
		[HumanReadableName ("Taylorcraft Aerospace")]
		TaylorcraftAerospace,
		Textron,
		[HumanReadableName ("Visionaire Corp")]
		VisionaireCorp,
		[HumanReadableName ("Wilson Aircraft")]
		WilsonAircraft
	}
#endif
	
	public enum AircraftCategory {
		Airplane           = 0,
		Rotorcraft         = 10,
		Glider             = 20,
		[HumanReadableName ("Lighter-Than-Air")]
		LighterThanAir     = 30,
		[HumanReadableName ("Powered-Lift")]
		PoweredLift        = 40,
		[HumanReadableName ("Powered-Parachute")]
		PoweredParachute   = 50,
		[HumanReadableName ("Weight-Shift-Control")]
		WeightShiftControl = 60
	}
	
	public enum AircraftClassification {
		#region Airplane
		[HumanReadableName ("Single-Engine Land")]
		SingleEngineLand       = (int) AircraftCategory.Airplane,
		[HumanReadableName ("Multi-Engine Land")]
		MultiEngineLand,
		[HumanReadableName ("Single-Engine Sea")]
		SingleEngineSea,
		[HumanReadableName ("Multi-Engine Sea")]
		MultiEngineSea,
		#endregion
		
		#region Rotorcraft
		Helicoptor             = (int) AircraftCategory.Rotorcraft,
		Gryoplane,
		#endregion
		
		#region Glider
		Glider                 = (int) AircraftCategory.Glider,
		#endregion
		
		#region LighterThanAir
		Airship                = (int) AircraftCategory.LighterThanAir,
		Balloon,
		#endregion
		
		#region PoweredLift
		[HumanReadableName ("Powered-Lift")]
		PoweredLift            = (int) AircraftCategory.PoweredLift,
		#endregion
		
		#region PoweredParachute
		[HumanReadableName ("Powered-Parachute Land")]
		PoweredParachuteLand   = (int) AircraftCategory.PoweredParachute,
		[HumanReadableName ("Powered-Parachute Sea")]
		PoweredParachuteSea,
		#endregion
		
		#region WeightShiftControl
		[HumanReadableName ("Weight-Shift-Control Land")]
		WeightShiftControlLand = (int) AircraftCategory.WeightShiftControl,
		[HumanReadableName ("Weight-Shift-Control Sea")]
		WeightShiftControlSea,
		#endregion
	}
	
	public class Aircraft
	{
		public static int CategoryStep = 10;
		
		public Aircraft ()
		{
		}
		
		/// <summary>
		/// Gets or sets the aircraft's tail number.
		/// </summary>
		/// <value>
		/// The aircraft's tail number.
		/// </value>
		[PrimaryKey][Indexed][MaxLength (9)]
		public string TailNumber {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the make of the aircraft.
		/// </summary>
		/// <value>
		/// The make of the aircraft.
		/// </value>
		[Indexed][MaxLength (30)]
		public string Make {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the model of the aircraft.
		/// </summary>
		/// <value>
		/// The model of the aircraft.
		/// </value>
		[Indexed][MaxLength (20)] // 15 might be enough?
		public string Model {
			get; set;
		}
		
		/// <summary>
		/// Gets the category from the specified classification.
		/// </summary>
		/// <returns>
		/// The appropriate category for the specified classification.
		/// </returns>
		/// <param name='classification'>
		/// An AircraftClassification.
		/// </param>
		public static AircraftCategory GetCategoryFromClass (AircraftClassification classification)
		{
			return (AircraftCategory) ((((int) classification) / CategoryStep) * CategoryStep);
		}
		
		/// <summary>
		/// Gets the aircraft's category based on the classification.
		/// </summary>
		/// <value>
		/// The category based on the classification.
		/// </value>
		public AircraftCategory Category {
			get {
				return GetCategoryFromClass (Classification);
			}
		}
		
		/// <summary>
		/// Gets or sets the aircraft's classification.
		/// </summary>
		/// <value>
		/// The classification.
		/// </value>
		public AircraftClassification Classification {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this aircraft is complex.
		/// Typically an aircraft is considered complex if it has all three
		/// of the following features: (1) retractable landing gear, 
		/// (2) constant-speed prop, and (3) flaps.
		/// </summary>
		/// <value>
		/// <c>true</c> if this aircraft is complex; otherwise, <c>false</c>.
		/// </value>
		public bool IsComplex {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this aircraft is high-performance.
		/// An aircraft is considered high-performance if its engine has more than
		/// 200 horsepower.
		/// </summary>
		/// <value>
		/// <c>true</c> if this aircraft is high performance; otherwise, <c>false</c>.
		/// </value>
		public bool IsHighPerformance {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this aircraft is a tail-dragger.
		/// </summary>
		/// <value>
		/// <c>true</c> if this aircraft is a tail-dragger; otherwise, <c>false</c>.
		/// </value>
		public bool IsTailDragger {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this aircraft is a simulator.
		/// </summary>
		/// <value>
		/// <c>true</c> if this aircraft is a simulator; otherwise, <c>false</c>.
		/// </value>
		public bool IsSimulator {
			get; set;
		}
		
		/// <summary>
		/// Gets or sets the notes on the aircraft.
		/// </summary>
		/// <value>
		/// The notes on the aircraft.
		/// </value>
		public string Notes {
			get; set;
		}
		
		/// <summary>
		/// Event that gets emitted when the Aircraft gets updated.
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
