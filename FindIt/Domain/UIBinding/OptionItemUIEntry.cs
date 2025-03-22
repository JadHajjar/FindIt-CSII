﻿using Colossal.UI.Binding;
using FindIt.Utilities;

namespace FindIt.Domain.UIBinding
{
    public class OptionItemUIEntry : IJsonWritable
	{
		private string _name;

		public int Id { get; set; }
		public string Name { get => _name; set => _name = LocaleHelper.Translate(value); }
		public string Icon { get; set; }
		public bool Selected { get; set; }
		public bool IsValue { get; set; }
		public string Value { get; set; }
		public bool Hidden { get; set; }
		public bool Disabled { get; set; }

		public void Write(IJsonWriter writer)
		{
			writer.TypeBegin(GetType().FullName);

			writer.PropertyName("id");
			writer.Write(Id);

			writer.PropertyName("name");
			writer.Write(Name);

			writer.PropertyName("icon");
			writer.Write(Icon);

			writer.PropertyName("selected");
			writer.Write(Selected);

			writer.PropertyName("isValue");
			writer.Write(IsValue);

			writer.PropertyName("value");
			writer.Write(Value);

			writer.PropertyName("disabled");
			writer.Write(Disabled);

			writer.TypeEnd();
		}
	}
}
