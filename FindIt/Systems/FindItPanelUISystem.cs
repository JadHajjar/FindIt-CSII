using Colossal.UI.Binding;
using Game.Prefabs;
using Game.Tools;
using Game.UI;
using System;
using UnityEngine.InputSystem;
using Unity.Entities;

namespace FindIt.Systems
{
	internal partial class FindItPanelUISystem : UISystemBase
	{
        private const string ModId = "FindIt";

        private ValueBinding<bool> _ShowFindItPanel;
        private ValueBinding<Entity> _ActivePrefabEntity;
        private ToolSystem _ToolSystem;
        private PrefabSystem _PrefabSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            _PrefabSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<PrefabSystem>();
            // ToolSystem toolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>(); // I don't know why vanilla game did this.
            _ToolSystem.EventPrefabChanged = (Action<PrefabBase>)Delegate.Combine(_ToolSystem.EventPrefabChanged, new Action<PrefabBase>(OnPrefabChanged));
            _ToolSystem.EventToolChanged = (Action<ToolBaseSystem>)Delegate.Combine(_ToolSystem.EventToolChanged, new Action<ToolBaseSystem>(OnToolChanged));

            // These establish the bindings with UI code.
            AddBinding(_ShowFindItPanel = new ValueBinding<bool>(ModId, "ShowFindItPanel", false));

            // These establish the bindings with UI code.
            AddBinding(_ActivePrefabEntity = new ValueBinding<Entity>(ModId, "ActivePrefabEntity", Entity.Null));

            // These establish listeners to trigger events from UI.
            AddBinding(new TriggerBinding(ModId, "FindItIconToggled", FindItIconToggled));

            // This setup a keybinding for activating FindItPanel.
            InputAction hotKey = new(ModId);
            hotKey.AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Keyboard>/ctrl").With("Button", "<Keyboard>/f");
            hotKey.performed += OnKeyPressed;
            hotKey.Enable();
        }

        /// <summary>
        /// This event toggles the ShowFindItPanel binding.
        /// </summary>
        private void FindItIconToggled() => _ShowFindItPanel.Update(!_ShowFindItPanel.value);

        /// <summary>
        /// This handles the keybinding pressed event.
        /// </summary>
        /// <param name="context">Not used yet.</param>
        private void OnKeyPressed(InputAction.CallbackContext context)
        {
            FindItIconToggled();

            // We may need to implement a check for active prefab to filter for zoning prefabs, maybe bulldozer prefabs, or others that do not make sense for FindIt.
        }

        /// <summary>
        /// The event happens after the toolsystem changes prefabs.
        /// </summary>
        /// <param name="prefab"></param>
        private void OnPrefabChanged(PrefabBase prefab)
        {
            if (prefab == null)
            {
                return;
            }

            if(_PrefabSystem.TryGetEntity(prefab, out Entity entity))
            {
                _ActivePrefabEntity.Update(entity);
            };

            Mod.Log.Debug($"{nameof(FindItPanelUISystem)}.{nameof(OnPrefabChanged)} Prefab Changed to {prefab.name}.");
        }

        /// <summary>
        /// The event happens after the toolsystem changes tools.
        /// </summary>
        /// <param name="tool"></param>
        private void OnToolChanged(ToolBaseSystem tool)
        {
            if (_ToolSystem.activePrefab == null)
            {
                return;
            }

            if (_PrefabSystem.TryGetEntity(_ToolSystem.activePrefab, out Entity entity))
            {
                _ActivePrefabEntity.Update(entity);
            };

            Mod.Log.Debug($"{nameof(FindItPanelUISystem)}.{nameof(OnToolChanged)} ActivePrefab is {_ToolSystem.activePrefab.name}.");

        }
    }
}
