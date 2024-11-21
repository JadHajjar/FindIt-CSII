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
	internal partial class OptionsUISystem : ExtendedUISystemBase
	{
		private readonly Dictionary<int, IOptionSection> _sections = new();
		private PrefabUISystem _prefabUISystem;
		private PrefabSystem _prefabSystem;
		private FindItUISystem _findItUISystem;
		private ValueBindingHelper<OptionSectionUIEntry[]> _optionsList;

		protected override void OnCreate()
		{
			base.OnCreate();

			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
			_findItUISystem = World.GetOrCreateSystemManaged<FindItUISystem>();
			_optionsList = CreateBinding("OptionsList", new OptionSectionUIEntry[0]);

			CreateTrigger<int, int, int>("OptionClicked", OptionClicked);
		}

		internal void RefreshOptions()
		{
			if (!FindItUtil.IsReady)
			{
				return;
			}

			if (_sections.Count == 0)
			{
				foreach (var type in typeof(OptionsUISystem).Assembly.GetTypes())
				{
					if (typeof(IOptionSection).IsAssignableFrom(type) && !type.IsAbstract)
					{
						var section = (IOptionSection)Activator.CreateInstance(type, this);

						_sections.Add(section.Id, section);
					}
				}
			}

			_optionsList.Value = GetVisibleSections()
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
				else
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

		public string GetAssetName(PrefabBase prefab)
		{
			_prefabUISystem.GetTitleAndDescription(_prefabSystem.GetEntity(prefab), out var titleId, out var _);

			if (GameManager.instance.localizationManager.activeDictionary.TryGetValue(titleId, out var name))
			{
				return name;
			}

			return prefab.name.Replace('_', ' ').FormatWords();
		}

		public void TriggerSearch()
		{
			_findItUISystem.TriggerSearch();
		}

		internal void UpdateCategoriesAndPrefabList()
		{
			_findItUISystem.UpdateCategoriesAndPrefabList();
		}
	}
}
