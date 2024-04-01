using Colossal.UI.Binding;

using Game.Tools;
using Game.UI;

using System;

using UnityEngine.InputSystem;

namespace FindIt.Systems
{
	internal partial class PickerUISystem : UISystemBase
	{
		private ToolSystem _ToolSystem;
		private PickerToolSystem _PickerToolSystem;
		private DefaultToolSystem _DefaultToolSystem;
		private FindItPanelUISystem _FindItPanelUISystem;
		private ValueBinding<bool> _PickerEnabled;

		protected override void OnCreate()
		{
			base.OnCreate();

			_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			_PickerToolSystem = World.GetOrCreateSystemManaged<PickerToolSystem>();
			_DefaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
			_FindItPanelUISystem = World.GetOrCreateSystemManaged<FindItPanelUISystem>();
			_ToolSystem.EventToolChanged += OnToolChanged;

			AddBinding(_PickerEnabled = new ValueBinding<bool>(Mod.Id, "PickerEnabled", false));
			AddBinding(new TriggerBinding(Mod.Id, "PickerIconToggled", PickerClicked));

			InputAction hotKeyCtrlP = new($"{Mod.Id}/CtrlP");
			hotKeyCtrlP.AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Keyboard>/ctrl").With("Button", "<Keyboard>/p");
			hotKeyCtrlP.performed += OnCtrlPKeyPressed;
			hotKeyCtrlP.Enable();
		}

		private void OnCtrlPKeyPressed(InputAction.CallbackContext context)
		{
			_FindItPanelUISystem.ToggleFindItPanel(false);

			_ToolSystem.activeTool = _PickerToolSystem;
		}

		private void OnToolChanged(ToolBaseSystem system)
		{
			_PickerEnabled.Update(_ToolSystem.activeTool == _PickerToolSystem);
		}

		private void PickerClicked()
		{
			if (_ToolSystem.activeTool == _PickerToolSystem)
			{
				_ToolSystem.activeTool = _DefaultToolSystem;
			}
			else
			{
				_FindItPanelUISystem.ToggleFindItPanel(false);

				_ToolSystem.activeTool = _PickerToolSystem;
			}
		}
	}
}
