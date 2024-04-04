using Colossal.UI.Binding;

using FindIt.Domain.Enums;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;

using Game.UI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FindIt.Systems
{
	internal partial class PrefabSearchUISystem : UISystemBase
	{
		private CancellationTokenSource tokenSource;

		private ValueBinding<bool> _IsSearchLoading;
		private ValueBinding<string> _CurrentSearch;
		private ValueBinding<double> _ScrollIndex;
		private ValueBinding<double> _MaxScrollIndex;
		private ValueBinding<CategoryUIEntry[]> _CategoryBinding;
		private ValueBinding<SubCategoryUIEntry[]> _SubCategoryBinding;
		private ValueBinding<PrefabUIEntry[]> _PrefabListBinding;

		private bool filterCompleted;
		private double scrollIndex;
		private readonly double columns = 8;
		private readonly double displayedRows = 2;

		protected override void OnCreate()
		{
			base.OnCreate();

			AddBinding(_IsSearchLoading = new ValueBinding<bool>(Mod.Id, "IsSearchLoading", false));
			AddBinding(_ScrollIndex = new ValueBinding<double>(Mod.Id, "ScrollIndex", 0));
			AddBinding(_MaxScrollIndex = new ValueBinding<double>(Mod.Id, "MaxScrollIndex", 0));
			AddBinding(_CategoryBinding = new ValueBinding<CategoryUIEntry[]>(Mod.Id, "CategoryList", new CategoryUIEntry[] { new(PrefabCategory.Any) }, new ArrayWriter<CategoryUIEntry>(new ValueWriter<CategoryUIEntry>())));
			AddBinding(_SubCategoryBinding = new ValueBinding<SubCategoryUIEntry[]>(Mod.Id, "SubCategoryList", new SubCategoryUIEntry[] { new(PrefabSubCategory.Any) }, new ArrayWriter<SubCategoryUIEntry>(new ValueWriter<SubCategoryUIEntry>())));
			AddBinding(_PrefabListBinding = new ValueBinding<PrefabUIEntry[]>(Mod.Id, "PrefabList", new PrefabUIEntry[0], new ArrayWriter<PrefabUIEntry>(new ValueWriter<PrefabUIEntry>())));
			AddBinding(_CurrentSearch = new ValueBinding<string>(Mod.Id, "CurrentSearch", ""));

			AddBinding(new TriggerBinding<string>(Mod.Id, "SearchChanged", SearchChanged));
			AddBinding(new TriggerBinding<int>(Mod.Id, "OnScroll", OnScroll));
			AddBinding(new TriggerBinding<int>(Mod.Id, "SetScrollIndex", SetScrollIndex));
		}

		private void SetScrollIndex(int index)
		{
			SetScroll(index);

			_PrefabListBinding.Update(GetDisplayedPrefabs());
		}

		private void OnScroll(int direction)
		{
			scrollIndex += Mod.Settings.ScrollSpeed * (direction > 0 ? 1 : -1);

			_PrefabListBinding.Update(GetDisplayedPrefabs());
		}

		private PrefabUIEntry[] GetDisplayedPrefabs()
		{
			var list = FindItUtil.GetFilteredPrefabs();

			var rows = Math.Ceiling(list.Count / columns);

			scrollIndex = Math.Min(Math.Max(scrollIndex, 0), rows - displayedRows);

			_ScrollIndex.Update(scrollIndex);
			_MaxScrollIndex.Update(rows - displayedRows);

			var uiEntries = new List<PrefabUIEntry>();
			var startIndex = (int)(Math.Floor(scrollIndex) * columns);
			var maxIndex = startIndex + (int)(columns * (1 + displayedRows));

			for (var i = startIndex; i < maxIndex && i < list.Count; i++)
			{
				uiEntries.Add(new PrefabUIEntry(list[i]));
			}

			return uiEntries.ToArray();
		}

		internal PrefabUIEntry UpdateCategoriesList()
		{
			_CategoryBinding.Update(FindItUtil.GetCategories().Select(x => new CategoryUIEntry(x)).ToArray());
			_SubCategoryBinding.Update(FindItUtil.GetSubCategories().Select(x => new SubCategoryUIEntry(x)).ToArray());

			var prefabs = GetDisplayedPrefabs();

			_PrefabListBinding.Update(prefabs);

			return prefabs.FirstOrDefault();
		}

		internal void SetScroll(double index)
		{
			scrollIndex = index;
		}

		public void SearchChanged(string text)
		{
			FindItUtil.CurrentSearch = text;

			_IsSearchLoading.Update(true);
			_CurrentSearch.Update(text);

			Task.Run(DelayedSearch);
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

				_IsSearchLoading.Update(false);
				_PrefabListBinding.Update(GetDisplayedPrefabs());
			}

			base.OnUpdate();
		}
	}
}
