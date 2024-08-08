using FindIt.Domain.Interfaces;

using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Utilities
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
				Category = Enums.PrefabCategory.Props,
				SubCategory = Enums.PrefabSubCategory.Props_Surfaces,
			};

			return true;
		}
	}
}
