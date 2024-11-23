using FindIt.Domain.UIBinding;

namespace FindIt.Domain.Interfaces
{
	internal interface IOptionSection
	{
		int Id { get; }

		bool IsDefault();
		bool IsVisible();
		OptionSectionUIEntry AsUIEntry();
		void OnOptionClicked(int optionId, int value);
		void OnReset();
	}
}
