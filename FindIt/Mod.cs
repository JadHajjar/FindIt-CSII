using Colossal.IO.AssetDatabase;
using Colossal.Logging;

using FindIt.Domain.Utilities;
using FindIt.Systems;

using Game;
using Game.Modding;
using Game.SceneFlow;

using Skyve.Mod.CS2.Shared;

using System.IO;

namespace FindIt
{
	public class Mod : IMod
	{
		public const string Id = "FindIt";

		public static ILog Log { get; } = LogManager.GetLogger(nameof(FindIt)).SetShowsErrorsInUI(false);
		public static FindItSettings Settings { get; private set; }

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

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
			{
				PrefabIconUtil.Initialize(Path.GetDirectoryName(asset.path));
			}

			FindItUtil.LoadCustomPrefabData();

			updateSystem.UpdateAfter<PrefabIndexingSystem>(SystemUpdatePhase.PrefabReferences);
			updateSystem.UpdateAt<FindItPanelUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<PrefabSearchUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<OptionsUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<PickerToolSystem>(SystemUpdatePhase.ToolUpdate);
			updateSystem.UpdateAt<PickerUISystem>(SystemUpdatePhase.UIUpdate);
		}

		public void OnDispose()
		{
			Log.Info(nameof(OnDispose));

			PrefabIconUtil.OnDispose();

			if (Settings != null)
			{
				Settings.UnregisterInOptionsUI();
				Settings = null;
			}
		}
	}
}
