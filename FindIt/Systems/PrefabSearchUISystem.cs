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
		private ValueBindingHelper<string> _CurrentSearch;
		private ValueBindingHelper<double> _ScrollIndex;
		private ValueBindingHelper<double> _MaxScrollIndex;
		private ValueBindingHelper<double> _ColumnCount;
		private ValueBindingHelper<double> _RowCount;
		private ValueBindingHelper<string> _ViewStyle;
		private ValueBindingHelper<CategoryUIEntry[]> _CategoryBinding;
		private ValueBindingHelper<SubCategoryUIEntry[]> _SubCategoryBinding;
		private ValueBindingHelper<PrefabUIEntry[]> _PrefabListBinding;

		private bool filterCompleted;
		private double scrollIndex;

		public string ViewStyle
		{
			get => _ViewStyle;
			set
			{
				Mod.Settings.DefaultViewStyle = _ViewStyle.Value = value;
				Mod.Settings.ApplyAndSave();
				UpdateCategoriesList();
			}
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			_IsSearchLoading = CreateBinding("IsSearchLoading", false);
			_ScrollIndex = CreateBinding("ScrollIndex", 0D);
			_MaxScrollIndex = CreateBinding("MaxScrollIndex", 0D);
			_ColumnCount = CreateBinding("ColumnCount", 0D);
			_RowCount = CreateBinding("RowCount", 0D);
			_CategoryBinding = CreateBinding("CategoryList", new CategoryUIEntry[] { new(PrefabCategory.Any) });
			_SubCategoryBinding = CreateBinding("SubCategoryList", new SubCategoryUIEntry[] { new(PrefabSubCategory.Any) });
			_PrefabListBinding = CreateBinding("PrefabList", new PrefabUIEntry[0]);
			_CurrentSearch = CreateBinding("CurrentSearch", "");
			_ViewStyle = CreateBinding("ViewStyle", Mod.Settings.DefaultViewStyle);

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
			scrollIndex += Mod.Settings.ScrollSpeed * (direction > 0 ? 2f : -2f) * GridUtil.GetScrollMultiplier() / GridUtil.GetCurrentRowCount();

			_PrefabListBinding.Value = GetDisplayedPrefabs();
		}

		private PrefabUIEntry[] GetDisplayedPrefabs()
		{
			var list = FindItUtil.GetFilteredPrefabs();

			var columns = GridUtil.GetCurrentColumnCount();
			var displayedRows = GridUtil.GetCurrentRowCount();
			var rows = Math.Ceiling(list.Count / (float)columns);

			scrollIndex = Math.Max(Math.Min(scrollIndex, rows - displayedRows), 0);

			_ScrollIndex.Value = scrollIndex;
			_MaxScrollIndex.Value = rows - displayedRows;
			_ColumnCount.Value = columns;
			_RowCount.Value = displayedRows;

			var uiEntries = new List<PrefabUIEntry>();
			var startIndex = (int)(Math.Floor(scrollIndex) * columns);
			var maxIndex = startIndex + (columns * (2 + displayedRows));

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
				var columns = (float)GridUtil.GetCurrentColumnCount();
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
