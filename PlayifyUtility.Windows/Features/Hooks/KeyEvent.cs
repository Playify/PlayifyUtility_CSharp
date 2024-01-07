using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Features.Interact;

namespace PlayifyUtility.Windows.Features.Hooks;

public delegate void GlobalKeyEventHandler(KeyEvent e);

[PublicAPI]
public class KeyEvent{
	public readonly int ScanCode;
	public readonly int VkCode;

	public KeyEvent(Keys key,int vkCode,int scanCode){
		Key=key;
		VkCode=vkCode;
		ScanCode=scanCode;
	}

	public bool Handled{get;set;}

	public Keys Key{get;}

	private static readonly byte[] KeyboardState=new byte[256];

	public string GetUnicode(){
		if(Key==Keys.Packet) return ((char)ScanCode).ToString();//char.ConvertFromUtf32(ScanCode);

		var str=new StringBuilder(10);

		KeyboardState[(int)Keys.LControlKey]=(byte)(Modifiers.IsKeyDown(Keys.LControlKey)?0x80:0);
		KeyboardState[(int)Keys.RControlKey]=(byte)(Modifiers.IsKeyDown(Keys.RControlKey)?0x80:0);
		KeyboardState[(int)Keys.ControlKey]=(byte)(KeyboardState[(int)Keys.LControlKey]|KeyboardState[(int)Keys.RControlKey]);
		KeyboardState[(int)Keys.LMenu]=(byte)(Modifiers.IsKeyDown(Keys.LMenu)?0x80:0);
		KeyboardState[(int)Keys.RMenu]=(byte)(Modifiers.IsKeyDown(Keys.RMenu)?0x80:0);
		KeyboardState[(int)Keys.Menu]=(byte)(KeyboardState[(int)Keys.LMenu]|KeyboardState[(int)Keys.RMenu]);

		KeyboardState[(int)Keys.LShiftKey]=(byte)(Modifiers.IsKeyDown(Keys.LShiftKey)?0x80:0);
		KeyboardState[(int)Keys.RShiftKey]=(byte)(Modifiers.IsKeyDown(Keys.RShiftKey)?0x80:0);
		KeyboardState[(int)Keys.ShiftKey]=(byte)(KeyboardState[(int)Keys.LShiftKey]|KeyboardState[(int)Keys.RShiftKey]);
		KeyboardState[(int)Keys.CapsLock]=(byte)(Modifiers.IsCapsLock?0x80:0);
		KeyboardState[(int)Keys.NumLock]=(byte)(Modifiers.IsNumLock?0x80:0);
		var i=ToUnicode(VkCode,ScanCode,KeyboardState,str,str.Capacity,4);
		if(i>0) return str.ToString();
		//i will be 0 for empty, and negative for dead keys
		return "";
	}

	public override string ToString()=>$"{nameof(KeyEvent)}({Key},Handled={Handled},ScanCode={ScanCode},VkCode={VkCode})";

	[DllImport("user32.dll",CharSet=CharSet.Unicode)]
	private static extern int ToUnicode(int wVirtKey,int wScanCode,byte[] lpKeyState,[MarshalAs(UnmanagedType.LPWStr)]StringBuilder str,int capacity,int flags);

	[DllImport("user32.dll")]
	private static extern bool GetKeyboardState(byte[] lpKeyState);
}