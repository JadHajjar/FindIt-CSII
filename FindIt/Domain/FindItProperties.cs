using Game.Prefabs;

using System.Collections.Generic;

using Unity.Entities;

namespace FindIt.Domain
{
	[ComponentMenu("FindIt/", typeof(PrefabBase))]
	public class FindItProperties : ComponentBase
	{
		public string[] m_Tags;

		public override void GetArchetypeComponents(HashSet<ComponentType> components)
		{
		}

		public override void GetPrefabComponents(HashSet<ComponentType> components)
		{
		}
	}
}
