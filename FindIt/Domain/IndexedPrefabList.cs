using FindIt.Domain.Enums;
using FindIt.Systems;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FindIt.Domain
{
	public class IndexedPrefabList : IEnumerable<PrefabIndex>
	{
		private readonly Dictionary<int, PrefabIndex> _dictionary;
		private List<PrefabIndex> _orderedList;

		public static PrefabSorting Sorting { get; set; }
		public static bool SortingDescending { get; set; }

		public IndexedPrefabList()
		{
			_dictionary = new();
		}

		public int Count => _dictionary.Count;

		public List<PrefabIndex> OrderedList => _orderedList ??= Sort(_dictionary.Values).ToList();

		public PrefabIndex this[int index]
		{
			get => _dictionary[index];
			set
			{
				_dictionary[index] = value;
				_orderedList = null;
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
			return OrderedList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return OrderedList.GetEnumerator();
		}

		internal void Remove(PrefabIndex prefabIndex)
		{
			_dictionary.Remove(prefabIndex.Id);

			ResetOrder();
		}

		internal void ResetOrder()
		{
			_orderedList = null;
		}

		private IEnumerable<PrefabIndex> Sort(IEnumerable<PrefabIndex> values)
		{
			if (SortingDescending)
			{
				return Sorting switch
				{
					PrefabSorting.MostUsed => values.OrderBy(PrefabTrackingSystem.GetMostUsedCount).ThenByDescending(PrefabName),
					PrefabSorting.LastUsed => values.OrderBy(PrefabTrackingSystem.GetLastUsedIndex).ThenByDescending(PrefabName),
					PrefabSorting.UIOrder => values.OrderByDescending(PrefabGroupIndex).ThenByDescending(PrefabUIOrder).ThenByDescending(PrefabName),
					_ => values.OrderByDescending(PrefabName),
				};
			}
			else
			{
				return Sorting switch
				{
					PrefabSorting.MostUsed => values.OrderByDescending(PrefabTrackingSystem.GetMostUsedCount).ThenBy(PrefabName),
					PrefabSorting.LastUsed => values.OrderByDescending(PrefabTrackingSystem.GetLastUsedIndex).ThenBy(PrefabName),
					PrefabSorting.UIOrder => values.OrderBy(PrefabGroupIndex).ThenBy(PrefabUIOrder).ThenBy(PrefabName),
					_ => values.OrderBy(PrefabName),
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

		private static string PrefabName(PrefabIndex prefabIndex)
		{
			return prefabIndex.Name;
		}

		public static implicit operator List<PrefabIndex>(IndexedPrefabList prefabs)
		{
			return prefabs.OrderedList;
		}
	}
}
