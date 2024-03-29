using Colossal.UI.Binding;

using FindIt.Domain.Enums;
using Game.Prefabs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindIt.Domain.UIBinding
{
	public struct PrefabUIEntry : IJsonWritable
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Thumbnail { get; set; }

		public void Write(IJsonWriter writer)
		{
			throw new NotImplementedException();
		}
	}
}
