using FindIt.Domain.Enums;
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
		private readonly Dictionary<PrefabSorting, string> _sortOptions;

		public int Id { get; } = 3;

		public SortingOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
			_sortOptions = new()
			{
				[PrefabSorting.Name] = "coui://uil/Standard/NameSort.svg",
				[PrefabSorting.MostUsed] = "coui://uil/Standard/Statistics.svg",
				[PrefabSorting.LastUsed] = "coui://uil/Standard/ClockArrowBackward.svg",
			};
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.Sorting]"),
				Options = _sortOptions.Select(x => new OptionItemUIEntry
				{
					Id = (int)x.Key,
					Name = LocaleHelper.GetTooltip($"Sorting{x.Key}"),
					Icon = x.Value,
					Selected = IndexedPrefabList.Sorting == x.Key
				}).ToArray()
			};
		}

		public bool IsVisible()
		{
			return true;
		}

		public void OnOptionClicked(int optionId, int value)
		{
			FindItUtil.SetSorting(sorting: (PrefabSorting)optionId);

			_optionsUISystem.UpdateCategoriesAndPrefabList();
		}

		public void OnReset()
		{ }

		public bool IsDefault()
		{
			return true;
		}
	}
}
