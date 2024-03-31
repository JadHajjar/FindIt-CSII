﻿using FindIt.Domain.Interfaces;

using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class SmallRoadsPrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		private readonly EntityManager _entityManager;

		public EntityQuery Query { get; set; }

		public SmallRoadsPrefabCategoryProcessor(EntityManager entityManager)
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
						ComponentType.ReadOnly<RoadData>(),
					},
					None = new[]
					{
						ComponentType.ReadOnly<BridgeData>(),
					},
				},
			};
		}

		public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
		{
			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Networks,
				SubCategory = Enums.PrefabSubCategory.Networks_Roads
			};

			return true;
		}
	}
}