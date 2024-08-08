using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.PSI.Environment;

using FindIt.Domain.Utilities;
using FindIt.Systems;

using Game;
using Game.Modding;
using Game.SceneFlow;

using System.IO;
using System.Linq;

namespace FindIt
{
	public class Mod : IMod
	{
		public const string Id = "FindIt";
		private static bool? isExtraDetailingEnabled;
		private static bool? isAssetIconLibraryEnabled;
		private static bool? isRoadBuilderEnabled;

		public static ILog Log { get; } = LogManager.GetLogger(nameof(FindIt)).SetShowsErrorsInUI(false);
		public static FindItSettings Settings { get; private set; }

		public static bool IsExtraDetailingEnabled => isExtraDetailingEnabled ??= GameManager.instance.modManager.ListModsEnabled().Any(x => x.StartsWith("ExtraDetailingTools, "));
		public static bool IsAssetIconLibraryEnabled => isAssetIconLibraryEnabled ??= GameManager.instance.modManager.ListModsEnabled().Any(x => x.StartsWith("AssetIconLibrary, "));
		public static bool IsRoadBuilderEnabled => isRoadBuilderEnabled ??= GameManager.instance.modManager.ListModsEnabled().Any(x => x.StartsWith("RoadBuilder, "));

		public void OnLoad(UpdateSystem updateSystem)
		{
			Log.Info(nameof(OnLoad));

			Settings = new FindItSettings(this);
			Settings.RegisterInOptionsUI();
			Settings.RegisterKeyBindings();

			foreach (var item in new LocaleHelper("FindIt.Locale.json").GetAvailableLanguages())
			{
				GameManager.instance.localizationManager.AddSource(item.LocaleId, item);
			}

			AssetDatabase.global.LoadSettings(nameof(FindIt), Settings, new FindItSettings(this));

			FindItUtil.LoadCustomPrefabData();

			updateSystem.UpdateAfter<PrefabIndexingSystem>(SystemUpdatePhase.PrefabUpdate);
			updateSystem.UpdateAt<FindItUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<OptionsUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<PickerToolSystem>(SystemUpdatePhase.ToolUpdate);
			updateSystem.UpdateAt<PickerUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<PrefabTrackingSystem>(SystemUpdatePhase.PrefabUpdate);

			GameManager.instance.RegisterUpdater(ClearGooee);
		}

		private void ClearGooee()
		{
			try
			{
				if (!GameManager.instance.modManager.ListModsEnabled().Any(x => x.StartsWith("Gooee,")))
				{
					var folder1 = new DirectoryInfo(Path.Combine(EnvPath.kUserDataPath, "ModsData", "Gooee"));
					var folder2 = new DirectoryInfo(Path.Combine(EnvPath.kUserDataPath, "Mods", "Gooee"));

					if (folder1.Exists)
					{
						folder1.Delete(true);
					}

					if (folder2.Exists)
					{
						folder2.Delete(true);
					}
				}
			}
			catch { }
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
