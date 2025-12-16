using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Win;

public static partial class WinSystem{
	[DllImport("user32.dll")]
	private static extern bool SystemParametersInfo(int uiAction,int uiParam,out int pvParam,int fWinIni);

	[DllImport("user32.dll")]
	private static extern bool SystemParametersInfo(int uiAction,int uiParam,int pvParam,int fWinIni);


	[DllImport("user32.dll")]
	private static extern int GetSystemMetrics(int nIndex);


	[UsedImplicitly(ImplicitUseTargetFlags.Members)]
	private struct LastInputInfo{
		public uint CbSize;
		public uint DwTime;
	}

	[DllImport("User32.dll")]
	private static extern bool GetLastInputInfo(ref LastInputInfo lii);


	[DllImport("shell32.dll",CharSet=CharSet.Auto,SetLastError=true)]
	private static extern int ShellExecute(IntPtr hwnd,string lpOperation,string lpFile,string? lpParameters,string? lpDirectory,int nShowCmd);

	[DllImport("shell32.dll")]
	private static extern int SHOpenFolderAndSelectItems(IntPtr pidlFolder,uint cidl,[In,MarshalAs(UnmanagedType.LPArray)]IntPtr[] apidl,uint dwFlags);

	[DllImport("shell32.dll",SetLastError=true)]
	private static extern void SHParseDisplayName([MarshalAs(UnmanagedType.LPWStr)]string name,IntPtr bindingContext,[Out]out IntPtr pidl,uint sfgaoIn,[Out]out uint psfgaoOut);

	[DllImport("user32.dll")]
	private static extern bool GetCursorPos(out NativePoint lpPoint);
}