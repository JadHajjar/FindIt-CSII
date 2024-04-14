using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindIt.Domain.Enums
{
	public enum PrefabCategory
	{
		[CategoryIcon("coui://uil/Standard/StarAll.svg")]
		Any = -1,
		[CategoryIcon("coui://uil/Colored/StarFilledSmallIso.svg")]
		Favorite = 0,
		[CategoryIcon("coui://uil/Colored/BuildingZoneSignature.svg")]
		Buildings = 100,
		[CategoryIcon("coui://uil/Colored/Road.svg")]
		Networks = 200,
		[CategoryIcon("coui://uil/Colored/Nature.svg")]
		Trees = 300,
		[CategoryIcon("coui://uil/Colored/BenchAndLampProps.svg")]
		Props = 400,
		[CategoryIcon("coui://uil/Colored/GenericVehicleIsometric.svg")]
		Vehicles = 500
	}
}
