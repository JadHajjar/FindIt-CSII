using FindIt.Domain;
using FindIt.Domain.Enums;
using FindIt.Domain.Interfaces;
using Game.Prefabs;

using System.Linq;

using Unity.Entities;

namespace FindIt.Utilities.PrefabCategoryProcessor
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
                        ComponentType.ReadOnly<ServiceUpgradeBuilding>(),
                        ComponentType.ReadOnly<BuildingData>(),
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
                        ComponentType.ReadOnly<ServiceUpgradeBuilding>(),
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

            var subCategory = PrefabSubCategory.ServiceBuildings_Misc;

            if (prefab.TryGet<ServiceObject>(out var serviceObject) || prefab.TryGet<ServiceUpgrade>(out var serviceUpgrade) && (serviceUpgrade.m_Buildings.FirstOrDefault()?.TryGet(out serviceObject) ?? false))
            {
                subCategory = serviceObject.m_Service.name switch
                {
                    "Roads" => PrefabSubCategory.ServiceBuildings_Roads,
                    "Electricity" => PrefabSubCategory.ServiceBuildings_Electricity,
                    "Water & Sewage" => PrefabSubCategory.ServiceBuildings_Water,
                    "Communications" => PrefabSubCategory.ServiceBuildings_Communications,
                    "Health & Deathcare" => PrefabSubCategory.ServiceBuildings_Health,
                    "Police & Administration" => PrefabSubCategory.ServiceBuildings_Police,
                    "Fire & Rescue" => PrefabSubCategory.ServiceBuildings_Fire,
                    "Education & Research" => PrefabSubCategory.ServiceBuildings_EducationResearch,
                    "Garbage Management" => PrefabSubCategory.ServiceBuildings_Garbage,
                    "Transportation" => PrefabSubCategory.ServiceBuildings_Transportation,
                    "Landscaping" => PrefabSubCategory.ServiceBuildings_Landscaping,
                    "Parks & Recreation" => PrefabSubCategory.ServiceBuildings_Parks,
                    _ => PrefabSubCategory.ServiceBuildings_Misc
                };
            }

            prefabIndex = new PrefabIndex(prefab)
            {
                Category = PrefabCategory.ServiceBuildings,
                SubCategory = subCategory
            };

            return true;
        }
    }
}
