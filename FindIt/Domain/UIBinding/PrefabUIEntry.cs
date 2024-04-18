using Colossal.UI.Binding;

using FindIt.Domain.Enums;
using Game.Prefabs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindIt.Domain.UIBinding
{
	public struct PrefabUIEntry : IJsonWritable
	{
		public readonly int Id;
		private readonly string Name;
		private readonly string Thumbnail;
		private readonly string FallbackThumbnail;
		private readonly string DlcThumbnail;
		private readonly string CategoryThumbnail;
		private readonly bool Favorited;
		private readonly bool Random;

		public PrefabUIEntry(PrefabIndexBase prefab)
        {
            Id = prefab.Id;
			Name = prefab.Name;
			Thumbnail = prefab.Thumbnail;
			FallbackThumbnail = prefab.FallbackThumbnail;
			DlcThumbnail = prefab.DlcThumbnail;
			CategoryThumbnail = prefab.CategoryThumbnail;
			Favorited = prefab.IsFavorited;
			Random = prefab.IsRandom;
		}

        public readonly void Write(IJsonWriter writer)
		{
			writer.TypeBegin(GetType().FullName);

			writer.PropertyName("id");
			writer.Write(Id);

			writer.PropertyName("name");
			writer.Write(Name);

			writer.PropertyName("thumbnail");
			writer.Write(Thumbnail);

			writer.PropertyName("fallbackThumbnail");
			writer.Write(FallbackThumbnail);

			writer.PropertyName("dlcThumbnail");
			writer.Write(DlcThumbnail);

			writer.PropertyName("categoryThumbnail");
			writer.Write(CategoryThumbnail);

			writer.PropertyName("favorited");
			writer.Write(Favorited);

			writer.PropertyName("random");
			writer.Write(Random);

			writer.TypeEnd();
		}
	}
}
