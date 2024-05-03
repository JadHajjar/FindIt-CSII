using FindIt.Domain.Interfaces;

using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class StorefrontPrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		public EntityQueryDesc[] GetEntityQuery()
		{
			return new[]
			{
				new EntityQueryDesc
				{
					All = new[]
					{
						ComponentType.ReadOnly<StaticObjectData>(),
						ComponentType.ReadOnly<BuildingExtensionData>(),
						ComponentType.ReadOnly<SpawnableObjectData>(),
					}
				},
			};
		}

		public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
		{
			if (!prefab.name.Contains("Storefront"))
			{
				prefabIndex = null;
				return false;
			}

			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Props,
				SubCategory = Enums.PrefabSubCategory.Props_Commercial
			};

			return true;
		}
	}
}
