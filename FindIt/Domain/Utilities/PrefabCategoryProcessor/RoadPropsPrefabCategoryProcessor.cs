using FindIt.Domain.Interfaces;

using Game.Prefabs;

using System;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class RoadPropsPrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		private readonly EntityManager _entityManager;

		public RoadPropsPrefabCategoryProcessor(EntityManager entityManager)
		{
			_entityManager = entityManager;
		}

		public EntityQueryDesc[] GetEntityQuery()
		{
			return new[]
			{
				new EntityQueryDesc
				{
					Any = new[]
					{
						ComponentType.ReadOnly<TrafficSignData>(),
						ComponentType.ReadOnly<LaneDirectionData>(),
						ComponentType.ReadOnly<TrafficLightData>(),
					},
				}
			};
		}

		public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
		{
			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Props,
			};

			if (_entityManager.IsDecal(entity))
			{
				return false;
			}
			else
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Road;
			}

			return true;
		}
	}
}
