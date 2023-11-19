using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace PlayifyUtility.Windows;

public static class NativeMethods{
	[DllImport("User32.dll")]
	public static extern bool MoveWindow(IntPtr handle,int x,int y,int width,int height,bool redraw);

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	public static extern IntPtr FindWindow(string lpClassName,string lpWindowName);

	[DllImport("user32.dll",CharSet=CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd,uint msg,uint wParam,uint lParam);
	[DllImport("user32.dll",SetLastError=true,CharSet=CharSet.Auto)]
	public static extern bool PostMessage(IntPtr hWnd,uint msg,int wParam,int lParam);

	[DllImport("user32.dll")]
	public static extern bool SetWindowPos(IntPtr hWnd,IntPtr hWndInsertAfter,int x,int y,int cx,int cy,uint uFlags);
	[DllImport("user32.dll")]
	public static extern bool SetWindowPos(IntPtr hWnd,int hWndInsertAfter,int x,int y,int cx,int cy,uint uFlags);

	[DllImport("user32.dll")]
	public static extern bool GetWindowRect(IntPtr hWnd,out Rect rect);

	[DllImport("user32.dll")]
	public static extern int GetWindowLong(IntPtr hWnd,int nIndex);

	[DllImport("user32.dll")]
	public static extern int SetWindowLong(IntPtr hWnd,int nIndex,int dwNewLong);


	[DllImport("user32.dll")]
	internal static extern IntPtr WindowFromPoint(Point lpPoint);

	[DllImport("user32.dll")]
	internal static extern bool GetCursorPos(out Point lpPoint);

	[DllImport("user32.dll")]
	internal static extern bool SetCursorPos(int x,int y);



	// Enumerates all windows
	public delegate bool EnumWindowsProc(IntPtr hWnd,IntPtr lParam);

	[DllImport("user32.dll")]
	public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc,IntPtr lParam);

	[DllImport("user32.dll")]
	public static extern bool EnumChildWindows(IntPtr hwndParent,EnumWindowsProc lpEnumFunc,IntPtr lParam);


	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	internal static extern int GetClassName(IntPtr hWnd,StringBuilder lpClassName,int nMaxCount);



	[DllImport("user32.dll")]
	internal static extern int GetWindowThreadProcessId(IntPtr handle,out uint processId);

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	internal static extern int GetWindowText(IntPtr hWnd,StringBuilder title,int size);

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	internal static extern int GetWindowTextLength(IntPtr hWnd);

	[DllImport("user32.dll")]
	internal static extern bool ShowWindow(IntPtr hWnd,ShowWindowCommands nCmdShow);

	[DllImport("user32.dll")]
	internal static extern bool SetLayeredWindowAttributes(IntPtr hWnd,ColorRef color,byte alpha,int dwFlags);

	[DllImport("user32.dll")]
	internal static extern bool GetLayeredWindowAttributes(IntPtr hWnd,out ColorRef color,out byte alpha,out int dwFlags);

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	[StructLayout(LayoutKind.Sequential)]
	public struct ColorRef{
		private readonly byte R;
		private readonly byte G;
		private readonly byte B;
		private readonly byte A;

		public ColorRef(Color color){
			R=color.R;
			G=color.G;
			B=color.B;
			A=0;
		}

		public uint GetRgb()=>(uint) ((R<<16)|(G<<8)|B);
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Point{
		public int x;
		public int y;
		
		public static implicit operator System.Drawing.Point(Point point)=>new(point.x,point.y);
		public static implicit operator Point(System.Drawing.Point point)=>new(){x=point.X,y=point.Y};
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct AnimationInfo{
		public uint cbSize;
		public int iMinAnimate;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Rect{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;

		public override string ToString()=>$"({Left},{Top})->({Right},{Bottom})";
	}

	[DllImport("User32.dll")]
	public static extern bool SystemParametersInfo(uint uiAction,uint uiParam,ref AnimationInfo pvParam,uint fWinIni);


	[DllImport("gdi32.dll",CharSet=CharSet.Auto,SetLastError=true,ExactSpelling=true)]
	internal static extern int BitBlt(IntPtr hDc,int x,int y,int nWidth,int nHeight,IntPtr hSrcDc,int xSrc,int ySrc,int dwRop);



	[DllImport("user32.dll")]
	internal static extern bool GetClientRect(IntPtr hWnd,out Rect lpRect);


	[DllImport("user32.dll")]
	public static extern bool DestroyWindow(IntPtr hwnd);

	[DllImport("user32.dll")]
	[return:MarshalAs(UnmanagedType.Bool)]
	public static extern bool IsWindow(IntPtr hWnd);


	[DllImport("user32.dll")]
	internal static extern bool GetWindowPlacement(IntPtr hwnd,ref WindowPlacement placement);


	[Serializable,StructLayout(LayoutKind.Sequential)]
	internal struct WindowPlacement{
		public int length;
		public int flags;
		public ShowWindowCommands showCmd;
		public Point ptMinPosition;
		public Point ptMaxPosition;
		public Rectangle rcNormalPosition;
	}

	internal enum ShowWindowCommands{
		Hide=0,
		Normal=1,
		Minimized=2,
		Maximized=3,
		Show=5,
	}
}