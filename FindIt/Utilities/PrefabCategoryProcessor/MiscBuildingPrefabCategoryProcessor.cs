﻿using FindIt.Domain;
using FindIt.Domain.Interfaces;
using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Utilities.PrefabCategoryProcessor
{
    public class MiscBuildingPrefabCategoryProcessor : IPrefabCategoryProcessor
    {
        private readonly EntityManager _entityManager;

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
                    }
                },
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
                Category = Domain.Enums.PrefabCategory.Buildings,
                SubCategory = Domain.Enums.PrefabSubCategory.Buildings_Miscellaneous
            };

            if (_entityManager.HasComponent<ExtractorFacilityData>(entity))
            {
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Buildings_Specialized;
            }

            return true;
        }
    }
}
