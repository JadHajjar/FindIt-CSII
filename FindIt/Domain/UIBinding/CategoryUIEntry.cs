using Colossal.UI.Binding;

using FindIt.Domain.Enums;

using Game.Debug.Tests;

namespace FindIt.Domain.UIBinding
{
	public struct CategoryUIEntry : IJsonWritable
	{
		public int Id { get; set; }
		public string Icon { get; set; }

        public CategoryUIEntry(PrefabCategory category)
		{
			Id = (int)category;
			Icon = CategoryIconAttribute.GetAttribute(category).Icon;
		}

		public readonly void Write(IJsonWriter writer)
		{
			writer.TypeBegin(GetType().FullName);
			writer.PropertyName("id");
			writer.Write(Id);
			writer.PropertyName("icon");
			writer.Write(Icon);
			writer.TypeEnd();
		}
	}
}
