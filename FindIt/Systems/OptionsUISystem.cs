using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;

namespace FindIt.Systems
{
	internal partial class OptionsUISystem : ExtendedUISystemBase
	{
		private readonly Dictionary<int, IOptionSection> _sections = new();
		private ValueBindingHelper<string> _ViewStyle;
		private ValueBindingHelper<OptionSectionUIEntry[]> _OptionsList;

		public string ViewStyle { get => _ViewStyle; set => _ViewStyle.Value = value; }

		protected override void OnCreate()
		{
			base.OnCreate();

			foreach (var type in typeof(OptionsUISystem).Assembly.GetTypes())
			{
				if (typeof(IOptionSection).IsAssignableFrom(type) && !type.IsAbstract)
				{
					var section = (IOptionSection)Activator.CreateInstance(type, this);

					_sections.Add(section.Id, section);
				}
			}

			_ViewStyle = CreateBinding("ViewStyle", "GridWithText");
			_OptionsList = CreateBinding("OptionsList", GetOptionsList());

			CreateTrigger<int, int>("OptionClicked", OptionClicked);
		}

		internal void RefreshOptions()
		{
			_OptionsList.Value = GetOptionsList();
		}

		private OptionSectionUIEntry[] GetOptionsList()
		{
			return _sections.Values
				.Where(x => x.IsVisible())
				.Select(x => x.AsUIEntry())
				.ToArray();
		}

		private void OptionClicked(int sectionId, int optionId)
		{
			if (!_sections.TryGetValue(sectionId, out var section))
			{
				return;
			}

			section.OnOptionClicked(optionId);
		}
	}
}
