using PlayifyUtility.Loggers;
using PlayifyUtility.Utils;
using PlayifyUtility.Web;

namespace PlayifyUtils.Test;

internal class Program:WebBase{
	[STAThread]
	public static async Task Main(string[] args){
		Console.WriteLine(PlatformUtils.GetWindowsVersion()+" "+AnsiColor.IsSupported);
		await new Program().RunHttp();
	}

	protected override Task HandleRequest(WebSession session){
		Console.WriteLine(session.Path);
		return session.Send.Text(session.Path);
	}
}