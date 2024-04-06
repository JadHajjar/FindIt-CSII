using Colossal.Logging;
using Colossal.PSI.Common;
using Colossal.Serialization.Entities;

using FindIt.Domain;
using FindIt.Domain.Enums;
using FindIt.Domain.Interfaces;
using FindIt.Domain.Utilities;

using Game;
using Game.Prefabs;
using Game.SceneFlow;
using Game.UI;
using Game.UI.InGame;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Unity.Collections;
using Unity.Entities;

namespace FindIt.Systems
{
	public partial class PrefabIndexingSystem : GameSystemBase
	{
		private PrefabSystem _prefabSystem;
		private ImageSystem _imageSystem;
		private PrefabUISystem _prefabUISystem;
		private HashSet<string> _blackList;

		private readonly List<IPrefabCategoryProcessor> _prefabCategoryProcessors = new();

		protected override void OnCreate()
		{
			base.OnCreate();

			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_imageSystem = World.GetOrCreateSystemManaged<ImageSystem>();
			_prefabUISystem = World.GetOrCreateSystemManaged<PrefabUISystem>();

			using var stream = typeof(Mod).Assembly.GetManifestResourceStream("FindIt.Resources.Blacklist.txt");
			using var reader = new StreamReader(stream);

			_blackList = new HashSet<string>(reader.ReadToEnd().Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

			foreach (var type in typeof(PrefabIndexingSystem).Assembly.GetTypes())
			{
				if (typeof(IPrefabCategoryProcessor).IsAssignableFrom(type) && !type.IsAbstract)
				{
					var constructor = type.GetConstructors()[0];
					var parameters = constructor.GetParameters();
					var objectParams = new object[parameters.Length];

					for (var i = 0; i < parameters.Length; i++)
					{
						if (parameters[i].ParameterType == typeof(EntityManager))
						{
							objectParams[i] = EntityManager;
						}
						else if (parameters[i].ParameterType == typeof(PrefabSystem))
						{
							objectParams[i] = _prefabSystem;
						}
						else if (parameters[i].ParameterType == typeof(ImageSystem))
						{
							objectParams[i] = _imageSystem;
						}
					}

					_prefabCategoryProcessors.Add((IPrefabCategoryProcessor)Activator.CreateInstance(type, objectParams));
				}
			}
		}

		protected override void OnGamePreload(Purpose purpose, GameMode mode)
		{
			base.OnGamePreload(purpose, mode);

			Enabled = true;
		}

		protected override void OnUpdate()
		{
			var stopWatch = Stopwatch.StartNew();

			FindItUtil.CategorizedPrefabs.Clear();

			AddAllCategories();

			var tm_ExcludeCategories = new HashSet<string>();
			var tm_IncludeCategories = new HashSet<string>();

			for (var ind = 0; ind < _prefabCategoryProcessors.Count; ind++)
			{
				var processor = _prefabCategoryProcessors[ind];

				Mod.Log.Info($"Indexing prefabs with {processor.GetType().Name}");

				try
				{
					var entities = GetEntityQuery(_prefabCategoryProcessors[ind].GetEntityQuery()).ToEntityArray(Allocator.Temp);

					Mod.Log.Info($"\tTotal Entities Count: {entities.Length}");

					for (var i = 0; i < entities.Length; i++)
					{
						var entity = entities[i];

						if (_prefabSystem.TryGetPrefab<PrefabBase>(entity, out var prefab) && prefab?.name is not null)
						{
							if (_blackList.Contains(prefab.name))
							{
								continue;
							}

							if (Mod.Log.isLevelEnabled(Level.Debug))
							{
								Mod.Log.Debug($"\tProcessing: {prefab.name}");
#if DEBUG
								Mod.Log.Debug($"\t\t> {prefab.GetType().Name} - {string.Join(", ", EntityManager.GetComponentTypes(entity).Select(x => x.GetManagedType()?.Name ?? string.Empty))}");
#endif
							}

							if (processor.TryCreatePrefabIndex(prefab, entity, out var prefabIndex))
							{
								AddPrefab(prefab, entity, prefabIndex);
							}
							else
							{
								Mod.Log.Debug($"\t\tSkipped: {prefab.name}");
							}
						}
					}
				}
				catch (Exception ex)
				{
					Mod.Log.Error(ex, $"Prefab indexing failed for processor {processor.GetType().Name}");
				}
			}

			foreach (var grp in FindItUtil.CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any].GroupBy(x => x.Name))
			{
				var count = grp.Count();

				if (count == 1)
				{
					continue;
				}

				var format = new string('0', count.ToString().Length);
				var index = 1;

				foreach (var prefab in grp)
				{
					prefab.Name = $"{prefab.Name} {index++.ToString(format)}";
				}
			}

			stopWatch.Stop();

			Mod.Log.Info($"Prefab Indexing completed in {stopWatch.Elapsed.TotalSeconds}s");
			Mod.Log.Info($"Indexed Prefabs Count: {FindItUtil.CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any].Count}");

			Enabled = false;
		}

		private void AddAllCategories()
		{
			foreach (PrefabCategory category in Enum.GetValues(typeof(PrefabCategory)))
			{
				FindItUtil.CategorizedPrefabs[category] = new()
				{
					{ PrefabSubCategory.Any, new() }
				};

				if (category == PrefabCategory.Any)
				{
					continue;
				}

				foreach (PrefabSubCategory subCategory in Enum.GetValues(typeof(PrefabSubCategory)))
				{
					if ((int)subCategory > (int)category && (int)subCategory < (int)category + 100)
					{
						FindItUtil.CategorizedPrefabs[category][subCategory] = new();
					}
				}
			}
		}

		private void AddPrefab(PrefabBase prefab, Entity entity, PrefabIndex prefabIndex)
		{
			if (PrefabIconUtil.TryGetCustomThumbnail(prefab.name, out var prefabThumbnail))
			{
				prefabIndex.Thumbnail = prefabThumbnail;
			}

			prefabIndex.Id = entity.Index;
			prefabIndex.Name = GetAssetName(prefab);
			prefabIndex.Thumbnail ??= _imageSystem.GetThumbnail(entity);
			prefabIndex.Favorited = FindItUtil.IsFavorited(prefab);
			prefabIndex.FallbackThumbnail ??= CategoryIconAttribute.GetAttribute(prefabIndex.SubCategory).Icon;
			prefabIndex.CategoryThumbnail ??= CategoryIconAttribute.GetAttribute(prefabIndex.Category).Icon;

			if (prefab.TryGet<ContentPrerequisite>(out var contentPrerequisites) && contentPrerequisites.m_ContentPrerequisite.TryGet<DlcRequirement>(out var dlcRequirements))
			{
				prefabIndex.DlcThumbnail = $"Media/DLC/{PlatformManager.instance.GetDlcName(dlcRequirements.m_Dlc)}.svg";
			}

			FindItUtil.CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any][prefabIndex.Id] = prefabIndex;
			FindItUtil.CategorizedPrefabs[prefabIndex.Category][PrefabSubCategory.Any][prefabIndex.Id] = prefabIndex;
			FindItUtil.CategorizedPrefabs[prefabIndex.Category][prefabIndex.SubCategory][prefabIndex.Id] = prefabIndex;

			if (prefabIndex.Favorited)
			{
				FindItUtil.CategorizedPrefabs[PrefabCategory.Favorite][PrefabSubCategory.Any][prefabIndex.Id] = prefabIndex;

				if (!FindItUtil.CategorizedPrefabs[PrefabCategory.Favorite].ContainsKey(prefabIndex.SubCategory))
				{
					FindItUtil.CategorizedPrefabs[PrefabCategory.Favorite][prefabIndex.SubCategory] = new();
				}

				FindItUtil.CategorizedPrefabs[PrefabCategory.Favorite][prefabIndex.SubCategory][prefabIndex.Id] = prefabIndex;
			}
		}

		private string GetAssetName(PrefabBase prefab)
		{
			_prefabUISystem.GetTitleAndDescription(prefab, out var titleId, out var _);

			if (GameManager.instance.localizationManager.activeDictionary.TryGetValue(titleId, out var name))
			{
				return name;
			}

			return prefab.name.Replace('_', ' ').FormatWords();
		}
	}
}
