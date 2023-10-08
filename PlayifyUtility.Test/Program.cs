using System.Net;
using PlayifyUtility.Web;

namespace PlayifyRpc;

internal class Program:WebBase{
	public static async Task Main(string[] args){
		try{
			var server=new Program(args.Length==0?"rpc.js":args[0]);
			var task=server.RunHttp(new IPEndPoint(new IPAddress(new byte[]{127,2,4,8}),4590));

			await task;
		} catch(Exception e){
			Console.WriteLine(e);
			Environment.Exit(-1);
		}
	}

	private readonly string _rpcJs;

	private Program(string rpcJs){
		_rpcJs=rpcJs;
	}

	protected override async Task HandleRequest(WebSession session){
		if(await session.CreateWebSocket() is{} webSocket){
			return;
		}
		var s=Uri.UnescapeDataString(session.RawUrl);

		await session.Send.Document()
		             .MimeType("application/json")
		             .Set(s)
		             .Send();
	}
}