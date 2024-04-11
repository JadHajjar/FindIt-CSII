using FindIt.Domain.UIBinding;
using FindIt.Domain.Utilities;

namespace FindIt.Systems
{
	internal partial class OptionsUISystem : ExtendedUISystemBase
	{
		private ValueBindingHelper<string> _ViewStyle;
		private ValueBindingHelper<OptionSection[]> _OptionsList;

		protected override void OnCreate()
		{
			base.OnCreate();

			_ViewStyle = CreateBinding("ViewStyle", "GridWithText");
			_OptionsList = CreateBinding("OptionsList", new OptionSection[]
			{
				new()
				{
					Id = 1,
					Name = "View Style",
					Options = new[]
					{
						new OptionItem()
						{
							Id = 1,
							Name = "",
							Icon = "coui://uil/Standard/PencilPaper.svg",
							Selected = _ViewStyle == "GridWithText",
						},
						new OptionItem()
						{
							Id = 2,
							Name = "",
							Icon = "coui://uil/Standard/House.svg",
							Selected = _ViewStyle == "GridNoText",
						}
					}
				}
			});

			CreateTrigger<int, int>("OptionClicked", OptionClicked);
		}

		private void OptionClicked(int sectionId, int optionId)
		{
			if (optionId == 1)
			{
				_ViewStyle.Value = "GridWithText";
			}
			else
			{
				_ViewStyle.Value = "GridNoText";
			}

			_OptionsList.Value = new OptionSection[]
			{
				new()
				{
					Id = 1,
					Name = "View Style",
					Options = new[]
					{
						new OptionItem()
						{
							Id = 1,
							Name = "",
							Icon = "coui://uil/Standard/PencilPaper.svg",
							Selected = _ViewStyle == "GridWithText",
						},
						new OptionItem()
						{
							Id = 2,
							Name = "",
							Icon = "coui://uil/Standard/House.svg",
							Selected = _ViewStyle == "GridNoText",
						}
					}
				}
			};
		}
	}
}
