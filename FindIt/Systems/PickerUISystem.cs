using Colossal.UI.Binding;

using Game.Tools;
using Game.UI;

using UnityEngine.InputSystem;

namespace FindIt.Systems
{
	internal partial class PickerUISystem : UISystemBase
	{
		private ToolSystem _toolSystem;
		private PickerToolSystem _pickerToolSystem;
		private DefaultToolSystem _defaultToolSystem;
		private FindItUISystem _findItUISystem;
		private ValueBinding<bool> _pickerEnabled;

		protected override void OnCreate()
		{
			base.OnCreate();

			_toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			_pickerToolSystem = World.GetOrCreateSystemManaged<PickerToolSystem>();
			_defaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
			_findItUISystem = World.GetOrCreateSystemManaged<FindItUISystem>();
			_toolSystem.EventToolChanged += OnToolChanged;

			AddBinding(_pickerEnabled = new ValueBinding<bool>(Mod.Id, "PickerEnabled", false));
			AddBinding(new TriggerBinding(Mod.Id, "PickerIconToggled", PickerClicked));

			InputAction hotKeyCtrlP = new($"{Mod.Id}/CtrlP");
			hotKeyCtrlP.AddCompositeBinding("ButtonWithOneModifier").With("Modifier", "<Keyboard>/ctrl").With("Button", "<Keyboard>/p");
			hotKeyCtrlP.performed += OnCtrlPKeyPressed;
			hotKeyCtrlP.Enable();
		}

		private void OnToolChanged(ToolBaseSystem system)
		{
			_pickerEnabled.Update(_toolSystem.activeTool == _pickerToolSystem);
		}

		private void PickerClicked()
		{
			if (_toolSystem.activeTool == _pickerToolSystem)
			{
				_toolSystem.activeTool = _defaultToolSystem;
			}
			else
			{
				_findItUISystem.ToggleFindItPanel(false);

				_toolSystem.activeTool = _pickerToolSystem;
			}
		}

		private void OnCtrlPKeyPressed(InputAction.CallbackContext context)
		{
			_findItUISystem.ToggleFindItPanel(false);

			_toolSystem.activeTool = _pickerToolSystem;
		}
	}
}
