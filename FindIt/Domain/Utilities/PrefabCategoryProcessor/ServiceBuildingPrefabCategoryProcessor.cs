using Colossal.Entities;

using FindIt.Domain.Interfaces;

using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class ServiceBuildingPrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		private readonly EntityManager _entityManager;

		public ServiceBuildingPrefabCategoryProcessor(EntityManager entityManager)
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
						ComponentType.ReadOnly<ServiceObjectData>()
					},
					None = new[] 
					{
						ComponentType.ReadOnly<TrafficSpawnerData>(),
						ComponentType.ReadOnly<SpawnableBuildingData>(),
						ComponentType.ReadOnly<PlaceholderBuildingData>()
					}
				},
				new EntityQueryDesc
				{
					All = new[]
					{
						ComponentType.ReadOnly<ServiceUpgradeBuilding>()
					},
					Any = new[]
					{
						ComponentType.ReadOnly<BuildingData>(),
						ComponentType.ReadOnly<BuildingExtensionData>()
					},
					None = new[]
					{
						ComponentType.ReadOnly<TrafficSpawnerData>(),
						ComponentType.ReadOnly<SpawnableBuildingData>(),
						ComponentType.ReadOnly<PlaceholderBuildingData>() 
					}
				}
			};
		}

		public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
		{
			if (prefab is not BuildingPrefab)
			{
				prefabIndex = null;
				return false;
			}

			//var isExtension = _entityManager.HasComponent<BuildingExtensionData>(entity);

			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Buildings,
				SubCategory = Enums.PrefabSubCategory.Buildings_Services
			};

			return true;
		}
	}
}
