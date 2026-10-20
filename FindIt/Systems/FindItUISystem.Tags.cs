using FindIt.Utilities;

using System.Linq;

namespace FindIt.Systems
{
	internal partial class FindItUISystem : ExtendedUISystemBase
	{
		private void OpenTagEditor(int id)
		{
			if (_TagEditorPrefabId.Value == id)
			{
				CloseTagEditor();
				return;
			}

			if (FindItUtil.GetPrefabIndex(id) is not { } prefabIndex)
			{
				return;
			}

			_TagEditorPrefabId.Value = id;
			_TagEditorPrefabName.Value = prefabIndex.Name;

			RefreshTagEditor();
		}

		private void CloseTagEditor()
		{
			_TagEditorPrefabId.Value = 0;
		}

		private void AddTag(string tag)
		{
			if (FindItUtil.AddTag(_TagEditorPrefabId.Value, tag))
			{
				OnTagsChanged();
			}
		}

		private void RemoveTag(string tag)
		{
			if (FindItUtil.RemoveTag(_TagEditorPrefabId.Value, tag))
			{
				OnTagsChanged();
			}
		}

		private void OnTagsChanged()
		{
			RefreshTagEditor();

			_PrefabListBinding.Value = GetDisplayedPrefabs();

			if (FindItUtil.Filters.GetFilterList().Any())
			{
				TriggerSearch(false);
			}
		}

		private void RefreshTagEditor()
		{
			var prefabIndex = FindItUtil.GetPrefabIndex(_TagEditorPrefabId.Value);

			_TagEditorTags.Value = prefabIndex?.Tags?.ToArray() ?? new string[0];
			_TagEditorCreatorTags.Value = prefabIndex?.CreatorTags ?? new string[0];
			_AllTags.Value = FindItUtil.GetAllTags();
		}
	}
}
