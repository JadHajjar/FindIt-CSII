using Colossal.PSI.Common;
using Colossal.PSI.PdxSdk;

using PDX.SDK.Contracts;
using PDX.SDK.Contracts.Service.Mods.Result;

using System.Threading.Tasks;

namespace FindIt.Utilities
{
	public static class PdxModsUtil
	{
		private static readonly PdxSdkPlatform _pdxPlatform;
		private static readonly IContext _context;

		static PdxModsUtil()
		{
			_pdxPlatform = PlatformManager.instance.GetPSI<PdxSdkPlatform>("PdxSdk");
			_context = typeof(PdxSdkPlatform).GetField("m_SDKContext", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_pdxPlatform) as IContext;
		}

		public static Task<GetDetailsResult> GetLocalModDetails(int id)
		{
			return _context.Mods.GetLocalModDetails(id);
		}
	}
}
