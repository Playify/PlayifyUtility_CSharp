using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Win.Native;

[PublicAPI]
public enum ShowWindowCommands{// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
	Hide=0,
	Normal=1,
	Minimized=2,//Keeps focus on current window
	Maximized=3,
	ShowNoActivate=4,
	Show=5,
	MinimizeFocusNext=6,//Activates next window in Z-Order
	ForceMinimize=11,

	ShowMinNoActivate=7,
	ShowNa=8,
	Restore=9,
	ShowDefault=10,
}