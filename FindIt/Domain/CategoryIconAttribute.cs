using System;
using System.Linq;

namespace FindIt.Domain
{
	public class CategoryIconAttribute : Attribute
	{
		public string Icon { get; set; }

		public CategoryIconAttribute(string icon)
		{
			Icon = icon;
		}

		public static CategoryIconAttribute GetAttribute<TEnum>(TEnum enumValue) where TEnum : struct, Enum
		{
			var memberInfo = typeof(TEnum).GetMember(enumValue.ToString()).FirstOrDefault() ?? throw new ArgumentException($"Invalid enum value: {enumValue}");

			var crnAttribute = (CategoryIconAttribute)memberInfo.GetCustomAttributes(typeof(CategoryIconAttribute), false).FirstOrDefault();

			return crnAttribute ?? throw new ArgumentException($"Enum value {enumValue} is missing CategoryIconAttribute");
		}
	}
}
