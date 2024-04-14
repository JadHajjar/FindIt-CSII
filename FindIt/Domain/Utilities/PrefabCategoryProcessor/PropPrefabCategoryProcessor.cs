using FindIt.Domain.Interfaces;

using Game.Prefabs;

using System;
using System.Linq;

using Unity.Entities;

namespace FindIt.Domain.Utilities
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
						ComponentType.ReadOnly<PlaceableObjectData>(),
					},
					None = new[]
					{
						ComponentType.ReadOnly<BuildingData>(),
						ComponentType.ReadOnly<NetObjectData>(),
						ComponentType.ReadOnly<BuildingExtensionData>(),
						ComponentType.ReadOnly<PillarData>(),
						ComponentType.ReadOnly<PlantData>(),
					},
				},
			};
		}

		public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
		{
			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Props,
			};

			if (_entityManager.HasComponent<LightEffectData>(entity) && _entityManager.HasComponent<EffectData>(entity))
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Lights;
				return true;
			}

			if (_entityManager.IsBrandEntity(entity) || prefab.name.Contains("ADDAD"))
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Branding;
				return true;
			}

			var assetCategoryOverride = prefab.GetComponent<EditorAssetCategoryOverride>();

			if (assetCategoryOverride?.m_IncludeCategories.Any() ?? false)
			{
				switch (assetCategoryOverride.m_IncludeCategories[0])
				{
					case "Props/Props/Brand Graphics":
					case "Props/Decorations/Brand Graphics":
						prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Branding;
						return true;

					case "Props/Nature Decorations/Boulders":
					case "Props/Nature Decorations":
						prefabIndex.Category = Enums.PrefabCategory.Trees;
						prefabIndex.SubCategory = Enums.PrefabSubCategory.Trees_Rocks;
						return true;

					case "Props/Decorations/Industrial Manufacturing":
					case "Props/Decorations/Industrial":
					case "Props/Decorations/Industrial Extractor":
					case "Props/Props/Industrial":
						prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Industrial;
						return true;

					case "Props/Props/Commercial":
					case "Props/Decorations/Commercial":
						prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Commercial;
						return true;

					case "Props/Decorations/Residential":
					case "Props/Props/Residential":
					case "Props/Props/Residential Plants":
						prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Residential;
						return true;

					case "Props/Decorations/Service":
						prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Service;
						return true;

					case "Props/Decorations/Net":
						prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Fences;
						return true;

					case "Props/Props/Dynamic":
					case "Props/Props/Park":
					case "Props/Decorations/Park":
					case "Props/Decorations/Dynamic":
						prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Park;
						return true;

					case "Props/Road Decorations":
						prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Road;
						return true;

					case "Lights":
						prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Lights;
						return true;
				}
			}

			if (prefab.name.IndexOf("decal", StringComparison.InvariantCultureIgnoreCase) >= 0)
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Decals;
				return true;
			}

			if (prefab.name.StartsWith("NotreDame"))
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Misc;
				return true;
			}

			if (!_entityManager.HasComponent<SpawnableObjectData>(entity) && !_entityManager.HasComponent<PlaceholderObjectData>(entity))
			{
				return false;
			}

			prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Misc;

			return true;
		}
	}
}
