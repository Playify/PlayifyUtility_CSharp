using System.Runtime.InteropServices;

namespace PlayifyUtility.Windows.Features;

public static partial class Screenshot{
	[DllImport("gdi32.dll")]
	private static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);
	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);
	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateCompatibleDC(IntPtr hDC);
	[DllImport("gdi32.dll")]
	private static extern bool DeleteDC(IntPtr hDC);
	[DllImport("gdi32.dll")]
	private static extern bool DeleteObject(IntPtr hObject);
	[DllImport("gdi32.dll")]
	private static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
	
	
	[DllImport("user32.dll")]
	private static extern IntPtr GetWindowDC(IntPtr hWnd);
	[DllImport("user32.dll")]
	private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
	
	[DllImport("user32.dll")]
	static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, int nFlags);
}