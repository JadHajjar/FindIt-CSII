using FindIt.Domain;
using FindIt.Domain.Enums;
using FindIt.Domain.UIBinding;
using FindIt.Utilities;

using Game;
using Game.Prefabs;
using Game.Tools;
using Game.UI.Editor;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Entities;

namespace FindIt.Systems
{
	internal partial class CreatorPropertiesUISystem : ExtendedUISystemBase
	{
		private PrefabSystem _prefabSystem;
		private InspectorPanelSystem _inspectorPanelSystem;
		private EditorToolUISystem _editorToolUISystem;
		private FindItOptionsUISystem _optionsUISystem;
		private ToolSystem _toolSystem;
		private PrefabBase _prefab;
		private Entity _selectedEntity;

		private ValueBindingHelper<bool> _IsOpen;
		private ValueBindingHelper<bool> _ShowInfo;
		private ValueBindingHelper<string> _PrefabName;
		private ValueBindingHelper<bool> _UseComponent;
		private ValueBindingHelper<string[]> _Tags;
		private ValueBindingHelper<string[]> _AllTags;
		private ValueBindingHelper<int> _SubCategory;
		private ValueBindingHelper<bool> _HideFromFindIt;
		private ValueBindingHelper<CategoryUIEntry[]> _Categories;
		private ValueBindingHelper<SubCategoryUIEntry[]> _SubCategories;

		public override GameMode gameMode => GameMode.Editor;

		public bool IsOpen => _IsOpen.Value;

		protected override void OnCreate()
		{
			base.OnCreate();

			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_inspectorPanelSystem = World.GetOrCreateSystemManaged<InspectorPanelSystem>();
			_editorToolUISystem = World.GetOrCreateSystemManaged<EditorToolUISystem>();
			_toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			_optionsUISystem = World.GetOrCreateSystemManaged<FindItOptionsUISystem>();

			_IsOpen = CreateBinding("CreatorIsOpen", false);
			_ShowInfo = CreateBinding("CreatorShowInfo", false);
			_PrefabName = CreateBinding("CreatorPrefabName", string.Empty);
			_UseComponent = CreateBinding("CreatorUseComponent", true);
			_Tags = CreateBinding("CreatorTags", new string[0]);
			_AllTags = CreateBinding("CreatorAllTags", new string[0]);
			_SubCategory = CreateBinding("CreatorSubCategory", 0);
			_HideFromFindIt = CreateBinding("CreatorHideFromFindIt", false);
			_Categories = CreateBinding("CreatorCategories", new CategoryUIEntry[0]);
			_SubCategories = CreateBinding("CreatorSubCategories", new SubCategoryUIEntry[0]);

			CreateTrigger("CreatorToggle", Toggle);
			CreateTrigger("CreatorDismissInfo", () => _ShowInfo.Value = false);
			CreateTrigger("CreatorClose", ReturnToAsset);
			CreateTrigger<bool>("CreatorSetUseComponent", SetUseComponent);
			CreateTrigger<string>("CreatorAddTag", AddTag);
			CreateTrigger<string>("CreatorRemoveTag", RemoveTag);
			CreateTrigger<int>("CreatorSetSubCategory", SetSubCategory);
			CreateTrigger<bool>("CreatorSetHideFromFindIt", SetHideFromFindIt);
		}

		protected override void OnUpdate()
		{
			if (IsOpen)
			{
				if (_inspectorPanelSystem.selectedObject is not PrefabBase selectedPrefab)
				{
					Close();
				}
				else if (selectedPrefab != _prefab)
				{
					Open();
				}
			}

			base.OnUpdate();
		}

		private void Toggle()
		{
			if (IsOpen)
			{
				Close();
			}
			else if (_inspectorPanelSystem.selectedObject is PrefabBase)
			{
				Open();
			}
			else
			{
				_ShowInfo.Value = true;
			}
		}

		internal void Open()
		{
			if (_inspectorPanelSystem.selectedObject is not PrefabBase prefab)
			{
				return;
			}

			_prefab = prefab;
			_selectedEntity = _toolSystem.selected;
			_UseComponent.Value = prefab.Has<FindItProperties>() || (!GetExcludeTags(prefab).Any() && GetFindItInclude(prefab) is null);
			_PrefabName.Value = _optionsUISystem.GetAssetName(prefab);
			_Categories.Value = FindItUtil.GetCategories()
				.Where(x => x is not PrefabCategory.Any and not PrefabCategory.Favorite)
				.Select(x => new CategoryUIEntry(x))
				.ToArray();
			_SubCategories.Value = Enum.GetValues(typeof(PrefabSubCategory))
				.Cast<PrefabSubCategory>()
				.Where(x => (int)x > 0 && (int)x % 100 != 0)
				.Select(x => new SubCategoryUIEntry(x))
				.ToArray();
			_IsOpen.Value = true;
			_ShowInfo.Value = false;

			Refresh();
		}

		private void ReturnToAsset()
		{
			var prefab = _prefab;
			var selectedEntity = _selectedEntity;

			Close();

			if (prefab is null)
			{
				return;
			}

			if (selectedEntity != Entity.Null && EntityManager.Exists(selectedEntity))
			{
				_editorToolUISystem.SelectEntity(selectedEntity);
			}
			else if (_prefabSystem.TryGetEntity(prefab, out var prefabEntity))
			{
				_editorToolUISystem.SelectPrefab(prefabEntity);
			}
		}

		private void Close()
		{
			_prefab = null;
			_IsOpen.Value = false;
		}

		private void Refresh()
		{
			if (_prefab is null)
			{
				return;
			}

			var properties = _prefab.GetComponent<FindItProperties>();

			_Tags.Value = ReadTags().ToArray();
			_AllTags.Value = FindItUtil.GetAllTags();
			_SubCategory.Value = ReadSubCategory() is PrefabSubCategory subCategory ? (int)subCategory : 0;
			_HideFromFindIt.Value = properties?.m_HideFromFindIt ?? false;
		}

		private List<string> ReadTags()
		{
			IEnumerable<string> tags = _UseComponent.Value
				? _prefab.GetComponent<FindItProperties>()?.m_Tags ?? new string[0]
				: GetExcludeTags(_prefab);

			return tags
				.Select(FindItUtil.NormalizeTag)
				.Where(x => x.Length > 0)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		private void AddTag(string tag)
		{
			tag = FindItUtil.NormalizeTag(tag);

			if (_prefab is null || tag.Length == 0)
			{
				return;
			}

			var tags = ReadTags();

			if (tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
			{
				return;
			}

			tags.Add(FindItUtil.GetAllTags().FirstOrDefault(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase)) ?? tag);

			WriteTags(tags);
		}

		private void RemoveTag(string tag)
		{
			if (_prefab is null)
			{
				return;
			}

			var tags = ReadTags();

			if (tags.RemoveAll(x => string.Equals(x, tag, StringComparison.OrdinalIgnoreCase)) > 0)
			{
				WriteTags(tags);
			}
		}

		private void WriteTags(List<string> tags)
		{
			if (_UseComponent.Value)
			{
				_prefab.AddOrGetComponent<FindItProperties>().m_Tags = tags.ToArray();
				RemoveEmptyProperties();
			}
			else
			{
				SetExcludeTags(tags);
			}

			Apply();
		}

		private void SetUseComponent(bool useComponent)
		{
			if (_prefab is null || useComponent == _UseComponent.Value)
			{
				return;
			}

			var tags = ReadTags();
			var subCategory = ReadSubCategory();

			if (useComponent)
			{
				SetExcludeTags(new List<string>());
				SetIncludeSubCategory(null);

				var properties = _prefab.AddOrGetComponent<FindItProperties>();
				properties.m_Tags = tags.ToArray();
				properties.m_SubCategory = subCategory?.ToString();

				RemoveEmptyProperties();
			}
			else
			{
				ApplyPrefabsSystem.RemoveComponent<FindItProperties>(_prefab);
				SetExcludeTags(tags);
				SetIncludeSubCategory(subCategory);
			}

			_UseComponent.Value = useComponent;

			Apply();
		}

		private PrefabSubCategory? ReadSubCategory()
		{
			if (_UseComponent.Value)
			{
				return FindItUtil.TryParseSubCategory(_prefab.GetComponent<FindItProperties>()?.m_SubCategory, out var subCategory) ? subCategory : null;
			}

			var split = GetFindItInclude(_prefab)?.Split('/');

			return split is not null
				&& split.Length >= 3
				&& int.TryParse(split[2], out var id)
				&& FindItUtil.TryParseSubCategory(((PrefabSubCategory)id).ToString(), out var includeSubCategory)
				? includeSubCategory
				: null;
		}

		private void SetSubCategory(int id)
		{
			if (_prefab is null)
			{
				return;
			}

			PrefabSubCategory? subCategory = FindItUtil.TryParseSubCategory(((PrefabSubCategory)id).ToString(), out var parsed) ? parsed : null;

			if (_UseComponent.Value)
			{
				_prefab.AddOrGetComponent<FindItProperties>().m_SubCategory = subCategory?.ToString();

				RemoveEmptyProperties();
			}
			else
			{
				SetIncludeSubCategory(subCategory);
			}

			Apply();
		}

		private void SetHideFromFindIt(bool hide)
		{
			if (_prefab is null || !_UseComponent.Value)
			{
				return;
			}

			_prefab.AddOrGetComponent<FindItProperties>().m_HideFromFindIt = hide;

			RemoveEmptyProperties();
			Apply();
		}

		private void RemoveEmptyProperties()
		{
			var properties = _prefab.GetComponent<FindItProperties>();

			if (properties is not null
				&& (properties.m_Tags?.Length ?? 0) == 0
				&& string.IsNullOrWhiteSpace(properties.m_SubCategory)
				&& !properties.m_HideFromFindIt)
			{
				ApplyPrefabsSystem.RemoveComponent<FindItProperties>(_prefab);
			}
		}

		private static IEnumerable<string> GetExcludeTags(PrefabBase prefab)
		{
			return prefab.GetComponent<EditorAssetCategoryOverride>()?.m_ExcludeCategories?
				.Where(x => x is not null && x.StartsWith("#"))
				?? Enumerable.Empty<string>();
		}

		private static string GetFindItInclude(PrefabBase prefab)
		{
			return prefab.GetComponent<EditorAssetCategoryOverride>()?.m_IncludeCategories?
				.FirstOrDefault(IsFindItInclude);
		}

		private static bool IsFindItInclude(string category)
		{
			return category is not null && category.StartsWith("FindIt/");
		}

		private void SetExcludeTags(List<string> tags)
		{
			var overrides = _prefab.GetComponent<EditorAssetCategoryOverride>();
			var exclude = (overrides?.m_ExcludeCategories ?? new string[0])
				.Where(x => x is not null && !x.StartsWith("#"))
				.Concat(tags.Select(x => "#" + x))
				.ToArray();

			WriteOverride(overrides, overrides?.m_IncludeCategories ?? new string[0], exclude);
		}

		private void SetIncludeSubCategory(PrefabSubCategory? subCategory)
		{
			var overrides = _prefab.GetComponent<EditorAssetCategoryOverride>();
			var existing = GetFindItInclude(_prefab)?.Split('/');
			var include = (overrides?.m_IncludeCategories ?? new string[0])
				.Where(x => x is not null && !IsFindItInclude(x))
				.ToList();

			if (subCategory is PrefabSubCategory value)
			{
				var entry = $"FindIt/{(int)FindItUtil.GetCategory(value)}/{(int)value}";

				include.Add(existing is not null && existing.Length >= 4 ? $"{entry}/{existing[3]}" : entry);
			}

			WriteOverride(overrides, include.ToArray(), overrides?.m_ExcludeCategories ?? new string[0]);
		}

		private void WriteOverride(EditorAssetCategoryOverride overrides, string[] include, string[] exclude)
		{
			if (include.Length == 0 && exclude.Length == 0)
			{
				if (overrides is not null)
				{
					ApplyPrefabsSystem.RemoveComponent<EditorAssetCategoryOverride>(_prefab);
				}

				return;
			}

			overrides ??= _prefab.AddComponent<EditorAssetCategoryOverride>();
			overrides.m_IncludeCategories = include;
			overrides.m_ExcludeCategories = exclude;
		}

		private void Apply()
		{
			_prefabSystem.UpdatePrefab(_prefab);
			_prefab.asset?.MarkDirty();

			Refresh();
		}
	}
}
