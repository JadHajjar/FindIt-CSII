using FindIt.Domain;
using FindIt.Domain.Interfaces;
using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Utilities.PrefabCategoryProcessor
{
    public class RoundaboutPrefabCategoryProcessor : IPrefabCategoryProcessor
    {
        private readonly EntityManager _entityManager;

        public RoundaboutPrefabCategoryProcessor(EntityManager entityManager)
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
                        ComponentType.ReadOnly<NetObjectData>(),
                    },
                    None = new[]
                    {
                        ComponentType.ReadOnly<PillarData>(),
                    }
                },
            };
        }

        public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
        {
            var netObjectData = _entityManager.GetComponentData<NetObjectData>(entity);

            prefabIndex = new PrefabIndex(prefab)
            {
                Category = Domain.Enums.PrefabCategory.Networks,
            };

            if (netObjectData.m_CompositionFlags.m_General.HasFlag(CompositionFlags.General.Roundabout))
            {
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Networks_Intersections;
            }
            else if (_entityManager.HasComponent<PillarData>(entity))
            {
                prefabIndex.Category = Domain.Enums.PrefabCategory.Props;
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Road;
            }
            else
            {
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Networks_Stops;
            }

            return true;
        }
    }
}
