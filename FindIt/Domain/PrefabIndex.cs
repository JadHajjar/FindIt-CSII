using Colossal.PSI.Common;

using FindIt.Domain.Enums;

using Game.Prefabs;

using System.Collections.Generic;

using Unity.Mathematics;

namespace FindIt.Domain
{
	public class PrefabIndex : PrefabIndexBase
	{
		public PrefabCategory Category { get; set; }
		public PrefabSubCategory SubCategory { get; set; }
		public DlcId DlcId { get; set; }
		public ZoneTypeFilter ZoneType { get; set; } = ZoneTypeFilter.Any;
		public BuildingCornerFilter CornerType { get; set; }
		public int2 LotSize { get; set; }
		public int BuildingLevel { get; set; }
		public bool IsVanilla { get; set; }
		public bool IsUniqueMesh { get; set; }
		public ThemePrefab Theme { get; set; }
		public AssetPackPrefab[] AssetPacks { get; set; }
		public int[] RandomPrefabs { get; set; }
		public List<string> Tags { get; set; }

		public PrefabIndex(PrefabBase prefabBase) : base(prefabBase)
		{
		}
	}
}
