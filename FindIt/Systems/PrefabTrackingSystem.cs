using Colossal.Serialization.Entities;

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

		private PrefabSystem _prefabSystem;
		private ToolSystem _toolSystem;
		private EntityQuery generalEntityQuery;
		private Timer timer;

		protected override void OnCreate()
		{
			base.OnCreate();

			_prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
			_toolSystem = World.GetOrCreateSystemManaged<ToolSystem>();
			_toolSystem.EventPrefabChanged += OnPrefabChanged;

			generalEntityQuery = GetEntityQuery(ComponentType.ReadOnly<Game.Objects.Object>(), ComponentType.Exclude<Owner>());

			RequireForUpdate(generalEntityQuery);

			timer = new Timer(TimeSpan.FromMinutes(1).TotalMilliseconds);
			timer.Elapsed += (s, e) => Enabled = true;
			timer.Start();
		}

		protected override void OnUpdate()
		{
			var stopWatch = Stopwatch.StartNew();
			var prefabRefs = generalEntityQuery.ToComponentDataArray<PrefabRef>(Allocator.Temp);
			var dictionary = new Dictionary<int, int>();

			for (var i = 0; i < prefabRefs.Length; i++)
			{
				var id = prefabRefs[i].m_Prefab.Index;

				if (dictionary.TryGetValue(id, out var count))
				{
					dictionary[id] = count + 1;
				}
				else
				{
					dictionary[id] = 1;
				}
			}

			_mostUsedPrefabs = dictionary;

			Enabled = false;

			stopWatch.Stop();

			Mod.Log.Info($"Most used tracking finished in {stopWatch.Elapsed.TotalSeconds:0.000}s");
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

		internal static int GetLastUsedIndex(PrefabIndex x)
		{
			return _lastUsedPrefabs.IndexOf(x.Id);
		}

		internal static int GetMostUsedCount(PrefabIndex x)
		{
			return _mostUsedPrefabs.TryGetValue(x.Id, out var count) ? count : 0;
		}
	}
}
