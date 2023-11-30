using System.Runtime.InteropServices;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Win;

public static partial class WinCursor{
	[DllImport("user32.dll")]
	private static extern bool GetCursorPos(out NativePoint lpPoint);

	[DllImport("gdi32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true)]
	private static extern IntPtr BitBlt(IntPtr hDc,int x,int y,int nWidth,int nHeight,IntPtr hSrcDc,int xSrc,int ySrc,int dwRop);
}