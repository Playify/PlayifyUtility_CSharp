using PlayifyUtility.Windows.Features;
using PlayifyUtility.Windows.Win;

namespace PlayifyUtils.Test;

internal static class Program{
	[STAThread]
	public static void Main(string[] args){

		var teams=new WinWindow(new IntPtr(0x10568));
		Screenshot.CopyFromScreen().Save(@"C:\Users\TE282179\Downloads\x.png");

	}
}