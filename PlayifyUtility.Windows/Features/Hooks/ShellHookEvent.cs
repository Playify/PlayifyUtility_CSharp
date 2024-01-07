using JetBrains.Annotations;
using PlayifyUtility.Windows.Win;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
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

[PublicAPI]
public readonly struct ShellHookEvent{//https://learn.microsoft.com/de-de/windows/win32/winmsg/shellproc
	public readonly ShellHookEnum ShellHookEnum;
	public readonly bool Injected;
	public readonly WinWindow Window;
	public readonly IntPtr ExtraData;

	public ShellHookEvent(int code,IntPtr wParam,IntPtr lParam){
		ShellHookEnum=(ShellHookEnum)(code&~0x8000);
		Injected=(code&0x8000)!=0;
		Window=new WinWindow(wParam);
		ExtraData=lParam;
	}

	public override string ToString()=>$"{nameof(ShellHookEnum)}({ShellHookEnum},Injected={Injected},{Window})";
}