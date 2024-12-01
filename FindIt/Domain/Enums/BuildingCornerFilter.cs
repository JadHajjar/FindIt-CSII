using System;

namespace FindIt.Domain.Enums
{
	[Flags]
	public enum BuildingCornerFilter
	{
		Any = Left | Front | Right,
		Left = 1,
		Front = 2,
		Right = 4
	}
}
