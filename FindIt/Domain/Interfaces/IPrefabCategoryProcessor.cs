using Game.Prefabs;

using Unity.Entities;

namespace FindIt.Domain.Interfaces
{
	public interface IPrefabCategoryProcessor
	{
		EntityQuery Query { get; set; }

		EntityQueryDesc[] GetEntityQuery();
		bool TryCreatePrefabIndex(PrefabBase prefab, Entity entity, out PrefabIndex prefabIndex);
	}
}
