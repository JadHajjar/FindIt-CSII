using System.Collections.Generic;
using System.Linq;

namespace FindIt.Domain
{
	public class OptionListHelper<T>
	{
		private readonly List<T> _allValues;
		private readonly List<T> _selectedValues;
		private readonly T _allValue;
		private readonly bool _invert;

		public OptionListHelper(IEnumerable<T> values, T allValue, bool invert)
		{
			_allValues = values.ToList();
			_selectedValues = values.ToList();
			_allValue = allValue;
			_invert = invert;
		}

		public IEnumerable<T> SelectedValues => _selectedValues;

		public bool IsDefault()
		{
			return _allValues.Count == _selectedValues.Count;
		}

		public bool Contains(T value)
		{
			return _selectedValues.Contains(value);
		}

		public bool Contains(IEnumerable<T> values)
		{
			foreach (var item in values)
			{
				if (!_selectedValues.Contains(item))
				{
					return false;
				}
			}

			return true;
		}

		public bool IsSelected(T value)
		{
			if (value?.Equals(_allValue) ?? _allValue is null)
			{
				return IsDefault();
			}
			else if (_invert && IsDefault())
			{
				return false;
			}

			return Contains(value);
		}

		public void Toggle(T value)
		{
			if (value?.Equals(_allValue) ?? _allValue is null)
			{
				if (IsDefault())
				{
					_selectedValues.Clear();
				}
				else
				{
					_selectedValues.Clear();
					_selectedValues.AddRange(_allValues);
				}
			}
			else
			{
				if (_invert && IsDefault())
				{
					_selectedValues.Clear();
					_selectedValues.Add(value);
				}
				if (Contains(value))
				{
					_selectedValues.Remove(value);
				}
				else
				{
					_selectedValues.Add(value);
				}
			}
		}

		public void Reset()
		{
			_selectedValues.Clear();
			_selectedValues.AddRange(_allValues);
		}
	}
}
