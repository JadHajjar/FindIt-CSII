using Colossal.UI.Binding;

using FindIt.Systems;

using Unity.Entities;

namespace FindIt.Domain.UIBinding
{
	public class PrefabUIEntry : IJsonWritable
	{
		private readonly PrefabIndexBase _prefab;
		private readonly string[] _thumbnails;
		private readonly bool _placed;

		public PrefabUIEntry(PrefabIndexBase prefab)
		{
			_prefab = prefab;
			_placed = PrefabTrackingSystem.GetMostUsedCount(prefab) > 0;
		}

		public void Write(IJsonWriter writer)
		{
			writer.TypeBegin(GetType().FullName);

			writer.PropertyName("id");
			writer.Write(_prefab.Id);

			writer.PropertyName("name");
			writer.Write(_prefab.Name);

			writer.PropertyName("fallbackThumbnail");
			writer.Write(_prefab.FallbackThumbnail);

			writer.PropertyName("dlcThumbnail");
			writer.Write(_prefab.DlcThumbnail);

			writer.PropertyName("categoryThumbnail");
			writer.Write(_prefab.CategoryThumbnail);

			writer.PropertyName("themeThumbnail");
			writer.Write(_prefab.ThemeThumbnail);

			writer.PropertyName("packThumbnails");
			writer.Write(_prefab.PackThumbnails);

			writer.PropertyName("favorited");
			writer.Write(_prefab.IsFavorited);

			writer.PropertyName("random");
			writer.Write(_prefab.IsRandom);

			writer.PropertyName("pdxId");
			writer.Write(_prefab.PdxModsId);

			writer.PropertyName("placed");
			writer.Write(_placed);

			writer.PropertyName("thumbnails");
			if (_prefab.IsRandom && _prefab.RandomPrefabThumbnails != null)
			{
				writer.Write(_prefab.RandomPrefabThumbnails);
			}
			else
			{
				writer.Write(new[] { _prefab.Thumbnail });
			}

			writer.TypeEnd();
		}
	}
}
