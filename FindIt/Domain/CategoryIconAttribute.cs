using System;
using System.Collections.Generic;
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
			if (!Cache<TEnum>.Attributes.TryGetValue(enumValue, out var attribute))
			{
				Cache<TEnum>.Attributes[enumValue] = attribute = ReadAttribute(enumValue);
			}

			return attribute;
		}

		private static CategoryIconAttribute ReadAttribute<TEnum>(TEnum enumValue)
			where TEnum : struct, Enum
		{
			var memberInfo = typeof(TEnum).GetMember(enumValue.ToString()).FirstOrDefault() ?? throw new ArgumentException($"Invalid enum value: {enumValue}");

			var crnAttribute = (CategoryIconAttribute)memberInfo.GetCustomAttributes(typeof(CategoryIconAttribute), false).FirstOrDefault();

			return crnAttribute ?? throw new ArgumentException($"Enum value {enumValue} is missing CategoryIconAttribute");
		}

		// Caches the attribute per enum value, since looking it up uses reflection and indexing
		// reads it for every prefab.
		private static class Cache<TEnum> where TEnum : struct, Enum
		{
			public static readonly Dictionary<TEnum, CategoryIconAttribute> Attributes = new();
		}
	}
}
