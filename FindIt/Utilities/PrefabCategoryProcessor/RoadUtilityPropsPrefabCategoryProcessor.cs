using FindIt.Domain;
using FindIt.Domain.Interfaces;
using FindIt.Utilities;
using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Utilities.PrefabCategoryProcessor
{
    public class RoadUtilityPropsPrefabCategoryProcessor : IPrefabCategoryProcessor
    {
        private readonly EntityManager _entityManager;

        public RoadUtilityPropsPrefabCategoryProcessor(EntityManager entityManager)
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
                        ComponentType.ReadOnly<UtilityObjectData>(),
                        ComponentType.ReadOnly<SubLane>(),
                    },
                    None = new[]
                    {
                        ComponentType.ReadOnly<PillarData>(),
                        ComponentType.ReadOnly<PlaceholderObjectElement>(),
                    },
                },
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
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Decals;
            }
            else
            {
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Road;
            }

            return true;
        }
    }
}
