using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Win.Native;

[Flags]
[PublicAPI]
public enum GwlStyle{//https://learn.microsoft.com/en-us/windows/win32/winmsg/window-styles
	Border=0x800000,
	Caption=0xC00000,
	Child=0x40000000,
	ChildWindow=0x40000000,
	ClipChildren=0x2000000,
	ClipSiblings=0x4000000,
	Disabled=0x8000000,
	DlgFrame=0x400000,
	Group=0x20000,
	HScroll=0x100000,
	Iconic=0x20000000,
	Maximize=0x1000000,
	MaximizeBox=0x10000,
	Minimize=0x20000000,
	MinimizeBox=0x20000,
	Overlapped=0,
	OverlappedWindow=Overlapped|Caption|SysMenu|Thickframe|MinimizeBox|MaximizeBox,
	Popup=unchecked((int)0x80000000),
	PopupWindow=Popup|Border|SysMenu,
	SizeBox=0x40000,
	SysMenu=0x80000,
	TabStop=0x10000,
	Thickframe=0x40000,
	Tiled=0,
	TiledWindow=Overlapped|Caption|SysMenu|Thickframe|MinimizeBox|MaximizeBox,
	Visible=0x10000000,
	VScroll=0x200000,
}