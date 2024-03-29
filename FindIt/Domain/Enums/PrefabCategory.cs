using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindIt.Domain.Enums
{
	public enum PrefabCategory
	{
		[CategoryIcon("coui://uil/Colored/StarFilled.svg")]
		Favorite = 0,
		[CategoryIcon("Media/Game/Icons/ZoneSignature.svg")]
		Buildings = 100,
		[CategoryIcon("Media/Game/Icons/Roads.svg")]
		Networks = 200,
		[CategoryIcon("Media/Game/Icons/Forest.svg")]
		Trees = 300,
		[CategoryIcon("Media/Game/Resources/Furniture.svg")]
		Props = 400
	}
}
