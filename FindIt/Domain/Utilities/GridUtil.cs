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
				"GridNoText" => 90f,
				"GridSmall" => 54f,
				_ => float.MaxValue
			};

			return (float)Math.Max(1, Math.Floor(width / itemWidth));
		}

		internal static float GetWidth()
		{
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
			return 100 + ((_findItUISystem.IsExpanded ? Mod.Settings.ExpandedRowSize : Mod.Settings.RowSize) * 2.5f);
		}

		internal static float GetScrollMultiplier()
		{
			return _findItUISystem.ViewStyle switch
			{
				"GridSmall" => 4f,
				"ListSimple" => 12f,
				_ => 2f
			};
		}
	}
}
