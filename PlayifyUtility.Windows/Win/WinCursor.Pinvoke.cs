using System.Runtime.InteropServices;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Win;

public static partial class WinCursor{
	[DllImport("user32.dll")]
	private static extern bool GetCursorPos(out NativePoint lpPoint);
}