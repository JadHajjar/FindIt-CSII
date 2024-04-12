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

		public int Id { get; } = 100;

		public DlcOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			var dlcs = new List<OptionItemUIEntry>();

			foreach (var item in FindItUtil.GetFilteredPrefabs())
			{
				if (!dlcs.Any(x => x.Id == item.DlcId.id))
				{
					dlcs.Add(new OptionItemUIEntry
					{
						Id = item.DlcId.id,
						Name = PlatformManager.instance.GetDlcName(item.DlcId),
						Icon = $"Media/DLC/{PlatformManager.instance.GetDlcName(item.DlcId)}.svg",
						Selected = FindItUtil.SelectedDlc == item.DlcId.id
					});
				}
			}

			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.DlcFilter]"),
				Options = dlcs.ToArray()
			};
		}

		public bool IsVisible()
		{
			return FindItUtil.GetFilteredPrefabs().HasMoreThanOne(x => x.DlcId);
		}

		public void OnOptionClicked(int optionId)
		{
			FindItUtil.SelectedDlc = optionId;

			_optionsUISystem.World.GetOrCreateSystemManaged<PrefabSearchUISystem>().TriggerSearch();
		}
	}
}
