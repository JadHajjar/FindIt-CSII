using Colossal.Entities;

using FindIt.Domain.Interfaces;

using Game.Prefabs;
using Game.UI;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class ZonedBuildingPrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		private readonly EntityManager _entityManager;
		private readonly ImageSystem _imageSystem;
		private readonly PrefabSystem _prefabSystem;

		public ZonedBuildingPrefabCategoryProcessor(EntityManager entityManager, ImageSystem imageSystem, PrefabSystem prefabSystem)
		{
			_entityManager = entityManager;
			_imageSystem = imageSystem;
			_prefabSystem = prefabSystem;
		}

		public EntityQueryDesc[] GetEntityQuery()
		{
			return new[]
			{
				new EntityQueryDesc
				{
					All = new[]
					{
						ComponentType.ReadOnly<BuildingData>()
					},
					Any = new[]
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

			var zonePrefab = GetZonePrefab(entity, out var level);

			if (zonePrefab == Entity.Null)
			{
				prefabIndex = null;
				return false;
			}

			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Buildings,
				BuildingLevel = level
			};

			prefabIndex.CategoryThumbnail = prefabIndex.FallbackThumbnail = _imageSystem.GetIconOrGroupIcon(zonePrefab);

			if (_prefabSystem.TryGetPrefab<ZonePrefab>(zonePrefab, out var _zonePrefab))
			{
				prefabIndex.ZoneType = FindItUtil.GetZoneType(_zonePrefab);
			}

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

		private Entity GetZonePrefab(Entity entity, out int level)
		{
			if (_entityManager.TryGetComponent<SpawnableBuildingData>(entity, out var component))
			{
				level = component.m_Level;

				return component.m_ZonePrefab;
			}

			level = 0;

			if (_entityManager.TryGetComponent<PlaceholderBuildingData>(entity, out var component2))
			{
				return component2.m_ZonePrefab;
			}

			return Entity.Null;
		}
	}
}
