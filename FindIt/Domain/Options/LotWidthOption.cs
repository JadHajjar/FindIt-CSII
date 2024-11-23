using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

using System;

namespace FindIt.Domain.Options
{
	internal class LotWidthOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;

		public int Id { get; } = 10;

		public LotWidthOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.LotWidth]"),
				Options = new[]
				{
					new OptionItemUIEntry
					{
						IsValue = true,
						Value = FindItUtil.Filters.LotWidthFilter switch
						{
							0 => LocaleHelper.GetTooltip("Any"),
							10 => "10+",
							_ => FindItUtil.Filters.LotWidthFilter.ToString(),
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
			FindItUtil.Filters.LotWidthFilter = Math.Max(0, Math.Min(10, FindItUtil.Filters.LotWidthFilter + value));

			_optionsUISystem.TriggerSearch();
		}

		public void OnReset()
		{
			FindItUtil.Filters.LotWidthFilter = 0;
		}

		public bool IsDefault()
		{
			return FindItUtil.Filters.LotWidthFilter == 0;
		}
	}
}
