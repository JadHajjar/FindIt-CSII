using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

namespace FindIt.Domain.Options
{
	internal class LotWidthOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private readonly OptionItemUIEntry[] _options;

		public int Id { get; } = 10;

		public LotWidthOption(OptionsUISystem optionsUISystem)
		{
			_optionsUISystem = optionsUISystem;
			_options = new OptionItemUIEntry[8];

			for (var i = 0; i < _options.Length; i++)
			{
				_options[i] = new OptionItemUIEntry
				{
					Id = i,
					Name = i.ToString(),
					Icon = i is 0 ? "Media/Tools/Snap Options/All.svg" : i.ToString(),
				};
			}
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			for (var i = 0; i < _options.Length; i++)
			{
				var option = _options[i];

				option.Selected = FindItUtil.LotWidthFilter == i;

				_options[i] = option;
			}

			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.LotWidth]"),
				Options = _options
			};
		}

		public bool IsVisible()
		{
			return true;
		}

		public void OnOptionClicked(int optionId)
		{
			FindItUtil.LotWidthFilter = optionId;

			_optionsUISystem.World.GetOrCreateSystemManaged<PrefabSearchUISystem>().TriggerSearch();
		}
	}
}
