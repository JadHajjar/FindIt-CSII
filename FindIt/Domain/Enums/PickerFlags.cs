using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindIt.Domain.Enums
{
	[Flags]
	public enum PickerFlags
	{
		All = Buildings | SubObjects | Networks | Props | Surfaces,
		Default = Buildings | SubObjects | Networks | Props,

		None = 0,
		Buildings = 1,
		SubObjects = 2,
		Surfaces = 4,
		Networks = 8,
		Props = 16,
	}
}
