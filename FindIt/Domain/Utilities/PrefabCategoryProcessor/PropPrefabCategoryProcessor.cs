using FindIt.Domain.Interfaces;

using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class PropPrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		private readonly EntityManager _entityManager;

		public EntityQuery Query { get; set; }

		public PropPrefabCategoryProcessor(EntityManager entityManager)
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
						ComponentType.ReadOnly<StaticObjectData>(),
					},
					None = new[]
					{
						ComponentType.ReadOnly<BuildingData>(),
						ComponentType.ReadOnly<NetObjectData>(),
						ComponentType.ReadOnly<BuildingExtensionData>(),
						ComponentType.ReadOnly<PillarData>(),
						ComponentType.ReadOnly<PlantData>(),
					},
				},
			};
		}

		public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
		{
			//if (prefab is StaticObjectPrefab)
			//{
			//	prefabIndex = null;
			//	return false;
			//}

			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Props,
			};

			if (_entityManager.HasComponent<LightEffectData>(entity))
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Lights;
			}
			else if (_entityManager.HasComponent<BrandObjectData>(entity))
			{
				prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Branding;
			}
			else
			prefabIndex.SubCategory = Enums.PrefabSubCategory.Props_Lights;

			return true;
		}
	}
}
