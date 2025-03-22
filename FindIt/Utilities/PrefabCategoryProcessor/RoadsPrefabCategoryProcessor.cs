using FindIt.Domain;
using FindIt.Domain.Interfaces;
using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Utilities.PrefabCategoryProcessor
{
    public class RoadsPrefabCategoryProcessor : IPrefabCategoryProcessor
    {
        public EntityQueryDesc[] GetEntityQuery()
        {
            if (Mod.IsRoadBuilderEnabled)
            {
                return new[]
                {
                    new EntityQueryDesc
                    {
                        All = new[]
                        {
                            ComponentType.ReadOnly<RoadData>(),
                        },
                        None = new[]
                        {
                            ComponentType.ReadOnly<BridgeData>(),
                        },
                    },
                };
            }

            return new[]
            {
                new EntityQueryDesc
                {
                    All = new[]
                    {
                        ComponentType.ReadOnly<RoadData>(),
                    },
                    None = new[]
                    {
                        ComponentType.ReadOnly<BridgeData>(),
                    },
                },
            };
        }

        public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
        {
            if (prefab is not RoadPrefab roadPrefab)
            {
                prefabIndex = null;
                return false;
            }

            prefabIndex = new PrefabIndex(prefab)
            {
                Category = Domain.Enums.PrefabCategory.Networks,
                SubCategory = Domain.Enums.PrefabSubCategory.Networks_Roads
            };

            if (roadPrefab.m_HighwayRules)
            {
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Networks_Highways;
            }
            else
            {
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Networks_Roads;
            }

            return true;
        }
    }
}
