using System.Collections.Specialized;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PlayifyUtility.Streams;
using PlayifyUtility.Utils.Extensions;
using PlayifyUtility.Web.Multipart;
using PlayifyUtility.Web.Utils;

namespace PlayifyUtility.Web;

[PublicAPI]
public class WebSession:MultipartRequest<WebSession>{
	private static readonly Regex PathFixer=new("(?!^)/\\.*((?=/)|$)");
	private static readonly Regex FirstLineRegex=new(@"(GET|POST|PUT|HEAD|OPTIONS) (/.*?)(?:\?(.*?))? HTTP/[21]\.[01]");
	private static readonly Regex CookieSplitter=new(" *; *");
	public readonly NameValueCollection Args=new(StringComparer.OrdinalIgnoreCase);
	public readonly TcpClient Client;
	public readonly NameValueCollection Cookies=new(StringComparer.Ordinal);
	public readonly Stream Stream;
	private int _length;
	private bool _finished;


	public WebSession(WebBase webBase,TcpClient client,IWebStream input,Stream output):base(webBase,input){
		Client=client;
		Send=new WebSend(this);
		Stream=output;
		Path="";
		RawUrl="";
	}

	public RequestType Type{get;private set;}
	public string Path{get;private set;}
	public string RawUrl{get;private set;}
	public WebSend Send{get;}

	protected override WebSession Session=>this;

	protected override bool End=>!Client.Connected;
	protected override bool Finished=>_finished;

	public bool IsHttps=>Stream is SslStream;

	protected override async Task ReadHeaders(){
		string firstLine;
		try{
			firstLine=await WebStream.ReadLineAsync();
		} catch(EndOfStreamException){
			throw new CloseException();
		}
		var match=FirstLineRegex.Match(firstLine);
		if(!match.Success){
			throw new CloseException("Illegal HTTP Request: \""+firstLine+"\"");
		}
		Type=(RequestType) Enum.Parse(typeof(RequestType),match.Groups[1].Value,true);
		Path=Uri.UnescapeDataString(match.Groups[2].Value);
		RawUrl=match.Groups[2].Value;

		Args.Clear();
		if(match.Groups[3].Success){
			RawUrl+="?"+match.Groups[3].Value;
			Args.Add(WebUtils.ParseQueryString(match.Groups[3].Value));
			//WebUtils.GetUrlParameters(match.Groups[3].Value,Args);
		}


		//Fix Path
		if(WebBase.HandleIllegalRequests){
			//Validate Path
			if(Path.FirstOrDefault()!='/'){
				await Send.Error(400);
				return;
			}
			if(Path.Contains('\\')){
				await Send.Error(400);
				return;
			}

			//Redirect from wrong path (containing "/.", "/./", "/.." or "/../")
			var path=PathFixer.Replace(Path,"");
			if(Path!=path){
				var url=Uri.EscapeDataString(path);
				var i=RawUrl.IndexOf('?');
				if(i!=-1) url+=RawUrl.Substring(i);
				await Send.Redirect(url,false);
				return;
			}
		}

		await base.ReadHeaders();

		Cookies.Clear();
		var cookies=Headers.GetValues("Cookie");
		if(cookies!=null)
			foreach(var cookie in cookies)
			foreach(var s in CookieSplitter.Split(cookie)){
				var i=s.IndexOf('=');
				if(i!=-1) Cookies.Add(WebUtility.UrlDecode(s.Substring(0,i)),WebUtility.UrlDecode(s.Substring(i+1)));
			}

		if(Headers.Get("Content-Length").Push(out var lengthStr)!=null&&int.TryParse(lengthStr,out var length)){
			_length=length;
			_finished=false;
		} else{
			_length=0;
			_finished=true;
		}

		Send.AlreadySent=false;
	}


	public bool WantsWebSocket()=>Headers.TryGetValue("Upgrade",out var s)&&s.Equals("websocket",StringComparison.OrdinalIgnoreCase);

	public async Task<WebSocket?> CreateWebSocket(){
		if(!WantsWebSocket()) return null;
		var key=Headers["Sec-WebSocket-Key"];
		key+="258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
		var hash=WebUtils.Sha1(Encoding.ASCII.GetBytes(key));
		key=Convert.ToBase64String(hash);
		Send.Header("Connection","Upgrade");
		Send.Header("Upgrade","websocket");
		Send.Header("Sec-WebSocket-Accept",key);
		var stream=await Send.Begin(101);
		if(stream==Stream.Null) throw new CloseException();
		return new WebSocket(Client,WebStream,stream);
	}

	protected override Task SkipEnd()=>Task.CompletedTask;

	protected override async Task Cleanup(){
		await IgnoreAsync();
		if(!Send.AlreadySent){
			Console.WriteLine($"Didn't send anything to \"{Path}\"");
			await Send.Error(500);
		}
	}

	public override Task IgnoreAsync(){
		if(Finished) return Task.CompletedTask;
		var len=_length;
		_length=0;
		_finished=true;
		return WebStream.SkipAsync(len);
	}

	public override async Task<byte[]> ReadByteArrayAsync(){
		if(Finished) throw new EndOfStreamException("Already finished this Multipart");
		var bytes=new byte[_length];
		_length=0;
		_finished=true;
		await WebStream.ReadFullyAsync(bytes);
		return bytes;
	}

	public override async Task<bool> ReadToFileAsync(string path){
		if(Finished) return false;
		var buffer=new byte[Math.Min(1024*1024,_length)];
#if !NETFRAMEWORK
		await
#endif
		using var stream=new FileStream(path,FileMode.OpenOrCreate,FileAccess.Write,FileShare.Read,buffer.Length,FileOptions.Asynchronous);
		while(_length>0){
			var i=await WebStream.ReadAsync(buffer,0,Math.Min(buffer.Length,_length));
			_length-=i;
			await stream.WriteAsync(buffer,0,i);
		}
		_finished=true;
		return true;
	}

	protected override void MarkFinished(){
		_length=0;
		_finished=true;
	}
}