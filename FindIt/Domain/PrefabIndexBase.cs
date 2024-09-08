using Game.Prefabs;

namespace FindIt.Domain
{
	public class PrefabIndexBase
	{
		public PrefabBase Prefab { get; }
		public int Id { get; set; }
		public string PrefabName { get; set; }
		public string Name { get; set; }
		public string Thumbnail { get; set; }
		public string FallbackThumbnail { get; set; }
		public string DlcThumbnail { get; set; }
		public string CategoryThumbnail { get; set; }
		public bool IsFavorited { get; set; }
		public bool IsRandom { get; set; }
		public string[] RandomPrefabThumbnails { get; set; }

		public PrefabIndexBase(PrefabBase prefabBase)
		{
			Prefab = prefabBase;
		}
	}
}
