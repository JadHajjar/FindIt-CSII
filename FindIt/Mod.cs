using Colossal.Logging;

using FindIt.Domain.Utilities;
using FindIt.Systems;

using Game;
using Game.Modding;
using Game.SceneFlow;

namespace FindIt
{
	public class Mod : IMod
	{
		public const string Id = "FindIt";

		public static ILog Log = LogManager.GetLogger(nameof(FindIt)).SetShowsErrorsInUI(false);

		//private Setting m_Setting;

		public void OnLoad(UpdateSystem updateSystem)
		{
			Log.Info(nameof(OnLoad));

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
			{
				Log.Info($"Current mod asset at {asset.path}");
			}

			FindItUtil.LoadCustomPrefabData();

			//m_Setting = new Setting(this);
			//m_Setting.RegisterInOptionsUI();
			//GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(m_Setting));

			//AssetDatabase.global.LoadSettings(nameof(FindIt), m_Setting, new Setting(this));

			updateSystem.UpdateAfter<PrefabIndexingSystem>(SystemUpdatePhase.PrefabReferences);
			updateSystem.UpdateAt<FindItPanelUISystem>(SystemUpdatePhase.UIUpdate);
			updateSystem.UpdateAt<PrefabSearchUISystem>(SystemUpdatePhase.UIUpdate);
		}

		public void OnDispose()
		{
			Log.Info(nameof(OnDispose));
			//if (m_Setting != null)
			//{
			//	m_Setting.UnregisterInOptionsUI();
			//	m_Setting = null;
			//}
		}
	}
}
