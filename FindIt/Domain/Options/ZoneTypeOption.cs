﻿using FindIt.Domain.Enums;
using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Systems;
using FindIt.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace FindIt.Domain.Options
{
    internal class ZoneTypeOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private readonly Dictionary<ZoneTypeFilter, string> _styles;

		public int Id { get; } = 15;

		public ZoneTypeOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
			_styles = new()
			{
				[ZoneTypeFilter.Any] = "coui://uil/Standard/StarAll.svg",
				[ZoneTypeFilter.Low] = "coui://uil/Standard/LowLevel.svg",
				[ZoneTypeFilter.Row] = "coui://uil/Standard/Row.svg",
				[ZoneTypeFilter.Medium] = "coui://uil/Standard/MediumLevel.svg",
				[ZoneTypeFilter.High] = "coui://uil/Standard/HighLevel.svg",
				[ZoneTypeFilter.Signature] = "coui://findit/signature.svg",
			};
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.ZoneType]"),
				Options = _styles.Select(x => new OptionItemUIEntry
				{
					Id = (int)x.Key,
					Name = LocaleHelper.GetTooltip(x.Key == ZoneTypeFilter.Any ? "Any" : $"Zone{x.Key}"),
					Icon = x.Value,
					Selected = FindItUtil.Filters.SelectedZoneType == x.Key
				}).ToArray()
			};
		}

		public bool IsVisible()
		{
			return FindItUtil.CurrentCategory is PrefabCategory.Buildings
				&& (FindItUtil.CurrentSubCategory is PrefabSubCategory.Any or PrefabSubCategory.Buildings_Residential or PrefabSubCategory.Buildings_Mixed or PrefabSubCategory.Buildings_Commercial or PrefabSubCategory.Buildings_Office or PrefabSubCategory.Buildings_Industrial);
		}

		public void OnOptionClicked(int optionId, int value)
		{
			FindItUtil.Filters.SelectedZoneType = (ZoneTypeFilter)optionId;

			_optionsUISystem.TriggerSearch();
		}

		public void OnReset()
		{
			FindItUtil.Filters.SelectedZoneType = ZoneTypeFilter.Any;
		}

		public bool IsDefault()
		{
			return FindItUtil.Filters.SelectedZoneType == ZoneTypeFilter.Any;
		}
	}
}
