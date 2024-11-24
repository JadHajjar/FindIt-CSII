using FindIt.Domain.Enums;
using FindIt.Domain.Utilities;

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
		private int lastFindIndex;

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
			if (_ShowFindItPanel == visible)
			{
				return;
			}

			if (visible)
			{
				_PanelWidth.Value = GridUtil.GetWidth();
				_PanelHeight.Value = GridUtil.GetHeight();

				FindItUtil.SetSorting();

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
			else
			{
				Mod.Log.Info(new StackTrace());
			}

			_ShowFindItPanel.Value = visible;
		}

		private void ExpandedToggled()
		{
			_PanelWidth.Value = GridUtil.GetWidth();
			_PanelHeight.Value = GridUtil.GetHeight();

			UpdateCategoriesAndPrefabList();
		}

		private void OnSearchKeyPressed()
		{
			if (_ShowFindItPanel)
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
			currentIndex = Math.Min(Math.Max(0, currentIndex), prefabs.Count);

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
			var entities = PrefabTrackingSystem.GetPlacedEntities(id);

			if (entities.Count == 0)
			{
				return;
			}

			if (lastFindId != id)
			{
				lastFindIndex = 0;
			}
			else
			{
				lastFindIndex = lastFindIndex == entities.Count - 1 ? 0 : (lastFindIndex + 1);
			}

			lastFindId = id;

			JumpTo(entities[lastFindIndex]);
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
