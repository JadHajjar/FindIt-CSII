using FindIt.Domain.Enums;
using FindIt.Systems;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FindIt.Domain
{
	public class IndexedPrefabList : IEnumerable<PrefabIndex>
	{
		private static readonly IComparer<SortKey> _sortKeyComparer =
			Comparer<SortKey>.Create(SortKey.Compare);

		private readonly Dictionary<int, PrefabIndex> _dictionary;
		private List<PrefabIndex> _orderedList;
		private int _version;

		public static PrefabSorting Sorting { get; set; }
		public static bool SortingDescending { get; set; }
		public static bool IsSortedByUsage => Sorting is PrefabSorting.MostUsed or PrefabSorting.LastUsed;

		public IndexedPrefabList()
		{
			_dictionary = new();
		}

		public int Count => _dictionary.Count;

		public int Version => _version;

		public List<PrefabIndex> OrderedList => _orderedList ?? SortValues();

		public PrefabIndex this[int index]
		{
			get => _dictionary[index];
			set
			{
				lock (_dictionary)
				{
					_dictionary[index] = value;

					ResetOrder();
				}
			}
		}

		public bool Contains(int id)
		{
			return _dictionary.ContainsKey(id);
		}

		public bool TryGetValue(int id, out PrefabIndex prefabIndex)
		{
			return _dictionary.TryGetValue(id, out prefabIndex);
		}

		public IEnumerator<PrefabIndex> GetEnumerator()
		{
			return _dictionary.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _dictionary.Values.GetEnumerator();
		}

		internal void Remove(PrefabIndex prefabIndex)
		{
			lock (_dictionary)
			{
				_dictionary.Remove(prefabIndex.Id);

				ResetOrder();
			}
		}

		internal void ResetOrder()
		{
			lock (_dictionary)
			{
				_orderedList = null;
				_version++;
			}
		}

		// A search sorts on a worker while a partial index writes on the main thread.
		// The sort holds the lock only to copy and to cache, so a partial index never waits on it.
		private List<PrefabIndex> SortValues()
		{
			PrefabIndex[] values;
			int version;

			lock (_dictionary)
			{
				values = _dictionary.Values.ToArray();
				version = _version;
			}

			var orderedList = Sort(values).ToList();

			lock (_dictionary)
			{
				if (version == _version)
				{
					_orderedList = orderedList;
				}
			}

			return orderedList;
		}

		private IEnumerable<PrefabIndex> Sort(IEnumerable<PrefabIndex> values)
		{
			if (SortingDescending)
			{
				return Sorting switch
				{
					PrefabSorting.MostUsed => values.OrderBy(PrefabTrackingSystem.GetMostUsedCount).ThenByDescending(PrefabNameSortKey, _sortKeyComparer),
					PrefabSorting.LastUsed => values.OrderBy(PrefabTrackingSystem.GetLastUsedIndex).ThenByDescending(PrefabNameSortKey, _sortKeyComparer),
					PrefabSorting.UIOrder => values.OrderByDescending(PrefabGroupIndex).ThenByDescending(PrefabUIOrder).ThenByDescending(PrefabNameSortKey, _sortKeyComparer),
					PrefabSorting.UpdatedDate => values.OrderBy(UpdatedDate).ThenByDescending(PrefabNameSortKey, _sortKeyComparer),
					PrefabSorting.InstalledDate => values.OrderBy(InstalledDate).ThenByDescending(PrefabNameSortKey, _sortKeyComparer),
					_ => values.OrderByDescending(PrefabNameSortKey, _sortKeyComparer),
				};
			}
			else
			{
				return Sorting switch
				{
					PrefabSorting.MostUsed => values.OrderByDescending(PrefabTrackingSystem.GetMostUsedCount).ThenBy(PrefabNameSortKey, _sortKeyComparer),
					PrefabSorting.LastUsed => values.OrderByDescending(PrefabTrackingSystem.GetLastUsedIndex).ThenBy(PrefabNameSortKey, _sortKeyComparer),
					PrefabSorting.UIOrder => values.OrderBy(PrefabGroupIndex).ThenBy(PrefabUIOrder).ThenBy(PrefabNameSortKey, _sortKeyComparer),
					PrefabSorting.UpdatedDate => values.OrderByDescending(UpdatedDate).ThenBy(PrefabNameSortKey, _sortKeyComparer),
					PrefabSorting.InstalledDate => values.OrderByDescending(InstalledDate).ThenBy(PrefabNameSortKey, _sortKeyComparer),
					_ => values.OrderBy(PrefabNameSortKey, _sortKeyComparer),
				};
			}
		}

		private static int PrefabGroupIndex(PrefabIndex prefabIndex)
		{
			return (int)prefabIndex.SubCategory;
		}

		private static int PrefabUIOrder(PrefabIndex prefabIndex)
		{
			return prefabIndex.UIOrder;
		}

		private static DateTime InstalledDate(PrefabIndex prefabIndex)
		{
			return prefabIndex.InstalledDate ?? DateTime.MinValue;
		}

		private static DateTime UpdatedDate(PrefabIndex prefabIndex)
		{
			return prefabIndex.UpdatedDate ?? DateTime.MinValue;
		}

		private static SortKey PrefabNameSortKey(PrefabIndex prefabIndex)
		{
			return prefabIndex.NameSortKey;
		}

		public static implicit operator List<PrefabIndex>(IndexedPrefabList prefabs)
		{
			return prefabs.OrderedList;
		}
	}
}
