using Game.Prefabs;

namespace FindIt.Domain
{
	public class PrefabIndexBase
	{
		public PrefabBase Prefab { get; }
		public int Id { get; set; }
		public string Name { get; set; }
		public string Thumbnail { get; set; }
		public bool Favorited { get; set; }

		public PrefabIndexBase(PrefabBase prefabBase)
		{
			Prefab = prefabBase;
		}
	}
}
