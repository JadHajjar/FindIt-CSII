using FindIt.Domain.Interfaces;

using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class SurfacePrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		public EntityQuery Query { get; set; }

		public EntityQueryDesc[] GetEntityQuery()
		{
			return new[]
			{
				new EntityQueryDesc
				{
					Any = new[]
					{
						ComponentType.ReadOnly<AreaData>(),
						ComponentType.ReadOnly<SurfaceData>(),
					},
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
