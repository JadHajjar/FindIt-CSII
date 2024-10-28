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
	internal class PackOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private readonly List<AssetPackPrefab> _themeList;

		public int Id { get; } = 94;

		public PackOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
			_themeList = GetPackPrefabs();
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
					Selected = FindItUtil.Filters.SelectedAssetPack == null
				},
			};

			for (var i = 0; i < _themeList.Count; i++)
			{
				themes.Add(new OptionItemUIEntry
				{
					Id = i,
					Name = _optionsUISystem.GetAssetName(_themeList[i]),
					Icon = ImageSystem.GetThumbnail(_themeList[i]),
					Selected = FindItUtil.Filters.SelectedAssetPack == _themeList[i]
				});
			}

			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.AssetPackFilter]"),
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
					FindItUtil.Filters.SelectedAssetPack = null;
					break;
				default:
					FindItUtil.Filters.SelectedAssetPack = _themeList[optionId];
					break;
			}

			_optionsUISystem.TriggerSearch();
		}

		public void OnReset()
		{
			FindItUtil.Filters.SelectedAssetPack = null;

			_optionsUISystem.TriggerSearch();
		}

		private List<AssetPackPrefab> GetPackPrefabs()
		{
			var query = _optionsUISystem.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp).WithAll<AssetPackData>());
			var entities = query.ToEntityArray(Allocator.Temp);
			var prefabSystem = _optionsUISystem.World.GetOrCreateSystemManaged<PrefabSystem>();
			var list = new List<AssetPackPrefab>();

			for (var i = 0; i < entities.Length; i++)
			{
				if (prefabSystem.TryGetPrefab<AssetPackPrefab>(entities[i], out var prefab))
				{
					list.Add(prefab);
				}
			}

			return list;
		}
	}
}
