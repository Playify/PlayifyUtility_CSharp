using System.Net;
using PlayifyUtility.Web;

namespace PlayifyUtils.Test;

internal class Program:WebBase{
	public static async Task Main(string[] args){
		try{
			var server=new Program();
			var task=server.RunHttp(new IPEndPoint(new IPAddress(new byte[]{127,2,4,8}),4590));

			await task;
		} catch(Exception e){
			Console.WriteLine(e);
			Environment.Exit(-1);
		}
	}


	protected override async Task HandleRequest(WebSession session){
		if(await session.CreateWebSocket() is{} webSocket){
			await webSocket.Send("TEST");
			webSocket.Close();
			return;
		}
		var s=Uri.UnescapeDataString(session.RawUrl);

		await session.Send.Document()
		             .MimeType("application/json")
		             .Set(s)
		             .Send();
	}
}