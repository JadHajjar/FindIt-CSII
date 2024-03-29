using FindIt.Domain.Enums;

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
		public PrefabBase Prefab { get; }
        public int Id { get; set; }
        public string Name { get; set; }
        public PrefabCategory Category { get; set; }
        public PrefabSubCategory SubCategory { get; set; }
        public string Thumbnail { get; set; }

        public PrefabIndex(PrefabBase prefabBase)
		{
			Prefab = prefabBase;
		}
	}
}
