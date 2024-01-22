using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Features.Interact;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
public static class GlobalMouseHook{
	#region Events
	public static event GlobalMouseEventHandler MouseDown{add=>Hook(ref _down,value);remove=>Unhook(ref _down,value);}
	public static event GlobalMouseEventHandler MouseUp{add=>Hook(ref _up,value);remove=>Unhook(ref _up,value);}
	public static bool AutoHandleUpWhenHandledDown{get;set;}=true;
	public static event GlobalMouseEventHandler MouseMove{add=>Hook(ref _move,value);remove=>Unhook(ref _move,value);}
	public static event GlobalMouseEventHandler MouseScroll{add=>Hook(ref _scroll,value);remove=>Unhook(ref _scroll,value);}
	#endregion

	#region Instance Variables
	private static IntPtr _hook=IntPtr.Zero;
	private static readonly MouseHookProc Proc=HookProc;
	private static (GlobalMouseEventHandler? evt,List<GlobalMouseEventHandler> lst) _down=(null,new List<GlobalMouseEventHandler>());
	private static (GlobalMouseEventHandler? evt,List<GlobalMouseEventHandler> lst) _up=(null,new List<GlobalMouseEventHandler>());
	private static (GlobalMouseEventHandler? evt,List<GlobalMouseEventHandler> lst) _move=(null,new List<GlobalMouseEventHandler>());
	private static (GlobalMouseEventHandler? evt,List<GlobalMouseEventHandler> lst) _scroll=(null,new List<GlobalMouseEventHandler>());
	#endregion

	#region Private Methods
	private static void Hook(ref (GlobalMouseEventHandler? evt,List<GlobalMouseEventHandler> lst) tuple,GlobalMouseEventHandler value){
		tuple.lst.Add(value);
		tuple.evt+=value;
		if(_hook!=IntPtr.Zero) return;
		_hook=SetWindowsHookEx(WhMouseLl,Proc,GetModuleHandle(IntPtr.Zero),0);
	}

	private static void Unhook(ref (GlobalMouseEventHandler? evt,List<GlobalMouseEventHandler> lst) tuple,GlobalMouseEventHandler value){
		if(!tuple.lst.Remove(value)) return;
		tuple.evt-=value;
		if(_up.lst.Any()||_down.lst.Any()||_move.lst.Any()||_scroll.lst.Any()||_hook==IntPtr.Zero) return;
		UnhookWindowsHookEx(_hook);
		_hook=IntPtr.Zero;
	}

	private static int HookProc(int code,int wParam,ref MsLlHookStruct lParam){
		try{
			if(code>=0){
				if(wParam switch{
					   512=>HandleEvent(_move.evt,new MouseEvent(lParam.pt.x,lParam.pt.y,MouseButtons.None)),
					   522=>HandleEvent(_scroll.evt,new MouseEvent(lParam.pt.x,lParam.pt.y,MouseButtons.None,Math.Sign(lParam.mouseData))),
					   513=>HandleDown(ref lParam,MouseButtons.Left),
					   514=>HandleUp(ref lParam,MouseButtons.Left),
					   516=>HandleDown(ref lParam,MouseButtons.Right),
					   517=>HandleUp(ref lParam,MouseButtons.Right),
					   519=>HandleDown(ref lParam,MouseButtons.Middle),
					   520=>HandleUp(ref lParam,MouseButtons.Middle),
					   523=>HandleDown(ref lParam,lParam.mouseData==0x10000?MouseButtons.XButton1:MouseButtons.XButton2),
					   524=>HandleUp(ref lParam,lParam.mouseData==0x10000?MouseButtons.XButton1:MouseButtons.XButton2),
					   _=>false,
				   }) return 1;
			}
		} catch(Exception e){
			Console.WriteLine($"Error in {nameof(GlobalMouseHook)}: {e}");
		}
		return CallNextHookEx(_hook,code,wParam,ref lParam);
	}

	private static bool HandleEvent(GlobalMouseEventHandler? handler,MouseEvent evt){
		handler?.Invoke(evt);
		return evt.Handled;
	}

	private static bool HandleDown(ref MsLlHookStruct lParam,MouseButtons button){
		var evt=new MouseEvent(lParam.pt.x,lParam.pt.y,button);
		var key=evt.Key;

		_down.evt?.Invoke(evt);

		if(evt.Handled&&AutoHandleUpWhenHandledDown&&!GlobalKeyboardHook.OnRelease.ContainsKey(key))
			GlobalKeyboardHook.OnRelease.Add(key,null);
		return evt.Handled;
	}

	private static bool HandleUp(ref MsLlHookStruct lParam,MouseButtons button){
		var evt=new MouseEvent(lParam.pt.x,lParam.pt.y,button);
		var key=evt.Key;

		if(GlobalKeyboardHook.OnRelease.TryGetValue(key,out var onRelease)){
			GlobalKeyboardHook.OnRelease.Remove(key);
			if(key!=onRelease){
				evt.Handled=true;
				if(onRelease.HasValue)
					new Send().Key(onRelease.GetValueOrDefault(),false).SendNow();
			}
		}

		_up.evt?.Invoke(evt);
		return evt.Handled;
	}
	#endregion

	#region DLL imports
	[DllImport("user32.dll")]
	private static extern IntPtr SetWindowsHookEx(int idHook,MouseHookProc callback,IntPtr hInstance,uint threadId);

	[DllImport("user32.dll")]
	private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

	[DllImport("user32.dll")]
	private static extern int CallNextHookEx(IntPtr idHook,int nCode,int wParam,ref MsLlHookStruct lParam);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetModuleHandle(IntPtr zero);
	#endregion

	#region Constant, Structure and Delegate Definitions
	private delegate int MouseHookProc(int code,int wParam,ref MsLlHookStruct lParam);

	private const int WhMouseLl=14;

	[StructLayout(LayoutKind.Sequential)]
	private struct Point{
		public int x;
		public int y;
	}


	[StructLayout(LayoutKind.Sequential)]
	private struct MsLlHookStruct{
		public Point pt;
		public int mouseData;
		public uint flags;
		public uint time;
		public IntPtr dwExtraInfo;
	}
	#endregion
}