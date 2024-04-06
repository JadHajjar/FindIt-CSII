using Colossal.IO.AssetDatabase;
using Colossal.Logging;

using FindIt.Domain.Utilities;
using FindIt.Systems;

using Game;
using Game.Modding;
using Game.SceneFlow;

using System.IO;

namespace FindIt
{
	public class Mod : IMod
	{
		public const string Id = "FindIt";

		public static ILog Log { get; } = LogManager.GetLogger(nameof(FindIt)).SetShowsErrorsInUI(false);
		public static FindItSetting Settings { get; private set; }

		public void OnLoad(UpdateSystem updateSystem)
		{
			Log.Info(nameof(OnLoad));

			Settings = new FindItSetting(this);
			Settings.RegisterInOptionsUI();
			GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Settings));

			AssetDatabase.global.LoadSettings(nameof(FindIt), Settings, new FindItSetting(this)
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
