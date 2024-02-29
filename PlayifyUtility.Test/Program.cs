using PlayifyUtility.Windows.Features.Interact;

namespace PlayifyUtils.Test;

internal static class Program{
	[STAThread]
	public static void Main(string[] args){
		//WinConsole.CreateHiddenConsole();
		Thread.CurrentThread.Name="Main";


		new Thread(()=>{
			while(true){
				Console.WriteLine("ReadLine: ");
				var line=Console.ReadLine();
				var builder=new SendBuilder(line!);
				Console.WriteLine("C=> " + builder.ToConsoleString() + "\nH=> " + builder.ToHtmlString());
			}
		}).Start();


		/*
		GlobalMouseEventHandler handler=e=>{
			Console.WriteLine(e+" "+Thread.CurrentThread.Name);
		};

		GlobalKeyboardHook.KeyDown+=async e=>{
			Console.WriteLine(e+" "+Thread.CurrentThread.Name);
			if(e.Key==Keys.LWin){
				e.Handled=true;
				Console.WriteLine(Thread.CurrentThread.Name);
				Console.WriteLine(await GlobalClipboardHook.CopyString());
				Console.WriteLine(Thread.CurrentThread.Name);
				GlobalMouseHook.MouseDown-=handler;
			}
		};

		GlobalMouseHook.MouseDown+=handler;*/


		Application.Run();
	}
}