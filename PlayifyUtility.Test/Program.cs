using PlayifyUtility.Windows;
using PlayifyUtility.Windows.Win;

namespace PlayifyUtils.Test;

internal static class Program{
	[STAThread]
	public static void Main(string[] args){
		WinConsole.CreateHiddenConsole();


		new Thread(()=>{
			while(true){
				Console.WriteLine("ReadLine: ");
				Console.WriteLine(Console.ReadKey());
			}
		}).Start();

		MainThread.Init();


		Application.Run();
	}
}