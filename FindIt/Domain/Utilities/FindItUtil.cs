using Colossal.Json;

using FindIt.Domain.Enums;

using Game.Prefabs;

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
		public static string CurrentSearch { get; set; }
		public static bool IsActive { get; set; }

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
			return CategorizedPrefabs[CurrentCategory].Keys.OrderBy(x => (int)x);
		}

		public static IEnumerable<PrefabIndexBase> GetFilteredPrefabs()
		{
			if (string.IsNullOrWhiteSpace(CurrentSearch))
			{
				return _cachedSearch = CategorizedPrefabs[CurrentCategory][CurrentSubCategory];
			}

			return _cachedSearch;
		}

		public static PrefabBase GetPrefabBase(int id)
		{
			if (CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any].TryGetValue(id, out var prefabIndex))
			{
				return prefabIndex.Prefab;
			}

			return null;
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

				prefabIndex.Favorited = data.IsFavorited;
			}
			else
			{
				customPrefabsData[prefabIndex.Prefab.name] = new CustomPrefabData
				{
					IsFavorited = true
				};

				prefabIndex.Favorited = true;
			}

			UpdateFavoritesList(prefabIndex);

			SaveCustomPrefabData();
		}

		private static void UpdateFavoritesList(PrefabIndex prefabIndex)
		{
			if (!prefabIndex.Favorited)
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

		public static void ProcessSearch(CancellationToken token)
		{
			if (string.IsNullOrWhiteSpace(CurrentSearch))
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

				if (CurrentSearch.SearchCheck(prefabList[index].Name)
					|| prefabList[index].Prefab.name.IndexOf(CurrentSearch, System.StringComparison.InvariantCultureIgnoreCase) >= 0
					|| prefabList[index].SubCategory.ToString().IndexOf(CurrentSearch, System.StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					index++;
					continue;
				}

				prefabList.RemoveAt(index);
			}

			if (token.IsCancellationRequested)
			{
				return;
			}

			_cachedSearch = prefabList;
		}

		public static void SaveCustomPrefabData()
		{
			var path = Path.Combine(FolderUtil.ContentFolder, "CustomPrefabData.json");

			File.WriteAllText(path, JSON.Dump(customPrefabsData));
		}

		public static void LoadCustomPrefabData()
		{
			var path = Path.Combine(FolderUtil.ContentFolder, "CustomPrefabData.json");

			if (File.Exists(path))
			{
				customPrefabsData = JSON.MakeInto<Dictionary<string, CustomPrefabData>>(JSON.Load(File.ReadAllText(path))) ?? new();
			}
		}

		public static bool Pick(PrefabBase prefab, out int id)
		{
			var prefabIndex = CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any].FirstOrDefault(x => prefab.name == x.Prefab.name);

			if (prefabIndex is null)
			{
				id = 0;
				return false;
			}

			CurrentCategory = prefabIndex.Category;
			CurrentSubCategory = prefabIndex.SubCategory;
			id = prefabIndex.Id;
			return true;
		}
	}
}
