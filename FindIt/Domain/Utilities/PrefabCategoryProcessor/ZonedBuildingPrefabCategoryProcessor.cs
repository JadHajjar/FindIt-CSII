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
			if (prefab.name.Contains("ADDAD_") || prefab is not BuildingPrefab)
			{
				prefabIndex = null;
				return false;
			}

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
			var ambienceData = _entityManager.GetComponentData<GroupAmbienceData>(zonePrefab);

			switch (zoneData.m_AreaType)
			{
				case Game.Zones.AreaType.Residential:
					prefabIndex.SubCategory = ambienceData.m_AmbienceType == Game.Simulation.GroupAmbienceType.ResidentialMixed ? Enums.PrefabSubCategory.Buildings_Mixed : Enums.PrefabSubCategory.Buildings_Residential;
					break;
				case Game.Zones.AreaType.Commercial:
					prefabIndex.SubCategory = Enums.PrefabSubCategory.Buildings_Commercial;
					break;
				case Game.Zones.AreaType.Industrial:
					if (zoneData.m_ZoneFlags == ZoneFlags.Office)
					{
						prefabIndex.SubCategory = Enums.PrefabSubCategory.Buildings_Office;
					}
					else if (ambienceData.m_AmbienceType == Game.Simulation.GroupAmbienceType.Industrial)
					{
						prefabIndex.SubCategory = Enums.PrefabSubCategory.Buildings_Industrial;
					}
					else
					{
						prefabIndex.SubCategory = Enums.PrefabSubCategory.Buildings_Specialized;
					}

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
