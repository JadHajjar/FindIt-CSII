using Colossal.Entities;
using FindIt.Domain;
using FindIt.Domain.Interfaces;
using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Utilities.PrefabCategoryProcessor
{
    public class LanesPrefabCategoryProcessor : IPrefabCategoryProcessor
    {
        private readonly EntityManager _entityManager;

        public LanesPrefabCategoryProcessor(EntityManager entityManager)
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
                        ComponentType.ReadOnly<NetLaneData>(),
                        ComponentType.ReadOnly<SubMesh>(),
						//ComponentType.ReadOnly<NetData>(),
					},
                    None = new[]
                    {
                        ComponentType.ReadOnly<PathwayData>(),
                        ComponentType.ReadOnly<RoadData>(),
                        ComponentType.ReadOnly<BridgeData>(),
                        ComponentType.ReadOnly<TrackData>(),
                        ComponentType.ReadOnly<TrackLaneData>(),
                        ComponentType.ReadOnly<PlaceholderObjectElement>(),
                    }
                },
            };
        }

        public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
        {
            if (!Mod.IsExtraDetailingEnabled || prefab is not NetLaneGeometryPrefab)
            {
                prefabIndex = null;
                return false;
            }

            if (_entityManager.TryGetComponent<UtilityLaneData>(entity, out var utilityLaneData) && utilityLaneData.m_UtilityTypes < Game.Net.UtilityTypes.LowVoltageLine)
            {
                prefabIndex = null;
                return false;
            }

            prefabIndex = new PrefabIndex(prefab)
            {
                Category = Domain.Enums.PrefabCategory.Networks,
                SubCategory = Domain.Enums.PrefabSubCategory.Networks_Lanes
            };

            return true;
        }
    }
}
