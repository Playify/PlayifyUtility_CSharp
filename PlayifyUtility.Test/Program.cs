using PlayifyUtility.Windows;
using PlayifyUtility.Windows.Features.Hooks;
using PlayifyUtility.Windows.Win;

namespace PlayifyUtils.Test;

internal static class Program{
	[STAThread]
	public static void Main(string[] args){
		MainThread.Init();


		GlobalKeyboardHook.KeyDown+=e=>{
			if(e.Key==Keys.NumPad5){
				Console.WriteLine("NumPad5");
				e.Handled=true;
				WinWindow.Foreground.PostSysCommand(WinWindow.SysCommand.WindowsMenu);
			}
		};


		Application.Run();
	}
}