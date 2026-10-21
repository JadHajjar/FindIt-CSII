using Colossal.Json;

using FindIt.Domain;
using FindIt.Domain.Enums;
using FindIt.Systems;

using Game.Prefabs;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using Unity.Entities;

namespace FindIt.Utilities
{
	public static class FindItUtil
	{
		private static List<PrefabIndex> _cachedSearch = new();
		private static Dictionary<string, CustomPrefabData> customPrefabsData = new();

		public static Dictionary<PrefabCategory, Dictionary<PrefabSubCategory, IndexedPrefabList>> CategorizedPrefabs { get; } = new();
		public static Dictionary<int, PrefabIndex> AllPrefabs { get; } = new();
		public static Dictionary<string, string> AssetMap { get; } = new(); // Maps auto-generated FindIt prefab names with their original prefab
		public static Dictionary<string, string> ObsoleteAssetMap { get; } = new(); // Maps indexed prefabs' obsolete identifiers for fallback
		public static PrefabCategory CurrentCategory { get; set; } = PrefabCategory.Any;
		public static PrefabSubCategory CurrentSubCategory { get; set; } = PrefabSubCategory.Any;
		public static bool IsReady { get; set; }
		public static Filters Filters { get; } = new();
		public static AssetPackPrefab FavoritePack { get; private set; }

		public static IEnumerable<PrefabCategory> GetCategories()
		{
			return new[]
			{
				PrefabCategory.Any,
				PrefabCategory.Networks,
				PrefabCategory.Buildings,
				PrefabCategory.ServiceBuildings,
				PrefabCategory.Trees,
				PrefabCategory.Props,
				PrefabCategory.Vehicles,
				PrefabCategory.Favorite,
			};
		}

		public static IEnumerable<PrefabSubCategory> GetSubCategories()
		{
			return CategorizedPrefabs[CurrentCategory]
				.Where(x => x.Value.Count > 0)
				.Select(x => x.Key)
				.OrderBy(x => (int)x);
		}

		public static List<PrefabIndex> GetFilteredPrefabs()
		{
			if (!IsReady)
			{
				return new();
			}

			if (!Filters.GetFilterList().Any())
			{
				return _cachedSearch = CategorizedPrefabs[CurrentCategory][CurrentSubCategory];
			}

			return _cachedSearch;
		}

		public static void SetCachedSearch(List<PrefabIndex> prefabs)
		{
			_cachedSearch = prefabs;
		}

		public static void ClearCachedSearch()
		{
			_cachedSearch = new();
		}

		public static List<PrefabIndex> GetUnfilteredPrefabs()
		{
			if (!IsReady)
			{
				return new();
			}

			return CategorizedPrefabs[CurrentCategory][CurrentSubCategory];
		}

		// The filter options read every prefab of the list, which takes frames on a large playset.
		public static bool HasUnfilteredPrefabsChanged(ref IndexedPrefabList prefabs, ref int version)
		{
			var current = CategorizedPrefabs[CurrentCategory][CurrentSubCategory];

			if (current == prefabs && current.Version == version)
			{
				return false;
			}

			prefabs = current;
			version = current.Version;

			return true;
		}

		public static PrefabBase GetPrefabBase(int id)
		{
			if (AllPrefabs.TryGetValue(id, out var prefabIndex))
			{
				return prefabIndex.Prefab;
			}

			return null;
		}

		public static PrefabIndex GetPrefabIndex(int id)
		{
			if (AllPrefabs.TryGetValue(id, out var prefabIndex))
			{
				return prefabIndex;
			}

			return null;
		}

		public static void SetSorting(bool? descending = null, PrefabSorting? sorting = null)
		{
			IndexedPrefabList.Sorting = sorting ?? IndexedPrefabList.Sorting;
			IndexedPrefabList.SortingDescending = descending ?? IndexedPrefabList.SortingDescending;

			foreach (var item in CategorizedPrefabs)
			{
				foreach (var list in item.Value.Values)
				{
					list.ResetOrder();
				}
			}
		}

		public static bool IsFavorited(string prefab)
		{
			return customPrefabsData.TryGetValue(prefab, out var data) && data.IsFavorited;
		}

		public static void ToggleFavorited(int id)
		{
			if (!AllPrefabs.TryGetValue(id, out var prefabIndex))
			{
				return;
			}

			if (customPrefabsData.TryGetValue(prefabIndex.PrefabName, out var data))
			{
				data.IsFavorited = !data.IsFavorited;

				prefabIndex.IsFavorited = data.IsFavorited;
			}
			else
			{
				customPrefabsData[prefabIndex.PrefabName] = new CustomPrefabData
				{
					IsFavorited = true
				};

				prefabIndex.IsFavorited = true;
			}

			UpdateFavoritesList(prefabIndex);

			SaveCustomPrefabData();
		}

		private static void UpdateFavoritesList(PrefabIndex prefabIndex)
		{
			//UpdateFavoritesPack(prefabIndex);

			if (!prefabIndex.IsFavorited)
			{
				if (CategorizedPrefabs[PrefabCategory.Favorite].TryGetValue(prefabIndex.SubCategory, out var subCategoryList))
				{
					subCategoryList.Remove(prefabIndex);
				}

				CategorizedPrefabs[PrefabCategory.Favorite][PrefabSubCategory.Any].Remove(prefabIndex);

				return;
			}

			if (!CategorizedPrefabs[PrefabCategory.Favorite].ContainsKey(prefabIndex.SubCategory))
			{
				CategorizedPrefabs[PrefabCategory.Favorite][prefabIndex.SubCategory] = new();
			}

			CategorizedPrefabs[PrefabCategory.Favorite][PrefabSubCategory.Any][prefabIndex.Id] = prefabIndex;
			CategorizedPrefabs[PrefabCategory.Favorite][prefabIndex.SubCategory][prefabIndex.Id] = prefabIndex;
		}

		//public static void UpdateFavoritesPack(PrefabIndex prefabIndex)
		//{
		//	if (!prefabIndex.Prefab.TryGet<AssetPackItem>(out var component))
		//	{
		//		component = prefabIndex.Prefab.AddComponent<AssetPackItem>();
		//	}

		//	if (prefabIndex.IsFavorited)
		//	{
		//		component.m_Packs = component.m_Packs is null ? new[] { FavoritePack } : component.m_Packs.Append(FavoritePack).ToArray();
		//	}
		//	else if (component.m_Packs is not null)
		//	{
		//		component.m_Packs = component.m_Packs.Where(x => x != FavoritePack).ToArray();
		//	}
		//}

		public static List<string> GetTags(string prefab)
		{
			return customPrefabsData.TryGetValue(prefab, out var data) && data.Tags is not null
				? new List<string>(data.Tags)
				: new List<string>();
		}

		public static string[] GetAllTags()
		{
			var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

			foreach (var data in customPrefabsData.Values)
			{
				if (data.Tags is null)
				{
					continue;
				}

				foreach (var tag in data.Tags)
				{
					counts[tag] = counts.TryGetValue(tag, out var count) ? count + 1 : 1;
				}
			}

			foreach (var prefab in AllPrefabs.Values)
			{
				if (prefab.CreatorTags is null)
				{
					continue;
				}

				foreach (var tag in prefab.CreatorTags)
				{
					counts[tag] = counts.TryGetValue(tag, out var count) ? count + 1 : 1;
				}
			}

			return counts
				.OrderByDescending(x => x.Value)
				.ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
				.Select(x => x.Key)
				.ToArray();
		}

		public static bool TryParseSubCategory(string name, out PrefabSubCategory subCategory)
		{
			subCategory = default;

			return !string.IsNullOrWhiteSpace(name)
				&& !char.IsDigit(name.Trim()[0])
				&& Enum.TryParse(name.Trim(), true, out subCategory)
				&& IsValidSubCategory(subCategory);
		}

		public static bool IsValidSubCategory(PrefabSubCategory subCategory)
		{
			return Enum.IsDefined(typeof(PrefabSubCategory), subCategory)
				&& (int)subCategory > 0
				&& (int)subCategory % 100 != 0;
		}

		public static PrefabCategory GetCategory(PrefabSubCategory subCategory)
		{
			return (PrefabCategory)((int)subCategory / 100 * 100);
		}

		public static void RemoveFromCategories(int id)
		{
			foreach (var category in CategorizedPrefabs.Values)
			{
				foreach (var list in category.Values)
				{
					if (list.TryGetValue(id, out var prefabIndex))
					{
						list.Remove(prefabIndex);
					}
				}
			}
		}

		public static string[] GetCreatorTags(PrefabBase prefab)
		{
			var tags = new List<string>();

			if (prefab.TryGet<FindItProperties>(out var properties) && properties.m_Tags is not null)
			{
				foreach (var tag in properties.m_Tags)
				{
					AddCreatorTag(tags, tag);
				}
			}

			if (prefab.TryGet<EditorAssetCategoryOverride>(out var overrides) && overrides.m_ExcludeCategories is not null)
			{
				foreach (var category in overrides.m_ExcludeCategories)
				{
					if (category is not null && category.StartsWith("#"))
					{
						AddCreatorTag(tags, category);
					}
				}
			}

			return tags.Count == 0 ? Array.Empty<string>() : tags.ToArray();
		}

		private static void AddCreatorTag(List<string> tags, string tag)
		{
			tag = NormalizeTag(tag);

			if (tag.Length > 0 && !tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
			{
				tags.Add(tag);
			}
		}

		public static string NormalizeTag(string tag)
		{
			return string.IsNullOrWhiteSpace(tag)
				? string.Empty
				: string.Join("-", tag.Trim().TrimStart('#').Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
		}

		public static bool AddTag(int id, string tag)
		{
			tag = NormalizeTag(tag);

			if (tag.Length == 0
				|| GetPrefabIndex(id) is not PrefabIndex prefabIndex
				|| (prefabIndex.CreatorTags?.Contains(tag, StringComparer.OrdinalIgnoreCase) ?? false))
			{
				return false;
			}

			tag = GetAllTags().FirstOrDefault(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase)) ?? tag;

			if (!customPrefabsData.TryGetValue(prefabIndex.PrefabName, out var data))
			{
				customPrefabsData[prefabIndex.PrefabName] = data = new CustomPrefabData();
			}

			data.Tags ??= new();

			if (data.Tags.Any(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase)))
			{
				return false;
			}

			data.Tags.Add(tag);
			data.Tags.Sort(StringComparer.OrdinalIgnoreCase);

			prefabIndex.Tags = new List<string>(data.Tags);

			SaveCustomPrefabData();

			return true;
		}

		public static bool RemoveTag(int id, string tag)
		{
			if (GetPrefabIndex(id) is not PrefabIndex prefabIndex
				|| !customPrefabsData.TryGetValue(prefabIndex.PrefabName, out var data)
				|| data.Tags is null
				|| data.Tags.RemoveAll(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase)) == 0)
			{
				return false;
			}

			prefabIndex.Tags = new List<string>(data.Tags);

			SaveCustomPrefabData();

			return true;
		}

		public static void ResetTags()
		{
			foreach (var item in customPrefabsData.Values)
			{
				item.Tags?.Clear();
			}

			foreach (var item in AllPrefabs.Values)
			{
				item.Tags = new();
			}

			SaveCustomPrefabData();
		}

		public static void ResetFavorites()
		{
			if (CategorizedPrefabs.TryGetValue(PrefabCategory.Favorite, out var favorites))
			{
				foreach (var list in favorites.Values)
				{
					foreach (var item in list.ToArray())
					{
						list.Remove(item);
					}
				}
			}

			foreach (var item in AllPrefabs.Values)
			{
				item.IsFavorited = false;
			}

			foreach (var item in customPrefabsData.Values)
			{
				item.IsFavorited = false;
			}

			SaveCustomPrefabData();

			World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<FindItUISystem>()?.OnFavoritesChanged();
		}

		public static void ResetViewSizes()
		{
			Mod.Settings.RowSize = FindItSettings.DefaultRowSize;
			Mod.Settings.ColumnSize = FindItSettings.DefaultColumnSize;
			Mod.Settings.ExpandedRowSize = FindItSettings.DefaultExpandedRowSize;
			Mod.Settings.ExpandedColumnSize = FindItSettings.DefaultExpandedColumnSize;
			Mod.Settings.RightRowSize = FindItSettings.DefaultRightRowSize;
			Mod.Settings.RightColumnSize = FindItSettings.DefaultRightColumnSize;

			Mod.Settings.ApplyAndSave();

			// If the in-game UI is live, push the reset values into its bindings
			// and refresh the panel. In the main menu the system won't exist yet,
			// and the bindings will pick up the saved values when it is created.
			World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<FindItUISystem>()?.SyncViewSizesFromSettings();
		}

		public static void SaveCustomPrefabData()
		{
			var path = Path.Combine(FolderUtil.ContentFolder, "CustomPrefabData.json");
			var tempPath = path + ".tmp";

			try
			{
				File.WriteAllText(tempPath, JSON.Dump(customPrefabsData));

				if (File.Exists(path))
				{
					File.Replace(tempPath, path, null);
				}
				else
				{
					File.Move(tempPath, path);
				}
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Failed to save custom prefab data");
			}
		}

		public static void LoadCustomPrefabData()
		{
			//FavoritePack = ScriptableObject.CreateInstance<AssetPackPrefab>();
			//FavoritePack.name = "FindItFavoritesFilter";
			//FavoritePack.AddComponent<UIObject>().m_Icon = "coui://findit/starFavorite.svg";

			//World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>().AddPrefab(FavoritePack);

			var path = Path.Combine(FolderUtil.ContentFolder, "CustomPrefabData.json");

			if (!File.Exists(path))
			{
				return;
			}

			try
			{
				customPrefabsData = JSON.MakeInto<Dictionary<string, CustomPrefabData>>(JSON.Load(File.ReadAllText(path))) ?? new();

				foreach (var data in customPrefabsData.Values)
				{
					data.Tags = data.Tags?
						.Select(NormalizeTag)
						.Where(x => x.Length > 0)
						.Distinct(StringComparer.OrdinalIgnoreCase)
						.ToList() ?? new();
				}
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Failed to load custom prefab data");

				try
				{
					File.Copy(path, path + ".bak", true);
				}
				catch (Exception copyEx)
				{
					Mod.Log.Error(copyEx, "Failed to back up the unreadable custom prefab data");
				}
			}
		}

		public static bool Find(PrefabBase prefab, bool setCategory, out int id)
		{
			var prefabSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PrefabSystem>();

			// The index's ids are the prefabs' entity indexes, but a prefab indexed under another
			// name is the prop generated from it, which has an entity of its own.
			if (prefab is MovingObjectPrefab
				|| AssetMap.ContainsKey(prefab.name)
				|| !prefabSystem.TryGetEntity(prefab, out var entity))
			{
				return SelectFound(FindByName(prefab), setCategory, out id);
			}

			return SelectFound(GetPrefabIndex(entity.Index), setCategory, out id);
		}

		// Scans the whole index, but finds an entry whose prefab has moved to a new entity since.
		public static PrefabIndex FindByName(PrefabBase prefab)
		{
			return (prefab is MovingObjectPrefab ? FindByName($"Prop_{prefab.name}") : null) ?? FindByName(prefab.name);
		}

		private static bool SelectFound(PrefabIndex prefabIndex, bool setCategory, out int id)
		{
			if (prefabIndex is null)
			{
				id = 0;
				return false;
			}

			if (setCategory)
			{
				CurrentCategory = prefabIndex.Category;
				CurrentSubCategory = prefabIndex.SubCategory;
			}

			id = prefabIndex.Id;
			return true;
		}

		private static PrefabIndex FindByName(string name)
		{
			if (AssetMap.TryGetValue(name, out var newName))
			{
				name = newName;
			}

			if (ObsoleteAssetMap.TryGetValue(name, out newName))
			{
				name = newName;
			}

			return AllPrefabs.Values.FirstOrDefault(x => name == x.PrefabName);
		}

		public static List<PrefabIndex> ProcessSearch(CancellationToken token)
		{
			var filterList = Filters.GetFilterList().ToList();

			if (filterList.Count == 0)
			{
				return null;
			}

			var prefabs = CategorizedPrefabs[CurrentCategory][CurrentSubCategory].OrderedList;
			var prefabList = new List<PrefabIndex>();

			foreach (var prefab in prefabs)
			{
				if (token.IsCancellationRequested)
				{
					return null;
				}

				var allFiltersPass = true;

				for (var i = 0; i < filterList.Count; i++)
				{
					if (!filterList[i](prefab))
					{
						allFiltersPass = false;

						break;
					}
				}

				if (allFiltersPass)
				{
					prefabList.Add(prefab);
				}
			}

			return prefabList;
		}

		public static void RemoveItem(Entity entity)
		{
			RemoveItem(entity.Index);
		}

		public static void RemoveItem(int index)
		{
			if (AllPrefabs.Remove(index))
			{
				RemoveFromCategories(index);
			}
		}
	}
}
