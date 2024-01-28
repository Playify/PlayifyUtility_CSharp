using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
public static class GlobalShellHook{
	#region Events
	public static event Action<ShellHookEvent> ShellEvent{add=>Hook(value);remove=>Unhook(value);}
	#endregion

	#region Instance Variables
	private static readonly ShellHookProc Proc=HookProc;
	private static UiThread? _thread;
	private static IntPtr _hook;
	private static (Action<ShellHookEvent>? evt,List<Action<ShellHookEvent>> lst) _shell=(null,new List<Action<ShellHookEvent>>());
	#endregion

	#region Private Methods
	private static void Hook(Action<ShellHookEvent> value){
		_shell.lst.Add(value);
		_shell.evt+=value;
		if(_thread!=null) return;
		_thread=UiThread.Create(nameof(GlobalShellHook));
		_hook=_thread.Invoke(()=>SetWindowsHookEx(WhShell,Proc,GetModuleHandle(IntPtr.Zero),0));
	}

	private static void Unhook(Action<ShellHookEvent> value){
		if(!_shell.lst.Remove(value)) return;
		_shell.evt-=value;
		if(_shell.lst.Any()||_thread==null) return;
		var thread=_thread;
		_thread=null;
		thread.Exit(()=>UnhookWindowsHookEx(_hook));
	}

	private static int HookProc(int code,IntPtr wParam,IntPtr lParam){
		try{
			_shell.evt?.Invoke(new ShellHookEvent(code,wParam,lParam));
		} catch(Exception e){
			Console.WriteLine($"Error in {nameof(GlobalShellHook)}: {e}");
		}
		return CallNextHookEx(_hook,code,wParam,lParam);
	}
	#endregion


	#region DLL imports
	[DllImport("user32.dll")]
	private static extern IntPtr SetWindowsHookEx(int idHook,ShellHookProc callback,IntPtr hInstance,uint threadId);

	[DllImport("user32.dll")]
	private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

	[DllImport("user32.dll")]
	private static extern int CallNextHookEx(IntPtr idHook,int nCode,IntPtr wParam,IntPtr lParam);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetModuleHandle(IntPtr zero);
	#endregion

	#region Constant, Structure and Delegate Definitions
	private delegate int ShellHookProc(int code,IntPtr wParam,IntPtr lParam);

	private const int WhShell=10;
	#endregion
}