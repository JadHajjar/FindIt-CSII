﻿using FindIt.Domain.Enums;
using FindIt.Domain.Utilities;

using System.Linq;

using UnityEngine.InputSystem;

namespace FindIt.Systems
{
	internal partial class FindItUISystem : ExtendedUISystemBase
	{
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

			if (FindItUtil.Filters.GetFilterList().Any()) // Check if there are any active filters
			{
				// Trigger the delayed search instead of refreshing the list immediately

				TriggerSearch();
			}
			else
			{
				UpdateCategoriesAndPrefabList();
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

				var prefab = UpdateCategoriesAndPrefabList();

				_optionsUISystem.RefreshOptions();

				if (activatePrefab && Mod.Settings.SelectPrefabOnOpen)
				{
					TryActivatePrefabTool(prefab.Id);
				}
			}

			_ShowFindItPanel.Value = visible;
		}

		private void ExpandedToggled()
		{
			_PanelWidth.Value = GridUtil.GetWidth();
			_PanelHeight.Value = GridUtil.GetHeight();

			UpdateCategoriesAndPrefabList();
		}

		private void OnCtrlFKeyPressed(InputAction.CallbackContext context)
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
			scrollIndex = index;

			_PrefabListBinding.Value = GetDisplayedPrefabs();
		}

		private void OnScroll(int direction)
		{
			SetScrollIndex(scrollIndex + 
				Mod.Settings.ScrollSpeed
				* (direction > 0 ? 1f : -1f) 
				* GridUtil.GetScrollMultiplier() 
				/ GridUtil.GetCurrentRowCount());
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
	}
}
