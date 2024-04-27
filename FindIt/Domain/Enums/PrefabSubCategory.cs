using System;

namespace FindIt.Domain.Enums
{
	public enum PrefabSubCategory
	{
		[CategoryIcon("coui://uil/Standard/StarAll.svg")]
		Any = PrefabCategory.Any,

		[CategoryIcon("coui://uil/Colored/StarFilledSmallIso.svg")]
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
		[CategoryIcon("coui://uil/Colored/ServiceBuilding.svg")]
		Buildings_Services,
		[CategoryIcon("coui://uil/Colored/HouseAlternative.svg")]
		Buildings_Miscellaneous,

		[Obsolete("Use PrefabCategory", true)]
		Networks = PrefabCategory.Networks,
		[CategoryIcon("coui://uil/Colored/Road.svg")]
		Networks_Roads,
		[CategoryIcon("coui://uil/Colored/Highway.svg")]
		Networks_Highways,
		[CategoryIcon("coui://uil/Colored/RailTrack.svg")]
		Networks_Tracks,
		[CategoryIcon("coui://uil/Colored/Bridge.svg")]
		Networks_Bridges,
		[CategoryIcon("coui://uil/Colored/Intersection.svg")]
		Networks_Intersections,
		[CategoryIcon("coui://uil/Colored/PedestrianPath.svg")]
		Networks_Paths,
		[CategoryIcon("coui://uil/Colored/Lanes.svg")]
		Networks_Lanes,
		[CategoryIcon("coui://uil/Colored/BusShelter.svg")]
		Networks_Stops,
		[CategoryIcon("coui://uil/Colored/Pillar.svg")]
		Networks_Pillars,

		[Obsolete("Use PrefabCategory", true)]
		Trees = PrefabCategory.Trees,
		[CategoryIcon("coui://uil/Colored/TreeVanilla.svg")]
		Trees_Trees,
		[CategoryIcon("coui://uil/Colored/Bush.svg")]
		Trees_Shrubs,
		[CategoryIcon("Media/Game/Resources/Stone.svg")]
		Trees_Rocks,
		[CategoryIcon("coui://uil/Colored/FlowerPot.svg")]
		Trees_Props,
		[CategoryIcon("coui://uil/Colored/Chicken.svg")]
		Trees_Spawners,

		[Obsolete("Use PrefabCategory", true)]
		Props = PrefabCategory.Props,
		[CategoryIcon("coui://uil/Colored/FurnitureIso.svg")]
		Props_Misc,
		[CategoryIcon("Media/Game/Icons/LotTool.svg")]
		Props_Surfaces,
		[CategoryIcon("coui://uil/Colored/PropResidential.svg")]
		Props_Residential,
		[CategoryIcon("coui://uil/Colored/PropCommercial.svg")]
		Props_Commercial,
		[CategoryIcon("coui://uil/Colored/PropIndustrial.svg")]
		Props_Industrial,
		[CategoryIcon("coui://uil/Colored/HealthcareServiceProps.svg")]
		Props_Service,
		[CategoryIcon("coui://uil/Colored/FenceIsometric.svg")]
		Props_Fences,
		[CategoryIcon("coui://uil/Colored/BenchAndParkProps.svg")]
		Props_Park,
		[CategoryIcon("coui://uil/Colored/Decals.svg")]
		Props_Decals,
		[CategoryIcon("coui://uil/Colored/LampProps.svg")]
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
		[CategoryIcon("coui://uil/Colored/ServiceVehicles.svg")]
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
