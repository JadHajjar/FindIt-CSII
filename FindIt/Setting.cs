using Colossal;
using Colossal.IO.AssetDatabase;
using Colossal.Json;

using FindIt.Domain.Options;
using FindIt.Domain.Utilities;

using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;

using System.Collections.Generic;

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
		public bool DefaultBlock { get; set; }

		[SettingsUIHidden]
		public string DefaultViewStyle { get; set; } = "GridWithText";
	
		[SettingsUIButton]
		[SettingsUISection(MAIN_SECTION, OTHER)]
		public bool ResetFavorites { set { FindItUtil.ResetFavorites(); } }

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
