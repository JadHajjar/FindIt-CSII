using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

using Game.Prefabs;

using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;

namespace FindIt.Domain.Options
{
	internal class RegionOption// : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private readonly List<ThemePrefab> _themeList;

		public int Id { get; } = 9;

		public RegionOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
			_themeList = GetThemePrefabs();
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			var themes = new List<OptionItemUIEntry>
			{
				new() {
					Id = -1,
					Name = "Any",
					Icon = "coui://uil/Standard/StarAll.svg",
					Selected = FindItUtil.Filters.SelectedTheme == null
				},
			};

			for (var i = 0; i < _themeList.Count; i++)
			{
				themes.Add(new OptionItemUIEntry
				{
					Id = i,
					Name = _themeList[i].name,
					Icon = _themeList[i].thumbnailUrl,
					Selected = FindItUtil.Filters.SelectedTheme == _themeList[i]
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

		public void OnOptionClicked(int optionId)
		{
			FindItUtil.Filters.SelectedTheme = optionId == -1 ? null : _themeList[optionId];

			_optionsUISystem.World.GetOrCreateSystemManaged<FindItUISystem>().TriggerSearch();
		}

		public void OnReset()
		{
			FindItUtil.Filters.SelectedTheme = null;

			_optionsUISystem.World.GetOrCreateSystemManaged<FindItUISystem>().TriggerSearch();
		}

		private List<ThemePrefab> GetThemePrefabs()
		{
			var query = _optionsUISystem.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp).WithAll<ThemeData>());
			var entities = query.ToEntityArray(Allocator.Temp);
			var prefabSystem = _optionsUISystem.World.GetOrCreateSystemManaged<PrefabSystem>();
			var list = new List<ThemePrefab>();

			Mod.Log.Info("themes:" + entities.Length);

			for (var i = 0; i < entities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<ThemePrefab>(entities[i], out var prefab))
				{
					list.Add(prefab);
				}
			}

			Mod.Log.Info("list:" + list.Count);

			return list;
		}
	}
}
