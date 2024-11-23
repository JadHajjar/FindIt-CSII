using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

namespace FindIt.Domain.Options
{
	internal class SortingDirectionOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;

		public int Id { get; } = -1;

		public SortingDirectionOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.SortOrder]"),
				Options = new[]
				{
					new OptionItemUIEntry
					{
						Id = 0,
						Name = LocaleHelper.GetTooltip("Ascending"),
						Icon = "coui://uil/Standard/ArrowSortLowDown.svg",
						Selected = !IndexedPrefabList.SortingDescending
					},
					new OptionItemUIEntry
					{
						Id = 1,
						Name = LocaleHelper.GetTooltip("Descending"),
						Icon = "coui://uil/Standard/ArrowSortHighDown.svg",
						Selected = IndexedPrefabList.SortingDescending
					},
				}
			};
		}

		public bool IsVisible()
		{
			return true;
		}

		public void OnOptionClicked(int optionId, int value)
		{
			FindItUtil.SetSorting(optionId != 0);

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
