using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using PlayifyUtility.Streams;
using PlayifyUtility.Web.Utils;

namespace PlayifyUtility.Web;

[PublicAPI]
public abstract class WebBase{
	private readonly HashSet<TcpClient> _clients=new();
	private readonly HashSet<TcpListener> _running=new();

	//Automatically block illegal requests
	public virtual bool HandleIllegalRequests=>true;
	//Enable Caching by default
	public virtual bool CacheByDefault=>true;
	//Cache Time, if cache is set (60=1min,604800=1week)
	public virtual long CacheTime=>604800;

	public Task RunHttp(int port=80)=>RunHttp(new IPEndPoint(IPAddress.Any,port));

	public async Task RunHttp(IPEndPoint ipEndPoint){
		var listener=new TcpListener(ipEndPoint);
		try{
			lock(_running) _running.Add(listener);
			listener.Start();
			while(true){
				var client=await listener.AcceptTcpClientAsync().ConfigureAwait(false);
				lock(_clients) _clients.Add(client);
				_=HandleConnection(client).ContinueWith(_=>{
					lock(_clients) _clients.Remove(client);
				}).ConfigureAwait(false);
			}
		} finally{
			listener.Stop();
			lock(_running) _running.Remove(listener);
		}
	}

	public Task RunHttps(X509Certificate2 certificate,int port=443)=>RunHttps(certificate,new IPEndPoint(IPAddress.Any,port));
	public Task RunHttps(X509Certificate2 certificate,IPEndPoint ipEndPoint)=>RunHttps(()=>certificate,ipEndPoint);
	public Task RunHttps(Func<X509Certificate2> certificate,int port=443)=>RunHttps(certificate,new IPEndPoint(IPAddress.Any,port));

	public async Task RunHttps(Func<X509Certificate2> certificate,IPEndPoint ipEndPoint){
		var listener=new TcpListener(ipEndPoint);
		try{
			lock(_running) _running.Add(listener);
			listener.Start();
			while(true){
				var client=await listener.AcceptTcpClientAsync().ConfigureAwait(false);
				_=HandleConnection(client,certificate()).ConfigureAwait(false);
			}
		} finally{
			listener.Stop();
			lock(_running) _running.Remove(listener);
		}
	}

	private async Task HandleConnection(TcpClient client,X509Certificate? ssl=null){
		lock(_clients) _clients.Add(client);
		try{
			Stream stream;
			if(ssl==null) stream=client.GetStream();
			else{
				var sslStream=new SslStream(client.GetStream(),false);
				await sslStream.AuthenticateAsServerAsync(ssl,false,true);
				stream=sslStream;
			}


			var session=CreateSession(client,stream);
			await foreach(var request in session.LoopAsync())
				try{
					await HandleRequest(request);
				}catch(CloseException){
					if(!session.Send.AlreadySent) await session.Send.Error(500);
					return;
				}catch(Exception e){
					Console.WriteLine(e);
					if(!session.Send.AlreadySent) await session.Send.Error(e switch{
						FileNotFoundException=>404,
						_=>500,
					});
					return;
				}
		} catch(CloseException){
		} catch(Exception){
			//Console.WriteLine(e);
		} finally{
			client.Close();
			lock(_clients) _clients.Remove(client);
		}
	}

	protected virtual WebSession CreateSession(TcpClient client,Stream stream)=>new(this,client,new WebStream2(stream),stream);

	protected abstract Task HandleRequest(WebSession session);

	protected internal virtual void BeforeSend(WebSession session,WebDocument document){
	}

	public virtual void Close(){
		lock(_running)
			foreach(var listener in _running)
				listener.Stop();
	}
}