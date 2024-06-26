﻿using FindIt.Domain.Interfaces;

using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class PillarPrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		private readonly EntityManager _entityManager;

		public PillarPrefabCategoryProcessor(EntityManager entityManager)
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
						ComponentType.ReadOnly<PillarData>(),
						ComponentType.ReadOnly<SpawnableObjectData>(),
					}
				},
			};
		}

		public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
		{
			var netObjectData = _entityManager.GetComponentData<NetObjectData>(entity);

			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Networks,
				SubCategory = Enums.PrefabSubCategory.Networks_Pillars
			};

			return true;
		}
	}
}
