using System;

namespace FindIt.Domain.Enums
{
	[Flags]
	public enum BuildingCornerFilter
	{
		Any = 0,
		Left = 1,
		Front = 2,
		Right = 4
	}
}
