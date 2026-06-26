using FindIt.Domain.Enums;
using FindIt.Domain.UIBinding;
using FindIt.Utilities;
using Game.Input;
using Game.Prefabs;
using Game.Rendering;
using Game.Tools;

using System.Threading;
using System.Threading.Tasks;

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
		private TaskCompletionSource<PrefabBase> prefabSelectionTask = new();

		private ToolSystem _toolSystem;
		private PrefabSystem _prefabSystem;
		private FindItOptionsUISystem _optionsUISystem;
		private DefaultToolSystem _defaultToolSystem;
		private CameraUpdateSystem _cameraUpdateSystem;

		private ProxyAction _searchKeyBinding;
		private ProxyAction _randomKeyBinding;
		private ProxyAction _arrowLeftBinding;
		private ProxyAction _arrowUpBinding;
		private ProxyAction _arrowRightBinding;
		private ProxyAction _arrowDownBinding;

		private ValueBindingHelper<bool> _IsSearchLoading;
		private ValueBindingHelper<bool> _IsWindowLocked;
		private ValueBindingHelper<bool> _FocusSearchBar;
		private ValueBindingHelper<bool> _ClearSearchBar;
		private ValueBindingHelper<bool> _ShowFindItPanel;
		private ValueBindingHelper<bool> _IsExpanded;
		private ValueBindingHelper<bool> _IsPrefabSelection;
		private ValueBindingHelper<double> _ScrollIndex;
		private ValueBindingHelper<double> _MaxScrollIndex;
		private ValueBindingHelper<double> _ColumnCount;
		private ValueBindingHelper<double> _RowCount;
		private ValueBindingHelper<int> _ActivePrefabId;
		private ValueBindingHelper<int> _CurrentCategoryBinding;
		private ValueBindingHelper<int> _CurrentSubCategoryBinding;
		private ValueBindingHelper<float> _PanelHeight;
		private ValueBindingHelper<float> _PanelWidth;
		private ValueBindingHelper<float> _RowSize;
		private ValueBindingHelper<float> _ColumnSize;
		private ValueBindingHelper<float> _ExpandedRowSize;
		private ValueBindingHelper<float> _ExpandedColumnSize;
		private ValueBindingHelper<float> _RightRowSize;
		private ValueBindingHelper<float> _RightColumnSize;
		private ValueBindingHelper<string> _CurrentSearch;
		private ValueBindingHelper<string> _ViewStyle;
		private ValueBindingHelper<string> _AlignmentStyle;
		private ValueBindingHelper<string> _PrefabCountBinding;
		private ValueBindingHelper<string> _PrefabSelectionTitle;
		private ValueBindingHelper<string[]> _AllThumbnails;
		private ValueBindingHelper<CategoryUIEntry[]> _CategoryBinding;
		private ValueBindingHelper<SubCategoryUIEntry[]> _SubCategoryBinding;
		private ValueBindingHelper<PrefabUIEntry[]> _PrefabListBinding;

		public bool IsExpanded => _IsExpanded;
		public bool IsPrefabSelection => _IsPrefabSelection;
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
		public string AlignmentStyle
		{
			get => _AlignmentStyle;
			set
			{
				_IsExpanded.Value = false;

				Mod.Settings.DefaultAlignmentStyle = _AlignmentStyle.Value = value;
				Mod.Settings.ApplyAndSave();

				ExpandedToggled();
			}
		}

		// Panel size values mirrored from the (now hidden) settings into UI-layer
		// bindings. Setting any of these clamps, persists, and refreshes the panel.
		// They map to the three layout modes handled by GridUtil.
		public float RowSize
		{
			get => _RowSize;
			set => SetPanelSize(_RowSize, value, v => Mod.Settings.RowSize = v);
		}
		public float ColumnSize
		{
			get => _ColumnSize;
			set => SetPanelSize(_ColumnSize, value, v => Mod.Settings.ColumnSize = v);
		}
		public float ExpandedRowSize
		{
			get => _ExpandedRowSize;
			set => SetPanelSize(_ExpandedRowSize, value, v => Mod.Settings.ExpandedRowSize = v);
		}
		public float ExpandedColumnSize
		{
			get => _ExpandedColumnSize;
			set => SetPanelSize(_ExpandedColumnSize, value, v => Mod.Settings.ExpandedColumnSize = v);
		}
		public float RightRowSize
		{
			get => _RightRowSize;
			set => SetPanelSize(_RightRowSize, value, v => Mod.Settings.RightRowSize = v);
		}
		public float RightColumnSize
		{
			get => _RightColumnSize;
			set => SetPanelSize(_RightColumnSize, value, v => Mod.Settings.RightColumnSize = v);
		}

		// Width is driven by the column size and height by the row size of the
		// layout mode currently in effect (right-dock, expanded, or normal),
		// mirroring the selection logic in GridUtil.GetWidth/GetHeight.
		public float CurrentColumnSize
		{
			get => AlignmentStyle != "Center" ? RightColumnSize : IsExpanded ? ExpandedColumnSize : ColumnSize;
			set
			{
				if (AlignmentStyle != "Center")
				{
					RightColumnSize = value;
				}
				else if (IsExpanded)
				{
					ExpandedColumnSize = value;
				}
				else
				{
					ColumnSize = value;
				}
			}
		}
		public float CurrentRowSize
		{
			get => AlignmentStyle != "Center" ? RightRowSize : IsExpanded ? ExpandedRowSize : RowSize;
			set
			{
				if (AlignmentStyle != "Center")
				{
					RightRowSize = value;
				}
				else if (IsExpanded)
				{
					ExpandedRowSize = value;
				}
				else
				{
					RowSize = value;
				}
			}
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			// Instantiating systems 
			_toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_optionsUISystem = World.GetOrCreateSystemManaged<FindItOptionsUISystem>();
			_defaultToolSystem = World.GetOrCreateSystemManaged<DefaultToolSystem>();
			_cameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();

			// ToolSystem toolSystem = World.DefaultGameObjectInjectionWorld?.GetOrCreateSystemManaged<ToolSystem>(); // I don't know why vanilla game did this.
			_toolSystem.EventPrefabChanged += OnPrefabChanged;
			_toolSystem.EventToolChanged += OnToolChanged;

			// Keybinding caching
			_searchKeyBinding = Mod.Settings.GetAction(nameof(FindItSettings.SearchKeyBinding));
			_searchKeyBinding.shouldBeEnabled = true;

			_randomKeyBinding = Mod.Settings.GetAction(nameof(FindItSettings.RandomKeyBinding));

			_arrowLeftBinding = Mod.Settings.GetAction(nameof(FindItSettings.LeftArrow));
			_arrowUpBinding = Mod.Settings.GetAction(nameof(FindItSettings.UpArrow));
			_arrowRightBinding = Mod.Settings.GetAction(nameof(FindItSettings.RightArrow));
			_arrowDownBinding = Mod.Settings.GetAction(nameof(FindItSettings.DownArrow));

			// These establish the bindings for the categories
			_CurrentCategoryBinding = CreateBinding("CurrentCategory", "SetCurrentCategory", (int)FindItUtil.CurrentCategory, SetCurrentCategory);
			_CurrentSubCategoryBinding = CreateBinding("CurrentSubCategory", "SetCurrentSubCategory", (int)FindItUtil.CurrentSubCategory, SetCurrentSubCategory);

			// These establish the bindings with UI code.
			_FocusSearchBar = CreateBinding("FocusSearchBar", false);
			_ClearSearchBar = CreateBinding("ClearSearchBar", false);
			_ShowFindItPanel = CreateBinding("ShowFindItPanel", false);
			_IsSearchLoading = CreateBinding("IsSearchLoading", false);
			_IsWindowLocked = CreateBinding("IsWindowLocked", false);
			_IsPrefabSelection = CreateBinding("IsPrefabSelection", false);
			_IsExpanded = CreateBinding("IsExpanded", "SetIsExpanded", false, _ => ExpandedToggled());
			_ActivePrefabId = CreateBinding("ActivePrefabId", 0);
			_PanelHeight = CreateBinding("PanelHeight", 0f);
			_PanelWidth = CreateBinding("PanelWidth", 0f);
			_RowSize = CreateBinding("RowSize", Mod.Settings.RowSize);
			_ColumnSize = CreateBinding("ColumnSize", Mod.Settings.ColumnSize);
			_ExpandedRowSize = CreateBinding("ExpandedRowSize", Mod.Settings.ExpandedRowSize);
			_ExpandedColumnSize = CreateBinding("ExpandedColumnSize", Mod.Settings.ExpandedColumnSize);
			_RightRowSize = CreateBinding("RightRowSize", Mod.Settings.RightRowSize);
			_RightColumnSize = CreateBinding("RightColumnSize", Mod.Settings.RightColumnSize);
			_ScrollIndex = CreateBinding("ScrollIndex", 0D);
			_MaxScrollIndex = CreateBinding("MaxScrollIndex", 0D);
			_ColumnCount = CreateBinding("ColumnCount", 0D);
			_RowCount = CreateBinding("RowCount", 0D);
			_PrefabSelectionTitle = CreateBinding("PrefabSelectionTitle", string.Empty);
			_CurrentSearch = CreateBinding("CurrentSearch", string.Empty);
			_CategoryBinding = CreateBinding("CategoryList", new CategoryUIEntry[] { new(PrefabCategory.Any) });
			_SubCategoryBinding = CreateBinding("SubCategoryList", new SubCategoryUIEntry[] { new(PrefabSubCategory.Any) });
			_PrefabListBinding = CreateBinding("PrefabList", new PrefabUIEntry[0]);
			_PrefabCountBinding = CreateBinding("PrefabCount", string.Empty);
			_ViewStyle = CreateBinding("ViewStyle", Mod.Settings.DefaultViewStyle);
			_AlignmentStyle = CreateBinding("AlignmentStyle", Mod.Settings.DefaultAlignmentStyle);
			_AllThumbnails = CreateBinding("AllThumbnails", new string[0]);

			CreateBinding("NoAssetImage", () => Mod.Settings.NoAssetImage);

			// These establish UI actions triggering methods on the C# side.
			CreateTrigger<string>("SearchChanged", t => SearchChanged(t));
			CreateTrigger<int>("OnScroll", OnScroll);
			CreateTrigger<double>("SetScrollIndex", SetScrollIndex);
			CreateTrigger("FindItCloseToggled", () => ToggleFindItPanel(false));
			CreateTrigger("FindItIconToggled", FindItIconClicked);
			CreateTrigger<int>("SetCurrentPrefab", TryActivatePrefabTool);
			CreateTrigger<int>("ToggleFavorited", FindItUtil.ToggleFavorited);
			CreateTrigger("ToggleLock", ToggleLock);
			CreateTrigger("OnSearchFocused", () => _FocusSearchBar.Value = false);
			CreateTrigger("OnSearchCleared", () => _ClearSearchBar.Value = false);
			CreateTrigger("OnRandomButtonClicked", OnRandomButtonClicked);
			CreateTrigger<int>("OnLocateButtonClicked", OnLocateButtonClicked);
			CreateTrigger<int>("OnPdxModsButtonClicked", OnPdxModsButtonClicked);
			CreateTrigger("ClearThumbnails", () => _AllThumbnails.Value = new string[0]);
		}

		protected override void OnUpdate()
		{
			_randomKeyBinding.shouldBeEnabled =
			_arrowLeftBinding.shouldBeEnabled =
			_arrowUpBinding.shouldBeEnabled =
			_arrowRightBinding.shouldBeEnabled =
			_arrowDownBinding.shouldBeEnabled = _ShowFindItPanel || _IsWindowLocked;

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

			if (_searchKeyBinding.WasPerformedThisFrame())
			{
				OnSearchKeyPressed();
			}

			if (_ShowFindItPanel || _IsWindowLocked)
			{
				if (_randomKeyBinding.WasPerformedThisFrame())
				{
					OnRandomButtonClicked();
				}

				if (_arrowLeftBinding.WasPerformedThisFrame())
				{
					MoveSelectedItemGrid(-1, 0);
				}

				if (_arrowUpBinding.WasPerformedThisFrame())
				{
					MoveSelectedItemGrid(0, -1);
				}

				if (_arrowRightBinding.WasPerformedThisFrame())
				{
					MoveSelectedItemGrid(1, 0);
				}

				if (_arrowDownBinding.WasPerformedThisFrame())
				{
					MoveSelectedItemGrid(0, 1);
				}
			}

			base.OnUpdate();
		}
	}
}
