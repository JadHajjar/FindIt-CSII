using FindIt.Domain.Enums;
using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;
using FindIt.Systems;

using System.Collections.Generic;
using System.Linq;

namespace FindIt.Domain.Options.Picker
{
	internal class ObjectFilterOption : IOptionSection
	{
		private readonly OptionsUISystem _optionsUISystem;
		private readonly PickerToolSystem _pickerToolSystem;
		private readonly Dictionary<PickerFlags, string> _styles;

		public int Id { get; } = 14;

		public ObjectFilterOption(OptionsUISystem optionsUISystem)
		{
			_pickerToolSystem = optionsUISystem.World.GetOrCreateSystemManaged<PickerToolSystem>();
			_styles = new()
			{
				[PickerFlags.All] = "coui://uil/Standard/StarAll.svg",
				[PickerFlags.SubObjects] = "coui://findit/subitem.svg",
				[PickerFlags.Buildings] = "coui://findit/buildings.svg",
				[PickerFlags.Props] = "coui://findit/props.svg",
				[PickerFlags.Networks] = "coui://findit/networks.svg",
				[PickerFlags.Surfaces] = "coui://findit/areas.svg",
			};
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Tooltip.LABEL[FindIt.Filters]"),
				Options = _styles.Select(x => new OptionItemUIEntry
				{
					Id = (int)x.Key,
					Name = LocaleHelper.GetTooltip(x.Key == PickerFlags.All ? "Any" : $"Picker{x.Key}"),
					Icon = x.Value,
					Selected = _pickerToolSystem.Flags.HasFlag(x.Key)
				}).ToArray()
			};
		}

		public bool IsVisible()
		{
			return true;
		}

		public void OnOptionClicked(int optionId, int value)
		{
			var flag = (PickerFlags)optionId;

			if (_pickerToolSystem.Flags.HasFlag(flag))
			{
				_pickerToolSystem.Flags &= ~flag;
			}
			else
			{
				_pickerToolSystem.Flags |= flag;
			}
		}

		public void OnReset()
		{
		}

		public bool IsDefault()
		{
			return false;
		}
	}
}
