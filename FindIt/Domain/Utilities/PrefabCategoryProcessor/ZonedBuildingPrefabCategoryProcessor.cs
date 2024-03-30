using Colossal.Entities;

using FindIt.Domain.Interfaces;

using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class ZonedBuildingPrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		private readonly EntityManager _entityManager;

		public EntityQuery Query { get; set; }

		public ZonedBuildingPrefabCategoryProcessor(EntityManager entityManager)
		{
			_entityManager = entityManager;
		}

		public EntityQueryDesc[] GetEntityQuery()
		{
			return new[]
			{
				new EntityQueryDesc
				{
					All = new ComponentType[1] { ComponentType.ReadOnly<BuildingData>() },
					Any = new ComponentType[2]
					{
						ComponentType.ReadOnly<SpawnableBuildingData>(),
						ComponentType.ReadOnly<PlaceholderBuildingData>()
					}
				}
			};
		}

		public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
		{
			var zonePrefab = GetZonePrefab(entity);

			if (zonePrefab == Entity.Null)
			{
				prefabIndex = null;
				return false;
			}

			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Buildings,
			};

			var zoneData = _entityManager.GetComponentData<ZoneData>(zonePrefab);

			switch (zoneData.m_AreaType)
			{
				case Game.Zones.AreaType.Residential:
					prefabIndex.SubCategory = Enums.PrefabSubCategory.Buildings_Residential;
					break;
				case Game.Zones.AreaType.Commercial:
					prefabIndex.SubCategory = Enums.PrefabSubCategory.Buildings_Commercial;
					break;
				case Game.Zones.AreaType.Industrial:
					prefabIndex.SubCategory = zoneData.m_ZoneFlags == ZoneFlags.Office ? Enums.PrefabSubCategory.Buildings_Office : Enums.PrefabSubCategory.Buildings_Industrial;
					break;
				default:
					return false;
			}

			return true;
		}

		private Entity GetZonePrefab(Entity entity)
		{
			if (_entityManager.TryGetComponent<SpawnableBuildingData>(entity, out var component))
			{
				return component.m_ZonePrefab;
			}

			if (_entityManager.TryGetComponent<PlaceholderBuildingData>(entity, out var component2))
			{
				return component2.m_ZonePrefab;
			}

			return Entity.Null;
		}
	}
}
