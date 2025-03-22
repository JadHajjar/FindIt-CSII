using FindIt.Domain;
using FindIt.Domain.Interfaces;
using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Utilities.PrefabCategoryProcessor
{
    public class SurfacePrefabCategoryProcessor : IPrefabCategoryProcessor
    {
        public EntityQueryDesc[] GetEntityQuery()
        {
            return new[]
            {
                new EntityQueryDesc
                {
                    All = new[]
                    {
                        ComponentType.ReadOnly<AreaData>(),
                    },
                    None = new[]
                    {
                        ComponentType.ReadOnly<PlaceholderObjectElement>(),
                    }
                },
                new EntityQueryDesc
                {
                    All = new[]
                    {
                        ComponentType.ReadOnly<SurfaceData>(),
                    },
                    None = new[]
                    {
                        ComponentType.ReadOnly<PlaceholderObjectElement>(),
                    }
                },
            };
        }

        public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
        {
            prefabIndex = new PrefabIndex(prefab)
            {
                Category = Domain.Enums.PrefabCategory.Props,
                SubCategory = Domain.Enums.PrefabSubCategory.Props_Surfaces,
            };

            return true;
        }
    }
}
