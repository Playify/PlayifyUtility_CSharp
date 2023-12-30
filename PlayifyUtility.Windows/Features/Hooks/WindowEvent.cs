using JetBrains.Annotations;
using PlayifyUtility.Windows.Win;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
public readonly struct WindowEvent{
	public readonly int Event;
	public readonly IntPtr Hwnd;
	public readonly int IdObject;
	public readonly int IdChild;
	public readonly int IdEventThread;
	public readonly int EventTime;
	public WinWindow Window=>new(Hwnd);
	public WinControl Control=>new(Hwnd);
	
	public WindowEvent(int @event,IntPtr hwnd,int idObject,int idChild,int idEventThread,int eventTime){
		Event=@event;
		Hwnd=hwnd;
		IdObject=idObject;
		IdChild=idChild;
		IdEventThread=idEventThread;
		EventTime=eventTime;
	}
}