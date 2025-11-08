using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Systems;
using FindIt.Utilities;

namespace FindIt.Domain.Options
{
    internal class ExtraFiltersOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;

		public int Id { get; } = 90;

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
				Options = new[]
				{
					new OptionItemUIEntry
					{
						Id = 5,
						Name = LocaleHelper.GetTooltip("WithParking"),
						Icon = "coui://findit/YesParking.svg",
						Selected = FindItUtil.Filters.WithParking,
						Hidden = !(FindItUtil.CurrentCategory is Domain.Enums.PrefabCategory.Buildings or Domain.Enums.PrefabCategory.ServiceBuildings)
					},
					new OptionItemUIEntry
					{
						Id = 6,
						Name = LocaleHelper.GetTooltip("WithoutParking"),
						Icon = "coui://findit/NoParking.svg",
						Selected = FindItUtil.Filters.WithoutParking,
						Hidden = !(FindItUtil.CurrentCategory is Domain.Enums.PrefabCategory.Buildings or Domain.Enums.PrefabCategory.ServiceBuildings)
					},
					new OptionItemUIEntry
					{
						Id = 0,
						Name = LocaleHelper.GetTooltip("RemoveAds"),
						Icon = "coui://uil/Standard/NoAds.svg",
						Selected = FindItUtil.Filters.HideAds,
						Hidden = Mod.Settings.HideBrandsFromAny
					},
					new OptionItemUIEntry
					{
						Id = 1,
						Name = LocaleHelper.GetTooltip("RemoveRandoms"),
						Icon = "coui://uil/Standard/NoDice.svg",
						Selected = FindItUtil.Filters.HideRandoms,
						Hidden = Mod.Settings.HideRandomAssets
					},
					new OptionItemUIEntry
					{
						Id = 2,
						Name = LocaleHelper.GetTooltip("CustomAssets"),
						Icon = "coui://uil/Standard/PDXPlatypusHexagon.svg",
						Selected = FindItUtil.Filters.HideVanilla
					},
					new OptionItemUIEntry
					{
						Id = 3,
						Name = LocaleHelper.GetTooltip("OnlyPlaced"),
						Icon = "coui://uil/Standard/MapMarker.svg",
						Selected = FindItUtil.Filters.OnlyPlaced
					},
					new OptionItemUIEntry
					{
						Id = 4,
						Name = LocaleHelper.GetTooltip("UniqueBuildingMeshes"),
						Icon = "coui://findit/city.svg",
						Selected = FindItUtil.Filters.UniqueMesh
					}
				}
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
				case 1:
					FindItUtil.Filters.HideRandoms = !FindItUtil.Filters.HideRandoms;
					break;
				case 2:
					FindItUtil.Filters.HideVanilla = !FindItUtil.Filters.HideVanilla;
					break;
				case 3:
					FindItUtil.Filters.OnlyPlaced = !FindItUtil.Filters.OnlyPlaced;
					break;
				case 4:
					FindItUtil.Filters.UniqueMesh = !FindItUtil.Filters.UniqueMesh;
					break;
				case 5:
					FindItUtil.Filters.WithParking = !FindItUtil.Filters.WithParking;
					FindItUtil.Filters.WithoutParking = false;
					break;
				case 6:
					FindItUtil.Filters.WithoutParking = !FindItUtil.Filters.WithoutParking;
					FindItUtil.Filters.WithParking = false;
					break;
				default:
					return;
			}

			_optionsUISystem.TriggerSearch();
		}

		public void OnReset()
		{
			FindItUtil.Filters.HideAds = false;
			FindItUtil.Filters.HideVanilla = false;
			FindItUtil.Filters.HideRandoms = false;
			FindItUtil.Filters.OnlyPlaced = false;
			FindItUtil.Filters.UniqueMesh = false;
		}

		public bool IsDefault()
		{
			return !FindItUtil.Filters.HideAds
				&& !FindItUtil.Filters.HideRandoms
				&& !FindItUtil.Filters.HideVanilla
				&& !FindItUtil.Filters.OnlyPlaced
				&& !FindItUtil.Filters.UniqueMesh;
		}
	}
}
