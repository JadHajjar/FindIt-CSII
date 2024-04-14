using FindIt.Domain.Enums;
using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

using System.Collections.Generic;

namespace FindIt.Domain.Options
{
	internal class ExtraFiltersOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private readonly Dictionary<ZoneTypeFilter, string> _styles;

		public int Id { get; } = 99;

		public ExtraFiltersOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.ExtraFilters]"),
				Options = new[]{ new OptionItemUIEntry
				{
					Id = 0,
					Name = "Remove Ads",
					Icon = "coui://uil/Standard/NoAds.svg",
					Selected = FindItUtil.Filters.HideAds
				} }
			};
		}

		public bool IsVisible()
		{
			return true;
		}

		public void OnOptionClicked(int optionId, int value)
		{
			switch (optionId)
			{
				case 0:
					FindItUtil.Filters.HideAds = !FindItUtil.Filters.HideAds;
					break;
				default:
					return;
			}

			_optionsUISystem.World.GetOrCreateSystemManaged<PrefabSearchUISystem>().TriggerSearch();
		}

		public void OnReset()
		{
			if (!FindItUtil.Filters.HideAds)
			{
				return;
			}

			FindItUtil.Filters.HideAds = false;

			_optionsUISystem.World.GetOrCreateSystemManaged<PrefabSearchUISystem>().TriggerSearch();
		}
	}
}
