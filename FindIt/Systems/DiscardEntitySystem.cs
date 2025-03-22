using FindIt.Utilities;
using Game;
using Game.Common;
using Game.Prefabs;

using System.Reflection;

using Unity.Collections;
using Unity.Entities;

namespace FindIt.Systems
{
    public partial class DiscardEntitySystem : GameSystemBase
	{
		private EntityQuery query;
		private ComponentType? roadBuilderDiscarded;

		protected override void OnCreate()
		{
			base.OnCreate();

			query = SystemAPI.QueryBuilder().WithAll<PrefabData>().WithAny<Deleted, Updated>().Build();

			RequireForUpdate(query);
		}

		protected override void OnUpdate()
		{
			var entities = query.ToEntityArray(Allocator.Temp);

			for (var i = 0; i < entities.Length; i++)
			{
				var entity = entities[i];

				if (Mod.IsRoadBuilderEnabled)
				{
					roadBuilderDiscarded ??= new ComponentType(Assembly.Load("RoadBuilder").GetType("RoadBuilder.Domain.Components.DiscardedRoadBuilderPrefab"), ComponentType.AccessMode.ReadOnly);

					if (EntityManager.HasComponent(entity, roadBuilderDiscarded.Value))
					{
						FindItUtil.RemoveItem(entity);
					}
				}

				if (EntityManager.HasComponent<Deleted>(entity))
				{
					FindItUtil.RemoveItem(entity);
				}
			}
		}
	}
}
