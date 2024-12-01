using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

using Game.Prefabs;
using Game.UI;

using System.Collections.Generic;

using Unity.Collections;
using Unity.Entities;

using UnityEngine;

namespace FindIt.Domain.Options
{
	internal class PackOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private readonly List<AssetPackPrefab> _packList;

		public int Id { get; } = 94;

		public PackOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
			_packList = GetPackPrefabs();

			FindItUtil.Filters.SelectedAssetPacks = new(_packList, null, true);
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			var packs = new List<OptionItemUIEntry>
			{
				new()
				{
					Id = int.MinValue,
					Name = LocaleHelper.GetTooltip("Any"),
					Icon = "coui://uil/Standard/StarAll.svg",
					Selected = FindItUtil.Filters.SelectedAssetPacks .IsDefault()
				},
			};

			for (var i = 0; i < _packList.Count; i++)
			{
				packs.Add(new OptionItemUIEntry
				{
					Id = i,
					Name = i ==0 ?LocaleHelper.GetTooltip("NoPack") : _optionsUISystem.GetAssetName(_packList[i]),
					Icon = ImageSystem.GetThumbnail(_packList[i]),
					Selected = FindItUtil.Filters.SelectedAssetPacks.IsSelected(_packList[i])
				});
			}

			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.AssetPackFilter]"),
				Options = packs.ToArray()
			};
		}

		public bool IsVisible()
		{
			return true;
		}

		public void OnOptionClicked(int optionId, int value)
		{
			FindItUtil.Filters.SelectedAssetPacks.Toggle(optionId < 0 ? null : _packList[optionId]);

			_optionsUISystem.TriggerSearch();
		}

		public void OnReset()
		{
			FindItUtil.Filters.SelectedAssetPacks.Reset();
		}

		public bool IsDefault()
		{
			return FindItUtil.Filters.SelectedAssetPacks.IsDefault();
		}

		private List<AssetPackPrefab> GetPackPrefabs()
		{
			var query = _optionsUISystem.GetEntityQuery(new EntityQueryBuilder(Allocator.Temp).WithAll<AssetPackData>());
			var entities = query.ToEntityArray(Allocator.Temp);
			var prefabSystem = _optionsUISystem.World.GetOrCreateSystemManaged<PrefabSystem>();
			var list = new List<AssetPackPrefab>();

			var none = ScriptableObject.CreateInstance<AssetPackPrefab>();
			var uiObject = ScriptableObject.CreateInstance<UIObject>();
			none.name = "FindIt_NoPack";
			none.AddComponent<UIObject>().m_Icon = "";

			list.Add(none);

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
