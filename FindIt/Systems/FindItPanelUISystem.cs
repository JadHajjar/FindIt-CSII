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
        private ValueBinding<string> _ActivePrefab;
        private ToolSystem _ToolSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            _ToolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>();
            // ToolSystem toolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>(); // I don't know why vanilla game did this.
            _ToolSystem.EventPrefabChanged = (Action<PrefabBase>)Delegate.Combine(_ToolSystem.EventPrefabChanged, new Action<PrefabBase>(OnPrefabChanged));



            // These establish the bindings with UI code.
            AddBinding(_ShowFindItPanel = new ValueBinding<bool>(ModId, "ShowFindItPanel", false));

            // These establish listeners to trigger events from UI.
            AddBinding(new TriggerBinding(ModId, "FintItIconToggled", FindItIconToggled));

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

            // We will need to implement a check for active prefab to filter for zoning prefabs, maybe bulldozer prefabs, or others that do not make sense for FindIt.
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

            Mod.Log.Debug($"{nameof(FindItPanelUISystem)}.{nameof(OnPrefabChanged)} Prefab Changed to {prefab.name}.");
        }
    }
}
