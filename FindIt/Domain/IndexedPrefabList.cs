using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindIt.Domain
{
	public class IndexedPrefabList : IEnumerable<PrefabIndex>
	{
		private readonly Dictionary<int, PrefabIndex> _dictionary;
		private List<PrefabIndex> _orderedList;

        public IndexedPrefabList()
        {
			_dictionary = new();
		}

		public int Count => _dictionary.Count;

		public List<PrefabIndex> OrderedList => _orderedList ??= _dictionary.Values.OrderBy(x => x.Name).ToList();

		public PrefabIndex this[int index]
		{
			get => _dictionary[index];
			set
			{
				_dictionary[index] = value;
				_orderedList = null;
			}
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
			_orderedList?.Remove(prefabIndex);
		}

		public static implicit operator List<PrefabIndex>(IndexedPrefabList prefabs) => prefabs.OrderedList;
	}
}
