using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

using System;

namespace FindIt.Domain.Options
{
	internal class BuildingLevelOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;

		public int Id { get; } = 12;

		public BuildingLevelOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.BuildingLevel]"),
				Options = new[]
				{
					new OptionItemUIEntry
					{
						IsValue = true,
						Value = FindItUtil.Filters.BuildingLevelFilter switch
						{
							0 => LocaleHelper.GetTooltip("Any"),
							_ => FindItUtil.Filters.BuildingLevelFilter.ToString(),
						}
					}
				}
			};
		}

		public bool IsVisible()
		{
			return FindItUtil.CurrentCategory == Enums.PrefabCategory.Buildings;
		}

		public void OnOptionClicked(int optionId, int value)
		{
			FindItUtil.Filters.BuildingLevelFilter = Math.Max(0, Math.Min(5, FindItUtil.Filters.BuildingLevelFilter + value));

			_optionsUISystem.TriggerSearch();
		}

		public void OnReset()
		{
			FindItUtil.Filters.BuildingLevelFilter = 0;
		}

		public bool IsDefault()
		{
			return FindItUtil.Filters.BuildingLevelFilter == 0;
		}
	}
}
