using Game.UI.Editor;

using Unity.Entities;

namespace FindIt.Systems
{
	internal class CreatorPropertiesEditorTool : EditorTool
	{
		public const string ToolId = "FindIt.CreatorProperties";

		private readonly CreatorPropertiesUISystem _creatorPropertiesUISystem;

		public CreatorPropertiesEditorTool(World world, CreatorPropertiesUISystem creatorPropertiesUISystem) : base(world)
		{
			_creatorPropertiesUISystem = creatorPropertiesUISystem;

			id = ToolId;
			icon = "coui://findit/EditorTags.svg";
		}

		protected override bool IsActive()
		{
			return _creatorPropertiesUISystem.IsOpen;
		}

		protected override void OnEnable()
		{
			_creatorPropertiesUISystem.Open();
		}

		protected override void OnDisable()
		{
			_creatorPropertiesUISystem.Close();
		}
	}
}
