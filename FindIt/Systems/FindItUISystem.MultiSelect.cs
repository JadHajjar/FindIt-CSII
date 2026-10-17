using FindIt.Utilities;

using Game.Prefabs;
using Game.Tools;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine.InputSystem;

namespace FindIt.Systems
{
	internal partial class FindItUISystem : ExtendedUISystemBase
	{
		// A tool opts into multi-selection by exposing these public instance methods,
		// resolved by reflection (no cross-mod dependency):
		//   void SelectPrefab(PrefabBase), void DeselectPrefab(PrefabBase), IEnumerable<PrefabBase> GetSelectedPrefabs()
		private const string SelectMethodName = "SelectPrefab";
		private const string DeselectMethodName = "DeselectPrefab";
		private const string GetSelectedMethodName = "GetSelectedPrefabs";

		private ToolBaseSystem _multiSelectTool;
		private MethodInfo _selectMethod;
		private MethodInfo _deselectMethod;
		private MethodInfo _getSelectedMethod;
		private int _rangeAnchorId;
		private bool _suppressSelectionRefresh;

		private bool ActiveToolSupportsMultiSelect =>
			_multiSelectTool != null
			&& _multiSelectTool == _toolSystem.activeTool
			&& _selectMethod != null
			&& _deselectMethod != null
			&& _getSelectedMethod != null;

		private void CacheMultiSelectSupport(ToolBaseSystem tool)
		{
			_multiSelectTool = tool;
			_selectMethod = null;
			_deselectMethod = null;
			_getSelectedMethod = null;

			if (tool == null)
			{
				return;
			}

			try
			{
				var type = tool.GetType();
				var flags = BindingFlags.Public | BindingFlags.Instance;

				var select = type.GetMethod(SelectMethodName, flags, null, new[] { typeof(PrefabBase) }, null);
				var deselect = type.GetMethod(DeselectMethodName, flags, null, new[] { typeof(PrefabBase) }, null);
				var getSelected = type.GetMethod(GetSelectedMethodName, flags, null, Type.EmptyTypes, null);

				var getSelectedReturnsPrefabs = getSelected != null
					&& typeof(IEnumerable<PrefabBase>).IsAssignableFrom(getSelected.ReturnType);

				Mod.Log.Debug($"Multi-select probe for '{tool.toolID}' ({type.FullName}): " +
					$"{SelectMethodName}={select != null}, {DeselectMethodName}={deselect != null}, " +
					$"{GetSelectedMethodName}={getSelected != null} (IEnumerable<PrefabBase>={getSelectedReturnsPrefabs}).");

				if (select == null || deselect == null || !getSelectedReturnsPrefabs)
				{
					if (select != null || deselect != null || getSelected != null)
					{
						Mod.Log.Info(
							$"[FindIt] Tool '{tool.toolID}' only partially implements the multi-select contract; " +
							$"multi-select disabled. Required: void {SelectMethodName}(PrefabBase), " +
							$"void {DeselectMethodName}(PrefabBase), IEnumerable<PrefabBase> {GetSelectedMethodName}().");
					}

					return;
				}

				_selectMethod = select;
				_deselectMethod = deselect;
				_getSelectedMethod = getSelected;

				Mod.Log.Info($"[FindIt] Multi-select enabled for tool '{tool.toolID}'.");
			}
			catch (Exception ex)
			{
				_selectMethod = null;
				_deselectMethod = null;
				_getSelectedMethod = null;

				Mod.Log.Error(ex, "Failed to resolve multi-select contract for the active tool.");
			}
		}

		internal void OnPrefabClicked(int id)
		{
			var ctrl = CtrlHeld;
			var shift = ShiftHeld;

			Mod.Log.Debug($"Multi-select click id={id} ctrl={ctrl} shift={shift} " +
				$"pickerMode={(bool)_IsPrefabSelection} supported={ActiveToolSupportsMultiSelect} " +
				$"activeTool='{_toolSystem.activeTool?.toolID}'.");

			if (_IsPrefabSelection)
			{
				TryActivatePrefabTool(id);

				return;
			}

			if ((ctrl || shift) && ActiveToolSupportsMultiSelect)
			{
				if (shift)
				{
					RangeSelectTo(id);
				}
				else
				{
					TogglePrefabSelection(id);
				}

				return;
			}

			TryActivatePrefabTool(id);
			_rangeAnchorId = id;
			RefreshSelectionBinding();
		}

		private void TogglePrefabSelection(int id)
		{
			if (FindItUtil.GetPrefabBase(id) is not PrefabBase prefab)
			{
				return;
			}

			// Guard against the panel auto-closing on the EventPrefabChanged the tool fires.
			settingPrefab = true;

			if (GetToolSelectedPrefabs().Contains(prefab))
			{
				ToolDeselectPrefab(prefab);
			}
			else
			{
				ToolSelectPrefab(prefab);
			}

			settingPrefab = false;
			_rangeAnchorId = id;

			Mod.Log.Debug($"Multi-select toggle '{prefab.name}'; tool reports {GetToolSelectedPrefabs().Count} selected.");

			RefreshSelectionBinding();
		}

		private void RangeSelectTo(int id)
		{
			var list = FindItUtil.GetFilteredPrefabs();
			var clickedIndex = list.FindIndex(x => x.Id == id);

			if (clickedIndex < 0)
			{
				return;
			}

			var anchorIndex = list.FindIndex(x => x.Id == _rangeAnchorId);

			if (anchorIndex < 0)
			{
				TryActivatePrefabTool(id);
				_rangeAnchorId = id;
				RefreshSelectionBinding();

				return;
			}

			var start = Math.Min(anchorIndex, clickedIndex);
			var end = Math.Max(anchorIndex, clickedIndex);

			var desired = new List<PrefabBase>();

			for (var i = start; i <= end; i++)
			{
				if (FindItUtil.GetPrefabBase(list[i].Id) is PrefabBase prefab)
				{
					desired.Add(prefab);
				}
			}

			settingPrefab = true;
			_suppressSelectionRefresh = true;

			try
			{
				var current = GetToolSelectedPrefabs();

				foreach (var prefab in current)
				{
					if (!desired.Contains(prefab))
					{
						ToolDeselectPrefab(prefab);
					}
				}

				foreach (var prefab in desired)
				{
					if (!current.Contains(prefab))
					{
						ToolSelectPrefab(prefab);
					}
				}
			}
			finally
			{
				_suppressSelectionRefresh = false;
				settingPrefab = false;
			}

			Mod.Log.Debug($"Multi-select range [{start}..{end}]; tool reports {GetToolSelectedPrefabs().Count} selected.");
			RefreshSelectionBinding();
		}

		private void RefreshSelectionBinding()
		{
			if (_SelectedPrefabIds == null || _suppressSelectionRefresh)
			{
				return;
			}

			if (ActiveToolSupportsMultiSelect)
			{
				var ids = GetToolSelectedPrefabs()
					.Select(prefab => _prefabSystem.TryGetEntity(prefab, out var entity) ? entity.Index : 0)
					.Where(index => index != 0)
					.Distinct()
					.ToArray();

				_SelectedPrefabIds.Value = ids;

				if (ids.Length > 0 && Array.IndexOf(ids, _ActivePrefabId.Value) < 0)
				{
					_ActivePrefabId.Value = ids[ids.Length - 1];
				}

				return;
			}

			var activeId = _ActivePrefabId.Value;

			_SelectedPrefabIds.Value = activeId == 0 ? Array.Empty<int>() : new[] { activeId };
		}

		private void ToolSelectPrefab(PrefabBase prefab)
		{
			try
			{
				_selectMethod.Invoke(_multiSelectTool, new object[] { prefab });
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, $"Multi-select: {SelectMethodName} call failed.");
			}
		}

		private void ToolDeselectPrefab(PrefabBase prefab)
		{
			try
			{
				_deselectMethod.Invoke(_multiSelectTool, new object[] { prefab });
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, $"Multi-select: {DeselectMethodName} call failed.");
			}
		}

		private List<PrefabBase> GetToolSelectedPrefabs()
		{
			try
			{
				if (_getSelectedMethod.Invoke(_multiSelectTool, null) is IEnumerable<PrefabBase> prefabs)
				{
					return prefabs.Where(prefab => prefab != null).ToList();
				}
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, $"Multi-select: {GetSelectedMethodName} call failed.");
			}

			return new List<PrefabBase>();
		}

		private static bool CtrlHeld
		{
			get
			{
				var keyboard = Keyboard.current;

				return keyboard != null && (keyboard.leftCtrlKey.isPressed || keyboard.rightCtrlKey.isPressed);
			}
		}

		private static bool ShiftHeld
		{
			get
			{
				var keyboard = Keyboard.current;

				return keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
			}
		}
	}
}
