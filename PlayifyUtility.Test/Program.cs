using PlayifyUtility.Windows;
using PlayifyUtility.Windows.Features.Hooks;

namespace PlayifyUtils.Test;
internal class Program{
	
	[STAThread]
	public static void Main(string[] args){
		MainThread.Init();
		


		Test();
		



		Application.Run();
	}

	public static async Task Test(){
		try{
			Console.WriteLine("GO");
			await Task.Delay(10);
			Console.WriteLine("GO:"+await GlobalClipboardHook.GetNextString());
			Console.WriteLine("GO:"+await GlobalClipboardHook.GetNextString());
		} catch(Exception e){
			Console.WriteLine(e);
		}
	}
}