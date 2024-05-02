﻿using Colossal.UI.Binding;

using System.Collections.Generic;
using System.Linq;

namespace FindIt.Domain.UIBinding
{
	public struct OptionSectionUIEntry : IJsonWritable
	{
		private OptionItemUIEntry[] _options;

		public int Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<OptionItemUIEntry> Options 
		{
			get => _options; 
			set => _options = value.Where(x => !x.Hidden).ToArray();
		}

		public readonly void Write(IJsonWriter writer)
		{
			writer.TypeBegin(GetType().FullName);

			writer.PropertyName("id");
			writer.Write(Id);

			writer.PropertyName("name");
			writer.Write(Name);

			writer.PropertyName("options");
			writer.ArrayBegin(_options.Length);
			for (var i = 0; i < _options.Length; i++)
			{
				_options[i].Write(writer);
			}

			writer.ArrayEnd();

			writer.TypeEnd();
		}
	}
}
