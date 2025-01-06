using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;

using Game.Input;
using Game.Tools;
using Game.UI.Tooltip;

using System;
using System.Collections.Generic;
using System.Linq;

namespace FindIt.Systems
{
	internal partial class PickerUISystem : OptionsUISystem
	{
		private readonly Dictionary<int, IOptionSection> _sections = new();

		private ToolSystem _toolSystem;
		private PickerToolSystem _pickerToolSystem;
		private DefaultToolSystem _defaultToolSystem;
		private FindItUISystem _findItUISystem;
		private ValueBindingHelper<OptionSectionUIEntry[]> _pickerOptionsList;
		private ProxyAction _pickerKeyBinding;

		protected override void OnCreate()
		{
			base.OnCreate();

			_toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			_pickerToolSystem = World.GetOrCreateSystemManaged<PickerToolSystem>();
			_defaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
			_findItUISystem = World.GetOrCreateSystemManaged<FindItUISystem>();

			_pickerKeyBinding = Mod.Settings.GetAction(nameof(FindItSettings.PickerKeyBinding));
			_pickerKeyBinding.shouldBeEnabled = true;
			
			_pickerOptionsList = CreateBinding("PickerOptionsList", new OptionSectionUIEntry[0]);
			CreateBinding("PickerActive", () => _toolSystem.activeTool == _pickerToolSystem);

			CreateTrigger<int, int, int>("PickerOptionClicked", OptionClicked);
			CreateTrigger("PickerIconToggled", PickerClicked);
		}

		public override void RefreshOptions()
		{
			if (!FindItUtil.IsReady)
			{
				return;
			}

			if (_sections.Count == 0)
			{
				foreach (var type in typeof(OptionsUISystem).Assembly.GetTypes())
				{
					if (typeof(IOptionSection).IsAssignableFrom(type) && !type.IsAbstract && type.Namespace == "FindIt.Domain.Options.Picker")
					{
						var section = (IOptionSection)Activator.CreateInstance(type, this);

						section.OnReset();

						_sections.Add(section.Id, section);
					}
				}
			}

			_pickerOptionsList.Value = GetVisibleSections()
				.OrderBy(x => x.Id)
				.Select(x => x.AsUIEntry())
				.ToArray();
		}

		private IEnumerable<IOptionSection> GetVisibleSections()
		{
			foreach (var section in _sections.Values)
			{
				if (section.IsVisible())
				{
					yield return section;
				}
				else if (!section.IsDefault())
				{
					section.OnReset();
				}
			}
		}

		private void OptionClicked(int sectionId, int optionId, int value)
		{
			if (!_sections.TryGetValue(sectionId, out var section))
			{
				return;
			}

			section.OnOptionClicked(optionId, value);

			RefreshOptions();
		}

		protected override void OnUpdate()
		{
			if (_pickerKeyBinding.WasPerformedThisFrame())
			{
				OnPickerKeyPressed();
			}

			base.OnUpdate();
		}

		private void PickerClicked()
		{
			RefreshOptions();

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
			RefreshOptions();

			_findItUISystem.ToggleFindItPanel(false);

			_toolSystem.activeTool = _pickerToolSystem;
		}

		public override void TriggerSearch()
		{

		}

		public override void UpdateCategoriesAndPrefabList()
		{
		}
	}
}
