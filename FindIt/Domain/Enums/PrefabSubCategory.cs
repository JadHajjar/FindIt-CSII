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
		[CategoryIcon("coui://ui-mods/PrefabThumbnails/ServiceBuilding.svg")]
		Buildings_Services,
		[CategoryIcon("coui://uil/Colored/HouseAlternative.svg")]
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
		[CategoryIcon("Media/Game/Icons/Pathways.svg")]
		Networks_Paths,
		[CategoryIcon("coui://ui-mods/PrefabThumbnails/BusStop.png")]
		Networks_Stops,
		[CategoryIcon("coui://ui-mods/PrefabThumbnails/BridgePillar.png")]
		Networks_Pillars,

		[Obsolete("Use PrefabCategory", true)]
		Trees = PrefabCategory.Trees,
		[CategoryIcon("coui://uil/Colored/TreeVanilla.svg")]
		Trees_Trees,
		[CategoryIcon("coui://uil/Colored/Bush.svg")]
		Trees_Shrubs,
		[CategoryIcon("Media/Game/Resources/Stone.svg")]
		Trees_Rocks,
		[CategoryIcon("coui://ui-mods/PrefabThumbnails/FlowerPot.png")]
		Trees_Props,
		[CategoryIcon("coui://uil/Colored/Chicken.svg")]
		Trees_Spawners,

		[Obsolete("Use PrefabCategory", true)]
		Props = PrefabCategory.Props,
		[CategoryIcon("coui://uil/Colored/Furniture.svg")]
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
		[CategoryIcon("coui://ui-mods/PrefabThumbnails/RoadArrows.png")]
		Props_Decals,
		[CategoryIcon("coui://uil/Colored/LampProp.svg")]
		Props_Lights,
		[CategoryIcon("coui://uil/Colored/Billboard.svg")]
		Props_Branding,
		[CategoryIcon("Media/game/Icons/Lighting.svg")]
		Props_Road,

		[Obsolete("Use PrefabCategory", true)]
		Vehicles = PrefabCategory.Vehicles,
		[CategoryIcon("coui://uil/Colored/GenericVehicle.svg")]
		Vehicles_Residential,
		[CategoryIcon("coui://uil/Colored/Motorbike.svg")]
		Vehicles_Bikes,
		[CategoryIcon("coui://uil/Colored/DeliveryVan.svg")]
		Vehicles_Industrial,
		[CategoryIcon("coui://ui-mods/PrefabThumbnails/ServiceVehicles.svg")]
		Vehicles_Services,
		[CategoryIcon("Media/Game/Icons/Bus.svg")]
		Vehicles_Bus,
		[CategoryIcon("Media/Game/Icons/Train.svg")]
		Vehicles_Train,
		[CategoryIcon("Media/Game/Icons/Ship.svg")]
		Vehicles_Ship,
		[CategoryIcon("Media/Game/Icons/Airplane.svg")]
		Vehicles_Plane,
		[CategoryIcon("coui://uil/Colored/GenericVehicles.svg")]
		Vehicles_Misc,
	}
}
