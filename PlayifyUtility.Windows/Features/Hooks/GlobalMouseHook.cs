using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Features.Interact;
using PlayifyUtility.Windows.Win.Native;

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

	#region Pause
	private static bool _paused;
	public static bool Paused{
		get=>_paused;
		set{
			if(_paused==value) return;
			if(_thread?.IsCurrent??false) throw new ThreadStateException("Can't pause from current thread");//Unhooking would not work
			_paused=value;

			if(value){
				if(_thread==null) return;
				var thread=_thread;
				_thread=null;
				thread.Exit(()=>UnhookWindowsHookEx(_hook));
			} else if(_thread==null&&(_up.lst.Any()||_down.lst.Any()||_move.lst.Any()||_scroll.lst.Any())){
				_thread=UiThread.Create(nameof(GlobalMouseHook));
				_hook=_thread.Invoke(()=>SetWindowsHookEx(WhMouseLl,Proc,CommonHook.HInstance(),0));
			}
		}
	}
	#endregion

	#region Instance Variables
	private static readonly MouseHookProc Proc=HookProc;
	private static UiThread? _thread;
	private static IntPtr _hook;
	private static (GlobalMouseEventHandler? evt,List<GlobalMouseEventHandler> lst) _down=(null,new List<GlobalMouseEventHandler>());
	private static (GlobalMouseEventHandler? evt,List<GlobalMouseEventHandler> lst) _up=(null,new List<GlobalMouseEventHandler>());
	private static (GlobalMouseEventHandler? evt,List<GlobalMouseEventHandler> lst) _move=(null,new List<GlobalMouseEventHandler>());
	private static (GlobalMouseEventHandler? evt,List<GlobalMouseEventHandler> lst) _scroll=(null,new List<GlobalMouseEventHandler>());
	#endregion

	#region Private Methods
	private static void Hook(ref (GlobalMouseEventHandler? evt,List<GlobalMouseEventHandler> lst) tuple,GlobalMouseEventHandler value){
		tuple.lst.Add(value);
		tuple.evt+=value;
		if(_thread!=null||Paused) return;
		_thread=UiThread.Create(nameof(GlobalMouseHook));
		_hook=_thread.Invoke(()=>SetWindowsHookEx(WhMouseLl,Proc,CommonHook.HInstance(),0));
	}

	private static void Unhook(ref (GlobalMouseEventHandler? evt,List<GlobalMouseEventHandler> lst) tuple,GlobalMouseEventHandler value){
		if(!tuple.lst.Remove(value)) return;
		tuple.evt-=value;
		if(_up.lst.Any()||_down.lst.Any()||_move.lst.Any()||_scroll.lst.Any()||_thread==null) return;
		var thread=_thread;
		_thread=null;
		thread.Exit(()=>UnhookWindowsHookEx(_hook));
	}

	private static int HookProc(int code,WindowMessage wParam,ref MouseHookStruct lParam){
		try{
			if(code>=0){
				if(wParam switch{
					   WindowMessage.WM_MOUSEMOVE=>HandleEvent(_move.evt,new MouseEvent(lParam.pt.x,lParam.pt.y,MouseButtons.None)),
					   WindowMessage.WM_MOUSEWHEEL=>HandleEvent(_scroll.evt,new MouseEvent(lParam.pt.x,lParam.pt.y,MouseButtons.None,Math.Sign(lParam.mouseData))),
					   WindowMessage.WM_LBUTTONDOWN=>HandleDown(ref lParam,MouseButtons.Left),
					   WindowMessage.WM_LBUTTONUP=>HandleUp(ref lParam,MouseButtons.Left),
					   WindowMessage.WM_RBUTTONDOWN=>HandleDown(ref lParam,MouseButtons.Right),
					   WindowMessage.WM_RBUTTONUP=>HandleUp(ref lParam,MouseButtons.Right),
					   WindowMessage.WM_MBUTTONDOWN=>HandleDown(ref lParam,MouseButtons.Middle),
					   WindowMessage.WM_MBUTTONUP=>HandleUp(ref lParam,MouseButtons.Middle),
					   WindowMessage.WM_XBUTTONDOWN=>HandleDown(ref lParam,lParam.mouseData==0x10000?MouseButtons.XButton1:MouseButtons.XButton2),
					   WindowMessage.WM_XBUTTONUP=>HandleUp(ref lParam,lParam.mouseData==0x10000?MouseButtons.XButton1:MouseButtons.XButton2),
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

	private static bool HandleDown(ref MouseHookStruct lParam,MouseButtons button){
		var evt=new MouseEvent(lParam.pt.x,lParam.pt.y,button);
		var key=evt.Key;

		_down.evt?.Invoke(evt);

		if(evt.Handled&&AutoHandleUpWhenHandledDown&&!GlobalKeyboardHook.OnRelease.ContainsKey(key))
			GlobalKeyboardHook.OnRelease.Add(key,null);
		return evt.Handled;
	}

	private static bool HandleUp(ref MouseHookStruct lParam,MouseButtons button){
		var evt=new MouseEvent(lParam.pt.x,lParam.pt.y,button);
		var key=evt.Key;

		if(GlobalKeyboardHook.OnRelease.TryGetValue(key,out var onRelease)){
			GlobalKeyboardHook.OnRelease.Remove(key);
			if(key!=onRelease){
				evt.Handled=true;
				if(onRelease is{} release)
					new Send().Key(release,false).SendNow();
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
	private static extern bool UnhookWindowsHookEx(IntPtr hHook);

	[DllImport("user32.dll")]
	private static extern int CallNextHookEx(IntPtr idHook,int nCode,WindowMessage wParam,ref MouseHookStruct lParam);
	#endregion

	#region Constant, Structure and Delegate Definitions
	private delegate int MouseHookProc(int code,WindowMessage wParam,ref MouseHookStruct lParam);

	private const int WhMouseLl=14;

	[StructLayout(LayoutKind.Sequential)]
	private struct Point{
		public int x;
		public int y;
	}


	[StructLayout(LayoutKind.Sequential)]
	private struct MouseHookStruct{
		public Point pt;
		public int mouseData;
		public uint flags;
		public uint time;
		public IntPtr dwExtraInfo;
	}
	#endregion

}