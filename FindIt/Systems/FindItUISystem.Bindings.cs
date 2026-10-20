using FindIt.Domain;
using FindIt.Domain.Enums;
using FindIt.Utilities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Unity.Entities;

namespace FindIt.Systems
{
    internal partial class FindItUISystem : ExtendedUISystemBase
	{
		private int lastFindId;
		private Entity lastFoundEntity;

		private void FindItIconClicked()
		{
			if (_ShowFindItPanel)
			{
				ToggleFindItPanel(false);

				// Clear the selected prefab
				_toolSystem.ActivatePrefabTool(null);
			}
			else
			{
				ToggleFindItPanel(true);
			}
		}

		private void SetCurrentCategory(int category)
		{
			FindItUtil.CurrentCategory = (PrefabCategory)category;

			_CurrentSubCategoryBinding.Value = (int)PrefabSubCategory.Any;

			SetCurrentSubCategory((int)PrefabSubCategory.Any);
		}

		private void SetCurrentSubCategory(int category)
		{
			FindItUtil.CurrentSubCategory = (PrefabSubCategory)category;

			scrollIndex = 0;

			UpdateCategoriesAndPrefabList();

			if (FindItUtil.Filters.GetFilterList().Any()) // Check if there are any active filters
			{
				// Trigger the delayed search instead of refreshing the list immediately

				TriggerSearch();
			}

			_optionsUISystem.RefreshOptions();
		}

		internal void ToggleFindItPanel(bool visible, bool activatePrefab = true)
		{
			if (_ShowFindItPanel == visible || (_IsWindowLocked && _ShowFindItPanel))
			{
				return;
			}

			_ShowFindItPanel.Value = visible;
			_prefabTrackingSystem.Enabled = visible;

			if (!visible)
			{
				// Reset prefab selection state when closing the panel
				_TagEditorPrefabId.Value = 0;
				EndPrefabSelection(null);

				return;
			}

			_PanelWidth.Value = GridUtil.GetWidth();
			_PanelHeight.Value = GridUtil.GetHeight();

			// The "Most used" sort reads the counts, so they're counted before the lists sort.
			_prefabTrackingSystem.CountPlacedPrefabs();

			// The usage these sorts read changes while the panel is closed.
			// The index resets the lists it changes.
			if (IndexedPrefabList.IsSortedByUsage)
			{
				FindItUtil.SetSorting();
			}

			UpdateCategoriesAndPrefabList();

			_optionsUISystem.RefreshOptions();

			if (activatePrefab && Mod.Settings.SelectPrefabOnOpen)
			{
				TryActivatePrefabTool(_ActivePrefabId);

				var prefabs = FindItUtil.GetFilteredPrefabs();
				var columns = GridUtil.GetCurrentColumnCount();
				var rows = GridUtil.GetCurrentRowCount();
				var index = prefabs.IndexOf(FindItUtil.GetPrefabIndex(_ActivePrefabId));

				SetScrollIndex(Math.Max(0, Math.Floor(index / columns) - (rows / 4)));
			}
		}

		private void ToggleLock()
		{
			_IsWindowLocked.Value = !_IsWindowLocked;
		}

		private void ExpandedToggled()
		{
			RefreshPanelSize();

			_optionsUISystem.RefreshOptions();
		}

		private void RefreshPanelSize()
		{
			_PanelWidth.Value = GridUtil.GetWidth();
			_PanelHeight.Value = GridUtil.GetHeight();

			UpdateCategoriesAndPrefabList();
		}

		private void SetPanelSize(ValueBindingHelper<float> binding, float value, Action<float> persist)
		{
			value = Math.Max(FindItSettings.MinPanelSize, Math.Min(FindItSettings.MaxPanelSize, value));

			persist(value);
			binding.Value = value;

			Mod.Settings.ApplyAndSave();

			RefreshPanelSize();
		}

		internal void SyncViewSizesFromSettings()
		{
			_RowSize.Value = Mod.Settings.RowSize;
			_ColumnSize.Value = Mod.Settings.ColumnSize;
			_ExpandedRowSize.Value = Mod.Settings.ExpandedRowSize;
			_ExpandedColumnSize.Value = Mod.Settings.ExpandedColumnSize;
			_RightRowSize.Value = Mod.Settings.RightRowSize;
			_RightColumnSize.Value = Mod.Settings.RightColumnSize;

			RefreshPanelSize();
			_optionsUISystem.RefreshOptions();
		}

		private void OnSearchKeyPressed()
		{
			if (_ShowFindItPanel || _IsWindowLocked)
			{
				_FocusSearchBar.Value = true;
			}
			else
			{
				ToggleFindItPanel(true);
			}
		}

		private void SetScrollIndex(double index)
		{
			if (scrollIndex == index)
			{
				return;
			}

			scrollIndex = index;

			if (Mod.Settings.SmoothScroll)
			{
				_PrefabListBinding.Value = GetDisplayedPrefabs();
			}
			else
			{
				Task.Run(DelayedApplyScroll);
			}
		}

		private void OnScroll(int direction)
		{
			SetScrollIndex(scrollIndex +
				(Mod.Settings.ScrollSpeed
				* (direction > 0 ? 1f : -1f)
				* GridUtil.GetScrollMultiplier()
				/ GridUtil.GetCurrentRowCount()));
		}

		private async Task DelayedApplyScroll()
		{
			var token = scrollTokenSource.Token;

			await Task.Delay(50);

			if (!token.IsCancellationRequested)
			{
				scrollTokenSource.Cancel();
				scrollTokenSource = new();

				scrollCompleted = true;
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
			_CurrentSearch.ForceUpdate();

			TriggerSearch();
		}

		private void MoveSelectedItemGrid(int x, int y)
		{
			var prefabs = FindItUtil.GetFilteredPrefabs();
			var columns = GridUtil.GetCurrentColumnCount();
			var rows = GridUtil.GetCurrentRowCount();
			var currentIndex = prefabs.IndexOf(FindItUtil.GetPrefabIndex(_ActivePrefabId));

			if (prefabs.Count == 0)
			{
				return;
			}

			currentIndex += x + (y * (int)Math.Floor(columns));
			currentIndex = Math.Min(Math.Max(0, currentIndex), prefabs.Count - 1);

			TryActivatePrefabTool(prefabs[currentIndex].Id);
			SetScrollIndex(Math.Max(0, Math.Floor(currentIndex / columns) - (rows / 4)));
		}

		private void OnRandomButtonClicked()
		{
			var random = new Random(Guid.NewGuid().GetHashCode());
			var prefabs = FindItUtil.GetFilteredPrefabs();
			var columns = GridUtil.GetCurrentColumnCount();
			var rows = GridUtil.GetCurrentRowCount();

			if (prefabs.Count == 0)
			{
				return;
			}

			var index = random.Next(prefabs.Count);

			TryActivatePrefabTool(prefabs[index].Id);
			SetScrollIndex(Math.Max(0, Math.Floor(index / columns) - (rows / 4)));
		}

		private void OnLocateButtonClicked(int id)
		{
			var previous = lastFindId == id ? lastFoundEntity : Entity.Null;
			var entity = _prefabTrackingSystem.GetNextPlacedEntity(id, previous);

			if (entity == Entity.Null)
			{
				return;
			}

			lastFindId = id;
			lastFoundEntity = entity;

			JumpTo(entity);
		}

		private void OnPdxModsButtonClicked(int id)
		{
			try
			//{ Process.Start($"skyve://mods/{FindItUtil.GetPrefabIndex(id).PdxModsId}"); }
			{
				Process.Start($"https://mods.paradoxplaza.com/mods/{FindItUtil.GetPrefabIndex(id).PdxModsId}/Windows");
			}
			catch { }
		}

		private void JumpTo(Entity entity)
		{
			if (_cameraUpdateSystem.orbitCameraController != null && entity != Entity.Null)
			{
				_cameraUpdateSystem.orbitCameraController.followedEntity = entity;
				_cameraUpdateSystem.orbitCameraController.TryMatchPosition(_cameraUpdateSystem.activeCameraController);
				_cameraUpdateSystem.activeCameraController = _cameraUpdateSystem.orbitCameraController;
			}
		}
	}
}
