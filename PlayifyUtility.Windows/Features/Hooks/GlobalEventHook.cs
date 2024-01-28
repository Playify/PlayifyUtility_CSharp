using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
public sealed class GlobalEventHook:IDisposable{
	#region Events
	// https://learn.microsoft.com/en-us/windows/win32/winauto/event-constants
	public static IDisposable Hook(uint evt,WindowEventHandler handler)=>Hook(evt,evt,handler);
	public static IDisposable Hook(uint min,uint max,WindowEventHandler handler)=>Hook(_defaultThread??=UiThread.Create(nameof(GlobalEventHook)),min,max,handler);
	public static IDisposable HookCurrent(uint evt,WindowEventHandler handler)=>HookCurrent(evt,evt,handler);
	public static IDisposable HookCurrent(uint min,uint max,WindowEventHandler handler)=>new GlobalEventHook(
		UiThread.Current??throw new ThreadStateException("Current thread is not an UiThread"),
		min,max,handler);
	public static IDisposable Hook(UiThread thread,uint evt,WindowEventHandler handler)=>Hook(evt,evt,handler);
	public static IDisposable Hook(UiThread thread,uint min,uint max,WindowEventHandler handler)=>new GlobalEventHook(thread,min,max,handler);
	#endregion

	#region Instance Variables
	private static UiThread? _defaultThread;
	private readonly UiThread _thread;
	private static readonly HashSet<GlobalEventHook> Instances=new();//Single hooks are not allowed to be GCd while still hooked
	private readonly IntPtr _hook;
	private readonly WinEventProc _proc;
	#endregion

	#region Private Methods
	private GlobalEventHook(UiThread thread,uint min,uint max,WindowEventHandler handler){
		Instances.Add(this);
		_thread=thread;

		_proc=(_,@event,hwnd,idObject,idChild,idEventThread,eventTime)=>
		handler(new WindowEvent(@event,hwnd,idObject,idChild,idEventThread,eventTime));
		_hook=thread.Invoke(()=>SetWinEventHook(min,max,IntPtr.Zero,_proc,0,0,2));
	}

	public void Dispose(){
		if(!Instances.Remove(this)) return;
		_thread.BeginInvoke(()=>UnhookWinEvent(_hook));
		
		if(Instances.Count!=0&&_thread==_defaultThread) return;
		var defaultThread=_defaultThread;
		_defaultThread=null;
		defaultThread?.Exit();
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