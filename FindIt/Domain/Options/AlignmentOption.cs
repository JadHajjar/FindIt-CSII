using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

using System.Collections.Generic;
using System.Linq;

namespace FindIt.Domain.Options
{
	internal class AlignmentOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private readonly FindItUISystem _findItUISystem;
		private readonly Dictionary<int, (string Name, string Icon)> _styles;

		public int Id { get; } = -4;

		public AlignmentOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
			_findItUISystem = optionsUISystem.World.GetOrCreateSystemManaged<FindItUISystem>();
			_styles = new()
			{
				[1] = ("Center", "coui://uil/Standard/AlignCenter.svg"),
				[2] = ("Right", "coui://uil/Standard/AlignRight.svg"),
			};
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.AlignmentStyle]"),
				Options = _styles.Select(x => new OptionItemUIEntry
				{
					Id = x.Key,
					Name = LocaleHelper.GetTooltip($"Alignment{x.Value.Name}"),
					Icon = x.Value.Icon,
					Selected = _findItUISystem.AlignmentStyle == x.Value.Name
				}).ToArray()
			};
		}

		public bool IsVisible()
		{
			return true;
		}

		public void OnOptionClicked(int optionId, int value)
		{
			_findItUISystem.AlignmentStyle = _styles[optionId].Name;
		}

		public void OnReset()
		{ }

		public bool IsDefault()
		{
			return true;
		}
	}
}
