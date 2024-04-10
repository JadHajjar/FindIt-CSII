using Colossal.UI.Binding;

using FindIt.Domain.Enums;
using FindIt.Domain.Utilities;

using Game.Prefabs;
using Game.Tools;
using Game.UI;

using System;
using System.IO;
using System.Threading.Tasks;

using Unity.Entities;

using UnityEngine.InputSystem;

namespace FindIt.Systems
{
	internal partial class FindItPanelUISystem : UISystemBase
	{
		private ValueBinding<bool> _FocusSearchBar;
		private ValueBinding<bool> _ClearSearchBar;
		private ValueBinding<bool> _ShowFindItPanel;
		private ValueBinding<int> _ActivePrefabId;
		private ValueBinding<int> _CurrentCategoryBinding;
		private ValueBinding<int> _CurrentSubCategoryBinding;
		private ValueBinding<float> _RowCount;
		private ValueBinding<float> _ColumnCount;

		private ToolSystem _ToolSystem;
		private PrefabSystem _PrefabSystem;
		private PrefabSearchUISystem _PrefabSearchUISystem;
		private DefaultToolSystem _DefaultToolSystem;
		private bool settingPrefab;

		protected override void OnCreate()
		{
			base.OnCreate();

			_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_PrefabSearchUISystem = World.GetOrCreateSystemManaged<PrefabSearchUISystem>();
			_DefaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();

			// ToolSystem toolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>(); // I don't know why vanilla game did this.
			_ToolSystem.EventPrefabChanged += OnPrefabChanged;
			_ToolSystem.EventToolChanged += OnToolChanged;

			// These establish the bindings for the categories
			AddBinding(_CurrentCategoryBinding = new ValueBinding<int>(Mod.Id, "CurrentCategory", (int)FindItUtil.CurrentCategory));
			AddBinding(_CurrentSubCategoryBinding = new ValueBinding<int>(Mod.Id, "CurrentSubCategory", (int)FindItUtil.CurrentSubCategory));
			AddBinding(new TriggerBinding<int>(Mod.Id, "SetCurrentCategory", SetCurrentCategory));
			AddBinding(new TriggerBinding<int>(Mod.Id, "SetCurrentSubCategory", SetCurrentSubCategory));

			// These establish the bindings with UI code.
			AddBinding(_FocusSearchBar = new ValueBinding<bool>(Mod.Id, "FocusSearchBar", false));
			AddBinding(_ClearSearchBar = new ValueBinding<bool>(Mod.Id, "ClearSearchBar", false));
			AddBinding(_ShowFindItPanel = new ValueBinding<bool>(Mod.Id, "ShowFindItPanel", false));
			AddBinding(_ActivePrefabId = new ValueBinding<int>(Mod.Id, "ActivePrefabId", 0));
			AddBinding(_RowCount = new ValueBinding<float>(Mod.Id, "RowCount", 2f));
			AddBinding(_ColumnCount = new ValueBinding<float>(Mod.Id, "ColumnCount", 8f));

			// These establish listeners to trigger events from UI.
			AddBinding(new TriggerBinding(Mod.Id, "FindItCloseToggled", () => ToggleFindItPanel(false)));
			AddBinding(new TriggerBinding(Mod.Id, "FindItIconToggled", FindItIconClicked));
			AddBinding(new TriggerBinding<int>(Mod.Id, "SetCurrentPrefab", TryActivatePrefabTool));
			AddBinding(new TriggerBinding<int>(Mod.Id, "ToggleFavorited", ToggleFavorited));
			AddBinding(new TriggerBinding(Mod.Id, "OnSearchFocused", () => _FocusSearchBar.Update(false)));
			AddBinding(new TriggerBinding(Mod.Id, "OnSearchCleared", () => _ClearSearchBar.Update(false)));

			// This setup a keybinding for activating FindItPanel.
			InputAction hotKeyCtrlF = new($"{Mod.Id}/CtrlF");
			hotKeyCtrlF.AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Keyboard>/ctrl").With("Button", "<Keyboard>/f");
			hotKeyCtrlF.performed += OnCtrlFKeyPressed;
			hotKeyCtrlF.Enable();
		}

		private void FindItIconClicked()
		{
			if (_ShowFindItPanel.value)
			{
				ToggleFindItPanel(false);
				_ToolSystem.ActivatePrefabTool(null);
			}
			else
			{
				ToggleFindItPanel(true);
			}
		}

		private void SetCurrentCategory(int category)
		{
			FindItUtil.CurrentCategory = (PrefabCategory)category;

			_CurrentCategoryBinding.Update((int)FindItUtil.CurrentCategory);

			SetCurrentSubCategory((int)PrefabSubCategory.Any);
		}

		private void SetCurrentSubCategory(int category)
		{
			FindItUtil.CurrentSubCategory = (PrefabSubCategory)category;

			_CurrentSubCategoryBinding.Update((int)FindItUtil.CurrentSubCategory);

			_PrefabSearchUISystem.SetScroll(0);
			_PrefabSearchUISystem.UpdateCategoriesList();

			if (!string.IsNullOrWhiteSpace(FindItUtil.CurrentSearch))
			{
				_PrefabSearchUISystem.SearchChanged(FindItUtil.CurrentSearch, true);
			}
		}

		private void ToggleFavorited(int id)
		{
			FindItUtil.ToggleFavorited(id);
		}

		/// <summary>
		/// This event toggles the ShowFindItPanel binding.
		/// </summary>
		internal void ToggleFindItPanel(bool visible, bool activatePrefab = true)
		{
			if (_ShowFindItPanel.value == visible)
			{
				return;
			}

			if (visible)
			{
				_RowCount.Update(Mod.Settings.RowCount);
				_ColumnCount.Update(Mod.Settings.ColumnCount);

				var prefab = _PrefabSearchUISystem.UpdateCategoriesList();

				if (activatePrefab && Mod.Settings.SelectPrefabOnOpen)
				{
					TryActivatePrefabTool(prefab.Id);
				}
			}

			FindItUtil.IsActive = visible;
			_ShowFindItPanel.Update(visible);
		}

		/// <summary>
		/// This handles the keybinding pressed event.
		/// </summary>
		/// <param name="context">Not used yet.</param>
		private void OnCtrlFKeyPressed(InputAction.CallbackContext context)
		{
			if (_ShowFindItPanel.value)
			{
				_FocusSearchBar.Update(true);
			}
			else
			{
				ToggleFindItPanel(true);
			}
		}


		/// <summary>
		/// If prefab entity is valid activate prefab tool for that prefab base.
		/// </summary>
		/// <param name="e">Entity from UI.</param>
		internal void TryActivatePrefabTool(int id)
		{
			if (FindItUtil.GetPrefabBase(id) is PrefabBase prefabBase)
			{
				settingPrefab = true;
				_ToolSystem.ActivatePrefabTool(prefabBase);
				_ActivePrefabId.Update(id);
				settingPrefab = false;
			}
		}

		/// <summary>
		/// The event happens after the toolsystem changes prefabs.
		/// </summary>
		/// <param name="prefab"></param>
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

			if (_PrefabSystem.TryGetEntity(prefab, out var entity))
			{
				_ActivePrefabId.Update(entity.Index);
			}
		}

		/// <summary>
		/// The event happens after the toolsystem changes tools.
		/// </summary>
		/// <param name="tool"></param>
		private void OnToolChanged(ToolBaseSystem tool)
		{
			if (!settingPrefab && tool == _DefaultToolSystem || tool.toolID is "MoveItTool")
			{
				ToggleFindItPanel(false);
			}
		}

		internal void ClearSearch()
		{
			_ClearSearchBar.Update(true);
			_PrefabSearchUISystem.ClearSearch();
		}

		internal void RefreshCategoryAndSubCategory()
		{
			_CurrentCategoryBinding.Update((int)FindItUtil.CurrentCategory);
			_CurrentSubCategoryBinding.Update((int)FindItUtil.CurrentSubCategory);
		}
	}
}
