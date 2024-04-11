using Colossal.UI.Binding;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindIt.Domain.UIBinding
{
	public struct OptionSection : IJsonWritable
	{
        public int Id { get; set; }
        public string Name { get; set; }
        public OptionItem[] Options { get; set; }

		public void Write(IJsonWriter writer)
		{
			writer.TypeBegin(GetType().FullName);

			writer.PropertyName("id");
			writer.Write(Id);

			writer.PropertyName("name");
			writer.Write(Name);

			writer.PropertyName("options");
			writer.ArrayBegin(Options.Length);
			for (var i = 0; i < Options.Length; i++)
			{
				Options[i].Write(writer);
			}
			writer.ArrayEnd();

			writer.TypeEnd();
		}
	}
}
