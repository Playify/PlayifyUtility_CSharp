using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Features.Interact;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
public static class GlobalKeyboardHook{
	#region Constant, Structure and Delegate Definitions
	private delegate int KeyboardHookProc(int code,int wParam,ref KeyboardHookStruct lParam);

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	private struct KeyboardHookStruct{
		public int VkCode;
		public int ScanCode;
		public int Flags;
		public int Time;
		public IntPtr DwExtraInfo;

		public override string ToString()=>$"{nameof(VkCode)}: {VkCode}, {nameof(ScanCode)}: {ScanCode}, {nameof(Flags)}: {Flags}";
	}

	private const int WhKeyboardLl=13;
	private const int WmKeydown=0x100;
	private const int WmKeyup=0x101;
	private const int WmSysKeyDown=0x104;
	private const int WmSysKeyUp=0x105;
	#endregion

	#region Instance Variables
	private static IntPtr _hook=IntPtr.Zero;
	private static KeyboardHookProc _proc=null!;
	#endregion

	#region Events
	public static event KeyEventHandler? KeyDown;
	public static event KeyEventHandler? KeyUp;
	#endregion


	#region Public Methods
	public static void Hook(){
		if(_hook!=IntPtr.Zero) return;
		_proc=HookProc;
		_hook=SetWindowsHookEx(WhKeyboardLl,_proc,GetModuleHandle(IntPtr.Zero),0);
	}

	public static void Unhook(){
		if(_hook==IntPtr.Zero) return;
		UnhookWindowsHookEx(_hook);
		_hook=IntPtr.Zero;
	}


	private static int HookProc(int code,int wParam,ref KeyboardHookStruct lParam){
		try{
			if(code>=0&&lParam.DwExtraInfo!=Send.ProcessHandle){
				var key=(Keys) lParam.VkCode;
				var kea=new KeyEvent(key,lParam.VkCode,lParam.ScanCode);
				switch(wParam){
					case WmKeydown:
					case WmSysKeyDown:
						KeyDown?.Invoke(kea);
						break;
					case WmKeyup:
					case WmSysKeyUp:
						KeyUp?.Invoke(kea);
						break;
				}
				if(kea.Handled) return 1;
			}
		} catch(Exception e){
			Console.WriteLine("Error in KeyboardHook");
			Console.WriteLine(e);
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
}