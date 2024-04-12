using FindIt.Domain.UIBinding;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindIt.Domain.Interfaces
{
	internal interface IOptionSection
	{
		int Id { get; }

		bool IsVisible();
		OptionSectionUIEntry AsUIEntry();
		void OnOptionClicked(int optionId);
	}
}
