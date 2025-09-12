using System.Runtime.InteropServices;

namespace PlayifyUtility.Utils;

public static partial class PlatformUtils{
	[DllImport("ntdll.dll",SetLastError=true)]
	private static extern void RtlGetVersion(ref OsVersionInfo version);

	[StructLayout(LayoutKind.Sequential)]
	private struct OsVersionInfo{
		public int Size;
		public int Major;
		public int Minor;
		public int Build;
		[MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]
		public string CsdVersion;
	}
}