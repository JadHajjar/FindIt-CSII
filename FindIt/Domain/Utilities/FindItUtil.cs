﻿using Colossal.Json;

using FindIt.Domain.Enums;

using Game.Prefabs;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FindIt.Domain.Utilities
{
	public static class FindItUtil
	{
		private static List<PrefabIndex> _cachedSearch = new();
		private static Dictionary<string, CustomPrefabData> customPrefabsData = new();

		public static Dictionary<PrefabCategory, Dictionary<PrefabSubCategory, IndexedPrefabList>> CategorizedPrefabs { get; } = new();
		public static PrefabCategory CurrentCategory { get; set; } = PrefabCategory.Any;
		public static PrefabSubCategory CurrentSubCategory { get; set; } = PrefabSubCategory.Any;
		public static bool IsReady { get; set; }
		public static Filters Filters { get; } = new();

		public static IEnumerable<PrefabCategory> GetCategories()
		{
			return new[]
			{
				PrefabCategory.Any,
				PrefabCategory.Networks,
				PrefabCategory.Buildings,
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

		public static List<PrefabIndex> GetUnfilteredPrefabs()
		{
			if (!IsReady)
			{
				return new();
			}

			return CategorizedPrefabs[CurrentCategory][CurrentSubCategory];
		}

		public static PrefabBase GetPrefabBase(int id)
		{
			if (CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any].TryGetValue(id, out var prefabIndex))
			{
				return prefabIndex.Prefab;
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

		public static bool IsFavorited(PrefabBase prefab)
		{
			return customPrefabsData.TryGetValue(prefab.name, out var data) && data.IsFavorited;
		}

		public static void ToggleFavorited(int id)
		{
			if (!CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any].TryGetValue(id, out var prefabIndex))
			{
				return;
			}

			if (customPrefabsData.TryGetValue(prefabIndex.Prefab.name, out var data))
			{
				data.IsFavorited = !data.IsFavorited;

				prefabIndex.IsFavorited = data.IsFavorited;
			}
			else
			{
				customPrefabsData[prefabIndex.Prefab.name] = new CustomPrefabData
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
			if (!prefabIndex.IsFavorited)
			{
				if (CategorizedPrefabs[PrefabCategory.Favorite].ContainsKey(prefabIndex.SubCategory))
				{
					CategorizedPrefabs[PrefabCategory.Favorite][prefabIndex.SubCategory].Remove(prefabIndex);

					if (CategorizedPrefabs[PrefabCategory.Favorite][prefabIndex.SubCategory].Count == 0)
					{
						CategorizedPrefabs[PrefabCategory.Favorite].Remove(prefabIndex.SubCategory);
					}
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

		public static void ResetFavorites()
		{
			CategorizedPrefabs[PrefabCategory.Favorite] = new();
			CategorizedPrefabs[PrefabCategory.Favorite][PrefabSubCategory.Any] = new();

			foreach (var item in customPrefabsData.Values)
			{
				item.IsFavorited = false;
			}

			SaveCustomPrefabData();
		}

		public static void SaveCustomPrefabData()
		{
			var path = Path.Combine(FolderUtil.ContentFolder, "CustomPrefabData.json");

			File.WriteAllText(path, JSON.Dump(customPrefabsData));
		}

		public static void LoadCustomPrefabData()
		{
			var path = Path.Combine(FolderUtil.ContentFolder, "CustomPrefabData.json");

			if (!File.Exists(path))
			{
				return;
			}

			try
			{
				customPrefabsData = JSON.MakeInto<Dictionary<string, CustomPrefabData>>(JSON.Load(File.ReadAllText(path))) ?? new();
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Failed to load custom prefab data");
			}
		}

		public static bool Find(PrefabBase prefab, bool setCategory, out int id)
		{
			var prefabIndex = CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any].FirstOrDefault(x => prefab.name == x.Prefab.name);

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

		public static void ProcessSearch(CancellationToken token)
		{
			var filterList = Filters.GetFilterList().ToList();

			if (filterList.Count == 0)
			{
				return;
			}

			var prefabList = CategorizedPrefabs[CurrentCategory][CurrentSubCategory].ToList();
			var index = 0;

			while (index < prefabList.Count)
			{
				if (token.IsCancellationRequested)
				{
					return;
				}

				var prefab = prefabList[index];

				if (filterList.All(filter => filter(prefab)))
				{
					index++;
				}
				else
				{
					prefabList.RemoveAt(index);
				}
			}

			if (token.IsCancellationRequested)
			{
				return;
			}

			_cachedSearch = prefabList;
		}
	}
}
