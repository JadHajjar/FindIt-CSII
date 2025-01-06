using Colossal.UI;

using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;

using Game.Prefabs;
using Game.SceneFlow;
using Game.UI.InGame;

using System;
using System.Collections.Generic;
using System.Linq;

namespace FindIt.Systems
{
	internal partial class FindItOptionsUISystem : OptionsUISystem
	{
		private readonly Dictionary<int, IOptionSection> _sections = new();
		private PrefabUISystem _prefabUISystem;
		private PrefabSystem _prefabSystem;
		private FindItUISystem _findItUISystem;
		private ValueBindingHelper<OptionSectionUIEntry[]> _optionsList;
		private ValueBindingHelper<bool> _filtersSet;

		protected override void OnCreate()
		{
			base.OnCreate();

			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
			_findItUISystem = World.GetOrCreateSystemManaged<FindItUISystem>();

			_optionsList = CreateBinding("OptionsList", new OptionSectionUIEntry[0]);
			_filtersSet = CreateBinding("AreFiltersSet", false);

			CreateTrigger<int, int, int>("OptionClicked", OptionClicked);
			CreateTrigger("ClearFilters", ClearFilters);
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
					if (typeof(IOptionSection).IsAssignableFrom(type) && !type.IsAbstract && type.Namespace == "FindIt.Domain.Options")
					{
						var section = (IOptionSection)Activator.CreateInstance(type, this);

						section.OnReset();

						_sections.Add(section.Id, section);
					}
				}
			}

			_optionsList.Value = GetVisibleSections()
				.OrderBy(x => x.Id)
				.Select(x => x.AsUIEntry())
				.ToArray();

			_filtersSet.Value = _sections.Values.Any(x => !x.IsDefault());
		}

		private IEnumerable<IOptionSection> GetVisibleSections()
		{
			var requireRefresh = false;

			foreach (var section in _sections.Values)
			{
				if (section.IsVisible())
				{
					yield return section;
				}
				else if (!section.IsDefault())
				{
					requireRefresh = true;
					section.OnReset();
				}
			}

			if (requireRefresh)
			{
				TriggerSearch();
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

		private void ClearFilters()
		{
			var requireRefresh = false;

			foreach (var section in _sections.Values)
			{
				if (section.IsDefault())
				{
					continue;
				}

				requireRefresh = true;
				section.OnReset();
			}

			if (requireRefresh)
			{
				TriggerSearch();
			}

			RefreshOptions();
		}

		public override void TriggerSearch()
		{
			_findItUISystem.TriggerSearch();
		}

		public override void UpdateCategoriesAndPrefabList()
		{
			_findItUISystem.UpdateCategoriesAndPrefabList();
		}
	}
}
