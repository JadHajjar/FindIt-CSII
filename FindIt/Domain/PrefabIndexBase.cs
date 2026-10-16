using Game.Prefabs;
using Game.UI;

using System;
using System.Globalization;
using System.Linq;

namespace FindIt.Domain
{
	public class PrefabIndexBase
	{
		private SortKey _nameSortKey;
		private string _thumbnail;
		private string[] _randomPrefabThumbnails;

		public PrefabBase Prefab { get; }
		public int Id { get; set; }
		public string PrefabName { get; set; }
		public string Name { get; set; }
		// Caches the culture-aware sort key, so sorting by name doesn't redo it per comparison.
		// A full index computes the keys on a worker thread.
		// The getter computes a key that is missing or belongs to an older name, and any thread
		// may call it.
		public SortKey NameSortKey
		{
			get
			{
				var name = Name ?? string.Empty;
				var sortKey = _nameSortKey;

				if (sortKey is null || sortKey.OriginalString != name)
				{
					sortKey = CultureInfo.CurrentCulture.CompareInfo.GetSortKey(name);
					_nameSortKey = sortKey;
				}

				return sortKey;
			}
		}
		public string PdxModsId { get; set; }
		// The getter builds it on first display, since only the prefabs the player scrolls to
		// need it.
		// ImageSystem reads Unity objects, so read this from the main thread.
		public string Thumbnail
		{
			get
			{
				return _thumbnail ??= GetThumbnail(Prefab);
			}
		}
		public string FallbackThumbnail { get; set; }
		public string DlcThumbnail { get; set; }
		public string CategoryThumbnail { get; set; }
		public bool IsFavorited { get; set; }
		public bool IsRandom { get; set; }
		// The prefabs a random prefab picks from, with a null for each one the lookup can't find.
		public PrefabBase[] RandomPrefabBases { get; set; }
		// The getter builds it on first display, like Thumbnail.
		public string[] RandomPrefabThumbnails
		{
			get
			{
				return _randomPrefabThumbnails ??= RandomPrefabBases
					?.Select(x => x is null ? null : GetThumbnail(x))
					.ToArray();
			}
		}
		public string ThemeThumbnail { get; set; }
		public string[] PackThumbnails { get; set; }
		public bool IsResourceIntensive { get; set; }

		public PrefabIndexBase(PrefabBase prefabBase)
		{
			Prefab = prefabBase;
		}

		private string GetThumbnail(PrefabBase prefab)
		{
			// The UI reads thumbnails while it writes a binding, where an exception would stop the
			// update, so a prefab whose icon ImageSystem can't resolve gets the fallback icon.
			try
			{
				return ImageSystem.GetThumbnail(prefab);
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, $"Failed to get the thumbnail of prefab '{PrefabName}'");

				return FallbackThumbnail;
			}
		}
	}
}
