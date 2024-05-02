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
		private ValueBindingHelper<OptionSectionUIEntry[]> _OptionsList;

		protected override void OnCreate()
		{
			base.OnCreate();

			_OptionsList = CreateBinding("OptionsList", new OptionSectionUIEntry[0]);

			CreateTrigger<int, int, int>("OptionClicked", OptionClicked);
		}

		internal void RefreshOptions()
		{
			if (!FindItUtil.IsReady)
			{
				return;
			}

			if (_sections.Count == 0)
			{
				foreach (var type in typeof(OptionsUISystem).Assembly.GetTypes())
				{
					if (typeof(IOptionSection).IsAssignableFrom(type) && !type.IsAbstract)
					{
						var section = (IOptionSection)Activator.CreateInstance(type, this);

						_sections.Add(section.Id, section);
					}
				}
			}

			_OptionsList.Value = GetVisibleSections()
				.OrderBy(x => x.Id)
				.Select(x => x.AsUIEntry())
				.ToArray();
		}

		private IEnumerable<IOptionSection> GetVisibleSections()
		{
			foreach (var section in _sections.Values)
			{
				if (section.IsVisible())
				{
					yield return section;
				}
				else
				{
					section.OnReset();
				}
			}
		}

		private void OptionClicked(int sectionId, int optionId, int value)
		{
			if (!_sections.TryGetValue(sectionId, out var section))
			{
				return;
			}

			section.OnOptionClicked(optionId, value);

			RefreshOptions();
		}
	}
}
