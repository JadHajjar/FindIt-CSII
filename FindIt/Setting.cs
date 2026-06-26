using Colossal.IO.AssetDatabase;
using FindIt.Utilities;
using Game.Input;
using Game.Modding;
using Game.Settings;
using Game.UI;

namespace FindIt
{
    [FileLocation(nameof(FindIt))]
	[SettingsUITabOrder(SETTINGS, KEYBINDINGS)]
	[SettingsUIGroupOrder(BEHAVIOR, UIUX, OTHER, ACTIONS, NAVIGATION)]
	[SettingsUIShowGroupName(BEHAVIOR, UIUX, OTHER, ACTIONS, NAVIGATION)]
	[SettingsUIMouseAction(nameof(FindIt) + "Apply", "CustomUsage")]
	public class FindItSettings : ModSetting
	{
		public const string SETTINGS = "Settings";
		public const string KEYBINDINGS = "KeyBindings";
		public const string ACTIONS = "Actions";
		public const string NAVIGATION = "Navigation";
		public const string BEHAVIOR = "Behavior";
		public const string UIUX = "UIUX";
		public const string OTHER = "Other";

		// Bounds for the panel size values. These are no longer shown in the
		// settings UI; they are exposed through the mod's UI layer (see
		// FindItUISystem) and adjusted via the in-panel Width/Height options.
		public const float MinPanelSize = 0f;
		public const float MaxPanelSize = 200f;

		// Default panel sizes, shared by the property initializers below and the
		// "Reset View Sizes" button.
		public const float DefaultRowSize = 40f;
		public const float DefaultColumnSize = 40f;
		public const float DefaultExpandedRowSize = 80f;
		public const float DefaultExpandedColumnSize = 80f;
		public const float DefaultRightRowSize = 80f;
		public const float DefaultRightColumnSize = 30f;

		public FindItSettings(IMod mod) : base(mod)
		{

		}

		[SettingsUIMouseBinding(nameof(FindIt) + "Apply"), SettingsUIHidden]
		public ProxyBinding ApplyMimic { get; set; }

		[SettingsUIHidden]
		public string DefaultViewStyle { get; set; } = "GridWithText";

		[SettingsUIHidden]
		public string DefaultAlignmentStyle { get; set; } = "Center";

		[SettingsUIButton]
		[SettingsUIConfirmation]
		[SettingsUISection(SETTINGS, OTHER)]
		public bool ResetFavorites { set => FindItUtil.ResetFavorites(); }

		[SettingsUIButton]
		[SettingsUIConfirmation]
		[SettingsUISection(SETTINGS, OTHER)]
		public bool ResetViewSizes { set => FindItUtil.ResetViewSizes(); }

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

		// Window sizes are hidden from the settings UI and instead exposed as
		// bindings on the mod's UI layer (FindItUISystem), surfaced in-panel as
		// the Width/Height IOptionSection entries. They remain persisted here.
		[SettingsUIHidden]
		public float RowSize { get; set; } = DefaultRowSize;

		[SettingsUIHidden]
		public float ColumnSize { get; set; } = DefaultColumnSize;

		[SettingsUIHidden]
		public float ExpandedRowSize { get; set; } = DefaultExpandedRowSize;

		[SettingsUIHidden]
		public float ExpandedColumnSize { get; set; } = DefaultExpandedColumnSize;

		[SettingsUIHidden]
		public float RightRowSize { get; set; } = DefaultRightRowSize;

		[SettingsUIHidden]
		public float RightColumnSize { get; set; } = DefaultRightColumnSize;

		public override void SetDefaults()
		{
		}
	}
}
