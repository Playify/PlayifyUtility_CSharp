using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Win.Native;

[Flags]
[PublicAPI]
public enum ExStyle{//https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles
	AcceptFiles=0x10,
	AppWindow=0x40000,
	ClientEdge=0x200,
	Composited=0x2000000,
	ContextHelp=0x400,
	ControlParent=0x10000,
	DlgModalFrame=0x1,
	Layered=0x80000,
	LayoutRtl=0x400000,
	Left=0,
	LeftScrollBar=0x4000,
	LtrReading=0,
	MdiChild=0x40,
	NoActivate=0x8000000,
	NoInheritLayout=0x100000,
	NoParentNotify=0x4,
	NoRedirectionBitmap=0x200000,
	OverlappedWindow=WindowEdge|ClientEdge,
	PaletteWindow=WindowEdge|ToolWindow|TopMost,
	Right=0x1000,
	RightScrollBar=0,
	RtlReading=0x2000,
	StaticEdge=0x20000,
	ToolWindow=0x80,
	TopMost=0x8,
	Transparent=0x20,
	WindowEdge=0x100,
}