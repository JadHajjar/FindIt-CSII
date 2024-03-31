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
		Any = PrefabCategory.Any,

		[CategoryIcon("coui://uil/Colored/StarFilledSmall.svg")]
		Favorite = PrefabCategory.Favorite,

		[Obsolete("Use PrefabCategory", true)]
		Buildings = PrefabCategory.Buildings,
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
		Networks = PrefabCategory.Networks,
		[CategoryIcon("coui://uil/Colored/Road.svg")]
		Networks_Roads,
		[CategoryIcon("Media/Game/Icons/Highways.svg")]
		Networks_Highways,
		[CategoryIcon("Media/Game/Icons/TwoWayTrainTrack.svg")]
		Networks_Tracks,
		[CategoryIcon("Media/Game/Icons/CableStayed.svg")]
		Networks_Bridges,
		[CategoryIcon("Media/Game/Icons/Intersections.svg")]
		Networks_Intersections,

		[Obsolete("Use PrefabCategory", true)]
		Trees = PrefabCategory.Trees,
		[CategoryIcon("coui://uil/Colored/TreeVanilla.svg")]
		Trees_Trees,
		[CategoryIcon("Media/Game/Icons/Vegetation.svg")]
		Trees_Shrubs,
		[CategoryIcon("Media/Game/Resources/Stone.svg")]
		Trees_Props,

		[Obsolete("Use PrefabCategory", true)]
		Props = PrefabCategory.Props,
		[CategoryIcon("coui://uil/Colored/BenchAndLampProps.svg")]
		Props_Misc,
		[CategoryIcon("Media/Game/Icons/LotTool.svg")]
		Props_Surfaces,
		[CategoryIcon("Media/Game/Icons/ZoneResidential.svg")]
		Props_Residential,
		[CategoryIcon("Media/Game/Icons/ZoneCommercial.svg")]
		Props_Commercial,
		[CategoryIcon("Media/Game/Icons/ZoneIndustrial.svg")]
		Props_Industrial,
		[CategoryIcon("Media/Game/Icons/Police.svg")]
		Props_Service,
		[CategoryIcon("coui://uil/Colored/Fence.svg")]
		Props_Fences,
		[CategoryIcon("Media/Game/Icons/ParksAndRecreation.svg")]
		Props_Park,
		[CategoryIcon("coui://uil/Colored/LampProp.svg")]
		Props_Lights,
		[CategoryIcon("coui://uil/Colored/Billboard.svg")]
		Props_Branding,
		[CategoryIcon("Media/game/Icons/Lighting.svg")]
		Props_Road,

		[Obsolete("Use PrefabCategory", true)]
		Vehicles = PrefabCategory.Vehicles,
		[CategoryIcon("Media/Game/Icons/ZoneResidential.svg")]
		Vehicles_Residential,
	}
}
