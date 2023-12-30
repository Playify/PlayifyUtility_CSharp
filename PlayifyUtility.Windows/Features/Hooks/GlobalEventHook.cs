using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Features.Interact;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
public static class GlobalEventHook{
	#region Instance Variables
	private static readonly List<IntPtr> Hooks=new();
	private static readonly WinEventProc Proc=HookProc;
	#endregion

	#region Events
	public static event Action<WindowEvent>? OnEvent;
	#endregion


	#region Public Methods
	public static void Hook(uint min,uint max){
		Hooks.Add(SetWinEventHook(min,max,IntPtr.Zero,Proc,0,0,2));
	}

	public static void Unhook(){
		foreach(var hook in Hooks) UnhookWinEvent(hook);
		Hooks.Clear();
	}

	private static void HookProc(IntPtr _,int @event,IntPtr hwnd,int idObject,int idChild,int idEventThread,int eventTime){
		OnEvent?.Invoke(new WindowEvent(@event,hwnd,idObject,idChild,idEventThread,eventTime));
	}
	#endregion

	#region DLL imports
	private delegate void WinEventProc(IntPtr hWinEventHook,int iEvent,IntPtr hWnd,int idObject,int idChild,int dwEventThread,int dwmsEventTime);

	[DllImport("user32.dll")]
	private static extern IntPtr SetWinEventHook(uint eventMin,uint eventMax,IntPtr hmodWinEventProc,WinEventProc lpfnWinEventProc,int idProcess,int idThread,uint dwflags);

	[DllImport("user32.dll")]
	private static extern IntPtr UnhookWinEvent(IntPtr hWinEventHook);
	#endregion
	
}