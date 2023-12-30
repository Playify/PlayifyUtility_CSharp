using PlayifyUtility.Windows.Win;

namespace PlayifyUtility.Windows.Features.Hooks;

public enum ShellHookEnum:byte{
	WindowCreated=1,
	WindowDestroyed=2,
	ActivateShellWindow=3,
	WindowActivated=4,
	GetMinRect=5,
	Redraw=6,
	TaskMan=7,
	Language=8,
	SysMenu=9,
	EndTask=10,
	AccessibilityState=11,
	AppCommand=12,
	WindowReplaced=13,
	WindowReplacing=14,
}

public readonly struct ShellHookEvent{
	public readonly ShellHookEnum ShellHookEnum;
	public readonly bool Injected;
	public readonly WinWindow Window;

	public ShellHookEvent(int wParam,IntPtr lParam){
		ShellHookEnum=(ShellHookEnum)(wParam&~0x8000);
		Injected=(wParam&0x8000)!=0;
		Window=new WinWindow(lParam);
	}
}