using FindIt.Domain.Interfaces;

using Game.Prefabs;

using System;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class SportPropPrefabCategoryProcessor : IPrefabCategoryProcessor
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
						ComponentType.ReadOnly<PlaceableObjectData>(),
					},
					None = new[]
					{
						ComponentType.ReadOnly<BuildingData>(),
					}
				},
			};
		}

		public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
		{
			if (prefab is not BuildingExtensionPrefab)
			{
				prefabIndex = null;
				return false;
			}	

			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Props,
			};

			if (prefab.name.IndexOf("decal", StringComparison.InvariantCultureIgnoreCase) >= 0)
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Decals;
				return true;
			}

			prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Misc;

			return true;
		}
	}
}
