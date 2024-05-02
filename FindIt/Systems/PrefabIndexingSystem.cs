using Colossal.Entities;
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

			foreach (var processor in _prefabCategoryProcessors)
			{
				Mod.Log.Info($"Indexing prefabs with {processor.GetType().Name}");

				try
				{
					var queries = processor.GetEntityQuery();

					if (Mod.Settings.HideRandomAssets)
					{
						for (var i = 0; i < queries.Length; i++)
						{
							queries[i].None = queries[i].None.Concat(new[] { ComponentType.ReadOnly<PlaceholderObjectData>() }).ToArray();
						}
					}

					var entities = GetEntityQuery(queries).ToEntityArray(Allocator.Temp);

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

			AddNumberToDuplicatePrefabNames();

			CleanupBrandPrefabs();

			FindItUtil.IsReady = true;

			stopWatch.Stop();

			Mod.Log.Info($"Prefab Indexing completed in {stopWatch.Elapsed.TotalSeconds:0.000}s");
			Mod.Log.Info($"Indexed Prefabs Count: {FindItUtil.CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any].Count}");

			Enabled = false;
		}

		private void AddPrefab(PrefabBase prefab, Entity entity, PrefabIndex prefabIndex)
		{
			prefabIndex.Id = entity.Index;
			prefabIndex.Name = GetAssetName(prefab);
			prefabIndex.Thumbnail ??= ImageSystem.GetThumbnail(prefab);
			prefabIndex.IsFavorited = FindItUtil.IsFavorited(prefab);
			prefabIndex.FallbackThumbnail ??= CategoryIconAttribute.GetAttribute(prefabIndex.SubCategory).Icon;
			prefabIndex.CategoryThumbnail ??= CategoryIconAttribute.GetAttribute(prefabIndex.SubCategory).Icon;
			prefabIndex.Tags ??= new();
			prefabIndex.IsVanilla = prefab.builtin;
			prefabIndex.IsRandom = prefabIndex.SubCategory is not PrefabSubCategory.Networks_Pillars && EntityManager.HasComponent<PlaceholderObjectData>(entity);

			if (prefabIndex.IsRandom && EntityManager.TryGetBuffer<PlaceholderObjectElement>(entity, true, out var placeholderObjectElements))
			{
				prefabIndex.RandomPrefabs = new int[placeholderObjectElements.Length];
				prefabIndex.RandomPrefabThumbnails = new string[placeholderObjectElements.Length];

				for (var i = 0; i < placeholderObjectElements.Length; i++)
				{
					prefabIndex.RandomPrefabs[i] = placeholderObjectElements[i].m_Object.Index;

					if (_prefabSystem.TryGetPrefab<PrefabBase>(placeholderObjectElements[i].m_Object, out var randomPrefab))
					{
						prefabIndex.RandomPrefabThumbnails[i] = ImageSystem.GetThumbnail(randomPrefab);
					}
				}
			}

			if (prefab.TryGet<ContentPrerequisite>(out var contentPrerequisites) && contentPrerequisites.m_ContentPrerequisite.TryGet<DlcRequirement>(out var dlcRequirements))
			{
				prefabIndex.DlcId = dlcRequirements.m_Dlc;
				prefabIndex.DlcThumbnail = $"Media/DLC/{PlatformManager.instance.GetDlcName(dlcRequirements.m_Dlc)}.svg";
			}
			else
			{
				prefabIndex.DlcId = DlcId.Invalid;
			}

			if (EntityManager.TryGetComponent<BuildingData>(entity, out var buildingData))
			{
				prefabIndex.LotSize = buildingData.m_LotSize;
			}

			if (!Mod.Settings.HideBrandsFromAny || prefabIndex.SubCategory is not PrefabSubCategory.Props_Branding)
			{
				FindItUtil.CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any][prefabIndex.Id] = prefabIndex;
				FindItUtil.CategorizedPrefabs[prefabIndex.Category][PrefabSubCategory.Any][prefabIndex.Id] = prefabIndex;
			}

			FindItUtil.CategorizedPrefabs[prefabIndex.Category][prefabIndex.SubCategory][prefabIndex.Id] = prefabIndex;

			if (prefabIndex.IsFavorited)
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

		private static void AddNumberToDuplicatePrefabNames()
		{
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
		}

		private void CleanupBrandPrefabs()
		{
			var brands = new HashSet<string>(FindItUtil.CategorizedPrefabs[PrefabCategory.Props][PrefabSubCategory.Props_Branding].Select(x => x.Prefab.name));

			foreach (var category in FindItUtil.CategorizedPrefabs.Keys)
			{
				if (category is PrefabCategory.Any)
				{
					continue;
				}

				foreach (var subCategory in FindItUtil.CategorizedPrefabs[category].Keys)
				{
					if (subCategory is PrefabSubCategory.Props_Branding || (category is PrefabCategory.Props && subCategory is PrefabSubCategory.Any))
					{
						continue;
					}

					foreach (var item in FindItUtil.CategorizedPrefabs[category][subCategory].ToList())
					{
						if (brands.Contains(item.Prefab.name))
						{
							FindItUtil.CategorizedPrefabs[category][subCategory].Remove(item);

							Mod.Log.Debug($"Removed {item.Prefab.name} from {subCategory}");
						}
					}
				}
			}
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
	}
}
