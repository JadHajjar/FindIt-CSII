using FindIt.Domain.Interfaces;

using Game;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
