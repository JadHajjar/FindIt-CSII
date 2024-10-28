using Colossal.Entities;

using FindIt.Domain.Utilities;

using Game.Common;
using Game.Input;
using Game.Net;
using Game.Prefabs;
using Game.Tools;

using System.Collections.Generic;
using System;
using System.Linq;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using UnityEngine.InputSystem;

namespace FindIt.Systems
{
	public partial class PickerToolSystem : ToolBaseSystem
	{
		private PrefabSystem _prefabSystem;
		private FindItUISystem _findItUISystem;
		private EntityQuery _highlightedQuery;
		private ProxyAction _applyAction;

		public override string toolID => "FindIt.Picker";

		protected override void OnCreate()
		{
			base.OnCreate();

			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_findItUISystem = World.GetOrCreateSystemManaged<FindItUISystem>();

			_highlightedQuery = GetEntityQuery(ComponentType.ReadOnly<Highlighted>());

			_applyAction = Mod.Settings.GetAction(nameof(FindIt) + "Apply");

			var builtInApplyAction = InputManager.instance.FindAction(InputManager.kToolMap, "Apply");
            var mimicApplyBinding = _applyAction.bindings.FirstOrDefault(b => b.device == InputManager.DeviceType.Mouse);
			var builtInApplyBinding = builtInApplyAction.bindings.FirstOrDefault(b => b.device == InputManager.DeviceType.Mouse);

			mimicApplyBinding.path = builtInApplyBinding.path;
			mimicApplyBinding.modifiers = builtInApplyBinding.modifiers;

            InputManager.instance.SetBinding(mimicApplyBinding, out _);

			//ProxyBinding result = new ProxyBinding(Mod.Settings.id, nameof(FindIt) + "Apply", ActionComponent.Press, InputManager.CompositeComponentData.defaultData.m_BindingName, new CompositeInstance(nameof(Mouse)));
			//result.group = nameof(Mouse);
			//result.path = builtInApplyBinding.path;
			//result.originalPath = builtInApplyBinding.path;
			//result.modifiers = builtInApplyBinding.modifiers;
			//result.originalModifiers = builtInApplyBinding.modifiers;
		}

		public override void InitializeRaycast()
		{
			base.InitializeRaycast();

			m_ToolRaycastSystem.netLayerMask = Layer.All;
			m_ToolRaycastSystem.typeMask = TypeMask.StaticObjects | TypeMask.Net | TypeMask.MovingObjects;
			m_ToolRaycastSystem.raycastFlags = RaycastFlags.Placeholders | RaycastFlags.SubElements | RaycastFlags.Decals | RaycastFlags.Markers;
			m_ToolRaycastSystem.collisionMask = CollisionMask.Overground | CollisionMask.OnGround | CollisionMask.Underground;
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();

			_applyAction.shouldBeEnabled = true;
		}

		protected override void OnStopRunning()
		{
			base.OnStopRunning();

			_applyAction.shouldBeEnabled = false;
		}

		protected override JobHandle OnUpdate(JobHandle inputDeps)
		{
			var picked = false;
			var raycastHit = GetRaycastResult(out var entity, out RaycastHit hit);
			var entities = _highlightedQuery.ToEntityArray(Allocator.Temp);

			if (raycastHit)
			{
				if (_applyAction.WasPressedThisFrame()
					&& EntityManager.TryGetComponent<PrefabRef>(entity, out var prefabRef)
					&& _prefabSystem.TryGetPrefab<PrefabBase>(prefabRef, out var prefab))
				{
					if (Mod.Settings.OpenPanelOnPicker)
					{
						if (FindItUtil.Find(prefab, true, out var id))
						{
							_findItUISystem.ClearSearch();
							_findItUISystem.RefreshCategoryAndSubCategory();
							_findItUISystem.TryActivatePrefabTool(id);
							_findItUISystem.ScrollTo(id);
						}
						else
						{
							m_ToolSystem.ActivatePrefabTool(prefab);
						}

						_findItUISystem.ToggleFindItPanel(true, false);
					}
					else
					{
						m_ToolSystem.ActivatePrefabTool(prefab);
					}

					picked = true;
				}
				else if (!EntityManager.HasComponent<Highlighted>(entity))
				{
					EntityManager.AddComponent<Highlighted>(entity);
					EntityManager.AddComponent<BatchesUpdated>(entity);
				}
			}

			for (var i = 0; i < entities.Length; i++)
			{
				if (!picked && raycastHit && entity == entities[i])
				{
					continue;
				}

				EntityManager.RemoveComponent<Highlighted>(entities[i]);
				EntityManager.AddComponent<BatchesUpdated>(entities[i]);
			}

			return base.OnUpdate(inputDeps);
		}

		public override bool TrySetPrefab(PrefabBase prefab)
		{
			return false;
		}

		public override PrefabBase GetPrefab()
		{
			return default;
		}
	}
}
