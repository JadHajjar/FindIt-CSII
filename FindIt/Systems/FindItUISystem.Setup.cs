using FindIt.Domain.Enums;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;

using Game.Input;
using Game.Prefabs;
using Game.Tools;

using System.Threading;

namespace FindIt.Systems
{
	internal partial class FindItUISystem : ExtendedUISystemBase
	{
		private bool filterCompleted;
		private bool scrollCompleted;
		private bool settingPrefab;
		private double scrollIndex;
		private CancellationTokenSource searchTokenSource = new();
		private CancellationTokenSource scrollTokenSource = new();

		private ToolSystem _toolSystem;
		private PrefabSystem _prefabSystem;
		private OptionsUISystem _optionsUISystem;
		private DefaultToolSystem _defaultToolSystem;

		private ValueBindingHelper<bool> _IsSearchLoading;
		private ValueBindingHelper<bool> _FocusSearchBar;
		private ValueBindingHelper<bool> _ClearSearchBar;
		private ValueBindingHelper<bool> _ShowFindItPanel;
		private ValueBindingHelper<bool> _IsExpanded;
		private ValueBindingHelper<double> _ScrollIndex;
		private ValueBindingHelper<double> _MaxScrollIndex;
		private ValueBindingHelper<double> _ColumnCount;
		private ValueBindingHelper<double> _RowCount;
		private ValueBindingHelper<int> _ActivePrefabId;
		private ProxyAction _searchKeyBinding;
		private ValueBindingHelper<int> _CurrentCategoryBinding;
		private ValueBindingHelper<int> _CurrentSubCategoryBinding;
		private ValueBindingHelper<float> _PanelHeight;
		private ValueBindingHelper<float> _PanelWidth;
		private ValueBindingHelper<string> _CurrentSearch;
		private ValueBindingHelper<string> _ViewStyle;
		private ValueBindingHelper<CategoryUIEntry[]> _CategoryBinding;
		private ValueBindingHelper<SubCategoryUIEntry[]> _SubCategoryBinding;
		private ValueBindingHelper<PrefabUIEntry[]> _PrefabListBinding;

		public bool IsExpanded => _IsExpanded;
		public string ViewStyle
		{
			get => _ViewStyle;
			set
			{
				Mod.Settings.DefaultViewStyle = _ViewStyle.Value = value;
				Mod.Settings.ApplyAndSave();
				UpdateCategoriesAndPrefabList();
			}
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			// Instantiating systems 
			_toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_optionsUISystem = World.GetOrCreateSystemManaged<OptionsUISystem>();
			_defaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();

			// ToolSystem toolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>(); // I don't know why vanilla game did this.
			_toolSystem.EventPrefabChanged += OnPrefabChanged;
			_toolSystem.EventToolChanged += OnToolChanged;

			// Keybinding caching
			_searchKeyBinding = Mod.Settings.GetAction(nameof(FindItSettings.SearchKeyBinding));

			// These establish the bindings for the categories
			_CurrentCategoryBinding = CreateBinding("CurrentCategory", "SetCurrentCategory", (int)FindItUtil.CurrentCategory, SetCurrentCategory);
			_CurrentSubCategoryBinding = CreateBinding("CurrentSubCategory", "SetCurrentSubCategory", (int)FindItUtil.CurrentSubCategory, SetCurrentSubCategory);

			// These establish the bindings with UI code.
			_FocusSearchBar = CreateBinding("FocusSearchBar", false);
			_FocusSearchBar = CreateBinding("FocusSearchBar", false);
			_ClearSearchBar = CreateBinding("ClearSearchBar", false);
			_ShowFindItPanel = CreateBinding("ShowFindItPanel", false);
			_IsSearchLoading = CreateBinding("IsSearchLoading", false);
			_IsExpanded = CreateBinding("IsExpanded", "SetIsExpanded", false, _ => ExpandedToggled());
			_ActivePrefabId = CreateBinding("ActivePrefabId", 0);
			_PanelHeight = CreateBinding("PanelHeight", 0f);
			_PanelWidth = CreateBinding("PanelWidth", 0f);
			_ScrollIndex = CreateBinding("ScrollIndex", 0D);
			_MaxScrollIndex = CreateBinding("MaxScrollIndex", 0D);
			_ColumnCount = CreateBinding("ColumnCount", 0D);
			_RowCount = CreateBinding("RowCount", 0D);
			_CurrentSearch = CreateBinding("CurrentSearch", string.Empty);
			_CategoryBinding = CreateBinding("CategoryList", new CategoryUIEntry[] { new(PrefabCategory.Any) });
			_SubCategoryBinding = CreateBinding("SubCategoryList", new SubCategoryUIEntry[] { new(PrefabSubCategory.Any) });
			_PrefabListBinding = CreateBinding("PrefabList", new PrefabUIEntry[0]);
			_ViewStyle = CreateBinding("ViewStyle", Mod.Settings.DefaultViewStyle);

			CreateTrigger<string>("SearchChanged", t => SearchChanged(t));
			CreateTrigger<int>("OnScroll", OnScroll);
			CreateTrigger<double>("SetScrollIndex", SetScrollIndex);
			CreateTrigger("FindItCloseToggled", () => ToggleFindItPanel(false));
			CreateTrigger("FindItIconToggled", FindItIconClicked);
			CreateTrigger<int>("SetCurrentPrefab", TryActivatePrefabTool);
			CreateTrigger<int>("ToggleFavorited", FindItUtil.ToggleFavorited);
			CreateTrigger("OnSearchFocused", () => _FocusSearchBar.Value = false);
			CreateTrigger("OnSearchCleared", () => _ClearSearchBar.Value = false);
		}

		protected override void OnUpdate()
		{
			if (filterCompleted)
			{
				filterCompleted = false;
				scrollIndex = 0;

				_IsSearchLoading.Value = false;
				_PrefabListBinding.Value = GetDisplayedPrefabs();
			}

			if (scrollCompleted)
			{
				scrollCompleted = false;

				_PrefabListBinding.Value = GetDisplayedPrefabs();
			}

			if (_searchKeyBinding.WasPressedThisFrame())
			{
				OnSearchKeyPressed();
			}

			base.OnUpdate();
		}
	}
}
