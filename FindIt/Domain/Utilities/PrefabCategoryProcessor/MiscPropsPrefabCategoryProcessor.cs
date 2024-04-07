using FindIt.Domain.Interfaces;

using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class MiscPropsPrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		private readonly EntityManager _entityManager;

		public MiscPropsPrefabCategoryProcessor(EntityManager entityManager)
		{
			_entityManager = entityManager;
		}

		public EntityQueryDesc[] GetEntityQuery()
		{
			return new[]
			{
				new EntityQueryDesc
				{
					All = new[]
					{
						ComponentType.ReadOnly<StaticObjectData>(),
					},
					None = new[]
					{
						ComponentType.ReadOnly<PlaceableObjectData>(),
						ComponentType.ReadOnly<BuildingData>(),
						ComponentType.ReadOnly<BrandObjectData>(),
						ComponentType.ReadOnly<NetObjectData>(),
						ComponentType.ReadOnly<BuildingExtensionData>(),
						ComponentType.ReadOnly<PillarData>(),
						ComponentType.ReadOnly<PlantData>(),
						ComponentType.ReadOnly<TrafficSignData>(),
						ComponentType.ReadOnly<LaneDirectionData>(),
						ComponentType.ReadOnly<TrafficLightData>(),
						ComponentType.ReadOnly<UtilityObjectData>(),
						ComponentType.ReadOnly<QuantityObjectData>(),
					},
				},
			};
		}

		public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
		{
			if (prefab is not StaticObjectPrefab || _entityManager.IsBrandEntity(entity) || prefab.name.StartsWith("Billboard") || prefab.name.StartsWith("Sign") || prefab.name.StartsWith("GasStation") || prefab.name.StartsWith("Poster"))
			{
				prefabIndex = null;
				return false;
			}

			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Props,
				SubCategory = Enums.PrefabSubCategory.Props_Misc
			};

			if (prefab.name.Contains("FireHydrant"))
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Road;
			}

			if (prefab.name.StartsWith("Clothes"))
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Residential;
			}

			if (prefab.name.StartsWith("SportPark"))
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Park;
			}

			if (prefab.name == "TunnelLight01")
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Lights;
			}

			switch (prefab.name)
			{
				case "TunnelLight01":
					prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Lights;
					break;

				case "CarTrailer01_Props01":
				case "CarTrailer01_Props02":
				case "CarTrailer01_Props03":
				case "OreMiningTruck01_OrePile01":
				case "OreMiningTruck01_OrePile02":
				case "OreMiningTruck01_OrePile03":
				case "TractorTrailer01_CropPile01":
				case "TractorTrailer01_CropPile02":
				case "TractorTrailer01_CropPile03":
				case "TractorTrailer03_CropPile01":
				case "TractorTrailer03_CropPile02":
				case "TractorTrailer03_CropPile03":
					prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Industrial;
					break;
			}

			return true;
		}
	}
}
