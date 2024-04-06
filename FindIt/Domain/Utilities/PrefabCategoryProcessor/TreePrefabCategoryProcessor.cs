using FindIt.Domain.Interfaces;

using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	public class TreePrefabCategoryProcessor : IPrefabCategoryProcessor
	{
		public EntityQuery Query { get; set; }

		public EntityQueryDesc[] GetEntityQuery()
		{
			return new[]
			{
				new EntityQueryDesc
				{
					All = new[]
					{
						ComponentType.ReadOnly<TreeData>(),
					},
				},
			};
		}

		public bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex)
		{
			if (prefab.name.Contains("ADDAD_") || prefab.name.Contains("Billboard") || prefab.name is "GasStationPylon01 - AnnusInteriorDesign" or "GasStationPylon02 - AnnusInteriorDesign" or "GasStationPylon02 - BellAndCog" or "GasStationPylon03 - BellAndCog" or "PosterHuge01 - BellAndCog" or "PosterHuge02 - BellAndCog")
			{
				prefabIndex = null;
				return false;
			}

			prefabIndex = new PrefabIndex(prefab)
			{
				Category = Enums.PrefabCategory.Trees,
				SubCategory = Enums.PrefabSubCategory.Trees_Trees
			};

			return true;
		}
	}
}
