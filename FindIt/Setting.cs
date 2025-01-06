using Colossal.IO.AssetDatabase;

using FindIt.Domain.Utilities;

using Game.Input;
using Game.Modding;
using Game.Settings;
using Game.UI;

namespace FindIt
{
	[FileLocation(nameof(FindIt))]
	[SettingsUITabOrder(SETTINGS, KEYBINDINGS)]
	[SettingsUIGroupOrder(BEHAVIOR, UIUX,DISPLAY, OTHER, ACTIONS, NAVIGATION)]
	[SettingsUIShowGroupName(BEHAVIOR, UIUX,DISPLAY, OTHER, ACTIONS, NAVIGATION)]
	[SettingsUIMouseAction(nameof(FindIt) + "Apply", "CustomUsage")]
	public class FindItSettings : ModSetting
	{
		public const string SETTINGS = "Settings";
		public const string KEYBINDINGS = "KeyBindings";
		public const string ACTIONS = "Actions";
		public const string NAVIGATION = "Navigation";
		public const string BEHAVIOR = "Behavior";
		public const string UIUX = "UIUX";
		public const string DISPLAY = "Display";
		public const string OTHER = "Other";

		public FindItSettings(IMod mod) : base(mod)
		{

		}

		[SettingsUIMouseBinding(nameof(FindIt) + "Apply"), SettingsUIHidden]
		public ProxyBinding ApplyMimic { get; set; }

		[SettingsUIHidden]
		public string DefaultViewStyle { get; set; } = "GridWithText";

		[SettingsUIHidden]
		public string DefaultAlignmentStyle { get; set; } = "Center";

		[SettingsUIHidden]
		public bool VehicleWarningShown { get; set; }

		[SettingsUIButton]
		[SettingsUIConfirmation]
		[SettingsUISection(SETTINGS, OTHER)]
		public bool ResetFavorites { set => FindItUtil.ResetFavorites(); }

		[SettingsUIKeyboardBinding(BindingKeyboard.F, nameof(SearchKeyBinding), ctrl: true)]
		[SettingsUISection(KEYBINDINGS, ACTIONS)]
		public ProxyBinding SearchKeyBinding { get; set; }

		[SettingsUIKeyboardBinding(BindingKeyboard.P, nameof(PickerKeyBinding), ctrl: true)]
		[SettingsUISection(KEYBINDINGS, ACTIONS)]
		public ProxyBinding PickerKeyBinding { get; set; }

		[SettingsUIKeyboardBinding(BindingKeyboard.R, nameof(RandomKeyBinding), ctrl: true)]
		[SettingsUISection(KEYBINDINGS, ACTIONS)]
		public ProxyBinding RandomKeyBinding { get; set; }

		[SettingsUIKeyboardBinding(BindingKeyboard.LeftArrow, nameof(LeftArrow))]
		[SettingsUISection(KEYBINDINGS, NAVIGATION)]
		public ProxyBinding LeftArrow { get; set; }

		[SettingsUIKeyboardBinding(BindingKeyboard.RightArrow, nameof(RightArrow))]
		[SettingsUISection(KEYBINDINGS, NAVIGATION)]
		public ProxyBinding RightArrow { get; set; }

		[SettingsUIKeyboardBinding(BindingKeyboard.UpArrow, nameof(UpArrow))]
		[SettingsUISection(KEYBINDINGS, NAVIGATION)]
		public ProxyBinding UpArrow { get; set; }

		[SettingsUIKeyboardBinding(BindingKeyboard.DownArrow, nameof(DownArrow))]
		[SettingsUISection(KEYBINDINGS, NAVIGATION)]
		public ProxyBinding DownArrow { get; set; }

		[SettingsUISection(SETTINGS, BEHAVIOR)]
		public bool OpenPanelOnPicker { get; set; } = true;

		[SettingsUISection(SETTINGS, BEHAVIOR)]
		public bool SelectPrefabOnOpen { get; set; } = true;

		[SettingsUISection(SETTINGS, BEHAVIOR)]
		public bool HideRandomAssets { get; set; }

		[SettingsUISection(SETTINGS, BEHAVIOR)]
		public bool HideBrandsFromAny { get; set; }

		[SettingsUISection(SETTINGS, UIUX)]
		public bool StrictSearch { get; set; }

		[SettingsUISection(SETTINGS, UIUX)]
		public bool NoAssetImage { get; set; }

		[SettingsUISection(SETTINGS, UIUX)]
		public bool SmoothScroll { get; set; }

		[SettingsUISlider(min = 0.2f, max = 2f, step = 0.1f, unit = Unit.kFloatSingleFraction)]
		[SettingsUISection(SETTINGS, UIUX)]
		public float ScrollSpeed { get; set; } = 0.6f;

		[SettingsUISlider(min = 0, max = 200, unit = Unit.kPercentage)]
		[SettingsUISection(SETTINGS, DISPLAY)]
		public float RowSize { get; set; } = 40f;

		[SettingsUISlider(min = 0, max = 200, unit = Unit.kPercentage)]
		[SettingsUISection(SETTINGS, DISPLAY)]
		public float ColumnSize { get; set; } = 40f;

		[SettingsUISlider(min = 0, max = 200, unit = Unit.kPercentage)]
		[SettingsUISection(SETTINGS, DISPLAY)]
		public float ExpandedRowSize { get; set; } = 80f;

		[SettingsUISlider(min = 0, max = 200, unit = Unit.kPercentage)]
		[SettingsUISection(SETTINGS, DISPLAY)]
		public float ExpandedColumnSize { get; set; } = 80f;

		[SettingsUISlider(min = 0, max = 200, unit = Unit.kPercentage)]
		[SettingsUISection(SETTINGS, DISPLAY)]
		public float RightRowSize { get; set; } = 80f;

		[SettingsUISlider(min = 0, max = 200, unit = Unit.kPercentage)]
		[SettingsUISection(SETTINGS, DISPLAY)]
		public float RightColumnSize { get; set; } = 30f;

		public override void SetDefaults()
		{
		}
	}
}
