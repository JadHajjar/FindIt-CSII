using System.Collections.Generic;

namespace FindIt.Domain
{
	public class CustomPrefabData
	{
		public bool IsFavorited { get; set; }
		public List<string> Tags { get; set; } = new();
	}
}
