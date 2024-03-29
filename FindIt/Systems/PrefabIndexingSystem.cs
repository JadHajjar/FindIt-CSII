using Colossal.Logging;

using FindIt.Domain;
using FindIt.Domain.Interfaces;
using FindIt.Domain.Utilities;

using Game;
using Game.Prefabs;
using Game.UI;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Unity.Collections;
using Unity.Entities;

namespace FindIt.Systems
{
	public partial class PrefabIndexingSystem : GameSystemBase
	{
		private PrefabSystem _prefabSystem;
		private ImageSystem _imageSystem;
		private FieldInfo _prefabFieldInfo;
		private IPrefabCategoryProcessor[] _prefabCategoryProcessors;

		public List<PrefabIndex> Prefabs { get; private set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>()!;
			_imageSystem = World.GetOrCreateSystemManaged<ImageSystem>()!;
			_prefabFieldInfo = typeof(PrefabSystem).GetField("m_Prefabs", BindingFlags.Instance | BindingFlags.NonPublic);
			_prefabCategoryProcessors = new IPrefabCategoryProcessor[]
			{
				new ServiceBuildingPrefabCategoryProcessor(EntityManager),
				new ZonedBuildingPrefabCategoryProcessor(EntityManager)
			};

			for (var i = 0; i < _prefabCategoryProcessors.Length; i++)
			{
				_prefabCategoryProcessors[i].Query = GetEntityQuery(_prefabCategoryProcessors[i].GetEntityQuery());
			}
		}

		protected override void OnUpdate()
		{
			var stopWatch = Stopwatch.StartNew();

			Prefabs = new();

			for (var ind = 0; ind < _prefabCategoryProcessors.Length; ind++)
			{
				var processor = _prefabCategoryProcessors[ind];

				Mod.Log.Info($"Indexing prefabs with {processor.GetType().Name}");

				try
				{
					var entities = processor.Query.ToEntityArray(Allocator.Temp);

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
			Mod.Log.Info($"Indexed Prefabs Count: {Prefabs.Count}");

			foreach (var grp in Prefabs.GroupBy(x => x.Category))
			{
				Mod.Log.Debug(grp.Key);

				foreach (var sgrp in grp.GroupBy(x => x.SubCategory))
				{
					Mod.Log.Debug("\t"+sgrp.Key);

					foreach (var item in sgrp)
					{
						Mod.Log.Debug("\t\t" + item.Name);
					}
				}
			}
		}

		private void AddPrefab(PrefabBase prefab, Entity entity, PrefabIndex prefabIndex)
		{
			prefabIndex.Id = entity.Index;
			prefabIndex.Name = prefab.name;
			prefabIndex.Thumbnail = _imageSystem.GetThumbnail(entity);

			Prefabs.Add(prefabIndex);
		}
	}
}
