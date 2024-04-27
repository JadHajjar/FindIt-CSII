using FindIt.Systems;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Unity.Entities;

namespace FindIt.Domain.Utilities
{
	internal static class GridUtil
	{
		private static readonly PrefabSearchUISystem _prefabSearchUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PrefabSearchUISystem>();
		private static readonly FindItPanelUISystem _findItPanelUISystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<FindItPanelUISystem>();

		internal static float GetCurrentColumnCount()
		{
			var width = GetWidth();
			var itemWidth = _prefabSearchUISystem.ViewStyle switch
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
			return 325 + (_findItPanelUISystem.IsExpanded ? Mod.Settings.ExpandedColumnSize : Mod.Settings.ColumnSize) * 8.5f;
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
			var itemHeight = _prefabSearchUISystem.ViewStyle switch
			{
				"GridWithText" => 98f,
				"GridNoText" => GetItemWidth()+4,
				"GridSmall" => GetItemWidth()+2,
				"ListSimple" => 22.5f,
				_ => float.MaxValue
			};

			return (float)Math.Max(0.1, height / itemHeight);
		}

		internal static float GetHeight()
		{
			return 100 + (_findItPanelUISystem.IsExpanded ? Mod.Settings.ExpandedRowSize : Mod.Settings.RowSize) * 2.5f;
		}

		internal static float GetScrollMultiplier()
		{
			return _prefabSearchUISystem.ViewStyle switch
			{
				"GridSmall" => 2f,
				"ListSimple" => 6f,
				_ => 1f
			};
		}
	}
}
