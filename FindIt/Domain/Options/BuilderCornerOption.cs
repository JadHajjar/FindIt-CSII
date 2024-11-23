using FindIt.Domain.Enums;
using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

using System.Collections.Generic;
using System.Linq;

namespace FindIt.Domain.Options
{
	internal class BuilderCornerOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private readonly Dictionary<BuildingCornerFilter, string> _styles;

		public int Id { get; } = 14;

		public BuilderCornerOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
			_styles = new()
			{
				[BuildingCornerFilter.Any] = "coui://uil/Standard/StarAll.svg",
				[BuildingCornerFilter.Left] = "coui://uil/Standard/BuildingCornerLeft.svg",
				[BuildingCornerFilter.Front] = "coui://uil/Standard/BuildingFront.svg",
				[BuildingCornerFilter.Right] = "coui://uil/Standard/BuildingCornerRight.svg",
			};
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.BuilderCorner]"),
				Options = _styles.Select(x => new OptionItemUIEntry
				{
					Id = (int)x.Key,
					Name = LocaleHelper.GetTooltip(x.Key == BuildingCornerFilter.Any ? "Any" : $"Corner{x.Key}"),
					Icon = x.Value,
					Selected = FindItUtil.Filters.SelectedBuildingCorner == x.Key
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
			FindItUtil.Filters.SelectedBuildingCorner = (BuildingCornerFilter)optionId;

			_optionsUISystem.TriggerSearch();
		}

		public void OnReset()
		{
			FindItUtil.Filters.SelectedBuildingCorner = BuildingCornerFilter.Any;
		}

		public bool IsDefault()
		{
			return FindItUtil.Filters.SelectedBuildingCorner == BuildingCornerFilter.Any;
		}
	}
}
