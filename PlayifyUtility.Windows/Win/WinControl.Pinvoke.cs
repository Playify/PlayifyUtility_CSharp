using System.Runtime.InteropServices;
using System.Text;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Win;

public readonly partial struct WinControl{
	[DllImport("user32.dll")]
	private static extern IntPtr SetFocus(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern IntPtr GetFocus();


	[DllImport("user32.dll")]
	private static extern bool GetClientRect(IntPtr hWnd,out NativeRect lpRect);

	[DllImport("user32.dll")]
	private static extern bool MapWindowPoints(IntPtr from,IntPtr to,ref NativeRect lpRect,int two);

	private delegate bool EnumChildWindowFunc(IntPtr hwnd,int lParam);

	[DllImport("user32.dll")]
	private static extern bool EnumChildWindows(IntPtr hwndParent,EnumChildWindowFunc lpEnumFunc,int lParam);


	[DllImport("user32.dll")]
	private static extern IntPtr GetParent(IntPtr hWnd);

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	private static extern int GetClassName(IntPtr hWnd,StringBuilder lpClassName,int nMaxCount);
}