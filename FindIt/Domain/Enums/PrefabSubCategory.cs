using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindIt.Domain.Enums
{
	public enum PrefabSubCategory
	{
		//"Media/Tools/Snap Options/All.svg"
		Favorite = 0,

		Buildings = 100,
		[CategoryIcon("Media/Game/Icons/ZoneResidential.svg")]
		BuildingsResidential,
		[CategoryIcon("Media/Game/Icons/ZoneResidentialMixed.svg")]
		BuildingsMixed,
		[CategoryIcon("Media/Game/Icons/ZoneCommercial.svg")]
		BuildingsCommercial,
		[CategoryIcon("Media/Game/Icons/ZoneIndustrial.svg")]
		BuildingsIndustrial,
		[CategoryIcon("Media/Game/Icons/ZoneOffice.svg")]
		BuildingsOffice,
		[CategoryIcon("Media/Game/Icons/ZoneExtractors.svg")]
		BuildingsSpecialized,
		[CategoryIcon("Media/Game/Icons/ZoneExtractors.svg")]
		BuildingsServices,

		Networks = 200,

		Trees = 300,

		Props = 400
	}
}
