using PlayifyUtility.Loggers;
using PlayifyUtility.Utils;

namespace PlayifyUtils.Test;

internal static class Program{
	[STAThread]
	public static void Main(string[] args){
		Console.WriteLine(PlatformUtils.GetWindowsVersion()+" "+AnsiColor.IsSupported);
	}
}