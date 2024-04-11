using FindIt.Domain.Enums;
using FindIt.Domain.Utilities;

using Game.Prefabs;
using Game.Tools;

using System.IO;

using UnityEngine.InputSystem;

namespace FindIt.Systems
{
	internal partial class FindItPanelUISystem : ExtendedUISystemBase
	{
		private ValueBindingHelper<bool> _FocusSearchBar;
		private ValueBindingHelper<bool> _ClearSearchBar;
		private ValueBindingHelper<bool> _ShowFindItPanel;
		private ValueBindingHelper<int> _ActivePrefabId;
		private ValueBindingHelper<int> _CurrentCategoryBinding;
		private ValueBindingHelper<int> _CurrentSubCategoryBinding;
		private ValueBindingHelper<float> _RowCount;
		private ValueBindingHelper<float> _ColumnCount;

		private ToolSystem _toolSystem;
		private PrefabSystem _prefabSystem;
		private PrefabSearchUISystem _prefabSearchUISystem;
		private DefaultToolSystem _defaultToolSystem;
		private bool settingPrefab;

		protected override void OnCreate()
		{
			base.OnCreate();

			_toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_prefabSearchUISystem = World.GetOrCreateSystemManaged<PrefabSearchUISystem>();
			_defaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();

			// ToolSystem toolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>(); // I don't know why vanilla game did this.
			_toolSystem.EventPrefabChanged += OnPrefabChanged;
			_toolSystem.EventToolChanged += OnToolChanged;

			// These establish the bindings for the categories
			_CurrentCategoryBinding = CreateBinding("CurrentCategory", "SetCurrentCategory", (int)FindItUtil.CurrentCategory, SetCurrentCategory);
			_CurrentSubCategoryBinding = CreateBinding("CurrentSubCategory", "SetCurrentSubCategory", (int)FindItUtil.CurrentSubCategory, SetCurrentSubCategory);

			// These establish the bindings with UI code.
			_FocusSearchBar = CreateBinding("FocusSearchBar", false);
			_FocusSearchBar = CreateBinding("FocusSearchBar", false);
			_ClearSearchBar = CreateBinding("ClearSearchBar", false);
			_ShowFindItPanel = CreateBinding("ShowFindItPanel", false);
			_ActivePrefabId = CreateBinding("ActivePrefabId", 0);
			_RowCount = CreateBinding("RowCount", Mod.Settings.RowCount);
			_ColumnCount = CreateBinding("ColumnCount", (float)Mod.Settings.ColumnCount);

			// These establish listeners to trigger events from UI.
			CreateTrigger("FindItCloseToggled", () => ToggleFindItPanel(false));
			CreateTrigger("FindItIconToggled", FindItIconClicked);
			CreateTrigger<int>("SetCurrentPrefab", TryActivatePrefabTool);
			CreateTrigger<int>("ToggleFavorited", ToggleFavorited);
			CreateTrigger("OnSearchFocused", () => _FocusSearchBar.Value = false);
			CreateTrigger("OnSearchCleared", () => _ClearSearchBar.Value = false);

			// This setup a keybinding for activating FindItPanel.
			InputAction hotKeyCtrlF = new($"{Mod.Id}/CtrlF");
			hotKeyCtrlF.AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Keyboard>/ctrl").With("Button", "<Keyboard>/f");
			hotKeyCtrlF.performed += OnCtrlFKeyPressed;
			hotKeyCtrlF.Enable();
		}

		private void FindItIconClicked()
		{
			if (_ShowFindItPanel)
			{
				ToggleFindItPanel(false);
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

			SetCurrentSubCategory((int)PrefabSubCategory.Any);
		}

		private void SetCurrentSubCategory(int category)
		{
			FindItUtil.CurrentSubCategory = (PrefabSubCategory)category;

			_prefabSearchUISystem.SetScroll(0);
			_prefabSearchUISystem.UpdateCategoriesList();

			if (!string.IsNullOrWhiteSpace(FindItUtil.CurrentSearch))
			{
				_prefabSearchUISystem.SearchChanged(FindItUtil.CurrentSearch, true);
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
			if (_ShowFindItPanel == visible)
			{
				return;
			}

			if (visible)
			{
				_RowCount.Value = Mod.Settings.RowCount;
				_ColumnCount.Value = Mod.Settings.ColumnCount;

				var prefab = _prefabSearchUISystem.UpdateCategoriesList();

				if (activatePrefab && Mod.Settings.SelectPrefabOnOpen)
				{
					TryActivatePrefabTool(prefab.Id);
				}
			}

			FindItUtil.IsActive = visible;
			_ShowFindItPanel.Value = visible;
		}

		/// <summary>
		/// This handles the keybinding pressed event.
		/// </summary>
		/// <param name="context">Not used yet.</param>
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


		/// <summary>
		/// If prefab entity is valid activate prefab tool for that prefab base.
		/// </summary>
		/// <param name="e">Entity from UI.</param>
		internal void TryActivatePrefabTool(int id)
		{
			if (FindItUtil.GetPrefabBase(id) is PrefabBase prefabBase)
			{
				settingPrefab = true;
				_toolSystem.ActivatePrefabTool(prefabBase);
				_ActivePrefabId.Value = id;
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

			if (_prefabSystem.TryGetEntity(prefab, out var entity))
			{
				_ActivePrefabId.Value = entity.Index;
			}
		}

		/// <summary>
		/// The event happens after the toolsystem changes tools.
		/// </summary>
		/// <param name="tool"></param>
		private void OnToolChanged(ToolBaseSystem tool)
		{
			if (!settingPrefab && tool == _defaultToolSystem || tool.toolID is "MoveItTool")
			{
				ToggleFindItPanel(false);
			}
		}

		internal void ClearSearch()
		{
			_ClearSearchBar.Value = true;
			_prefabSearchUISystem.ClearSearch();
		}

		internal void RefreshCategoryAndSubCategory()
		{
			_CurrentCategoryBinding.Value = (int)FindItUtil.CurrentCategory;
			_CurrentSubCategoryBinding.Value = (int)FindItUtil.CurrentSubCategory;
		}
	}
}
