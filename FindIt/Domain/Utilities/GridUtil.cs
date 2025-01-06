using FindIt.Systems;

using System;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	internal static class GridUtil
	{
		private static readonly FindItUISystem _findItUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<FindItUISystem>();

		internal static float GetCurrentColumnCount()
		{
			var width = GetWidth();
			var itemWidth = _findItUISystem.ViewStyle switch
			{
				"GridWithText" => 113f,
				"GridNoText" => 100f,
				"GridSmall" => 64f,
				_ => float.MaxValue
			};

			return (float)Math.Max(1, Math.Floor(width / itemWidth));
		}

		internal static float GetWidth()
		{
			if (_findItUISystem.AlignmentStyle == "Right")
			{
				return 150 + (Mod.Settings.RightColumnSize * 8.5f);
			}

			return 325 + ((_findItUISystem.IsExpanded ? Mod.Settings.ExpandedColumnSize : Mod.Settings.ColumnSize) * 8.5f);
		}

		internal static float GetItemWidth()
		{
			var width = GetWidth();
			var columns = GetCurrentColumnCount();

			return (width - 15) / columns;
		}

		internal static float GetCurrentRowCount()
		{
			var height = GetHeight();
			var itemHeight = _findItUISystem.ViewStyle switch
			{
				"GridWithText" => 98f,
				"GridNoText" => GetItemWidth() + 4,
				"GridSmall" => GetItemWidth() + 2,
				"ListSimple" => 22.5f,
				_ => float.MaxValue
			};

			return (float)Math.Max(0.1, height / itemHeight);
		}

		internal static float GetHeight()
		{
			return 100 + ((_findItUISystem.AlignmentStyle == "Right" ? (Mod.Settings.RightRowSize * 2.5f) : _findItUISystem.IsExpanded ? Mod.Settings.ExpandedRowSize : Mod.Settings.RowSize) * 2.5f);
		}

		internal static float GetScrollMultiplier()
		{
			var multiplier = _findItUISystem.AlignmentStyle == "Right" ? 2.5f : 1f;

			return multiplier * _findItUISystem.ViewStyle switch
			{
				"GridSmall" => _findItUISystem.AlignmentStyle == "Right" ? 12f : 4f,
				"ListSimple" => GetHeight() / 12f,
				_ => _findItUISystem.AlignmentStyle == "Right" ? 5f : 2f
			};
		}
	}
}
