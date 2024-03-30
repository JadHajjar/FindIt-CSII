using Colossal;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Colossal.PSI.Environment;

namespace FindIt.Domain.Utilities
{
	internal class FolderUtil
	{
		public static string ContentFolder { get; }
		public static string SettingsFolder { get; }

		static FolderUtil()
		{
			ContentFolder = Path.Combine(EnvPath.kUserDataPath, "ModsData", nameof(FindIt));
			//SettingsFolder = Path.Combine(EnvPath.kUserDataPath, "ModsSettings", nameof(FindIt));

			Directory.CreateDirectory(ContentFolder);
			//Directory.CreateDirectory(SettingsFolder);
		}
	}
}
