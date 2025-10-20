using System.Collections.Specialized;
using System.IO.Pipes;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using JetBrains.Annotations;
using PlayifyUtility.Streams;
using PlayifyUtility.Utils;
using PlayifyUtility.Utils.Extensions;
using PlayifyUtility.Web.Utils;

namespace PlayifyUtility.Web;

[PublicAPI]
public class WebSocket:IAsyncEnumerable<(string? s,byte[] b)>{
	public static int PingCountUntilError=5;
	private readonly TcpClient? _client;
	private readonly IWebStream _input;
	private readonly Stream _output;
	private readonly BufferBlock<(string? s,byte[] b)> _receive=new();

	private readonly SemaphoreSlim _sendSemaphore=new(1,1);
	private volatile TaskCompletionSource<byte[]>? _pong;

	#region Constructor
	internal WebSocket(TcpClient? client,IWebStream input,Stream output){
		_client=client;
		_input=input;
		_output=output;
		if(_client!=null&&PingCountUntilError!=0) _=SendPings();
		_=ReceiveLoop();
	}

	private WebSocket(Stream @in,Stream @out):this(null,new WebStream3(@in),@out){
	}
	#endregion

	#region Static Methods
	public static Task<WebSocket> CreateWebSocketTo(string uri,NameValueCollection? headers=null)=>CreateWebSocketTo(new Uri(uri),headers);

	public static async Task<WebSocket> CreateWebSocketTo(Uri uri,NameValueCollection? headers=null){
		if(uri.Scheme is not "ws" and not "wss") throw new Exception("Wrong scheme, expected ws or wss, got "+uri.Scheme);
		var client=new TcpClient();
		await client.ConnectAsync(uri.Host,uri.Port);
		Stream stream=client.GetStream();
		if(uri.Scheme=="wss"){
			var ssl=new SslStream(stream,false);
			await ssl.AuthenticateAsClientAsync(uri.Host);
			stream=ssl;
		}

		return await CreateWebSocketTo(client,stream,uri.PathAndQuery,headers,uri.Host);
	}

	public static Task<WebSocket> CreateWebSocketTo(TcpClient client,string path,NameValueCollection? headers=null,string? host=null)=>CreateWebSocketTo(client,client.GetStream(),path,headers,host);

	public static async Task<WebSocket> CreateWebSocketTo(TcpClient client,Stream stream,string path,NameValueCollection? headers=null,string? host=null){
		var random=new Random();
		const string source="0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
		var webSocketKey=EnumerableUtils.RepeatSelect(16,()=>source[random.Next(source.Length)])
		                                .ConcatString();


		var header="GET "+path+" HTTP/1.1\r\n"+
		           "Connection: Upgrade\r\n"+
		           "Cache-Control: no-cache\r\n"+
		           "Upgrade: websocket\r\n"+
		           "Sec-WebSocket-Key: "+webSocketKey+"\r\n";

		if(headers!=null)
			foreach(string key in headers)
			foreach(var value in headers.GetValues(key)!)
				header+=key+": "+value+"\r\n";

		if(host!=null) header+="Host: "+host+"\r\n";


		header+="\r\n";
		var bytes=Encoding.UTF8.GetBytes(header);
		await stream.WriteAsync(bytes,0,bytes.Length);
		await stream.FlushAsync();

		webSocketKey+="258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
		var hash=WebUtils.Sha1(Encoding.ASCII.GetBytes(webSocketKey));
		webSocketKey=Convert.ToBase64String(hash);

		var webStream=new WebStream3(stream);

		var first=false;
		var valid=false;
		var con=false;
		var upgrade=false;
		var accept=false;
		var firstLine=new Regex("^HTTP/[0-9.]+ (\\d+)");
		while(true){
			var line=await webStream.ReadLineAsync();
			if(line=="") break;

			var match=firstLine.Match(line);
			if(match.Success){
				valid=match.Groups[1].Value=="101";
				first=true;
				if(!valid){
					client.Close();//force close. This connection would be useless after a failed attempt.
					throw new ProtocolViolationException("Error opening WebSocket, Wrong response code received: "+line);
				}
				continue;
			}
			var i=line.IndexOf(':');
			var key=line.Substring(0,i);
			var val=line.Substring(i+1).Trim();
			if(key.Equals("Connection",StringComparison.OrdinalIgnoreCase)) con=val.Equals("Upgrade",StringComparison.OrdinalIgnoreCase);
			else if(key.Equals("Upgrade",StringComparison.OrdinalIgnoreCase)) upgrade=val.Equals("websocket",StringComparison.OrdinalIgnoreCase);
			else if(key.Equals("Sec-WebSocket-Accept",StringComparison.OrdinalIgnoreCase)) accept=val==webSocketKey;
		}
		if(!first||!con||!upgrade||!accept||!valid){
			client.Close();//force close. This connection would be useless after a failed attempt.
			throw new ProtocolViolationException("Error opening WebSocket, Wrong response received.");
		}

		return new WebSocket(client,webStream,stream);
	}

	public static (WebSocket server,WebSocket client) CreateLinked(){
		var in1=new AnonymousPipeServerStream(PipeDirection.In);
		var out1=new AnonymousPipeClientStream(PipeDirection.Out,in1.ClientSafePipeHandle);
		var in2=new AnonymousPipeServerStream(PipeDirection.In);
		var out2=new AnonymousPipeClientStream(PipeDirection.Out,in2.ClientSafePipeHandle);

		return (new WebSocket(in1,out2),new WebSocket(in2,out1));
	}
	#endregion

	#region Receive
	private async Task ReceiveLoop(){
		var mem=new MemoryStream();
		var baseArray=new byte[8];
		var keyArray=new byte[4];
		var op=0;
		while(IsConnected()){
			await _input.ReadFullyAsync(baseArray,0,2);
			var read=baseArray[0];
			var fin=(read&128)!=0;
			var currOp=read&15;
			if(currOp!=0) op=currOp;
			var len=baseArray[1]&0xff;
			var mask=(len&128)!=0;
			len&=127;
			if(len is 126 or 127){
				len=len==126?2:8;
				await _input.ReadFullyAsync(baseArray,0,len);
				var l=0;
				for(var i=0;i<len;i++){
					l<<=8;
					l|=baseArray[i]&0xFF;
				}
				len=l;
			}
			if(mask) await _input.ReadFullyAsync(keyArray);
			var bytes=new byte[len];
			await _input.ReadFullyAsync(bytes);
			if(mask)
				for(var i=0;i<len;i++)
					bytes[i]^=keyArray[i&0b11];
			mem.Write(bytes,0,bytes.Length);
			if(fin)
				switch(op){
					case 0x8:
						Close();
						break;
					case 0x9:
						await Send(0xA,mem.GetBuffer(),0,(int)mem.Length);
						mem.SetLength(0);
						break;
					case 0xA:
						_pong?.TrySetResult(mem.ToArray());
						mem.SetLength(0);
						break;
					case 0x1://text
						var array=mem.ToArray();
						_receive.Post((Encoding.UTF8.GetString(array),array));
						mem.SetLength(0);
						break;
					case 0x2://binary
						_receive.Post((null,mem.ToArray()));
						mem.SetLength(0);
						break;
				}
		}
		Close();
	}


	public async IAsyncEnumerator<(string? s,byte[] b)> GetAsyncEnumerator(CancellationToken cancelToken=default){
		try{
			while(await _receive.OutputAvailableAsync(cancelToken).ConfigureAwait(false))
			while(_receive.TryReceive(out var item))
				yield return item;
			await _receive.Completion.ConfigureAwait(false);// Propagate possible exception
		} finally{
			Close();
		}
	}

	public async Task<(string? s,byte[] b)> ReceiveOne(CancellationToken cancel=default){
		try{
			while(await _receive.OutputAvailableAsync(cancel).ConfigureAwait(false))
				if(_receive.TryReceive(out var item))
					return item;
			await _receive.Completion.ConfigureAwait(false);// Propagate possible exception
		} finally{
			Close();
		}
		throw new EndOfStreamException();
	}

	public int PendingReceiveMessages=>_receive.Count;
	#endregion

	#region Connected
	private bool IsConnected()=>_client?.Connected??((PipeStream)_output).IsConnected;

	public void Close(){
		_pong?.TrySetCanceled();
		if(_client==null) _output.Close();
		else _client.Close();
		_receive.Complete();
	}
	#endregion

	#region Send
	private async Task SendPings(){
		try{
			var i=0;
			if(PingCountUntilError!=0) await Task.Delay(1000);

			while(IsConnected()&&PingCountUntilError!=0){

				var s=Encoding.UTF8.GetBytes("SendPing"+DateTime.Now.Ticks);
				var source=new TaskCompletionSource<byte[]>();
				_pong=source;
				await Send(0x9,s);
				var delay=Task.Delay(5000);
				var v=await Task.WhenAny(source.Task,delay);
				_pong=null;
				if(v!=source.Task||!s.SequenceEqual(source.Task.Result)){
					i++;
					if(PingCountUntilError==0) continue;
					if(i>=PingCountUntilError){
						Close();
						return;
					}
				}
				await delay;
			}
		} catch(OperationCanceledException){
		} catch(IOException){
		} finally{
			if(PingCountUntilError!=0) Close();
		}
	}

	public Task Send(string s)=>Send(1,s);

	private Task Send(int op,string s)=>Send(op,Encoding.UTF8.GetBytes(s));

	public Task Send(byte[] data)=>Send(2,data);

	private Task Send(int op,byte[] data)=>Send(op,data,0,data.Length);

	public Task Send(byte[] data,int offset,int len)=>Send(2,data,offset,len);

	private async Task Send(int op,byte[] data,int offset,int len){
		try{
			byte[] arr;
			if(len<=125) arr=[(byte)(128|op),(byte)len];
			else if(len<=65536) arr=[(byte)(128|op),126,(byte)(len>> 8),(byte)len];
			else
				arr=[
					(byte)(128|op),127,0,0,0,0,
					(byte)(len>> 24),(byte)(len>> 16),(byte)(len>> 8),unchecked((byte)len),
				];
			try{
				await _sendSemaphore.WaitAsync();
				await _output.WriteAsync(arr,0,arr.Length);
				await _output.WriteAsync(data,offset,len);
				await _output.FlushAsync();
			} finally{
				_sendSemaphore.Release();
			}
		} catch(IOException e){
			throw new CloseException(e);
		}
	}
	#endregion

}