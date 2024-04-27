using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

using System;
using System.Collections.Generic;
using System.Linq;

namespace FindIt.Domain.Options
{
	internal class ViewStyleOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private readonly PrefabSearchUISystem _prefabSearchUISystem;
		private readonly Dictionary<int, (string Name, string Icon)> _styles;

		public int Id { get; } = 1;

		public ViewStyleOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
			_prefabSearchUISystem = optionsUISystem.World.GetOrCreateSystemManaged<PrefabSearchUISystem>();
			_styles = new()
			{
				[1] = ("GridWithText", "coui://uil/Standard/GridTextViewSmall.svg"),
				[2] = ("GridNoText", "coui://uil/Standard/Image.svg"),
				[3] = ("GridSmall", "coui://uil/Standard/GridView.svg"),
				[4] = ("ListSimple", "coui://uil/Standard/ListViewDense.svg"),
			};
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.ViewStyle]"),
				Options = _styles.Select(x => new OptionItemUIEntry
				{
					Id = x.Key,
					Name = LocaleHelper.GetTooltip($"View{x.Value.Name}"),
					Icon = x.Value.Icon,
					Selected = _prefabSearchUISystem.ViewStyle == x.Value.Name
				}).ToArray()
			};
		}

		public bool IsVisible()
		{
			return true;
		}

		public void OnOptionClicked(int optionId, int value)
		{
			_prefabSearchUISystem.ViewStyle = _styles[optionId].Name;
		}

		public void OnReset()
		{ }
	}
}
