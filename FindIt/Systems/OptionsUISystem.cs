using FindIt.Domain.Utilities;

using Game.Prefabs;
using Game.SceneFlow;
using Game.UI.InGame;

using System;

namespace FindIt.Systems
{
	internal abstract partial class OptionsUISystem : ExtendedUISystemBase
	{
		private PrefabSystem _prefabSystem;
		private PrefabUISystem _prefabUISystem;

		protected override void OnCreate()
		{
			base.OnCreate();

			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();
		}

		public abstract void TriggerSearch();
		public abstract void RefreshOptions();
		public abstract void UpdateCategoriesAndPrefabList();

		public string GetAssetName(PrefabBase prefab)
		{
			_prefabUISystem.GetTitleAndDescription(_prefabSystem.GetEntity(prefab), out var titleId, out var _);

			if (GameManager.instance.localizationManager.activeDictionary.TryGetValue(titleId, out var name))
			{
				return name;
			}

			return prefab.name.Replace('_', ' ').FormatWords();
		}
	}
}
