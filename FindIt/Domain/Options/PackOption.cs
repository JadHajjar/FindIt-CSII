﻿using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Systems;
using FindIt.Utilities;
using Game.Prefabs;
using Game.UI;

using System.Collections.Generic;

using UnityEngine;

namespace FindIt.Domain.Options
{
    internal class PackOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private List<AssetPackPrefab> _packList = new();

		public int Id { get; } = 94;

		public PackOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;

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
					Name = i == 0 ? LocaleHelper.GetTooltip("NoPack") : _optionsUISystem.GetAssetName(_packList[i]),
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
			_packList = GetPackPrefabs();

			return _packList.Count > 1;
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
			var list = new List<AssetPackPrefab>();

			var none = ScriptableObject.CreateInstance<AssetPackPrefab>();
			var uiObject = ScriptableObject.CreateInstance<UIObject>();
			none.name = "FindIt_NoPack";
			none.AddComponent<UIObject>().m_Icon = "coui://findit/noPack.svg";

			list.Add(none);

			foreach (var item in FindItUtil.GetUnfilteredPrefabs())
			{
				foreach (var pack in item.AssetPacks)
				{
					if (!list.Contains(pack))
					{
						list.Add(pack);
					}
				}
			}

			list.Sort((x, y) => Comparer<int>.Default.Compare(GetOrder(x), GetOrder(y)));

			return list;
		}

		private int GetOrder(AssetPackPrefab pack)
		{
			if (pack.name == "FindIt_NoPack")
			{
				return 0;
			}

			if (pack.Has<DlcRequirement>())
			{
				return 1;
			}

			return 2;
		}
	}
}
