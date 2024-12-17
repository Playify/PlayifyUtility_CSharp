using PlayifyUtility.Loggers;
using PlayifyUtility.Utils.Extensions;

namespace PlayifyUtils.Test;

internal static class Program{
	[STAThread]
	public static void Main(string[] args){
		Console.Out.WriteLine("BE\nFORE");
		typeof(AnsiColor).RunClassConstructor();
		Console.Out.WriteLine("SET\nCON");
	}
}