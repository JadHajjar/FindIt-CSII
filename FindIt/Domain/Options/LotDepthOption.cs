using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

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
			return FindItUtil.CurrentCategory == Enums.PrefabCategory.Buildings;
		}

		public void OnOptionClicked(int optionId, int value)
		{
			FindItUtil.Filters.LotDepthFilter = Math.Max(0, Math.Min(10, FindItUtil.Filters.LotDepthFilter + value));

			_optionsUISystem.World.GetOrCreateSystemManaged<PrefabSearchUISystem>().TriggerSearch();
		}

		public void OnReset()
		{
			if (FindItUtil.Filters.LotDepthFilter == 0)
			{
				return;
			}

			FindItUtil.Filters.LotDepthFilter = 0;

			_optionsUISystem.World.GetOrCreateSystemManaged<PrefabSearchUISystem>().TriggerSearch();
		}
	}
}
