using Colossal.IO.AssetDatabase;
using Colossal.Logging;

using FindIt.Domain.Utilities;
using FindIt.Systems;

using Game;
using Game.Modding;
using Game.SceneFlow;

using System.Linq;

namespace FindIt
{
	public class Mod : IMod
	{
		public const string Id = "FindIt";
		private static bool? isExtraDetailingEnabled;
		private static bool? isAssetIconLibraryEnabled;

		public static ILog Log { get; } = LogManager.GetLogger(nameof(FindIt)).SetShowsErrorsInUI(false);
		public static FindItSettings Settings { get; private set; }

		public static bool IsExtraDetailingEnabled => isExtraDetailingEnabled ??= GameManager.instance.modManager.ListModsEnabled().Any(x => x == "ExtraDetailingTools");
		public static bool IsAssetIconLibraryEnabled => isAssetIconLibraryEnabled ??= GameManager.instance.modManager.ListModsEnabled().Any(x => x == "AssetIconLibrary");

		public void OnLoad(UpdateSystem updateSystem)
		{
			Log.Info(nameof(OnLoad));

			Settings = new FindItSettings(this);
			Settings.RegisterInOptionsUI();

			foreach (var item in new LocaleHelper("FindIt.Locale.json").GetAvailableLanguages())
			{
				GameManager.instance.localizationManager.AddSource(item.LocaleId, item);
			}

			AssetDatabase.global.LoadSettings(nameof(FindIt), Settings, new FindItSettings(this)
			{
				DefaultBlock = true
			});

			FindItUtil.LoadCustomPrefabData();

			updateSystem.UpdateAfter<PrefabIndexingSystem>(SystemUpdatePhase.PrefabReferences);
			updateSystem.UpdateAt<FindItPanelUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<PrefabSearchUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<OptionsUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<PickerToolSystem>(SystemUpdatePhase.ToolUpdate);
			updateSystem.UpdateAt<PickerUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<PrefabTrackingSystem>(SystemUpdatePhase.PrefabUpdate);
		}

		public void OnDispose()
		{
			Log.Info(nameof(OnDispose));

			if (Settings != null)
			{
				Settings.UnregisterInOptionsUI();
				Settings = null;
			}
		}
	}
}
