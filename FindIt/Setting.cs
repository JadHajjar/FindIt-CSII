using Colossal;
using Colossal.IO.AssetDatabase;
using Colossal.Json;

using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;

using System.Collections.Generic;

namespace FindIt
{
	[FileLocation(nameof(FindIt))]
	[SettingsUIGroupOrder(BEHAVIOR, DISPLAY)]
	[SettingsUIShowGroupName(BEHAVIOR, DISPLAY)]
	public class FindItSettings : ModSetting
	{
		public const string MAIN_SECTION = "Main";

		public const string BEHAVIOR = "Behavior";
		public const string DISPLAY = "Display";

		public FindItSettings(IMod mod) : base(mod)
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

		[SettingsUISection(MAIN_SECTION, BEHAVIOR)]
		public bool OpenPanelOnPicker { get; set; } = true;

		[SettingsUISection(MAIN_SECTION, BEHAVIOR)]
		public bool SelectPrefabOnOpen { get; set; } = true;

		[SettingsUISection(MAIN_SECTION, BEHAVIOR)]
		public bool HideRandomAssets { get; set; } = true;

		[SettingsUISlider(min = 0.2f, max = 2f, step = 0.1f, scalarMultiplier = 1f, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(MAIN_SECTION, BEHAVIOR)]
		public float ScrollSpeed { get; set; } = 0.6f;

		[SettingsUISlider(min = 1f, max = 4f, step = 0.25f, scalarMultiplier = 1f, unit = Unit.kFloatTwoFractions)]
		[SettingsUISection(MAIN_SECTION, DISPLAY)]
		public float RowCount { get; set; } = 2f;

		[SettingsUISlider(min = 4f, max = 12f, step = 1f, scalarMultiplier = 1f)]
		[SettingsUISection(MAIN_SECTION, DISPLAY)]
		public int ColumnCount { get; set; } = 6;

		[SettingsUISlider(min = 1f, max = 10f, step = 0.25f, scalarMultiplier = 1f, unit = Unit.kFloatTwoFractions)]
		[SettingsUISection(MAIN_SECTION, DISPLAY)]
		public float ExpandedRowCount { get; set; } = 3f;

		[SettingsUISlider(min = 4f, max = 30f, step = 1f, scalarMultiplier = 1f)]
		[SettingsUISection(MAIN_SECTION, DISPLAY)]
		public int ExpandedColumnCount { get; set; } = 8;

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
}
