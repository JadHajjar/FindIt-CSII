using FindIt.Domain;

using Game;
using Game.Prefabs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FindIt.Systems
{
	public partial class PrefabIndexingSystem : GameSystemBase
	{
		private PrefabSystem _prefabSystem;
		private FieldInfo _prefabFieldInfo;

		protected override void OnCreate()
		{
			base.OnCreate();

			_prefabSystem = World.GetExistingSystemManaged<PrefabSystem>();
			_prefabFieldInfo = typeof(PrefabSystem).GetField("m_Prefabs", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		protected override void OnUpdate()
		{
			var prefabs = ((List<PrefabBase>)_prefabFieldInfo.GetValue(_prefabSystem));

			Mod.Log.Info("Prefab Count: " + prefabs.Count);

			for (var i = 0; i < prefabs.Count; i++)
			{
				new PrefabIndex(prefabs[i]);
			}
		}
	}
}
