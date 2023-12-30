using PlayifyUtility.Windows.Features.Hooks;

namespace PlayifyUtils.Test;// ReSharper disable once EmptyNamespace
internal class Program{
	public static void Main(string[] args){
		GlobalKeyboardHook.Hook();

		GlobalKeyboardHook.KeyDown+=e=>{
			var unicode=e.GetUnicode();
			Console.WriteLine(e.Key+" \""+unicode+"\"");
		};

		Application.Run();
	}
}