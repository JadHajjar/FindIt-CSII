using FindIt.Domain.Enums;

using Game.Prefabs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindIt.Domain
{
	public class PrefabIndex : PrefabIndexBase
	{
		public PrefabCategory Category { get; set; }
		public PrefabSubCategory SubCategory { get; set; }

		public PrefabIndex(PrefabBase prefabBase) : base(prefabBase)
		{
		}
	}
}
