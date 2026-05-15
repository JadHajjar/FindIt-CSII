using Colossal.PSI.Common;
using Colossal.PSI.PdxSdk;

using PDX.SDK.Contracts;
using PDX.SDK.Contracts.Service.Mods.Results;

using System;
using System.Threading.Tasks;

namespace FindIt.Utilities
{
	public static class PdxModsUtil
	{
		private static readonly PdxSdkPlatform _pdxPlatform;
		private static readonly IContext _context;

		static PdxModsUtil()
		{
			try
			{
				_pdxPlatform = PlatformManager.instance.GetPSI<PdxSdkPlatform>("PdxSdk");
				_context = typeof(PdxSdkPlatform).GetField("m_SDKContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_pdxPlatform) as IContext;
			}
			catch (Exception ex)
			{
				Mod.Log.Error(ex, "Failed to initialize PdxModsUtil");
			}
		}

		public static async Task<IModDetailsResult> GetLocalModDetails(string id)
		{
			if (_context == null)
			{
				return null;
			}

			return await _context.Mods.GetLocalModDetails(id);
		}
	}
}
