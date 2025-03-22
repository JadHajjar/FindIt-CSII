using FindIt.Domain;
using FindIt.Domain.Interfaces;
using FindIt.Utilities;
using Game.Prefabs;

using System.Linq;

using Unity.Entities;

namespace FindIt.Utilities.PrefabCategoryProcessor
{
    public class PropPrefabCategoryProcessor : IPrefabCategoryProcessor
    {
        private readonly EntityManager _entityManager;

        public PropPrefabCategoryProcessor(EntityManager entityManager)
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
                    },
                    None = new[]
                    {
                        ComponentType.ReadOnly<BuildingData>(),
                        ComponentType.ReadOnly<NetObjectData>(),
                        ComponentType.ReadOnly<BuildingExtensionData>(),
                        ComponentType.ReadOnly<PillarData>(),
                        ComponentType.ReadOnly<PlantData>(),
						ComponentType.ReadOnly<TrafficSignData>(),
						ComponentType.ReadOnly<LaneDirectionData>(),
						ComponentType.ReadOnly<TrafficLightData>(),
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

            if (_entityManager.HasComponent<LightEffectData>(entity) && _entityManager.HasComponent<EffectData>(entity))
            {
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Lights;
                return true;
            }

            if (_entityManager.IsBrandEntity(entity))
            {
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Branding;
                return true;
            }

            var assetCategoryOverride = prefab.GetComponent<EditorAssetCategoryOverride>();

            for (var i = 0; i < (assetCategoryOverride?.m_IncludeCategories?.Length ?? 0); i++)
            {
                switch (assetCategoryOverride.m_IncludeCategories[i])
                {
                    case "Props/Props/Brand Graphics":
                    case "Props/Decorations/Brand Graphics":
                        prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Branding;
                        return true;

                    case "Props/Nature Decorations/Boulders":
                    case "Props/Nature Decorations":
                        prefabIndex.Category = Domain.Enums.PrefabCategory.Trees;
                        prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Trees_Rocks;
                        return true;

                    case "Props/Decorations/Industrial Manufacturing":
                    case "Props/Decorations/Industrial":
                    case "Props/Decorations/Industrial Extractor":
                    case "Props/Props/Industrial":
                        prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Industrial;
                        return true;

                    case "Props/Props/Commercial":
                    case "Props/Decorations/Commercial":
                        prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Commercial;
                        return true;

                    case "Props/Decorations/Residential":
                    case "Props/Props/Residential":
                    case "Props/Props/Residential Plants":
                        prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Residential;
                        return true;

                    case "Props/Decorations/Service":
                        prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Service;
                        return true;

                    case "Props/Decorations/Net":
                        prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Fences;
                        return true;

                    case "Props/Props/Dynamic":
                    case "Props/Props/Park":
                    case "Props/Decorations/Park":
                    case "Props/Decorations/Dynamic":
                        prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Park;
                        return true;

                    case "Props/Road Decorations":
                        prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Road;
                        return true;

					case "Lights":
						prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Lights;
						return true;

					case "Vehicles":
                        prefabIndex.Category = Domain.Enums.PrefabCategory.Vehicles;
						prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Vehicles_Misc;
						return true;
				}
            }

            if (_entityManager.IsDecal(entity))
            {
                return false;
            }

            if (prefab.builtin && prefab.name.StartsWith("NotreDame"))
            {
                prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Misc;
                return true;
            }

            if (!_entityManager.HasComponent<SpawnableObjectData>(entity) && !_entityManager.HasComponent<PlaceholderObjectData>(entity))
            {
                //return false;
            }

            prefabIndex.SubCategory = Domain.Enums.PrefabSubCategory.Props_Misc;

            return true;
        }
    }
}
