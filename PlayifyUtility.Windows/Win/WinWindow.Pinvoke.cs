using System.Runtime.InteropServices;
using System.Text;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Win;

public readonly partial struct WinWindow{
	private delegate bool EnumWindowFunc(IntPtr hwnd,int lParam);

	[DllImport("user32.dll")]
	private static extern bool EnumWindows(EnumWindowFunc lpEnumFunc,int lParam);

	[DllImport("user32.dll")]
	private static extern bool EnumThreadWindows(int dwThreadId,EnumWindowFunc lpEnumFunc,int lParam);

	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool IsWindow(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool IsWindowEnabled(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern bool EnableWindow(IntPtr hWnd,bool b);

	[DllImport("user32.dll")]
	private static extern IntPtr GetDesktopWindow();

	[DllImport("user32.dll",CharSet=CharSet.Unicode,EntryPoint="FindWindow")]
	private static extern IntPtr FindWindow_Hwnd(string? lpClassName,string? lpWindowName);

	[DllImport("user32.dll")]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	private static extern bool SetForegroundWindow(IntPtr hWnd);


	[DllImport("user32.dll")]
	private static extern int GetWindowLong(IntPtr hWnd,int nIndex);

	[DllImport("user32.dll")]
	private static extern IntPtr SetWindowLong(IntPtr hWnd,int nIndex,int dwNewLong);

	[DllImport("user32.dll")]
	private static extern bool SetWindowPos(IntPtr hWnd,int hWndInsertAfter,int x,int y,int cx,int cy,uint uFlags);

	[DllImport("user32.dll")]
	private static extern bool GetClientRect(IntPtr hWnd,out NativeRect lpRect);

	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(IntPtr hWnd,out NativeRect rect);

	[DllImport("User32.dll")]
	private static extern bool MoveWindow(IntPtr handle,int x,int y,int width,int height,bool redraw);

	[DllImport("user32.dll",CharSet=CharSet.Auto)]
	private static extern int SendMessage(IntPtr hWnd,WindowMessage msg,int wParam,int lParam);

	[DllImport("user32.dll",CharSet=CharSet.Auto)]
	private static extern int SendMessage(IntPtr hWnd,WindowMessage msg,int wParam,IntPtr lParam);

	[DllImport("user32.dll",CharSet=CharSet.Auto)]
	private static extern int SendMessage(IntPtr hWnd,WindowMessage msg,int wParam,StringBuilder lParam);

	[DllImport("user32.dll",CharSet=CharSet.Auto)]
	private static extern int SendMessage(IntPtr hWnd,WindowMessage msg,int wParam,string lParam);

	[DllImport("user32.dll",SetLastError=true,CharSet=CharSet.Auto)]
	private static extern bool PostMessage(IntPtr hWnd,WindowMessage msg,int wParam,int lParam);

	[DllImport("user32.dll",SetLastError=true,CharSet=CharSet.Auto)]
	private static extern bool PostMessage(IntPtr hWnd,WindowMessage msg,int wParam,IntPtr lParam);

	[Serializable,StructLayout(LayoutKind.Sequential)]
	private struct WindowPlacement{
		public int length;
		public int flags;
		public ShowWindowCommands showCmd;
		public NativePoint ptMinPosition;
		public NativePoint ptMaxPosition;
		public Rectangle rcNormalPosition;
	}

	[DllImport("user32.dll")]
	private static extern bool GetWindowPlacement(IntPtr hwnd,ref WindowPlacement placement);

	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd,ShowWindowCommands nCmdShow);

	[DllImport("user32.dll")]
	private static extern bool GetLayeredWindowAttributes(IntPtr hWnd,out NativeColor color,out byte alpha,out int dwFlags);

	[DllImport("user32.dll")]
	private static extern bool SetLayeredWindowAttributes(IntPtr hWnd,NativeColor color,byte alpha,int dwFlags);

	[DllImport("kernel32.dll")]
	private static extern int GetCurrentProcessId();
	[DllImport("user32.dll")]
	private static extern IntPtr GetWindowThreadProcessId(IntPtr handle,out int processId);


	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	private static extern IntPtr GetWindowText(IntPtr hWnd,StringBuilder title,int size);

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	private static extern int GetWindowTextLength(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern IntPtr WindowFromPoint(Point lpPoint);

	[DllImport("user32.dll")]
	private static extern bool DestroyWindow(IntPtr hwnd);


	[DllImport("dwmapi.dll")]
	private static extern int DwmSetWindowAttribute(IntPtr hwnd,int attr,ref bool attrValue,int four);

	[DllImport("user32.dll")]
	private static extern bool RedrawWindow(IntPtr hWnd,int lprcUpdate,int hrgnUpdate,uint flags);


	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	private static extern int GetProp(IntPtr hwnd,string s);

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	private static extern bool SetProp(IntPtr hwnd,string s,int value);

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	private static extern int RemoveProp(IntPtr hwnd,string s);
	
	[DllImport("user32.dll")]
	private static extern bool IsHungAppWindow(IntPtr hwnd);
}