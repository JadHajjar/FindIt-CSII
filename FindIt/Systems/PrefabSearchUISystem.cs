using FindIt.Domain.Enums;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FindIt.Systems
{
	internal partial class PrefabSearchUISystem : ExtendedUISystemBase
	{
		private CancellationTokenSource tokenSource;

		private ValueBindingHelper<bool> _IsSearchLoading;
		private ValueBindingHelper<bool> _IsExpanded;
		private ValueBindingHelper<string> _CurrentSearch;
		private ValueBindingHelper<double> _ScrollIndex;
		private ValueBindingHelper<double> _MaxScrollIndex;
		private ValueBindingHelper<string> _ViewStyle;
		private ValueBindingHelper<CategoryUIEntry[]> _CategoryBinding;
		private ValueBindingHelper<SubCategoryUIEntry[]> _SubCategoryBinding;
		private ValueBindingHelper<PrefabUIEntry[]> _PrefabListBinding;

		private bool filterCompleted;
		private double scrollIndex;

		public string ViewStyle { get => _ViewStyle; set { _ViewStyle.Value = value; UpdateCategoriesList(); } }

		protected override void OnCreate()
		{
			base.OnCreate();

			_IsSearchLoading = CreateBinding("IsSearchLoading", false);
			_IsExpanded = CreateBinding("IsExpanded", "SetIsExpanded", false, _ => UpdateCategoriesList());
			_ScrollIndex = CreateBinding("ScrollIndex", 0D);
			_MaxScrollIndex = CreateBinding("MaxScrollIndex", 0D);
			_CategoryBinding = CreateBinding("CategoryList", new CategoryUIEntry[] { new(PrefabCategory.Any) });
			_SubCategoryBinding = CreateBinding("SubCategoryList", new SubCategoryUIEntry[] { new(PrefabSubCategory.Any) });
			_PrefabListBinding = CreateBinding("PrefabList", new PrefabUIEntry[0]);
			_CurrentSearch = CreateBinding("CurrentSearch", "");
			_ViewStyle = CreateBinding("ViewStyle", "GridWithText");

			CreateTrigger<string>("SearchChanged", t => SearchChanged(t));
			CreateTrigger<int>("OnScroll", OnScroll);
			CreateTrigger<double>("SetScrollIndex", SetScrollIndex);
		}

		private void SetScrollIndex(double index)
		{
			SetScroll(index);

			_PrefabListBinding.Value = GetDisplayedPrefabs();
		}

		private void OnScroll(int direction)
		{
			scrollIndex += Mod.Settings.ScrollSpeed * (direction > 0 ? 2f : -2f) / (_IsExpanded ? Mod.Settings.ExpandedRowCount : Mod.Settings.RowCount);

			_PrefabListBinding.Value = GetDisplayedPrefabs();
		}

		private PrefabUIEntry[] GetDisplayedPrefabs()
		{
			var list = FindItUtil.GetFilteredPrefabs();

			var columns = (float)(ViewStyle is "ListSimple" ? 4 : _IsExpanded ? Mod.Settings.ExpandedColumnCount : Mod.Settings.ColumnCount);
			var displayedRows = _IsExpanded ? Mod.Settings.ExpandedRowCount : Mod.Settings.RowCount;
			var rows = Math.Ceiling(list.Count / columns);

			scrollIndex = Math.Max(Math.Min(scrollIndex, rows - displayedRows), 0);

			_ScrollIndex.Value = scrollIndex;
			_MaxScrollIndex.Value = rows - displayedRows;

			var uiEntries = new List<PrefabUIEntry>();
			var startIndex = (int)(Math.Floor(scrollIndex) * columns);
			var maxIndex = startIndex + (int)(columns * (2 + displayedRows));

			for (var i = startIndex; i < maxIndex && i < list.Count; i++)
			{
				uiEntries.Add(new PrefabUIEntry(list[i]));
			}

			return uiEntries.ToArray();
		}

		internal PrefabUIEntry UpdateCategoriesList()
		{
			_CategoryBinding.Value = FindItUtil.GetCategories().Select(x => new CategoryUIEntry(x)).ToArray();
			_SubCategoryBinding.Value = FindItUtil.GetSubCategories().Select(x => new SubCategoryUIEntry(x)).ToArray();

			var prefabs = GetDisplayedPrefabs();

			_PrefabListBinding.Value = prefabs;

			return prefabs.FirstOrDefault();
		}

		internal void SetScroll(double index)
		{
			scrollIndex = index;
		}

		internal void ScrollTo(int id)
		{
			var list = FindItUtil.GetFilteredPrefabs();
			var index = list.FindIndex(x => x.Id == id);

			if (index > -1)
			{
				var columns = ViewStyle is "ListSimple" ? 4 : (double)(_IsExpanded ? Mod.Settings.ExpandedColumnCount : Mod.Settings.ColumnCount);
				scrollIndex = Math.Floor(index / columns);
			}
		}

		private void SearchChanged(string text)
		{
			text = text.Replace("\r", "").Replace("\n", "");

			if (_CurrentSearch == text && FindItUtil.Filters.CurrentSearch == text)
			{
				return;
			}

			FindItUtil.Filters.CurrentSearch = text.Trim();

			_CurrentSearch.Value = text;

			TriggerSearch();
		}

		internal void TriggerSearch()
		{
			_IsSearchLoading.Value = true;

			Task.Run(DelayedSearch);
		}

		internal void ClearSearch()
		{
			FindItUtil.Filters.CurrentSearch = string.Empty;
			filterCompleted = false;
			_IsSearchLoading.Value = false;
			_CurrentSearch.Value = string.Empty;
			tokenSource?.Cancel();
		}

		private async Task DelayedSearch()
		{
			tokenSource?.Cancel();
			tokenSource = new();

			var token = tokenSource.Token;

			await Task.Delay(250);

			if (token.IsCancellationRequested)
			{
				return;
			}

			try
			{
				FindItUtil.ProcessSearch(token);

				if (!token.IsCancellationRequested)
				{
					filterCompleted = true;
				}
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Search Failed");
			}
		}

		protected override void OnUpdate()
		{
			if (filterCompleted)
			{
				filterCompleted = false;
				scrollIndex = 0;

				_IsSearchLoading.Value = false;
				_PrefabListBinding.Value = GetDisplayedPrefabs();
			}

			base.OnUpdate();
		}
	}
}
