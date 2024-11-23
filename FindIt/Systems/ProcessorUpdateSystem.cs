using FindIt.Domain.Interfaces;

using Game;

using System;

namespace FindIt.Systems
{
	public partial class ProcessorUpdateSystem<Processor> : GameSystemBase where Processor : IPrefabCategoryProcessor
	{
		protected override void OnUpdate()
		{
			throw new NotImplementedException();
		}
	}
}
