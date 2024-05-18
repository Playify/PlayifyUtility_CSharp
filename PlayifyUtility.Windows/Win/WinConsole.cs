using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using PlayifyUtility.Windows.Features.Hooks;
using PlayifyUtility.Windows.Features.Interact;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Win;

[PublicAPI]
public static partial class WinConsole{
	public static WinWindow ConsoleWindow=>new(GetConsoleWindow());

	public static bool Visible{
		get=>ConsoleWindow.IsVisible;
		set{
			var window=ConsoleWindow;
			window.IsVisible=value;
			if(value) window.SetForeground();
		}
	}

	public static bool EnableAnsi(){
		var iStdOut=GetStdHandle(-11);//STD_OUTPUT_HANDLE
		if(!GetConsoleMode(iStdOut,out var outConsoleMode)) return false;
		outConsoleMode|=0x0004|0x0008;//ENABLE_VIRTUAL_TERMINAL_PROCESSING|DISABLE_NEWLINE_AUTO_RETURN
		return SetConsoleMode(iStdOut,outConsoleMode);
	}


	/**Can be hidden using Win+Alt+M and shown using Win+Alt+N*/
	public static void CreateConsole()=>CreateHideAbleConsole(false);

	/**Can be hidden using Win+Alt+M and shown using Win+Alt+N*/
	public static void CreateHiddenConsole()=>CreateHideAbleConsole(true);

	private static void CreateHideAbleConsole(bool hidden){
		if(AllocConsole()&&ConsoleWindow.NonZero(out var console)){
			if(hidden) console.Hidden=true;

			var conIn=CreateFile("CONIN$",GenericRead,FileShareRead,0,OpenExisting,0,0);
			Console.SetIn(new StreamReader(
				new FileStream(
					new SafeFileHandle(
						conIn,
						true),
					FileAccess.Read),
				Encoding.Default));

			var conOut=CreateFile("CONOUT$",GenericWrite,FileShareWrite,0,OpenExisting,0,0);
			Console.SetOut(new StreamWriter(
				new FileStream(
					new SafeFileHandle(
						conOut,
						true),
					FileAccess.Write),
				Encoding.Default){
				AutoFlush=true,
			});
			Console.SetError(Console.Out);


			//For net48, Console has hidden fields, that need to be overridden, to enable Console.ReadKey()
			typeof(Console).GetField("_consoleInputHandle",BindingFlags.NonPublic|BindingFlags.Static)?.SetValue(null,conIn);
			typeof(Console).GetField("_consoleOutputHandle",BindingFlags.NonPublic|BindingFlags.Static)?.SetValue(null,conOut);

			//For net6, Console accesses the StdHandle every time, so it needs to be set correctly
			SetStdHandle(-10,conIn);
			SetStdHandle(-11,conOut);
			SetStdHandle(-12,conOut);

			if(!hidden) return;
		} else if(!hidden) return;
		else (console=ConsoleWindow).HidePush();

		GlobalKeyboardHook.KeyDown+=e=>{
			if(e.Handled) return;

			switch(e.Key){
				case Keys.M when Modifiers.Win&&Modifiers.Alt:{
					if(console.IsVisible&&WinWindow.Foreground==console){
						e.Handled=true;
						console.ShowWindowCommand=ShowWindowCommands.Hide;
					}
					break;
				}
				case Keys.N when Modifiers.Win&&Modifiers.Alt:{
					if(!console.IsVisible){
						e.Handled=true;
						console.ShowWindowCommand=ShowWindowCommands.Show;
						console.SetForeground();
					}
					break;
				}
			}
		};
	}
}