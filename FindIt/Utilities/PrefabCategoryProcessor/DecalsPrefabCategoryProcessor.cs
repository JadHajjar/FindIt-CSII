using FindIt.Domain;
using FindIt.Domain.Interfaces;
using FindIt.Utilities;
using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Utilities.PrefabCategoryProcessor
{
    public class DecalsPrefabCategoryProcessor : IPrefabCategoryProcessor
    {
        private readonly EntityManager _entityManager;

        public DecalsPrefabCategoryProcessor(EntityManager entityManager)
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
                        ComponentType.ReadOnly<StaticObjectData>(),
                        ComponentType.ReadOnly<SubMesh>(),
                    },
                    None = new[]
                    {
                        ComponentType.ReadOnly<BuildingData>(),
                        ComponentType.ReadOnly<BrandObjectData>(),
                        ComponentType.ReadOnly<PlaceholderObjectElement>(),
                    }
                },
            };
        }

        public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
        {
            if (prefab is not StaticObjectPrefab || !_entityManager.IsDecal(entity))
            {
                prefabIndex = null;
                return false;
            }

            prefabIndex = new PrefabIndex(prefab)
            {
                Category = Domain.Enums.PrefabCategory.Props,
                SubCategory = Domain.Enums.PrefabSubCategory.Props_Decals
            };

            return true;
        }
    }
}
