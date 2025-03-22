using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Systems;
using FindIt.Utilities;
using System;

namespace FindIt.Domain.Options
{
    internal class LotDepthOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;

		public int Id { get; } = 11;

		public LotDepthOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			return new OptionSectionUIEntry
			{
				Id = Id,
				ValueSign = FindItUtil.Filters.LotDepthFilter == 0 ? default : FindItUtil.Filters.LotDepthSign,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.LotDepth]"),
				Options = new[]
				{
					new OptionItemUIEntry
					{
						IsValue = true,
						Value = FindItUtil.Filters.LotDepthFilter switch
						{
							0 => LocaleHelper.GetTooltip("Any"),
							10 => "10+",
							_ => FindItUtil.Filters.LotDepthFilter.ToString(),
						}
					}
				}
			};
		}

		public bool IsVisible()
		{
			return FindItUtil.CurrentCategory is Domain.Enums.PrefabCategory.Buildings or Domain.Enums.PrefabCategory.ServiceBuildings;
		}

		public void OnOptionClicked(int optionId, int value)
		{
			if (value == 0)
			{
				var newSign = (Enums.ValueSign)((int)FindItUtil.Filters.LotDepthSign - 1);

				FindItUtil.Filters.LotDepthSign = newSign is Enums.ValueSign.None ? Enums.ValueSign.Equal : newSign;
			}
			else
			{
				FindItUtil.Filters.LotDepthFilter = Math.Max(0, Math.Min(10, FindItUtil.Filters.LotDepthFilter + value));
			}

			_optionsUISystem.TriggerSearch();
		}

		public void OnReset()
		{
			FindItUtil.Filters.LotDepthFilter = 0;
		}

		public bool IsDefault()
		{
			return FindItUtil.Filters.LotDepthFilter == 0;
		}
	}
}
