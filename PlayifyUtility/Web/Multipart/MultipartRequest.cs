using System.Collections.Specialized;
using System.Text;
using PlayifyUtility.Streams;
using PlayifyUtility.Web.Utils;

namespace PlayifyUtility.Web.Multipart;

public abstract class MultipartRequest<TThis> where TThis:class{
	public readonly NameValueCollection Headers=new(StringComparer.OrdinalIgnoreCase);
	protected internal readonly WebBase WebBase;
	protected readonly IWebStream WebStream;

	public MultipartRequest(WebBase webBase,IWebStream webStream){
		WebBase=webBase;
		WebStream=webStream;
	}

	protected abstract WebSession Session{get;}

	#region Loop
	protected abstract bool End{get;}

	protected internal async IAsyncEnumerable<TThis> LoopAsync(){
		try{
			if(!await Begin()){
				await Session.Send.Error(400);
				throw new CloseException();
			}
			while(!End){
				await ReadHeaders();
				yield return this is TThis thiz?thiz:throw new Exception("Generic TThis is wrong");
				await Cleanup();
			}
		} finally{
			await SkipEnd();
		}
	}

	protected virtual Task<bool> Begin()=>Task.FromResult(true);

	protected virtual async Task ReadHeaders(){
		Headers.Clear();
		string s;
		while(!string.IsNullOrEmpty(s=await WebStream.ReadLineAsync())){
			var i=s.IndexOf(": ",StringComparison.Ordinal);
			if(i!=-1) Headers[s.Substring(0,i).Trim()]=s.Substring(i+1).Trim();
			else{
				await Session.Send.Error(400);
				throw new CloseException("Invalid Header in "+GetType().Name+": \""+s+"\"");
			}
		}
	}

	protected abstract Task Cleanup();
	protected abstract Task SkipEnd();
	#endregion


	#region Handle Content
	protected abstract bool Finished{get;}

	public abstract Task IgnoreAsync();

	public async Task<string> ReadStringAsync()=>Encoding.UTF8.GetString(await ReadByteArrayAsync());

	public abstract Task<byte[]> ReadByteArrayAsync();

	public abstract Task<bool> ReadToFileAsync(string path);

	public IAsyncEnumerable<Multipart>? ReadMultipartAsync(){
		var type=Headers.Get("Content-Type");
		if(type==null) return null;
		if(!type.StartsWith("multipart/",StringComparison.OrdinalIgnoreCase)) return null;
		var i=type.IndexOf("boundary=",StringComparison.OrdinalIgnoreCase);
		if(i==-1) return null;
		MarkFinished();
		return new Multipart(Session,WebStream,type.Substring(i+"boundary=".Length)).LoopAsync();
	}

	protected abstract void MarkFinished();
	#endregion

}