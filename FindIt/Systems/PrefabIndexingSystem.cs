using Colossal.Logging;

using FindIt.Domain;
using FindIt.Domain.Enums;
using FindIt.Domain.Interfaces;
using FindIt.Domain.Utilities;

using Game;
using Game.Prefabs;
using Game.UI;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Unity.Collections;
using Unity.Entities;

namespace FindIt.Systems
{
	public partial class PrefabIndexingSystem : GameSystemBase
	{
		private PrefabSystem _prefabSystem;
		private ImageSystem _imageSystem;
		private readonly List<IPrefabCategoryProcessor> _prefabCategoryProcessors = new();

		protected override void OnCreate()
		{
			base.OnCreate();

			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>()!;
			_imageSystem = World.GetOrCreateSystemManaged<ImageSystem>()!;

			foreach (var type in typeof(PrefabIndexingSystem).Assembly.GetTypes())
			{
				if (typeof(IPrefabCategoryProcessor).IsAssignableFrom(type) && !type.IsAbstract)
				{
					_prefabCategoryProcessors.Add((IPrefabCategoryProcessor)Activator.CreateInstance(type, EntityManager));
				}
			}
		}

		protected override void OnUpdate()
		{
			var stopWatch = Stopwatch.StartNew();

			FindItUtil.CategorizedPrefabs.Clear();

			AddAllCategories();

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
							Mod.Log.Debug($"\tProcessing: {prefab.name}");

							if (Mod.Log.isLevelEnabled(Level.Verbose))
							{
								Mod.Log.Verbose($"\t\t> {string.Join(", ", EntityManager.GetComponentTypes(entity).Select(x => x.GetManagedType().Name))}");
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

			stopWatch.Stop();

			Mod.Log.Info($"Prefab Indexing completed in {stopWatch.Elapsed.TotalSeconds}s");
			Mod.Log.Info($"Indexed Prefabs Count: {FindItUtil.CategorizedPrefabs[PrefabCategory.Any][PrefabSubCategory.Any].Count}");

			//foreach (var grp in Prefabs.GroupBy(x => x.Category))
			//{
			//	Mod.Log.Debug(grp.Key);

			//	foreach (var sgrp in grp.GroupBy(x => x.SubCategory))
			//	{
			//		Mod.Log.Debug("\t" + sgrp.Key);

			//		foreach (var item in sgrp)
			//		{
			//			Mod.Log.Debug("\t\t" + item.Name);
			//		}
			//	}
			//}
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
			prefabIndex.Id = entity.Index;
			prefabIndex.Name = prefab.name.FormatWords();
			prefabIndex.Thumbnail = _imageSystem.GetThumbnail(entity);
			prefabIndex.Favorited = FindItUtil.IsFavorited(prefab);

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
	}
}
