using Colossal.Entities;

using FindIt.Domain.Interfaces;

using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class MiscBuildingPrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		private readonly EntityManager _entityManager;

		public EntityQuery Query { get; set; }

		public MiscBuildingPrefabCategoryProcessor(EntityManager entityManager)
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
						ComponentType.ReadOnly<BuildingData>(),
					},
					None = new[] 
					{
						ComponentType.ReadOnly<ServiceObjectData>(),
						ComponentType.ReadOnly<SpawnableBuildingData>(),
						ComponentType.ReadOnly<SignatureBuildingData>(),
						ComponentType.ReadOnly<ServiceUpgradeBuilding>(),
						ComponentType.ReadOnly<ExtractorFacilityData>()
					}
				}
			};
		}

		public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
		{
			if (prefab is not BuildingPrefab || _entityManager.HasComponent<BuildingPropertyData>(entity))
			{
				prefabIndex = null;
				return false;
			}
			
			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Buildings,
				SubCategory = Enums.PrefabSubCategory.Buildings_Miscellaneous
			};

			return true;
		}
	}
}
