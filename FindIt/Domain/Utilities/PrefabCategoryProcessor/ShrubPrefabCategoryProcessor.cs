using FindIt.Domain.Interfaces;

using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class ShrubPrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		private readonly EntityManager _entityManager;

		public EntityQuery Query { get; set; }

		public ShrubPrefabCategoryProcessor(EntityManager entityManager)
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
						ComponentType.ReadOnly<PlantData>(),
					},
					None = new[]
					{
						ComponentType.ReadOnly<TreeData>(),
					},
				},
			};
		}

		public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
		{
			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Trees,
				SubCategory = Enums.PrefabSubCategory.Trees_Shrubs
			};

			return true;
		}
	}
}
