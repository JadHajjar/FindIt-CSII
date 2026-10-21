using Colossal.Reflection;

using FindIt.Domain;
using FindIt.Systems;

using Game.Modding;
using Game.Prefabs;
using Game.SceneFlow;

using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Unity.Entities;

namespace FindIt
{
	public static class ApiUtil
	{
		private static FindItUISystem _uiSystem;

		private static FindItUISystem UISystem =>
			_uiSystem ??= World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<FindItUISystem>();

		/// <summary>
		/// Opens FindIt's prefab picker filtered to the given prefab type and returns the prefab selected by the user.
		/// Returns null if the user closes the picker without selecting.
		/// The title defaults to the type name split into words (e.g. "TreePrefab" → "Tree Prefab").
		/// </summary>
		public static Task<PrefabBase> PickPrefab(Type prefabType, string title = null)
		{
			if (prefabType == null)
				throw new ArgumentNullException(nameof(prefabType));

			if (!typeof(PrefabBase).IsAssignableFrom(prefabType))
				throw new ArgumentException($"{prefabType.Name} does not derive from PrefabBase", nameof(prefabType));

			var system = UISystem;

			if (system == null)
			{
				Mod.Log.Warn("PickPrefab called but FindItPickerUISystem is not available — FindIt may not be fully loaded yet.");
				return Task.FromResult<PrefabBase>(null);
			}

			title ??= FormatTypeName(prefabType);

			Mod.Log.Info($"PickPrefab called for {prefabType.Name}, title='{title}'");

			return system.PickPrefab(prefabType, title);
		}

		/// <summary>
		/// Scans all loaded mods and registers any that expose Find It API hooks.
		///
		/// Supported hooks (all must be public static methods on the mod's IMod type):
		///
		///   void SetFindItPrefabSelector(Func&lt;Type, string, Task&lt;PrefabBase&gt;&gt; selector)
		///     Called by Find It to pass the prefab-picker delegate. Store it and call it whenever
		///     you need the user to pick a prefab. The string parameter is the window title (nullable).
		///
		///   Func&lt;string, bool&gt; GetFindItSearchMethod(string searchText)
		///     Called by Find It on each search. Return a predicate that returns true for prefab
		///     names that should appear in the results.
		/// </summary>
		internal static void RegisterExternalMods()
		{
			foreach (var item in GameManager.instance.modManager)
			{
				// A mod that failed to load stays in this list, and its types may not load.
				if (item.state is not ModManager.ModInfo.State.Loaded)
					continue;

				var modType = item.asset.assembly?.GetTypesDerivedFrom<IMod>().FirstOrDefault();

				if (modType == null)
					continue;

				TryRegisterPrefabSelector(item.name, modType);
				TryRegisterSearchMethod(item.name, modType);
			}
		}

		private static void TryRegisterPrefabSelector(string modName, Type modType)
		{
			var method = modType.GetMethod("SetFindItPrefabSelector", BindingFlags.Static | BindingFlags.Public);

			if (method == null)
				return;

			var parameters = method.GetParameters();

			if (parameters.Length != 1
				|| parameters[0].ParameterType != typeof(Func<Type, string, Task<PrefabBase>>)
				|| method.ReturnType != typeof(void))
				return;

			try
			{
				method.Invoke(null, new object[] { new Func<Type, string, Task<PrefabBase>>(PickPrefab) });

				Mod.Log.Info($"Registered prefab selector API with {modName}");
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, $"Failed to register prefab selector API with {modName}");
			}
		}

		private static void TryRegisterSearchMethod(string modName, Type modType)
		{
			var method = modType.GetMethod("GetFindItSearchMethod", BindingFlags.Static | BindingFlags.Public);

			if (method == null)
				return;

			if (method.GetParameters().Length != 1
				|| method.GetParameters()[0].ParameterType != typeof(string)
				|| method.ReturnType != typeof(Func<string, bool>))
				return;

			try
			{
				Filters.GetCustomSearchFunction = WrapSearchFunction(method);

				Mod.Log.Info($"Registered search API from {modName}");
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, $"Failed to register search API from {modName}");
			}
		}

		private static Func<string, Func<PrefabIndex, bool>> WrapSearchFunction(MethodInfo method) => searchTerm =>
		{
			var searchMethod = (Func<string, bool>)method.Invoke(null, new object[] { searchTerm });

			return prefab => searchMethod(prefab.Name) || searchMethod(prefab.PrefabName);
		};

		private static string FormatTypeName(Type type)
		{
			var name = type.Name;
			var result = new StringBuilder(name.Length + 4);

			for (var i = 0; i < name.Length; i++)
			{
				if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
					result.Append(' ');

				result.Append(name[i]);
			}

			return result.ToString();
		}
	}
}
