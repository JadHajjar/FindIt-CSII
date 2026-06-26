using FindIt.Domain.Interfaces;
using FindIt.Domain.UIBinding;
using FindIt.Systems;
using FindIt.Utilities;

namespace FindIt.Domain.Options
{
	internal class HeightOption : IOptionSection
	{
		private const float Step = 5f;

		private readonly FindItUISystem _findItUISystem;

		public int Id { get; } = -5;

		public HeightOption(OptionsUISystem optionsUISystem)
		{
			_findItUISystem = optionsUISystem.World.GetOrCreateSystemManaged<FindItUISystem>();
		}

		public OptionSectionUIEntry AsUIEntry()
		{
			return new OptionSectionUIEntry
			{
				Id = Id,
				Name = LocaleHelper.Translate("Options.LABEL[FindIt.Height]", "Height"),
				Options = new[]
				{
					new OptionItemUIEntry
					{
						IsValue = true,
						Value = $"{(int)_findItUISystem.CurrentRowSize}%",
					}
				}
			};
		}

		public bool IsVisible()
		{
			return true;
		}

		public void OnOptionClicked(int optionId, int value)
		{
			if (value == 0)
			{
				return;
			}

			_findItUISystem.CurrentRowSize += value * Step;
		}

		public void OnReset()
		{ }

		public bool IsDefault()
		{
			return true;
		}
	}
}
