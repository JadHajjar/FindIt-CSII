using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

using System.Collections.Generic;
using System.Linq;

namespace FindIt.Domain.Options
{
	internal class SortingOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private readonly Dictionary<int, (string Name, string Icon)> _styles;

		public int Id { get; } = 2;

		public SortingOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
			_styles = new()
			{
				[1] = ("style1", ""),
				[2] = ("style2", ""),
				[3] = ("style2", ""),
			};
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.Sorting]"),
				Options = _styles.Select(x => new OptionItemUIEntry
				{
					Id = x.Key,
					Name = x.Value.Name,
					Icon = x.Value.Icon,
					Selected = _optionsUISystem.ViewStyle == x.Value.Name
				}).ToArray()
			};
		}

		public bool IsVisible()
		{
			return true;
		}

		public void OnOptionClicked(int optionId)
		{
			_optionsUISystem.ViewStyle = _styles[optionId].Name;
		}
	}
}
