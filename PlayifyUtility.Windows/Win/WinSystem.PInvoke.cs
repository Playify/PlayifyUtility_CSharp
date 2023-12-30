using System.Runtime.InteropServices;

namespace PlayifyUtility.Windows.Win;

public static partial class WinSystem{
	[DllImport("user32.dll")]
	private static extern bool SystemParametersInfo(int uiAction,int uiParam,out int pvParam,int fWinIni);

	[DllImport("user32.dll")]
	private static extern bool SystemParametersInfo(int uiAction,int uiParam,int pvParam,int fWinIni);
	
	
	[DllImport("user32.dll")]
	private static extern int GetSystemMetrics(int nIndex);
}