using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
public sealed class GlobalEventHook:IDisposable{
	#region Events
	// https://learn.microsoft.com/en-us/windows/win32/winauto/event-constants
	public static IDisposable Hook(uint evt,WindowEventHandler handler)=>new GlobalEventHook(evt,evt,handler);
	public static IDisposable Hook(uint min,uint max,WindowEventHandler handler)=>new GlobalEventHook(min,max,handler);
	#endregion

	#region Instance Variables
	private static readonly HashSet<GlobalEventHook> Instances=new();//SingleHooks are not allowed to be GCd while hooked
	private readonly IntPtr _hook;
	private readonly WinEventProc _proc;
	#endregion

	#region Private Methods
	private GlobalEventHook(uint min,uint max,WindowEventHandler handler){
		Instances.Add(this);
		_proc=(_,@event,hwnd,idObject,idChild,idEventThread,eventTime)=>
			handler(new WindowEvent(@event,hwnd,idObject,idChild,idEventThread,eventTime));
		_hook=SetWinEventHook(min,max,IntPtr.Zero,_proc,0,0,2);
	}

	public void Dispose(){
		if(Instances.Remove(this))
			UnhookWinEvent(_hook);
	}
	#endregion

	#region DLL imports
	private delegate void WinEventProc(IntPtr _,int evt,IntPtr hWnd,int idObject,int idChild,int idEventThread,int eventTime);

	[DllImport("user32.dll")]
	private static extern IntPtr SetWinEventHook(uint evtMin,uint evtMax,IntPtr mod,WinEventProc proc,
		int idProcess,int idThread,uint dwFlags);

	[DllImport("user32.dll")]
	private static extern IntPtr UnhookWinEvent(IntPtr hWinEventHook);
	#endregion
}