using System.Runtime.InteropServices;
using System.Text;

namespace PlayifyUtility.Windows.Native;

public readonly partial struct WinWindow{

	[DllImport("user32.dll")]
	private static extern bool EnumWindows(Func<IntPtr,int,bool> lpEnumFunc,int lParam);
	[DllImport("user32.dll")]
	private static extern bool IsWindowVisible(IntPtr hWnd);
	
	[DllImport("kernel32.dll")]
	private static extern IntPtr GetConsoleWindow();
	[DllImport("user32.dll")]
	private static extern IntPtr GetDesktopWindow();
	[DllImport("user32.dll",CharSet=CharSet.Unicode,EntryPoint = "FindWindow")]
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
	private static extern bool GetClientRect(IntPtr hWnd,out NativeMethods.Rect lpRect);

	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(IntPtr hWnd,out NativeMethods.Rect rect);
	[DllImport("User32.dll")]
	private static extern bool MoveWindow(IntPtr handle,int x,int y,int width,int height,bool redraw);

	[DllImport("user32.dll",CharSet=CharSet.Auto)]
	private static extern int SendMessage(IntPtr hWnd,int msg,int wParam,int lParam);
	[DllImport("user32.dll",SetLastError=true,CharSet=CharSet.Auto)]
	private static extern bool PostMessage(IntPtr hWnd,int msg,int wParam,int lParam);
	
	[DllImport("user32.dll")]
	private static extern bool GetWindowPlacement(IntPtr hwnd,ref NativeMethods.WindowPlacement placement);
	[DllImport("user32.dll")]
	private static extern bool ShowWindow(IntPtr hWnd,NativeMethods.ShowWindowCommands nCmdShow);

	[DllImport("user32.dll")]
	private static extern bool GetLayeredWindowAttributes(IntPtr hWnd,out NativeMethods.ColorRef color,out byte alpha,out int dwFlags);
	[DllImport("user32.dll")]
	private static extern bool SetLayeredWindowAttributes(IntPtr hWnd,NativeMethods.ColorRef color,byte alpha,int dwFlags);

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
}