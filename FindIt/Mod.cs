using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Colossal.PSI.Environment;
using Colossal.Reflection;
using Colossal.UI;

using FindIt.Domain;
using FindIt.Systems;
using FindIt.Utilities;

using Game;
using Game.Modding;
using Game.SceneFlow;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Unity.Entities;

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

			if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
			{
				UIManager.defaultUISystem.AddHostLocation($"findit", Path.Combine(Path.GetDirectoryName(asset.path), "images"), false);
			}

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
			updateSystem.UpdateAt<CustomAreaBorderRenderSystem>(SystemUpdatePhase.Rendering);
			updateSystem.UpdateAt<PickerTooltipSystem>(SystemUpdatePhase.UITooltip);
			updateSystem.UpdateAt<AutoVehiclePropGeneratorSystem>(SystemUpdatePhase.MainLoop);
			updateSystem.UpdateAt<AutoQuantityPropGeneratorSystem>(SystemUpdatePhase.MainLoop);

			GameManager.instance.RegisterUpdater(RegisterAPIs);
			GameManager.instance.RegisterUpdater(ClearGooee);
		}

		private void RegisterAPIs()
		{
			foreach (var item in GameManager.instance.modManager)
			{
				if (item.asset.assembly?.GetTypesDerivedFrom<IMod>().FirstOrDefault()?.GetMethod("GetFindItSearchMethod", BindingFlags.Static | BindingFlags.Public) is MethodInfo methodInfo)
				{
					if (methodInfo.GetParameters().Length == 1
						&& methodInfo.GetParameters()[0].ParameterType == typeof(string)
						&& methodInfo.ReturnType == typeof(Func<string, bool>))
					{
						Filters.GetCustomSearchFunction = wrapFunction(methodInfo);

						Log.Info($"Registered search API from {item.name}");
					}
				}
			}

			static Func<string, Func<PrefabIndex, bool>> wrapFunction(MethodInfo method) => searchTerm =>
			{
				var searchMethod = (Func<string, bool>)method.Invoke(null, new object[] { searchTerm });

				return prefab => searchMethod(prefab.Name) || searchMethod(prefab.PrefabName);
			};
		}

		//public static Func<string, bool> GetFindItSearchMethod(string searchText)
		//{
		//	_cachedMatcher?.Dispose();
		//	_cachedMatcher = new IbMatcher.Net.IbMatcher(CurrentSearch, IbMatcherConfig.WithPinyin());

		//	return name => _cachedMatcher.IsMatch(name);
		//}

		public static Dictionary<string, string> GetIconsMap()
		{
			return AutoVehiclePropGeneratorSystem.AssetReferenceMap;
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
