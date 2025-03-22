using FindIt.Domain;
using FindIt.Domain.Interfaces;
using FindIt.Utilities;
using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Utilities.PrefabCategoryProcessor
{
    public class RoadPropsPrefabCategoryProcessor : IPrefabCategoryProcessor
    {
        private readonly EntityManager _entityManager;

        public RoadPropsPrefabCategoryProcessor(EntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        public EntityQueryDesc[] GetEntityQuery()
        {
            return new[]
            {
                new EntityQueryDesc
                {
                    Any = new[]
                    {
                        ComponentType.ReadOnly<TrafficSignData>(),
                        ComponentType.ReadOnly<LaneDirectionData>(),
                        ComponentType.ReadOnly<TrafficLightData>(),
                    },
                }
            };
        }

        public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
        {
            prefabIndex = new PrefabIndex(prefab)
            {
                Category = Domain.Enums.PrefabCategory.Props,
            };

            if (_entityManager.IsDecal(entity))
            {
                return false;
            }
            else
            {
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Road;
            }

            return true;
        }
    }
}
