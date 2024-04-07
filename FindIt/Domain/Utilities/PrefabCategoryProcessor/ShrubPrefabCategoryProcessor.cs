using FindIt.Domain.Interfaces;

using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class ShrubPrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		private readonly EntityManager _entityManager;

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
						ComponentType.ReadOnly<LoadedIndex>(),
					},
					None = new[]
					{
						ComponentType.ReadOnly<TreeData>(),
						ComponentType.ReadOnly<NetLaneData>(),
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

			if (prefab.name is "TreeCityRandom01" or "TreeWildRandom01")
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Trees_Trees;
			}

			if (!_entityManager.HasComponent<Effect>(entity))
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Trees_Props;
			}

			return true;
		}
	}
}
