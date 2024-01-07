using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
public static class GlobalMouseHook{
	#region Events
	public static event GlobalMouseEventHandler KeyDown{add=>Hook(ref _down,value);remove=>Unhook(ref _down,value);}
	public static event GlobalMouseEventHandler KeyUp{add=>Hook(ref _up,value);remove=>Unhook(ref _up,value);}
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
				if(wParam==512){
					var mouseEvent=new MouseEvent(lParam.pt.x,lParam.pt.y,MouseButtons.None);
					_move.evt?.Invoke(mouseEvent);
					if(mouseEvent.Handled) return 1;
				} else if(wParam==522){
					var mouseEvent=new MouseEvent(lParam.pt.x,lParam.pt.y,MouseButtons.None,Math.Sign(lParam.mouseData));
					_scroll.evt?.Invoke(mouseEvent);
					if(mouseEvent.Handled) return 1;
				} else{
					var (button,down)=
						wParam switch{
							513=>(MouseButtons.Left,true),
							514=>(MouseButtons.Left,false),
							516=>(MouseButtons.Right,true),
							517=>(MouseButtons.Right,false),
							519=>(MouseButtons.Middle,true),
							520=>(MouseButtons.Middle,false),
							523=>(lParam.mouseData==0x10000?MouseButtons.XButton1:MouseButtons.XButton2,true),
							524=>(lParam.mouseData==0x10000?MouseButtons.XButton1:MouseButtons.XButton2,false),
							_=>(MouseButtons.None,false),
						};
					var mouseEvent=new MouseEvent(lParam.pt.x,lParam.pt.y,button);
					(down?_down:_up).evt?.Invoke(mouseEvent);
					if(mouseEvent.Handled) return 1;
				}
			}
		} catch(Exception e){
			Console.WriteLine($"Error in {nameof(GlobalMouseHook)}: {e}");
		}
		return CallNextHookEx(_hook,code,wParam,ref lParam);
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