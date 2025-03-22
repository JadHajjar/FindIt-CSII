using Colossal.UI.Binding;

using FindIt.Domain.Enums;
using FindIt.Utilities;

namespace FindIt.Domain.UIBinding
{
    public struct SubCategoryUIEntry : IJsonWritable
	{
		public int Id { get; set; }
		public string Icon { get; set; }
		public string ToolTip { get; set; }

		public SubCategoryUIEntry(PrefabSubCategory category)
		{
			Id = (int)category;
			Icon = CategoryIconAttribute.GetAttribute(category).Icon;
			ToolTip = LocaleHelper.Translate($"Tooltip.LABEL[FindIt.{category}]");
		}

		public readonly void Write(IJsonWriter writer)
		{
			writer.TypeBegin(GetType().FullName);

			writer.PropertyName("id");
			writer.Write(Id);

			writer.PropertyName("icon");
			writer.Write(Icon);

			writer.PropertyName("toolTip");
			writer.Write(ToolTip);

			writer.TypeEnd();
		}
	}
}
