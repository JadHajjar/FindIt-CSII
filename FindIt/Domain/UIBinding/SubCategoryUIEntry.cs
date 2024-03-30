using Colossal.UI.Binding;

using FindIt.Domain.Enums;

namespace FindIt.Domain.UIBinding
{
	public struct SubCategoryUIEntry : IJsonWritable
	{
		public int Id { get; set; }
		public string Icon { get; set; }

		public SubCategoryUIEntry(PrefabSubCategory category)
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
