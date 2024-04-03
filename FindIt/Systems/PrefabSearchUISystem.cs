using Colossal.UI.Binding;

using FindIt.Domain.Enums;
using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;

using Game.UI;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FindIt.Systems
{
	internal partial class PrefabSearchUISystem : UISystemBase
	{
		private CancellationTokenSource tokenSource;

		private ValueBinding<bool> _IsSearchLoading;
		private ValueBinding<string> _CurrentSearch;
		private ValueBinding<CategoryUIEntry[]> _CategoryBinding;
		private ValueBinding<SubCategoryUIEntry[]> _SubCategoryBinding;
		private ValueBinding<PrefabUIEntry[]> _PrefabListBinding;
		private bool filterCompleted;

		protected override void OnCreate()
		{
			base.OnCreate();

			AddBinding(_IsSearchLoading = new ValueBinding<bool>(Mod.Id, "IsSearchLoading", false));
			AddBinding(_CategoryBinding = new ValueBinding<CategoryUIEntry[]>(Mod.Id, "CategoryList", new CategoryUIEntry[] { new(PrefabCategory.Any) }, new ArrayWriter<CategoryUIEntry>(new ValueWriter<CategoryUIEntry>())));
			AddBinding(_SubCategoryBinding = new ValueBinding<SubCategoryUIEntry[]>(Mod.Id, "SubCategoryList", new SubCategoryUIEntry[] { new(PrefabSubCategory.Any) }, new ArrayWriter<SubCategoryUIEntry>(new ValueWriter<SubCategoryUIEntry>())));
			AddBinding(_PrefabListBinding = new ValueBinding<PrefabUIEntry[]>(Mod.Id, "PrefabList", new PrefabUIEntry[0], new ArrayWriter<PrefabUIEntry>(new ValueWriter<PrefabUIEntry>())));
			AddBinding(_CurrentSearch = new ValueBinding<string>(Mod.Id, "CurrentSearch", ""));

			AddBinding(new TriggerBinding<string>(Mod.Id, "SearchChanged", SearchChanged));
		}

		internal PrefabUIEntry UpdateCategoriesList()
		{
			_CategoryBinding.Update(FindItUtil.GetCategories().Select(x => new CategoryUIEntry(x)).ToArray());
			_SubCategoryBinding.Update(FindItUtil.GetSubCategories().Select(x => new SubCategoryUIEntry(x)).ToArray());

			var prefabs = FindItUtil.GetFilteredPrefabs().Take(16).Select(x => new PrefabUIEntry(x)).ToArray();
			
			_PrefabListBinding.Update(prefabs);

			return prefabs.FirstOrDefault();
		}

		public void SearchChanged(string text)
		{
			FindItUtil.CurrentSearch = text;

			_IsSearchLoading.Update(true);
			_CurrentSearch.Update(text);

			Task.Run(DelayedSearch);
		}

		private async Task DelayedSearch()
		{
			tokenSource?.Cancel();
			tokenSource = new();

			var token = tokenSource.Token;

			await Task.Delay(250);

			if (token.IsCancellationRequested)
			{
				return;
			}

			try
			{
				FindItUtil.ProcessSearch(token);

				if (!token.IsCancellationRequested)
				{
					filterCompleted = true;
				}
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Search Failed");
			}
		}

		protected override void OnUpdate()
		{
			if (filterCompleted)
			{
				filterCompleted = false;

				_IsSearchLoading.Update(false);
				_PrefabListBinding.Update(FindItUtil.GetFilteredPrefabs().Take(16).Select(x => new PrefabUIEntry(x)).ToArray());
			}

			base.OnUpdate();
		}
	}
}
