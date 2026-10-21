using FindIt.Domain;
using FindIt.Domain.Enums;
using FindIt.Domain.UIBinding;
using FindIt.Utilities;

using Game.Prefabs;
using Game.Tools;
using Game.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FindIt.Systems
{
	internal partial class FindItUISystem : ExtendedUISystemBase
	{
		internal void UpdateCategoriesAndPrefabList()
		{
			_CategoryBinding.Value = FindItUtil.GetCategories().Select(x => new CategoryUIEntry(x)).ToArray();
			_SubCategoryBinding.Value = FindItUtil.GetSubCategories().Select(x => new SubCategoryUIEntry(x)).ToArray();

			var prefabs = GetDisplayedPrefabs();

			_PrefabListBinding.Value = prefabs;
		}

		private PrefabUIEntry[] GetDisplayedPrefabs()
		{
			// in charge of paging prefabs and getting only the displayed prefabs
			// also updates display binding values like columns, rows, scroll, etc.

			var list = FindItUtil.GetFilteredPrefabs();

			var columns = GridUtil.GetCurrentColumnCount();
			var displayedRows = GridUtil.GetCurrentRowCount();
			var rows = Math.Ceiling(list.Count / (float)columns);

			scrollIndex = Math.Max(Math.Min(scrollIndex, rows - displayedRows), 0);

			_ScrollIndex.Value = scrollIndex;
			_MaxScrollIndex.Value = rows - displayedRows;
			_ColumnCount.Value = columns;
			_RowCount.Value = displayedRows;
			_PrefabCountBinding.Value = string.Format(LocaleHelper.GetTooltip("ItemCount"), list.Count.ToString("N0"));

			var uiEntries = new List<PrefabUIEntry>();
			var startIndex = (int)(Math.Floor(scrollIndex) * columns);
			var maxIndex = startIndex + (columns * (2 + displayedRows));

			for (var i = startIndex; i < maxIndex && i < list.Count; i++)
			{
				var prefab = list[i];

				if (prefab.IsRandom && prefab.RandomPrefabBases != null)
				{
					prefab.RandomPrefabThumbnails ??= prefab.RandomPrefabBases
						.Select(x => x is null ? null : GetThumbnail(prefab, x))
						.ToArray();
				}
				else
				{
					prefab.Thumbnail ??= GetThumbnail(prefab, prefab.Prefab);
				}

				uiEntries.Add(new PrefabUIEntry(prefab));
			}

			return uiEntries.ToArray();
		}

		private string GetThumbnail(PrefabIndexBase prefabIndex, PrefabBase prefab)
		{
			try
			{
				return ImageSystem.GetThumbnail(prefab);
			}
			catch (Exception ex)
			{
				Mod.Log.Error(
					ex,
					$"Failed to get the thumbnail of prefab '{prefabIndex.PrefabName}'");

				return prefabIndex.FallbackThumbnail;
			}
		}

		internal void TryActivatePrefabTool(int id)
		{
			if (FindItUtil.GetPrefabBase(id) is not PrefabBase prefabBase)
			{
				return;
			}

			if (_IsPrefabSelection && prefabSelectionTask != null)
			{
				// Send the selected prefab back to the caller and close the panel if we're in prefab selection mode
				EndPrefabSelection(prefabBase);

				ToggleFindItPanel(false);

				return;
			}

			if (_toolSystem.activePrefab == prefabBase)
			{
				return;
			}

			// Activates a prefab using its index from PrefabIndexingSystem
			settingPrefab = true;
			_toolSystem.ActivatePrefabTool(prefabBase);
			_ActivePrefabId.Value = id;
			settingPrefab = false;

			_rangeAnchorId = id;
			RefreshSelectionBinding();
		}

		internal void ScrollTo(int id)
		{
			// Scrolls to a specific prefab using its index from PrefabIndexingSystem

			if (searchTask is not null && !searchTokenSource.IsCancellationRequested)
			{
				scrollToAfterSearch = id;

				return;
			}

			var list = FindItUtil.GetFilteredPrefabs();
			var index = list.FindIndex(x => x.Id == id);

			if (index > -1)
			{
				var columns = (float)GridUtil.GetCurrentColumnCount();
				var rows = GridUtil.GetCurrentRowCount();

				scrollIndex = Math.Max(0, Math.Floor(index / columns) - (rows / 4));
			}
		}

		internal void TriggerSearch(bool resetScroll = true)
		{
			resetScrollAfterSearch = resetScroll;
			scrollToAfterSearch = 0;

			_IsSearchLoading.Value = true;

			if (FindItUtil.IsReady)
			{
				searchTokenSource.Cancel();
				searchTokenSource = new();

				var token = searchTokenSource.Token;

				searchTask = Task.Run(() => DelayedSearch(token));
			}
		}

		internal void RefreshPanel()
		{
			if (!_ShowFindItPanel)
			{
				return;
			}

			UpdateCategoriesAndPrefabList();

			_optionsUISystem.RefreshOptions();
		}

		internal void ClearSearch()
		{
			_ClearSearchBar.Value = true;
			FindItUtil.Filters.CurrentSearch = string.Empty;
			_CurrentSearch.Value = string.Empty;

			if (FindItUtil.Filters.GetFilterList().Any())
			{
				TriggerSearch();
			}
			else
			{
				_IsSearchLoading.Value = false;
				searchTokenSource.Cancel();
			}
		}

		internal void OnFavoritesChanged()
		{
			if (!FindItUtil.IsReady)
			{
				return;
			}

			ResetEmptySubCategory();

			UpdateCategoriesAndPrefabList();

			if (FindItUtil.CurrentCategory is PrefabCategory.Favorite && FindItUtil.Filters.GetFilterList().Any())
			{
				TriggerSearch(false);
			}
		}

		// An index pass or a change to the favorites can empty or remove the selected subcategory's
		// list, and an empty subcategory has no tab.
		internal void ResetEmptySubCategory()
		{
			var subCategories = FindItUtil.CategorizedPrefabs[FindItUtil.CurrentCategory];

			if (FindItUtil.CurrentSubCategory is not PrefabSubCategory.Any
				&& !(subCategories.TryGetValue(FindItUtil.CurrentSubCategory, out var list)
					&& list.Count > 0))
			{
				FindItUtil.CurrentSubCategory = PrefabSubCategory.Any;

				RefreshCategoryAndSubCategory();
			}
		}

		private void OnToggleFavorited(int id)
		{
			FindItUtil.ToggleFavorited(id);

			OnFavoritesChanged();
		}

		internal void CancelSearch()
		{
			searchTokenSource.Cancel();
		}

		private async Task<List<PrefabIndex>> DelayedSearch(CancellationToken token)
		{
			// Asyncronously proesses the search method with a 250ms delay
			// the Cancellation Token is used to stop any ongoing searches if a new one is requested
			// and is used to disregard outdated results

			await Task.Delay(250);

			if (token.IsCancellationRequested)
			{
				return null;
			}

			try
			{
				return FindItUtil.ProcessSearch(token);
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Search Failed");

				return new();
			}
		}

		internal void RefreshCategoryAndSubCategory()
		{
			_CurrentCategoryBinding.Value = (int)FindItUtil.CurrentCategory;
			_CurrentSubCategoryBinding.Value = (int)FindItUtil.CurrentSubCategory;
		}

		private void OnPrefabChanged(PrefabBase prefab)
		{
			if (!settingPrefab)
			{
				ToggleFindItPanel(false);
				return;
			}

			if (prefab == null)
			{
				_ActivePrefabId.Value = 0;
			}
			else if (_prefabSystem.TryGetEntity(prefab, out var entity))
			{
				_ActivePrefabId.Value = entity.Index;
			}

			// Mirror whatever the (possibly multi-select) tool now reports as selected.
			RefreshSelectionBinding();
		}

		private void OnToolChanged(ToolBaseSystem tool)
		{
			// Detect/cache whether the newly active tool supports prefab multi-selection,
			// then reflect its current selection in the UI.
			CacheMultiSelectSupport(tool);
			RefreshSelectionBinding();

			if ((!settingPrefab && tool == _defaultToolSystem) || tool.toolID is "RoadBuilderTool" or "MoveItTool" or "Terrain Tool" or "Zone Tool")
			{
				ToggleFindItPanel(false);
			}
		}

		public void SetAllThumbnails(IEnumerable<string> thumbnails)
		{
			_AllThumbnails.Value = thumbnails.ToArray();
		}

		internal Task<PrefabBase> PickPrefab(Type prefabType, string title)
		{
			if (!FindItUtil.IsReady)
			{
				Mod.Log.Warn("PickPrefab called before FindIt finished indexing prefabs.");
				return Task.FromResult<PrefabBase>(null);
			}

			EndPrefabSelection(null);

			_IsPrefabSelection.Value = true;

			ToggleFindItPanel(true, false);

			FindItUtil.Filters.SelectedType = prefabType;
			FindItUtil.CurrentCategory = PrefabCategory.Any;
			FindItUtil.CurrentSubCategory = PrefabSubCategory.Any;

			scrollIndex = 0;

			RefreshCategoryAndSubCategory();
			UpdateCategoriesAndPrefabList();

			TriggerSearch();

			_optionsUISystem.RefreshOptions();

			_PrefabSelectionTitle.Value = LocaleHelper.Translate(title ?? "Tooltip.TITLE[FindIt.SelectPrefab]", "Select an asset");

			prefabSelectionTask = new();

			return prefabSelectionTask.Task;
		}

		private void EndPrefabSelection(PrefabBase result)
		{
			var task = prefabSelectionTask;
			var wasSelecting = _IsPrefabSelection.Value;

			prefabSelectionTask = null;

			FindItUtil.Filters.SelectedType = null;
			_IsPrefabSelection.Value = false;

			if (wasSelecting && _ShowFindItPanel && _IsWindowLocked)
			{
				RefreshPanelSize();
				_optionsUISystem.RefreshOptions();
				TriggerSearch();
			}

			task?.TrySetResult(result);
		}
	}
}
