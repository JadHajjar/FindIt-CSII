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
		public int Id { get; set; }
		public string Name { get; set; }
		public string Thumbnail { get; set; }
		public string FallbackThumbnail { get; set; }
		public string DlcThumbnail { get; set; }
		public string CategoryThumbnail { get; set; }
		public bool Favorited { get; set; }

		public PrefabUIEntry(PrefabIndexBase prefab)
        {
            Id = prefab.Id;
			Name = prefab.Name;
			Thumbnail = prefab.Thumbnail;
			FallbackThumbnail = prefab.FallbackThumbnail;
			DlcThumbnail = prefab.DlcThumbnail;
			CategoryThumbnail = prefab.CategoryThumbnail;
			Favorited = prefab.Favorited;
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

			writer.TypeEnd();
		}
	}
}
