using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Features.Interact;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
public static class GlobalKeyboardHook{
	#region Events
	public static event GlobalKeyEventHandler KeyDown{add=>Hook(ref _down,value);remove=>Unhook(ref _down,value);}
	public static event GlobalKeyEventHandler KeyUp{add=>Hook(ref _up,value);remove=>Unhook(ref _up,value);}
	#endregion

	#region Instance Variables
	private static IntPtr _hook=IntPtr.Zero;
	private static readonly KeyboardHookProc Proc=HookProc;
	private static (GlobalKeyEventHandler? evt,List<GlobalKeyEventHandler> lst) _down=(null,new List<GlobalKeyEventHandler>());
	private static (GlobalKeyEventHandler? evt,List<GlobalKeyEventHandler> lst) _up=(null,new List<GlobalKeyEventHandler>());
	#endregion

	#region Private Methods
	private static void Hook(ref (GlobalKeyEventHandler? evt,List<GlobalKeyEventHandler> lst) tuple,GlobalKeyEventHandler value){
		tuple.lst.Add(value);
		tuple.evt+=value;
		if(_hook!=IntPtr.Zero) return;
		_hook=SetWindowsHookEx(WhKeyboardLl,Proc,GetModuleHandle(IntPtr.Zero),0);
	}

	private static void Unhook(ref (GlobalKeyEventHandler? evt,List<GlobalKeyEventHandler> lst) tuple,GlobalKeyEventHandler value){
		if(!tuple.lst.Remove(value)) return;
		tuple.evt-=value;
		if(_up.lst.Any()||_down.lst.Any()||_hook==IntPtr.Zero) return;
		UnhookWindowsHookEx(_hook);
		_hook=IntPtr.Zero;
	}


	private static int HookProc(int code,int wParam,ref KeyboardHookStruct lParam){
		try{
			if(code>=0&&lParam.DwExtraInfo!=Send.ProcessHandle){
				var key=(Keys)lParam.VkCode;
				var evt=new KeyEvent(key,lParam.VkCode,lParam.ScanCode);
				switch(wParam){
					case WmKeydown:
					case WmSysKeyDown:
						_down.evt?.Invoke(evt);
						break;
					case WmKeyup:
					case WmSysKeyUp:
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
	private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

	[DllImport("user32.dll")]
	private static extern int CallNextHookEx(IntPtr idHook,int nCode,int wParam,ref KeyboardHookStruct lParam);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetModuleHandle(IntPtr zero);
	#endregion

	#region Constant, Structure and Delegate Definitions
	private delegate int KeyboardHookProc(int code,int wParam,ref KeyboardHookStruct lParam);

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	private struct KeyboardHookStruct{
		public int VkCode;
		public int ScanCode;
		public int Flags;
		public int Time;
		public IntPtr DwExtraInfo;
	}

	private const int WhKeyboardLl=13;
	private const int WmKeydown=0x100;
	private const int WmKeyup=0x101;
	private const int WmSysKeyDown=0x104;
	private const int WmSysKeyUp=0x105;
	#endregion
}