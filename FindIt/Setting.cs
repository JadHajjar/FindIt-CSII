using Colossal.IO.AssetDatabase;

using FindIt.Domain.Utilities;

using Game.Input;
using Game.Modding;
using Game.Settings;
using Game.UI;

using UnityEngine.InputSystem;

namespace FindIt
{
	[FileLocation(nameof(FindIt))]
	[SettingsUIGroupOrder(BEHAVIOR, DISPLAY, OTHER)]
	[SettingsUIShowGroupName(BEHAVIOR, DISPLAY, OTHER)]
	public class FindItSettings : ModSetting
	{
		public const string MAIN_SECTION = "Main";

		public const string BEHAVIOR = "Behavior";
		public const string DISPLAY = "Display";
		public const string OTHER = "Other";

		public FindItSettings(IMod mod) : base(mod)
		{

		}

		[SettingsUIHidden]
		public string DefaultViewStyle { get; set; } = "GridWithText";

		[SettingsUIButton]
		[SettingsUIConfirmation]
		[SettingsUISection(MAIN_SECTION, OTHER)]
		public bool ResetFavorites { set => FindItUtil.ResetFavorites(); }

		[SettingsUIKeyboardBinding(Key.F, nameof(SearchKeyBinding), ctrl: true)]
		[SettingsUISection(MAIN_SECTION, BEHAVIOR)]
		public ProxyBinding SearchKeyBinding { get; set; }

		[SettingsUIKeyboardBinding(Key.P, nameof(PickerKeyBinding), ctrl: true)]
		[SettingsUISection(MAIN_SECTION, BEHAVIOR)]
		public ProxyBinding PickerKeyBinding { get; set; }

		[SettingsUISection(MAIN_SECTION, BEHAVIOR)]
		public bool OpenPanelOnPicker { get; set; } = true;

		[SettingsUISection(MAIN_SECTION, BEHAVIOR)]
		public bool SelectPrefabOnOpen { get; set; } = true;

		[SettingsUISection(MAIN_SECTION, BEHAVIOR)]
		public bool StrictSearch { get; set; }

		[SettingsUISection(MAIN_SECTION, BEHAVIOR)]
		public bool HideRandomAssets { get; set; }

		[SettingsUISection(MAIN_SECTION, BEHAVIOR)]
		public bool HideBrandsFromAny { get; set; }

		[SettingsUISection(MAIN_SECTION, BEHAVIOR)]
		public bool SmoothScroll { get; set; }

		[SettingsUISlider(min = 0.2f, max = 2f, step = 0.1f, scalarMultiplier = 1f, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(MAIN_SECTION, BEHAVIOR)]
		public float ScrollSpeed { get; set; } = 0.6f;

		[SettingsUISlider(min = 0, max = 200, step = 1, scalarMultiplier = 1f, unit = Unit.kPercentage)]
		[SettingsUISection(MAIN_SECTION, DISPLAY)]
		public float RowSize { get; set; } = 40f;

		[SettingsUISlider(min = 0, max = 200, step = 1, scalarMultiplier = 1f, unit = Unit.kPercentage)]
		[SettingsUISection(MAIN_SECTION, DISPLAY)]
		public float ColumnSize { get; set; } = 40;

		[SettingsUISlider(min = 0, max = 200, step = 1, scalarMultiplier = 1f, unit = Unit.kPercentage)]
		[SettingsUISection(MAIN_SECTION, DISPLAY)]
		public float ExpandedRowSize { get; set; } = 80;

		[SettingsUISlider(min = 0, max = 200, step = 1, scalarMultiplier = 1f, unit = Unit.kPercentage)]
		[SettingsUISection(MAIN_SECTION, DISPLAY)]
		public float ExpandedColumnSize { get; set; } = 80;

		public override void SetDefaults()
		{
		}
	}
}
