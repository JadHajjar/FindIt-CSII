using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindIt.Domain.Enums
{
	public enum PrefabSubCategory
	{
		[CategoryIcon("Media/Tools/Snap Options/All.svg")]
		Any = -1,

		[Obsolete("Use PrefabCategory", true)]
		Favorite = 0,

		[Obsolete("Use PrefabCategory", true)]
		Buildings = 100,
		[CategoryIcon("Media/Game/Icons/ZoneResidential.svg")]
		Buildings_Residential,
		[CategoryIcon("Media/Game/Icons/ZoneResidentialMixed.svg")]
		Buildings_Mixed,
		[CategoryIcon("Media/Game/Icons/ZoneCommercial.svg")]
		Buildings_Commercial,
		[CategoryIcon("Media/Game/Icons/ZoneIndustrial.svg")]
		Buildings_Industrial,
		[CategoryIcon("Media/Game/Icons/ZoneOffice.svg")]
		Buildings_Office,
		[CategoryIcon("Media/Game/Icons/ZoneExtractors.svg")]
		Buildings_Specialized,
		[CategoryIcon("Media/Game/Icons/ZoneExtractors.svg")]
		Buildings_Services,
		[CategoryIcon("Media/Game/Icons/ZoneExtractors.svg")]
		Buildings_Miscellaneous,

		[Obsolete("Use PrefabCategory", true)]
		Networks = 200,
		[CategoryIcon("coui://uil/Colored/Road.svg")]
		Networks_Roads,

		[Obsolete("Use PrefabCategory", true)]
		Trees = 300,
		[CategoryIcon("coui://uil/Colored/TreeVanilla.svg")]
		Trees_Trees,
		[CategoryIcon("Media/Game/Icons/Vegetation.svg")]
		Trees_Shrubs,

		[Obsolete("Use PrefabCategory", true)]
		Props = 400
	}
}
