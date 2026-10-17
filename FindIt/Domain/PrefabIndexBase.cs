using Game.Prefabs;

using System.Globalization;

namespace FindIt.Domain
{
	public class PrefabIndexBase
	{
		public PrefabBase Prefab { get; }
		public int Id { get; set; }
		public string PrefabName { get; set; }
		public string Name { get; set; }
		public SortKey NameSortKey { get; set; }
		public string PdxModsId { get; set; }
		public string FallbackThumbnail { get; set; }
		public string DlcThumbnail { get; set; }
		public string CategoryThumbnail { get; set; }
		public bool IsFavorited { get; set; }
		public bool IsRandom { get; set; }
		public PrefabBase[] RandomPrefabBases { get; set; }
		public string ThemeThumbnail { get; set; }
		public string[] PackThumbnails { get; set; }
		public bool IsResourceIntensive { get; set; }

		public PrefabIndexBase(PrefabBase prefabBase)
		{
			Prefab = prefabBase;
		}
	}
}
