using FindIt.Domain;
using FindIt.Domain.Interfaces;
using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Utilities.PrefabCategoryProcessor
{
    public class BridgesPrefabCategoryProcessor : IPrefabCategoryProcessor
    {
        public EntityQueryDesc[] GetEntityQuery()
        {
            return new[]
            {
                new EntityQueryDesc
                {
                    All = new[]
                    {
                        ComponentType.ReadOnly<BridgeData>(),
                    },
                },
            };
        }

        public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
        {
            if (prefab is StaticObjectPrefab)
            {
                prefabIndex = null;
                return false;
            }

            prefabIndex = new PrefabIndex(prefab)
            {
                Category = Domain.Enums.PrefabCategory.Networks,
                SubCategory = Domain.Enums.PrefabSubCategory.Networks_Bridges
            };

            return true;
        }
    }
}
