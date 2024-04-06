using Colossal.UI;

using System;
using System.Collections.Generic;
using System.IO;

namespace FindIt.Domain.Utilities
{
	internal class PrefabIconUtil
	{
		private static readonly Dictionary<string, string> _loadedIcons = new(StringComparer.InvariantCultureIgnoreCase);

		internal static void Initialize(string modFolder)
		{
			foreach (var item in Directory.EnumerateFiles(Path.Combine(modFolder, "PrefabThumbnails")))
			{
				_loadedIcons[Path.GetFileNameWithoutExtension(item)] = $"coui://ui-mods/PrefabThumbnails/{Path.GetFileName(item)}";
			}
		}

		internal static void OnDispose()
		{
			UIManager.defaultUISystem.RemoveHostLocation(Mod.Id);
		}

		internal static bool TryGetCustomThumbnail(string name, out string thumbnail)
		{
			return _loadedIcons.TryGetValue(name, out thumbnail);
		}
	}
}
