using System.Runtime.InteropServices;
using PlayifyUtility.Windows.Features.Hooks;
using PlayifyUtility.Windows.Win;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Features;

public static partial class ToolTip{
	static ToolTip(){
		GlobalMouseHook.Hook();
		GlobalMouseHook.MouseMove+=CorrectToolTip;
	}

	public static void CorrectToolTip(MouseEvent? e){//Coordinates can be out of screen, therefore, GetCursorPos is required
		if(_toolInfo.lpszText==null) return;
		if(!WinCursor.TryGetCursorPos(out var p)) return;
		p.X+=16;
		p.Y+=16;
		_currentToolTip.SendMessage(0x412,0,((p.Y&0xffff)<<16)|(p.X&0xffff));//TTM_TRACKPOSITION
	}
	
	[StructLayout(LayoutKind.Sequential)]
	private struct ToolInfo{
		public uint cbSize;
		public uint uFlags;
		public IntPtr hwnd;
		public UIntPtr uId;
		public NativeRect rect;
		public IntPtr hInst;
		[MarshalAs(UnmanagedType.LPWStr)]
		public string? lpszText;
		// ReSharper disable once MemberCanBePrivate.Global
		public IntPtr lParam;
	}

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	private static extern IntPtr CreateWindowEx(int dwExStyle,string lpClassName,string? lpWindowName,uint dwStyle,int x,int y,int nWidth,int nHeight,IntPtr hWndParent,IntPtr hMenu,IntPtr hInstance,IntPtr lpParam);

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	private static extern bool SendMessage(IntPtr hWnd,uint msg,uint wParam,ref ToolInfo lParam);

	private static WinWindow _currentToolTip;
	private static ToolInfo _toolInfo;
	private static CancellationTokenSource _cancel=new();
}