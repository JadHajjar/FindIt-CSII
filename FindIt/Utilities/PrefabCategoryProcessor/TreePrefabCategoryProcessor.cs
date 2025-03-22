using FindIt.Domain;
using FindIt.Domain.Interfaces;
using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Utilities.PrefabCategoryProcessor
{
    public class TreePrefabCategoryProcessor : IPrefabCategoryProcessor
    {
        private readonly EntityManager _entityManager;

        public TreePrefabCategoryProcessor(EntityManager entityManager)
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
                        ComponentType.ReadOnly<TreeData>(),
                        ComponentType.ReadOnly<GrowthScaleData>(),
                    },
                },
            };
        }

        public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
        {
            prefabIndex = new PrefabIndex(prefab)
            {
                Category = Domain.Enums.PrefabCategory.Trees,
                SubCategory = Domain.Enums.PrefabSubCategory.Trees_Trees
            };

            if (prefab.name is "FlowerBushWild01" or "FlowerBushWild02" or "GreenBushWild01" or "GreenBushWild02")
            {
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Trees_Shrubs;
            }

            return true;
        }
    }
}
