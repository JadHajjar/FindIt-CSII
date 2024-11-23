using FindIt.Domain;
using FindIt.Domain.Utilities;

using Game;
using Game.Common;
using Game.Prefabs;
using Game.Tools;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;

using Unity.Collections;
using Unity.Entities;

namespace FindIt.Systems
{
	internal partial class PrefabTrackingSystem : GameSystemBase
	{
		private static readonly List<int> _lastUsedPrefabs = new();
		private static Dictionary<int, int> _mostUsedPrefabs = new();
		private static Dictionary<int, List<Entity>> _usedPrefabs;
		private ToolSystem _toolSystem;
		private EntityQuery generalEntityQuery;
		private Timer timer;

		protected override void OnCreate()
		{
			base.OnCreate();

			_toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			_toolSystem.EventPrefabChanged += OnPrefabChanged;

			generalEntityQuery = SystemAPI.QueryBuilder().WithAll<Game.Objects.Object, PrefabRef>().WithNone<Owner>().Build();

			RequireForUpdate(generalEntityQuery);

			timer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
			timer.Elapsed += (s, e) => Enabled = true;
			timer.Start();
		}

		protected override void OnUpdate()
		{
			var stopWatch = Stopwatch.StartNew();
			var prefabRefs = generalEntityQuery.ToComponentDataArray<PrefabRef>(Allocator.Temp);
			var entities = generalEntityQuery.ToEntityArray(Allocator.Temp);
			var dictionary = new Dictionary<int, int>();
			var entitiesDictionary = new Dictionary<int, List<Entity>>();

			for (var i = 0; i < prefabRefs.Length; i++)
			{
				var id = prefabRefs[i].m_Prefab.Index;

				if (dictionary.TryGetValue(id, out var count))
				{
					dictionary[id] = count + 1;
					entitiesDictionary[id].Add(entities[i]);
				}
				else
				{
					dictionary[id] = 1;
					entitiesDictionary[id] = new List<Entity> { entities[i] };
				}
			}

			_mostUsedPrefabs = dictionary;
			_usedPrefabs = entitiesDictionary;

			Enabled = false;

			stopWatch.Stop();

			Mod.Log.Debug($"Most used tracking finished in {stopWatch.Elapsed.TotalSeconds:0.000}s");
		}

		private void OnPrefabChanged(PrefabBase prefab)
		{
			if (prefab == null)
			{
				return;
			}

			if (FindItUtil.Find(prefab, false, out var id))
			{
				_lastUsedPrefabs.Remove(id);
				_lastUsedPrefabs.Add(id);
			}
		}

		internal static int GetLastUsedIndex(PrefabIndexBase prefab)
		{
			return _lastUsedPrefabs.IndexOf(prefab.Id);
		}

		internal static int GetMostUsedCount(PrefabIndexBase prefab)
		{
			return _mostUsedPrefabs.TryGetValue(prefab.Id, out var count) ? count : 0;
		}

		internal static List<Entity> GetPlacedEntities(int id)
		{
			return _usedPrefabs.TryGetValue(id, out var entities) ? entities : new();
		}
	}
}
