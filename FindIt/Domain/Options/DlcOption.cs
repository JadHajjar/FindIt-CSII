using Colossal.PSI.Common;

using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

using System.Collections.Generic;
using System.Linq;

namespace FindIt.Domain.Options
{
	internal class DlcOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private readonly List<OptionItemUIEntry> _dlcs;

		public int Id { get; } = 100;

		public DlcOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
			_dlcs = new List<OptionItemUIEntry>
			{
				new()
				{
					Id = int.MinValue,
					Name = LocaleHelper.GetTooltip("Any"),
					Icon = "coui://uil/Standard/StarAll.svg",
				},
				new()
				{
					Id = -1,
					Name = LocaleHelper.GetTooltip("BaseGame"),
					Icon = "coui://uil/Colored/BaseGame.svg",
				}
			};

			foreach (var item in FindItUtil.CategorizedPrefabs[Enums.PrefabCategory.Any][Enums.PrefabSubCategory.Any])
			{
				if (item.DlcId.id >= 0 && !_dlcs.Any(x => x.Id == item.DlcId.id))
				{
					_dlcs.Add(new OptionItemUIEntry
					{
						Id = item.DlcId.id,
						Name = LocaleHelper.Translate($"Common.DLC_TITLE[{PlatformManager.instance.GetDlcName(item.DlcId)}]"),
						Icon = $"Media/DLC/{PlatformManager.instance.GetDlcName(item.DlcId)}.svg",
					});
				}
			}
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			for (var i = 0; i < _dlcs.Count; i++)
			{
				var dlc = _dlcs[i];

				dlc.Selected = FindItUtil.Filters.SelectedDlc == dlc.Id;

				_dlcs[i] = dlc;
			}

			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.DlcFilter]"),
				Options = _dlcs.ToArray()
			};
		}

		public bool IsVisible()
		{
			return _dlcs.Count > 2;
		}

		public void OnOptionClicked(int optionId, int value)
		{
			if (FindItUtil.Filters.SelectedDlc == optionId)
			{
				FindItUtil.Filters.SelectedDlc = int.MinValue;
			}
			else
			{
				FindItUtil.Filters.SelectedDlc = optionId;
			}

			_optionsUISystem.World.GetOrCreateSystemManaged<FindItUISystem>().TriggerSearch();
		}

		public void OnReset()
		{
			if (FindItUtil.Filters.SelectedDlc == int.MinValue)
			{
				return;
			}

			FindItUtil.Filters.SelectedDlc = int.MinValue;

			_optionsUISystem.World.GetOrCreateSystemManaged<FindItUISystem>().TriggerSearch();
		}
	}
}
