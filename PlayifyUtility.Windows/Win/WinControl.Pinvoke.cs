using System.Runtime.InteropServices;
using System.Text;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Win;

public readonly partial struct WinControl{
	[DllImport("user32.dll")]
	private static extern IntPtr SetFocus(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool AttachThreadInput(uint idAttach,int idAttachTo,bool fAttach);

	[DllImport("user32.dll")]
	private static extern IntPtr GetFocus();

	[DllImport("kernel32.dll")]
	private static extern uint GetCurrentThreadId();

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	private static extern int GetWindowThreadProcessId(IntPtr handle,out uint processId);


	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();


	[DllImport("user32.dll")]
	internal static extern bool GetClientRect(IntPtr hWnd,out NativeRect lpRect);

	[DllImport("user32.dll")]
	internal static extern bool MapWindowPoints(IntPtr from,IntPtr to,ref NativeRect lpRect,int two);

	private delegate bool EnumChildWindowFunc(IntPtr hwnd,int lParam);

	[DllImport("user32.dll")]
	private static extern bool EnumChildWindows(IntPtr hwndParent,EnumChildWindowFunc lpEnumFunc,int lParam);


	[DllImport("user32.dll")]
	private static extern IntPtr SendMessage(IntPtr hWnd,int msg,int wParam,int lParam);

	[DllImport("user32.dll")]
	private static extern IntPtr PostMessage(IntPtr hWnd,int msg,int wParam,int lParam);

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	private static extern IntPtr SendMessage(IntPtr hWnd,int msg,int wParam,StringBuilder lParam);

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	private static extern IntPtr SendMessage(IntPtr hWnd,int msg,int wParam,string lParam);


	[DllImport("user32.dll")]
	private static extern IntPtr GetParent(IntPtr hWnd);

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	internal static extern int GetClassName(IntPtr hWnd,StringBuilder lpClassName,int nMaxCount);
}