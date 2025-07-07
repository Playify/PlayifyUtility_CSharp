using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Features.Interact;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
public static class GlobalKeyboardHook{

	#region Events
	public static event GlobalKeyEventHandler KeyDown{add=>Hook(ref _down,value);remove=>Unhook(ref _down,value);}
	public static event GlobalKeyEventHandler KeyUp{add=>Hook(ref _up,value);remove=>Unhook(ref _up,value);}

	public static readonly Dictionary<Keys,Keys?> OnRelease=new();
	public static bool AutoHandleUpWhenHandledDown{get;set;}=true;
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
			} else if(_thread==null&&(_up.lst.Any()||_down.lst.Any())){
				_thread=UiThread.Create(nameof(GlobalKeyboardHook));
				_hook=_thread.Invoke(()=>SetWindowsHookEx(WhKeyboardLl,Proc,CommonHook.HInstance(),0));
			}
		}
	}
	#endregion

	#region Instance Variables
	private static readonly KeyboardHookProc Proc=HookProc;
	private static UiThread? _thread;
	private static IntPtr _hook;
	private static (GlobalKeyEventHandler? evt,List<GlobalKeyEventHandler> lst) _down=(null,new List<GlobalKeyEventHandler>());
	private static (GlobalKeyEventHandler? evt,List<GlobalKeyEventHandler> lst) _up=(null,new List<GlobalKeyEventHandler>());
	#endregion

	#region Private Methods
	private static void Hook(ref (GlobalKeyEventHandler? evt,List<GlobalKeyEventHandler> lst) tuple,GlobalKeyEventHandler value){
		tuple.lst.Add(value);
		tuple.evt+=value;
		if(_thread!=null||Paused) return;
		_thread=UiThread.Create(nameof(GlobalKeyboardHook));
		_hook=_thread.Invoke(()=>SetWindowsHookEx(WhKeyboardLl,Proc,CommonHook.HInstance(),0));
	}

	private static void Unhook(ref (GlobalKeyEventHandler? evt,List<GlobalKeyEventHandler> lst) tuple,GlobalKeyEventHandler value){
		if(!tuple.lst.Remove(value)) return;
		tuple.evt-=value;
		if(_up.lst.Any()||_down.lst.Any()||_thread==null) return;
		var thread=_thread;
		_thread=null;
		thread.Exit(()=>UnhookWindowsHookEx(_hook));
	}


	private static int HookProc(int code,WindowMessage wParam,ref KeyboardHookStruct lParam){
		try{
			if(code>=0&&lParam.DwExtraInfo!=Send.ProcessHandle){
				var key=(Keys)lParam.VkCode;
				var evt=new KeyEvent(key,lParam.VkCode,lParam.ScanCode);
				switch(wParam){
					case WindowMessage.WM_KEYDOWN:
					case WindowMessage.WM_SYSKEYDOWN:
						_down.evt?.Invoke(evt);
						if(evt.Handled&&AutoHandleUpWhenHandledDown&&!OnRelease.ContainsKey(key)){
							OnRelease.Add(key,null);
						}
						break;
					case WindowMessage.WM_KEYUP:
					case WindowMessage.WM_SYSKEYUP:
						if(OnRelease.TryGetValue(key,out var onRelease)){
							OnRelease.Remove(key);
							if(key!=onRelease){
								evt.Handled=true;
								if(onRelease is{} release)
									new Send().Key(release,false).SendNow();
							}
						}

						_up.evt?.Invoke(evt);
						break;
				}
				if(evt.Handled) return 1;
			}
		} catch(Exception e){
			Console.WriteLine($"Error in {nameof(GlobalKeyboardHook)}: {e}");
		}
		return CallNextHookEx(_hook,code,wParam,ref lParam);
	}
	#endregion

	#region DLL imports
	[DllImport("user32.dll")]
	private static extern IntPtr SetWindowsHookEx(int idHook,KeyboardHookProc callback,IntPtr hInstance,uint threadId);

	[DllImport("user32.dll")]
	private static extern bool UnhookWindowsHookEx(IntPtr hHook);

	[DllImport("user32.dll")]
	private static extern int CallNextHookEx(IntPtr idHook,int nCode,WindowMessage wParam,ref KeyboardHookStruct lParam);
	#endregion

	#region Constant, Structure and Delegate Definitions
	private delegate int KeyboardHookProc(int code,WindowMessage wParam,ref KeyboardHookStruct lParam);

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	private struct KeyboardHookStruct{
		public int VkCode;
		public int ScanCode;
		public int Flags;
		public int Time;
		public IntPtr DwExtraInfo;
	}

	private const int WhKeyboardLl=13;
	#endregion

}