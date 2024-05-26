using Colossal.Entities;

using FindIt.Domain.Enums;
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
			if (prefab is not BuildingPrefab)
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
				Category = PrefabCategory.Buildings,
				BuildingLevel = level
			};

			prefabIndex.CategoryThumbnail = prefabIndex.FallbackThumbnail = _imageSystem.GetIconOrGroupIcon(zonePrefab);

			if (_prefabSystem.TryGetPrefab<ZonePrefab>(zonePrefab, out var _zonePrefab))
			{
				prefabIndex.ZoneType = GetZoneType(_zonePrefab);
				prefabIndex.Theme = _zonePrefab.GetComponent<ThemeObject>()?.m_Theme;
			}

			var zoneData = _entityManager.GetComponentData<ZoneData>(zonePrefab);
			var ambienceData = _entityManager.GetComponentData<GroupAmbienceData>(zonePrefab);

			switch (zoneData.m_AreaType)
			{
				case Game.Zones.AreaType.Residential:
					prefabIndex.SubCategory = ambienceData.m_AmbienceType == Game.Simulation.GroupAmbienceType.ResidentialMixed ? PrefabSubCategory.Buildings_Mixed : PrefabSubCategory.Buildings_Residential;
					break;
				case Game.Zones.AreaType.Commercial:
					prefabIndex.SubCategory = PrefabSubCategory.Buildings_Commercial;
					break;
				case Game.Zones.AreaType.Industrial:
					if (zoneData.m_ZoneFlags == ZoneFlags.Office)
					{
						prefabIndex.SubCategory = PrefabSubCategory.Buildings_Office;
					}
					else if (ambienceData.m_AmbienceType == Game.Simulation.GroupAmbienceType.Industrial)
					{
						prefabIndex.SubCategory = PrefabSubCategory.Buildings_Industrial;
					}
					else
					{
						prefabIndex.SubCategory = PrefabSubCategory.Buildings_Specialized;
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

		private static ZoneTypeFilter GetZoneType(ZonePrefab zonePrefab)
		{
			if (zonePrefab.name.Contains(" Row"))
			{
				return ZoneTypeFilter.Row;
			}
			else if (zonePrefab.name.Contains(" Medium") || zonePrefab.name.Contains(" Mixed"))
			{
				return ZoneTypeFilter.Medium;
			}
			else if (zonePrefab.name.Contains(" High") || zonePrefab.name.Contains(" LowRent"))
			{
				return ZoneTypeFilter.High;
			}
			else if (zonePrefab.name.Contains(" Low"))
			{
				return ZoneTypeFilter.Low;
			}

			return ZoneTypeFilter.Any;
		}
	}
}
