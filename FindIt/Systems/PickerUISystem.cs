using Colossal.UI.Binding;

using Game.Input;
using Game.Tools;
using Game.UI;

namespace FindIt.Systems
{
	internal partial class PickerUISystem : UISystemBase
	{
		private ToolSystem _toolSystem;
		private PickerToolSystem _pickerToolSystem;
		private DefaultToolSystem _defaultToolSystem;
		private FindItUISystem _findItUISystem;
		private ValueBinding<bool> _pickerEnabled;
		private ProxyAction _pickerKeyBinding;

		protected override void OnCreate()
		{
			base.OnCreate();

			_toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			_pickerToolSystem = World.GetOrCreateSystemManaged<PickerToolSystem>();
			_defaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
			_findItUISystem = World.GetOrCreateSystemManaged<FindItUISystem>();
			_toolSystem.EventToolChanged += OnToolChanged;

			_pickerKeyBinding = Mod.Settings.GetAction(nameof(FindItSettings.PickerKeyBinding));

			AddBinding(_pickerEnabled = new ValueBinding<bool>(Mod.Id, "PickerEnabled", false));
			AddBinding(new TriggerBinding(Mod.Id, "PickerIconToggled", PickerClicked));
		}

		protected override void OnUpdate()
		{
			if (_pickerKeyBinding.WasPressedThisFrame())
			{
				OnPickerKeyPressed();
			}

			base.OnUpdate();
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

		private void OnPickerKeyPressed()
		{
			_findItUISystem.ToggleFindItPanel(false);

			_toolSystem.activeTool = _pickerToolSystem;
		}
	}
}
