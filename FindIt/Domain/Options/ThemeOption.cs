using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

using Game.Prefabs;
using Game.UI;

using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;

namespace FindIt.Domain.Options
{
	internal class ThemeOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private readonly List<ThemePrefab> _themeList;

		public int Id { get; } = 95;

		public ThemeOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
			_themeList = GetThemePrefabs();
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			var themes = new List<OptionItemUIEntry>
			{
				new()
				{
					Id = -2,
					Name = LocaleHelper.GetTooltip("Any"),
					Icon = "coui://uil/Standard/StarAll.svg",
					Selected = !FindItUtil.Filters.SelectedThemeNone&&FindItUtil.Filters.SelectedTheme == null
				},
				new()
				{
					Id = -1,
					Name = LocaleHelper.GetTooltip("No Theme"),
					Icon = "coui://ui-mods/images/globe.svg",
					Selected = FindItUtil.Filters.SelectedThemeNone
				},
			};

			for (var i = 0; i < _themeList.Count; i++)
			{
				themes.Add(new OptionItemUIEntry
				{
					Id = i,
					Name = _optionsUISystem.GetAssetName(_themeList[i]),
					Icon = ImageSystem.GetThumbnail(_themeList[i]),
					Selected = !FindItUtil.Filters.SelectedThemeNone && FindItUtil.Filters.SelectedTheme == _themeList[i]
				});
			}

			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.ThemeFilter]"),
				Options = themes.ToArray()
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
				case -2:
					FindItUtil.Filters.SelectedThemeNone = false;
					FindItUtil.Filters.SelectedTheme = null;
					break;
				case -1:
					FindItUtil.Filters.SelectedThemeNone = true;
					FindItUtil.Filters.SelectedTheme = null;
					break;
				default:
					FindItUtil.Filters.SelectedThemeNone = false;
					FindItUtil.Filters.SelectedTheme = _themeList[optionId];
					break;
			}

			_optionsUISystem.TriggerSearch();
		}

		public void OnReset()
		{
			FindItUtil.Filters.SelectedTheme = null;
			FindItUtil.Filters.SelectedThemeNone = false;
		}

		public bool IsDefault()
		{
			return FindItUtil.Filters.SelectedTheme == null && !FindItUtil.Filters.SelectedThemeNone;
		}

		private List<ThemePrefab> GetThemePrefabs()
		{
			var query = _optionsUISystem.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp).WithAll<ThemeData>());
			var entities = query.ToEntityArray(Allocator.Temp);
			var prefabSystem = _optionsUISystem.World.GetOrCreateSystemManaged<PrefabSystem>();
			var list = new List<ThemePrefab>();

			for (var i = 0; i < entities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<ThemePrefab>(entities[i], out var prefab))
				{
					list.Add(prefab);
				}
			}

			return list;
		}
	}
}
