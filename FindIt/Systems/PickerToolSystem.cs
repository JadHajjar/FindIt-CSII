using Colossal.Entities;

using FindIt.Domain.Utilities;

using Game.Common;
using Game.Input;
using Game.Net;
using Game.Prefabs;
using Game.Tools;

using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace FindIt.Systems
{
	public partial class PickerToolSystem : ToolBaseSystem
	{
		private PrefabSystem _prefabSystem;
		private FindItPanelUISystem _findItPanelUISystem;
		private PrefabSearchUISystem _prefabSearchUISystem;
		private EntityQuery _highlightedQuery;
		private ProxyAction _applyAction;

		public override string toolID => "FindIt.Picker";

		protected override void OnCreate()
		{
			base.OnCreate();

			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_findItPanelUISystem = World.GetOrCreateSystemManaged<FindItPanelUISystem>();
			_prefabSearchUISystem = World.GetOrCreateSystemManaged<PrefabSearchUISystem>();

			_highlightedQuery = GetEntityQuery(ComponentType.ReadOnly<Highlighted>());

			_applyAction = InputManager.instance.FindAction("Tool", "Apply");
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
							_findItPanelUISystem.ClearSearch();
							_findItPanelUISystem.RefreshCategoryAndSubCategory();
							_findItPanelUISystem.TryActivatePrefabTool(id);
							_prefabSearchUISystem.ScrollTo(id);
						}
						else
						{
							m_ToolSystem.ActivatePrefabTool(prefab);
						}

						_findItPanelUISystem.ToggleFindItPanel(true, false);
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
