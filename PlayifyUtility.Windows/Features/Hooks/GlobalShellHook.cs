using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
public static class GlobalShellHook{// https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registershellhookwindow
	private static bool _hooked;
	internal static int Msg;

	public static void Hook(){
		if(_hooked) return;
		_hooked=true;
		Msg=RegisterWindowMessage("SHELLHOOK");
		RegisterShellHookWindow(GlobalClipboardHook.HookForm.Instance.Handle);
	}

	public static event Action<ShellHookEvent>? OnEvent;

	internal static void Invoke(ref Message message)=>OnEvent?.Invoke(new ShellHookEvent(message.WParam.ToInt32(),message.LParam));

	#region DLL imports
	[DllImport("user32.dll",EntryPoint="RegisterWindowMessageA",CharSet=CharSet.Unicode)]
	private static extern int RegisterWindowMessage(string lpString);

	[DllImport("user32.dll")]
	private static extern IntPtr RegisterShellHookWindow(IntPtr hWnd);
	#endregion
}