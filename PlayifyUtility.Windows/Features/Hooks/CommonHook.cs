using System.Runtime.InteropServices;

namespace PlayifyUtility.Windows.Features.Hooks;

internal static class CommonHook{
	/*
	[DllImport("kernel32.dll")]
	private static extern IntPtr GetModuleHandle(IntPtr zero);
	public static IntPtr HInstance()=>GetModuleHandle(IntPtr.Zero);//*/

	/*
	public static IntPtr HInstance()=>Marshal.GetHINSTANCE(typeof(GlobalMouseHook).Module);//*/


	//*
	[DllImport("kernel32.dll")]
	private static extern IntPtr LoadLibrary(string lpFileName);

	public static IntPtr HInstance()=>LoadLibrary("User32");//*/
}