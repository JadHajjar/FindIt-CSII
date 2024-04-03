using Colossal.IO.AssetDatabase;
using Colossal.UI.Binding;

using FindIt.Domain.Enums;
using FindIt.Domain.Utilities;

using Game.Prefabs;
using Game.Tools;
using Game.UI;

using System;

using Unity.Entities;

using UnityEngine.InputSystem;

namespace FindIt.Systems
{
	internal partial class FindItPanelUISystem : UISystemBase
	{
		private ValueBinding<bool> _FocusSearchBar;
		private ValueBinding<bool> _ShowFindItPanel;
		private ValueBinding<int> _ActivePrefabId;
		private ValueBinding<int> _CurrentCategoryBinding;
		private ValueBinding<int> _CurrentSubCategoryBinding;

		private ToolSystem _ToolSystem;
		private PrefabSystem _PrefabSystem;
		private PrefabIndexingSystem _PrefabIndexingSystem;
		private PrefabSearchUISystem _PrefabSearchUISystem;
		private DefaultToolSystem _DefaultToolSystem;
		private bool settingPrefab;

		protected override void OnCreate()
		{
			base.OnCreate();

			_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_PrefabIndexingSystem = World.GetOrCreateSystemManaged<PrefabIndexingSystem>();
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
			AddBinding(_ShowFindItPanel = new ValueBinding<bool>(Mod.Id, "ShowFindItPanel", false));
			AddBinding(_ActivePrefabId = new ValueBinding<int>(Mod.Id, "ActivePrefabId", 0));

			// These establish listeners to trigger events from UI.
			AddBinding(new TriggerBinding(Mod.Id, "FindItCloseToggled", () => ToggleFindItPanel(false)));
			AddBinding(new TriggerBinding(Mod.Id, "FindItIconToggled", FindItIconClicked));
			AddBinding(new TriggerBinding<int>(Mod.Id, "SetCurrentPrefab", TryActivatePrefabTool));
			AddBinding(new TriggerBinding<int>(Mod.Id, "ToggleFavorited", ToggleFavorited));
			AddBinding(new TriggerBinding(Mod.Id, "OnSearchFocused", () => _FocusSearchBar.Update(false)));

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
				_ToolSystem.activeTool = _DefaultToolSystem;
				_ToolSystem.ActivatePrefabTool(null);
			}
			else
			ToggleFindItPanel(!_ShowFindItPanel.value);
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
			_FocusSearchBar.Update(Mod.Settings.AutoFocusOnCategory);

			_PrefabSearchUISystem.UpdateCategoriesList();

			if (!string.IsNullOrWhiteSpace(FindItUtil.CurrentSearch))
			{
				_PrefabSearchUISystem.SearchChanged(FindItUtil.CurrentSearch);
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
				var prefab = _PrefabSearchUISystem.UpdateCategoriesList();

				if (activatePrefab)
				{
					TryActivatePrefabTool(prefab.Id);
				}

				_FocusSearchBar.Update(Mod.Settings.AutoFocusOnOpen);
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
				_FocusSearchBar.Update(true);
			else
				ToggleFindItPanel(true);


			// We may need to implement a check for active prefab to filter for zoning prefabs, maybe bulldozer prefabs, or others that do not make sense for FindIt.
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
			if (!settingPrefab && tool == _DefaultToolSystem)
			{
				ToggleFindItPanel(false);
			}
		}
	}
}
