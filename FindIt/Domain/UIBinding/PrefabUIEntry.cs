using Colossal.UI.Binding;

using FindIt.Systems;

namespace FindIt.Domain.UIBinding
{
	public class PrefabUIEntry : IJsonWritable
	{
		public readonly int Id;
		private readonly string Name;
		private readonly string[] Thumbnails;
		private readonly string FallbackThumbnail;
		private readonly string DlcThumbnail;
		private readonly string CategoryThumbnail;
		private readonly bool Favorited;
		private readonly bool Random;
		private readonly bool Placed;
		private readonly string ThemeThumbnail;
		private readonly string[] PackThumbnails;

		public PrefabUIEntry(PrefabIndexBase prefab)
		{
			Id = prefab.Id;
			Name = prefab.Name;
			FallbackThumbnail = prefab.FallbackThumbnail;
			DlcThumbnail = prefab.DlcThumbnail;
			CategoryThumbnail = prefab.CategoryThumbnail;
			ThemeThumbnail = prefab.ThemeThumbnail;
			PackThumbnails = prefab.PackThumbnails;
			Favorited = prefab.IsFavorited;
			Random = prefab.IsRandom;
			Placed = PrefabTrackingSystem.GetMostUsedCount(prefab) > 0;

			if (prefab.IsRandom && prefab.RandomPrefabThumbnails != null)
			{
				Thumbnails = prefab.RandomPrefabThumbnails;
			}
			else
			{
				Thumbnails = new[] { prefab.Thumbnail };
			}
		}

		public void Write(IJsonWriter writer)
		{
			writer.TypeBegin(GetType().FullName);

			writer.PropertyName("id");
			writer.Write(Id);

			writer.PropertyName("name");
			writer.Write(Name);

			writer.PropertyName("thumbnails");
			writer.Write(Thumbnails);

			writer.PropertyName("fallbackThumbnail");
			writer.Write(FallbackThumbnail);

			writer.PropertyName("dlcThumbnail");
			writer.Write(DlcThumbnail);

			writer.PropertyName("categoryThumbnail");
			writer.Write(CategoryThumbnail);

			writer.PropertyName("themeThumbnail");
			writer.Write(ThemeThumbnail);

			writer.PropertyName("packThumbnails");
			writer.Write(PackThumbnails);

			writer.PropertyName("favorited");
			writer.Write(Favorited);

			writer.PropertyName("random");
			writer.Write(Random);

			writer.PropertyName("placed");
			writer.Write(Placed);

			writer.TypeEnd();
		}
	}
}
