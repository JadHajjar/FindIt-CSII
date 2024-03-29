using Game.Prefabs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindIt.Domain
{
	public class PrefabIndex
	{
		private PrefabBase Prefab { get; }

		public PrefabIndex(PrefabBase prefabBase)
		{
			Prefab = prefabBase;

		}
	}
}
