using System.Globalization;
using System.Text;
using PlayifyUtility.Utils;
using PlayifyUtility.Web.Utils;

namespace PlayifyUtility.Web;

public class WebSend{
	private readonly List<string> _headers=new();
	private readonly WebBase _webBase;
	public readonly WebSession Session;
	private bool _caching;

	public WebSend(WebSession session){
		Session=session;
		_webBase=session.WebBase;
		Cache(_webBase.CacheByDefault);
	}

	public bool Caching{
		get=>_caching;
		set=>Cache(value);
	}
	public bool AlreadySent{get;internal set;}

	public async Task<Stream> Begin(int code){
		var output=Session.Stream;
		var str=new StringBuilder("HTTP/1.1 ").Append(code).Append(" ").Append(WebUtils.GetHttpCodeName(code)).Append("\r\n");
		foreach(var header in _headers) str.Append(header).Append("\r\n");
		str.Append("\r\n");
		var bytes=Encoding.UTF8.GetBytes(str.ToString());
		await output.WriteAsync(bytes,0,bytes.Length);
		await output.FlushAsync();
		AlreadySent=true;
		return Session.Type==RequestType.Head?Stream.Null:output;
	}

	public WebDocument Document()=>new(this);

	#region Settings
	public WebSend Cache(bool b){
		_caching=b;
		return Header("Cache-Control",!b?"Cache-Control: no-cache, no-store, must-revalidate":"public,max-age="+_webBase.CacheTime);//60=1min , 604800=1week
	}

	public WebSend Header(string key,string value){
		key+=": ";
		if(!key.Equals("Set-Cookie",StringComparison.OrdinalIgnoreCase))//Remove previous variants from Header 
			_headers.RemoveAll(s=>s.StartsWith(key,true,CultureInfo.InvariantCulture));
		_headers.Add(key+value);
		return this;
	}

	public WebSend Cookie(string key,string? value){
		if(value==null) key+="=\"\";expires=Thu, 01 Jan 1970 00:00:00 GMT";
		else key+="="+value+"; Max-Age="+60*60*24*30;
		if(Session.Headers.TryGetValue("Host",out var host)){
			if(host.Count(c=>c=='.')==2)//If Host is domain and contains subdomain
				host=host[(host.IndexOf('.')+1)..];
			key+="; domain="+host;
		}
		return Header("Set-Cookie",key);
	}
	#endregion

	#region SendVariants
	public async Task File(string path,bool download=false){
		var fileInfo=new FileInfo(path);
		if(!fileInfo.Exists) throw new FileNotFoundException(path);
		if(download) Header("Content-Disposition","attachment; filename=\""+fileInfo.Name.Replace("\"","\\\"")+"\"");
		var length=fileInfo.Length;
		if(length>10*1024*1024){//bigger than 10MB => send ranges
			var start=0L;
			var size=length;
			var end=length-1;
			Header("Content-Type",WebUtils.MimeType(fileInfo.Extension));
			Header("Accept-Ranges","bytes");
			var httpCode=200;
			if(Session.Headers.TryGetValue("Range",out var range)){
				range=range[(range.IndexOf('=')+1)..];
				if(range.Contains(',')){
					Header("Content-Range","bytes "+start+"-"+end+"/"+size);
					await Begin(416);
					return;
				}
				var dash=range.IndexOf('-');
				var cStart=long.Parse(range[..dash]);
				if(!long.TryParse(range[(dash+1)..],out var cEnd)) cEnd=size;
				cEnd=Math.Min(cEnd,end);
				if(cStart>cEnd||cStart>size-1||cEnd>=size){
					Header("Content-Range","bytes "+start+"-"+end+"/"+size);
					await Begin(416);
					return;
				}
				start=cStart;
				end=cEnd;
				length=end-start+1;
				httpCode=206;
			}
			Header("Content-Range","bytes "+start+"-"+end+"/"+size);
			Header("Content-Length",length.ToString());

			var buffer=new byte[1024*1024];
			using var input=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read);
			input.Seek(start,SeekOrigin.Begin);
			var stream=await Begin(httpCode);
			while(start<=size&&start<=end){
				var a=await input.ReadAsync(buffer,0,(int)Math.Min(buffer.Length,end-start+1));
				if(a==-1) break;
				start+=a;
				await stream.WriteAsync(buffer,0,buffer.Length);
			}
		} else{
			if(_caching){
				var hash=$"\"{await WebUtils.GetHashAsync(fileInfo)}\"";
				Header("Etag",hash);
				var ifNoneMatch=Session.Headers.Get("If-None-Match");
				if(ifNoneMatch=="*"||ifNoneMatch!=null&&ifNoneMatch.Contains(hash)){
					await Begin(304);
					return;
				}
			}
			Header("Content-Type",WebUtils.MimeType(Path.GetExtension(path)));
			Header("Content-Length",length.ToString());

			var stream=await Begin(200);
			if(stream==Stream.Null) return;
			using var fileStream=new FileStream(path,FileMode.Open,FileAccess.Read);

			await fileStream.CopyToAsync(stream);
			fileStream.Close();
			await stream.FlushAsync();
		}
	}


	public async Task Error(int code){
		Header("Content-Length","0");
		Header("Connection","close");
		await Begin(code);
		throw new CloseException();
	}

	public Task Redirect(string url,bool permanent){
		Header("Location",url);
		Header("Content-Length","0");
		if(!permanent) return Begin(303);
		Cache(true);
		return Begin(301);
	}
	#endregion

}