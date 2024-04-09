using FindIt.Domain.Interfaces;

using Game.Prefabs;

using System;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class RoadUtilityPropsPrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		public EntityQueryDesc[] GetEntityQuery()
		{
			return new[]
			{
				new EntityQueryDesc
				{
					All = new[]
					{
						ComponentType.ReadOnly<UtilityObjectData>(),
						ComponentType.ReadOnly<SubLane>(),
					},
					None = new[]
					{
						ComponentType.ReadOnly<PillarData>(),
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

			if (prefab.name.IndexOf("marking", StringComparison.InvariantCultureIgnoreCase) >= 0
				|| prefab.name.IndexOf("arrow", StringComparison.InvariantCultureIgnoreCase) >= 0
				|| prefab.name.IndexOf("decal", StringComparison.InvariantCultureIgnoreCase) >= 0)
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Decals;
			}
			else
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Road;
			}

			return true;
		}
	}
}
