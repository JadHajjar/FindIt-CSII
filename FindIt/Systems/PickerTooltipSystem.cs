using Game.Prefabs;
using Game.Tools;
using Game.UI.Tooltip;

using Unity.Entities;

namespace FindIt.Systems
{
	internal partial class PickerTooltipSystem : TooltipSystemBase
	{
		private ToolSystem m_ToolSystem;
		private PrefabSystem m_PrefabSystem;
		private PickerToolSystem m_PickerToolSystem;
		private PickerUISystem m_PickerUISystem;
		private EntityQuery m_Query;
		private StringTooltip m_Tooltip;

		protected override void OnCreate()
		{
			base.OnCreate();

			m_ToolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			m_PrefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			m_PickerToolSystem = World.GetOrCreateSystemManaged<PickerToolSystem>();
			m_PickerUISystem = World.GetOrCreateSystemManaged<PickerUISystem>();

			m_Query = GetEntityQuery(ComponentType.ReadOnly<Highlighted>(), ComponentType.ReadOnly<PrefabRef>());

			RequireForUpdate(m_Query);

			m_Tooltip = new StringTooltip
			{
				path = "pickerTool"
			};
		}

		protected override void OnUpdate()
		{
			if (m_ToolSystem.activeTool == m_PickerToolSystem && m_PickerToolSystem.HoveredPrefab is not null)
			{
				//m_Tooltip.icon = Mod.Settings.NoAssetImage ? string.Empty : m_PickerToolSystem.HoveredPrefab.TryGet<UIObject>(out var icon) ? icon.m_Icon : string.Empty;
				m_Tooltip.value = m_PickerUISystem.GetAssetName(m_PickerToolSystem.HoveredPrefab);

				AddMouseTooltip(m_Tooltip);
			}
		}
	}
}
