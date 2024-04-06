using Colossal;
using Colossal.IO.AssetDatabase;

using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;

using System.Collections.Generic;

namespace FindIt
{
	[FileLocation(nameof(FindIt))]
	[SettingsUIGroupOrder(kButtonGroup, kToggleGroup, kSliderGroup, kDropdownGroup)]
	[SettingsUIShowGroupName(kButtonGroup, kToggleGroup, kSliderGroup, kDropdownGroup)]
	public class FindItSetting : ModSetting
	{
		public const string kSection = "Main";

		public const string kButtonGroup = "Button";
		public const string kToggleGroup = "Toggle";
		public const string kSliderGroup = "Slider";
		public const string kDropdownGroup = "Dropdown";

		public FindItSetting(IMod mod) : base(mod)
		{

		}

		[SettingsUIHidden]
		public bool DefaultBlock { get; set; }

		//[SettingsUISection(kSection, kButtonGroup)]
		//public bool Button { set { Mod.Log.Info("Button clicked"); } }

		//[SettingsUIButton]
		//[SettingsUIConfirmation]
		//[SettingsUISection(kSection, kButtonGroup)]
		//public bool ButtonWithConfirmation { set { Mod.Log.Info("ButtonWithConfirmation clicked"); } }

		[SettingsUISection(kSection, kToggleGroup)]
		public bool OpenPanelOnPicker { get; set; } = true;

		[SettingsUISection(kSection, kToggleGroup)]
		public bool SelectPrefabOnOpen { get; set; } = true;

		[SettingsUISlider(min = 0.2f, max = 2f, step = 0.1f, scalarMultiplier = 1f, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(kSection, kToggleGroup)]
		public float ScrollSpeed { get; set; } = 0.6f;

		[SettingsUISlider(min = 1f, max = 6f, step = 0.25f, scalarMultiplier = 1f, unit = Unit.kFloatTwoFractions)]
		[SettingsUISection(kSection, kToggleGroup)]
		public float RowCount { get; set; } = 2f;

		[SettingsUISlider(min = 4f, max = 20f, step = 1f, scalarMultiplier = 1f)]
		[SettingsUISection(kSection, kToggleGroup)]
		public int ColumnCount { get; set; } = 8;

		//[SettingsUIDropdown(typeof(Setting), nameof(GetIntDropdownItems))]
		//[SettingsUISection(kSection, kDropdownGroup)]
		//public int IntDropdown { get; set; }

		//[SettingsUISection(kSection, kDropdownGroup)]
		//public SomeEnum EnumDropdown { get; set; } = SomeEnum.Value1;

		//public DropdownItem<int>[] GetIntDropdownItems()
		//{
		//	var items = new List<DropdownItem<int>>();

		//	for (var i = 0; i < 3; i += 1)
		//	{
		//		items.Add(new DropdownItem<int>()
		//		{
		//			value = i,
		//			displayName = i.ToString(),
		//		});
		//	}

		//	return items.ToArray();
		//}

		public override void SetDefaults()
		{

		}

		//public enum SomeEnum
		//{
		//	Value1,
		//	Value2,
		//	Value3,
		//}
	}

	public class LocaleEN : IDictionarySource
	{
		private readonly FindItSetting _setting;

		public LocaleEN(FindItSetting setting)
		{
			_setting = setting;
		}

		public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
		{
			return new Dictionary<string, string>
			{
				{ _setting.GetSettingsLocaleID(), "Find It" },
				{ _setting.GetOptionTabLocaleID(FindItSetting.kSection), "General Settings" },

				//{ _setting.GetOptionGroupLocaleID(FindItSetting.kButtonGroup), "Buttons" },
				{ _setting.GetOptionGroupLocaleID(FindItSetting.kToggleGroup), "Behavior" },
				//{ _setting.GetOptionGroupLocaleID(FindItSetting.kSliderGroup), "Sliders" },
				//{ _setting.GetOptionGroupLocaleID(FindItSetting.kDropdownGroup), "Dropdowns" },

				//{ _setting.GetOptionLabelLocaleID(nameof(FindItSetting.Button)), "Button" },
				//{ _setting.GetOptionDescLocaleID(nameof(FindItSetting.Button)), $"Simple single button. It should be bool property with only setter or use [{nameof(SettingsUIButtonAttribute)}] to make button from bool property with setter and getter" },

				//{ _setting.GetOptionLabelLocaleID(nameof(FindItSetting.ButtonWithConfirmation)), "Button with confirmation" },
				//{ _setting.GetOptionDescLocaleID(nameof(FindItSetting.ButtonWithConfirmation)), $"Button can show confirmation message. Use [{nameof(SettingsUIConfirmationAttribute)}]" },
				//{ _setting.GetOptionWarningLocaleID(nameof(FindItSetting.ButtonWithConfirmation)), "is it confirmation text which you want to show here?" },

				//{ _setting.GetOptionLabelLocaleID(nameof(FindItSetting.AutoFocusOnOpen)), "Auto-focus the search bar on open" },
				//{ _setting.GetOptionDescLocaleID(nameof(FindItSetting.AutoFocusOnOpen)), $"Automatically focuses the search bar when opening the Find It panel." },

				//{ _setting.GetOptionLabelLocaleID(nameof(FindItSetting.AutoFocusOnCategory)), "Auto-focus the search bar when changing categories" },
				//{ _setting.GetOptionDescLocaleID(nameof(FindItSetting.AutoFocusOnCategory)), $"Automatically focuses the search bar when you open a different category." },
				
				{ _setting.GetOptionLabelLocaleID(nameof(FindItSetting.OpenPanelOnPicker)), "Open Find It's panel after picking an object" },
				{ _setting.GetOptionDescLocaleID(nameof(FindItSetting.OpenPanelOnPicker)), $"Choose between opening the Find It panel after selecting an object with Picker, or the vanilla panel if available." },

				{ _setting.GetOptionLabelLocaleID(nameof(FindItSetting.SelectPrefabOnOpen)), "Select the first asset when opening Find It" },
				{ _setting.GetOptionDescLocaleID(nameof(FindItSetting.SelectPrefabOnOpen)), $"Choose between opening the Find It panel after selecting an object with Picker, or the vanilla panel if available." },

				{ _setting.GetOptionLabelLocaleID(nameof(FindItSetting.ScrollSpeed)), "Scroll Speed" },
				{ _setting.GetOptionDescLocaleID(nameof(FindItSetting.ScrollSpeed)), $"Represents a multiplier of the size of one row of items." },

				{ _setting.GetOptionLabelLocaleID(nameof(FindItSetting.RowCount)), "Row Count" },
				{ _setting.GetOptionDescLocaleID(nameof(FindItSetting.RowCount)), $"Customize how many rows are displayed in the Find It panel." },

				{ _setting.GetOptionLabelLocaleID(nameof(FindItSetting.ColumnCount)), "Column Count" },
				{ _setting.GetOptionDescLocaleID(nameof(FindItSetting.ColumnCount)), $"Customize how many columns are displayed in the Find It panel." },

				//{ _setting.GetOptionLabelLocaleID(nameof(FindItSetting.IntSlider)), "Int slider" },
				//{ _setting.GetOptionDescLocaleID(nameof(FindItSetting.IntSlider)), $"Use int property with getter and setter and [{nameof(SettingsUISliderAttribute)}] to get int slider" },

				//{ _setting.GetOptionLabelLocaleID(nameof(FindItSetting.IntDropdown)), "Int dropdown" },
				//{ _setting.GetOptionDescLocaleID(nameof(FindItSetting.IntDropdown)), $"Use int property with getter and setter and [{nameof(SettingsUIDropdownAttribute)}(typeof(SomeType), nameof(SomeMethod))] to get int dropdown: Method must be static or instance of your FindItSetting class with 0 parameters and returns {typeof(DropdownItem<int>).Name}" },

				//{ _setting.GetOptionLabelLocaleID(nameof(FindItSetting.EnumDropdown)), "Simple enum dropdown" },
				//{ _setting.GetOptionDescLocaleID(nameof(FindItSetting.EnumDropdown)), $"Use any enum property with getter and setter to get enum dropdown" },

				//{ _setting.GetEnumValueLocaleID(FindItSetting.SomeEnum.Value1), "Value 1" },
				//{ _setting.GetEnumValueLocaleID(FindItSetting.SomeEnum.Value2), "Value 2" },
				//{ _setting.GetEnumValueLocaleID(FindItSetting.SomeEnum.Value3), "Value 3" },
			};
		}

		public void Unload()
		{

		}
	}
}
