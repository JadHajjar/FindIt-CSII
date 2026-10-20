using FindIt.Domain;
using FindIt.Utilities;
using Game;
using Game.Common;
using Game.Net;
using Game.Prefabs;
using Game.Tools;
using Game.Vehicles;

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

using Object = Game.Objects.Object;

namespace FindIt.Systems
{
    internal partial class PrefabTrackingSystem : GameSystemBase
	{
		// Each count walks every placed object, and the counts only need to catch up between clicks.
		private static readonly TimeSpan _trackingInterval = TimeSpan.FromMinutes(1);

		// A sort on the search worker reads it while the main thread records a use, so each use
		// replaces the whole dictionary.
		private static volatile Dictionary<int, int> _lastUsedPrefabs = new();
		private static int _lastUseStamp;
		private static Dictionary<int, int> _mostUsedPrefabs = new();
		private readonly Stopwatch _sinceLastTracking = new();
		private ToolSystem _toolSystem;
		private EntityQuery generalEntityQuery;

		protected override void OnCreate()
		{
			base.OnCreate();

			_toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			_toolSystem.EventPrefabChanged += OnPrefabChanged;

			// An object tool's preview of the picked prefab is a Temp object with its PrefabRef and no Owner.
			generalEntityQuery = SystemAPI.QueryBuilder().WithAll<PrefabRef>().WithAny<Object, Edge>().WithNone<Owner, Controller, Overridden, Temp, Deleted>().Build();

			RequireForUpdate(generalEntityQuery);

			// Only the panel reads the counts, so the UI system enables this system while it's shown.
			Enabled = false;
		}

		protected override void OnUpdate()
		{
			if (_sinceLastTracking.Elapsed >= _trackingInterval)
			{
				CountPlacedPrefabs();
			}
		}

		internal void CountPlacedPrefabs()
		{
			_sinceLastTracking.Restart();

			var stopWatch = Stopwatch.StartNew();
			using var prefabRefs = generalEntityQuery.ToComponentDataArray<PrefabRef>(Allocator.TempJob);
			using var counts = new NativeHashMap<int, int>(1024, Allocator.TempJob);

			new CountPrefabsJob { PrefabRefs = prefabRefs, Counts = counts }.Run();

			var dictionary = new Dictionary<int, int>(counts.Count);

			foreach (var pair in counts)
			{
				dictionary[pair.Key] = pair.Value;
			}

			_mostUsedPrefabs = dictionary;

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
				_lastUsedPrefabs = new Dictionary<int, int>(_lastUsedPrefabs) { [id] = ++_lastUseStamp };
			}
		}

		internal static int GetLastUsedIndex(PrefabIndexBase prefab)
		{
			return _lastUsedPrefabs.TryGetValue(prefab.Id, out var stamp) ? stamp : -1;
		}

		internal static int GetMostUsedCount(PrefabIndexBase prefab)
		{
			return _mostUsedPrefabs.TryGetValue(prefab.Id, out var count) ? count : 0;
		}

		// A stored list could be a minute old and hold deleted objects, so each click queries the prefab's objects.
		internal Entity GetNextPlacedEntity(int id, Entity previous)
		{
			using var prefabRefs = generalEntityQuery.ToComponentDataArray<PrefabRef>(Allocator.TempJob);
			using var entities = generalEntityQuery.ToEntityArray(Allocator.TempJob);
			using var result = new NativeReference<Entity>(Allocator.TempJob);

			new NextPlacedEntityJob
			{
				PrefabRefs = prefabRefs,
				Entities = entities,
				Id = id,
				Previous = previous,
				Result = result
			}.Run();

			return result.Value;
		}

		[BurstCompile]
		private struct CountPrefabsJob : IJob
		{
			[ReadOnly]
			public NativeArray<PrefabRef> PrefabRefs;
			public NativeHashMap<int, int> Counts;

			public void Execute()
			{
				for (var i = 0; i < PrefabRefs.Length; i++)
				{
					var id = PrefabRefs[i].m_Prefab.Index;

					if (Counts.TryGetValue(id, out var count))
					{
						Counts[id] = count + 1;
					}
					else
					{
						Counts.Add(id, 1);
					}
				}
			}
		}

		[BurstCompile]
		private struct NextPlacedEntityJob : IJob
		{
			[ReadOnly]
			public NativeArray<PrefabRef> PrefabRefs;
			[ReadOnly]
			public NativeArray<Entity> Entities;
			public int Id;
			public Entity Previous;
			public NativeReference<Entity> Result;

			public void Execute()
			{
				var first = Entity.Null;
				var next = Entity.Null;

				// The query's order changes as objects move between chunks, so Locate steps by entity index.
				for (var i = 0; i < PrefabRefs.Length; i++)
				{
					if (PrefabRefs[i].m_Prefab.Index != Id)
					{
						continue;
					}

					var entity = Entities[i];

					if (first == Entity.Null || entity.Index < first.Index)
					{
						first = entity;
					}

					if (Previous != Entity.Null
						&& entity.Index > Previous.Index
						&& (next == Entity.Null || entity.Index < next.Index))
					{
						next = entity;
					}
				}

				Result.Value = next == Entity.Null ? first : next;
			}
		}
	}
}
