using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;

using Game.Prefabs;
using Game.Tools;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

			var uiEntries = new List<PrefabUIEntry>();
			var startIndex = (int)(Math.Floor(scrollIndex) * columns);
			var maxIndex = startIndex + (columns * (2 + displayedRows));

			for (var i = startIndex; i < maxIndex && i < list.Count; i++)
			{
				uiEntries.Add(new PrefabUIEntry(list[i]));
			}

			return uiEntries.ToArray();
		}

		internal void TryActivatePrefabTool(int id)
		{
			// Activates a prefab using its index from PrefabIndexingSystem

			if (FindItUtil.GetPrefabBase(id) is PrefabBase prefabBase && _toolSystem.activePrefab != prefabBase)
			{
				settingPrefab = true;
				_toolSystem.ActivatePrefabTool(prefabBase);
				_ActivePrefabId.Value = id;
				settingPrefab = false;
			}
		}

		internal void ScrollTo(int id)
		{
			// Scrolls to a specific prefab using its index from PrefabIndexingSystem

			var list = FindItUtil.GetFilteredPrefabs();
			var index = list.FindIndex(x => x.Id == id);

			if (index > -1)
			{
				var columns = (float)GridUtil.GetCurrentColumnCount();
				scrollIndex = Math.Floor(index / columns);
			}
		}

		internal void TriggerSearch()
		{
			_IsSearchLoading.Value = true;

			Task.Run(DelayedSearch);
		}

		internal void ClearSearch()
		{
			_ClearSearchBar.Value = true;
			FindItUtil.Filters.CurrentSearch = string.Empty;
			filterCompleted = false;
			_IsSearchLoading.Value = false;
			_CurrentSearch.Value = string.Empty;
			searchTokenSource?.Cancel();
		}

		private async Task DelayedSearch()
		{
			// Asyncronously proesses the search method with a 250ms delay
			// the Cancellation Token is used to stop any ongoing searches if a new one is requested
			// and is used to disregard outdated results

			searchTokenSource.Cancel();
			searchTokenSource = new();

			var token = searchTokenSource.Token;

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
					// toggle the filterCompleted to signal to the OnUpdate method
					// that search has concluded and results are ready

					filterCompleted = true;
				}
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Search Failed");
			}
		}

		internal void RefreshCategoryAndSubCategory()
		{
			_CurrentCategoryBinding.Value = (int)FindItUtil.CurrentCategory;
			_CurrentSubCategoryBinding.Value = (int)FindItUtil.CurrentSubCategory;
		}

		private void OnPrefabChanged(PrefabBase prefab)
		{
#if DEBUG
			if (prefab?.name is not null)
			{
				File.WriteAllText(Path.Combine(FolderUtil.ContentFolder, "Prefab.txt"), prefab.name);
			}
#endif

			if (!settingPrefab)
			{
				ToggleFindItPanel(false);
				return;
			}

			if (prefab == null)
			{
				return;
			}

			if (_prefabSystem.TryGetEntity(prefab, out var entity))
			{
				_ActivePrefabId.Value = entity.Index;
			}
		}

		private void OnToolChanged(ToolBaseSystem tool)
		{
			if ((!settingPrefab && tool == _defaultToolSystem) || tool.toolID is "RoadBuilderTool" or "MoveItTool" or "Terrain Tool" or "Zone Tool")
			{
				ToggleFindItPanel(false);
			}
		}
	}
}
