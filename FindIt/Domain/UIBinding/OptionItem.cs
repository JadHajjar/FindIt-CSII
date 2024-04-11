using Colossal.UI.Binding;

namespace FindIt.Domain.UIBinding
{
	public struct OptionItem : IJsonWritable
	{
        public int Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public bool Selected { get; set; }

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
			writer.TypeEnd();
		}
	}
}
