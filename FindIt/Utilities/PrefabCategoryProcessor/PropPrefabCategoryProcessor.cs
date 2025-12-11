using FindIt.Domain;
using FindIt.Domain.Enums;
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
						ComponentType.ReadOnly<QuantityObjectData>(),
					},
                },
            };
        }

        public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
        {
            if (TryGetSubCategory(prefab, entity, _entityManager, out var subCategory))
			{
				prefabIndex = new PrefabIndex(prefab)
				{
					Category = (PrefabCategory)((int)subCategory - ((int)subCategory%100)),
					SubCategory = subCategory,
				};

				return true;
			}

            prefabIndex = null;
            return false;
        }

        public static bool TryGetSubCategory(PrefabBase prefab, Entity entity, EntityManager entityManager, out PrefabSubCategory subCategory)
        {
			if (entityManager.HasComponent<LightEffectData>(entity) && entityManager.HasComponent<EffectData>(entity))
			{
				subCategory = PrefabSubCategory.Props_Lights;
				return true;
			}

			if (entityManager.IsBrandEntity(entity))
			{
				subCategory = PrefabSubCategory.Props_Branding;
				return true;
			}

			var assetCategoryOverride = prefab.GetComponent<EditorAssetCategoryOverride>();

			for (var i = 0; i < (assetCategoryOverride?.m_IncludeCategories?.Length ?? 0); i++)
			{
				switch (assetCategoryOverride.m_IncludeCategories[i])
				{
					case "Props/Props/Brand Graphics":
					case "Props/Decorations/Brand Graphics":
						subCategory = PrefabSubCategory.Props_Branding;
						return true;

					case "Props/Nature Decorations/Boulders":
					case "Props/Nature Decorations":
						subCategory = PrefabSubCategory.Trees_Rocks;
						return true;

					case "Props/Decorations/Industrial Manufacturing":
					case "Props/Decorations/Industrial":
					case "Props/Decorations/Industrial Extractor":
					case "Props/Props/Industrial":
						subCategory = PrefabSubCategory.Props_Industrial;
						return true;

					case "Props/Props/Commercial":
					case "Props/Decorations/Commercial":
						subCategory = PrefabSubCategory.Props_Commercial;
						return true;

					case "Props/Decorations/Residential":
					case "Props/Props/Residential":
					case "Props/Props/Residential Plants":
						subCategory = PrefabSubCategory.Props_Residential;
						return true;

					case "Props/Decorations/Service":
						subCategory = PrefabSubCategory.Props_Service;
						return true;

					case "Props/Decorations/Net":
						subCategory = PrefabSubCategory.Props_Fences;
						return true;

					case "Props/Props/Dynamic":
					case "Props/Props/Park":
					case "Props/Decorations/Park":
					case "Props/Decorations/Dynamic":
						subCategory = PrefabSubCategory.Props_Park;
						return true;

					case "Props/Road Decorations":
						subCategory = PrefabSubCategory.Props_Road;
						return true;

					case "Lights":
						subCategory = PrefabSubCategory.Props_Lights;
						return true;
				}
			}

			if (entityManager.IsDecal(entity))
			{
                subCategory = default;
				return false;
			}

			if (prefab.isBuiltin && prefab.name.StartsWith("NotreDame"))
			{
				subCategory = PrefabSubCategory.Props_Misc;
				return true;
			}

			subCategory = PrefabSubCategory.Props_Misc;

			return true;
		}
    }
}
