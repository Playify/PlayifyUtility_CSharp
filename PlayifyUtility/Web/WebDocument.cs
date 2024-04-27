using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PlayifyUtility.Web.Utils;

namespace PlayifyUtility.Web;

[PublicAPI]
public class WebDocument{
	private static readonly Regex Variables=new(@"##([\s\S]*?)##");
	private readonly WebSend _webSend;
	private int _code=200;
	private string? _type="text/html; charset=UTF-8";

	public WebDocument(WebSend webSend)=>_webSend=webSend;

	public string Document{get;set;}="";

	public WebDocument Code(int i){
		_code=i;
		return this;
	}

	public WebDocument MimeType(string? s){
		_type=s==null?null:s.Length!=0&&s[0]=='.'?WebUtils.MimeType(s):s;
		return this;
	}

	public async Task Send(){
		_webSend.Session.WebBase.BeforeSend(_webSend.Session,this);

		for(var match=Variables.Match(Document);match.Success;match=match.NextMatch())
			Console.WriteLine("[Web|Warning] Sending Document \""+_webSend.Session.Path+"\" without replacing Variable \""+match.Groups[1].Value+"\"");
		
		
		
		var bytes=Encoding.UTF8.GetBytes(Document.Replace('\x04','#'));
		if(_webSend.Caching){
			var hash=$"\"{WebUtils.GetHash(bytes)}\"";
			_webSend.Header("Etag",hash);
			var ifNoneMatch=_webSend.Session.Headers.Get("If-None-Match");
			if(ifNoneMatch=="*"||ifNoneMatch!=null&&ifNoneMatch.Contains(hash)){
				await _webSend.Begin(304);
				return;
			}
		}
		if(_type!=null) _webSend.Header("Content-Type",_type);
		_webSend.Header("Content-Length",bytes.Length.ToString());

		var stream=await _webSend.Begin(_code);
		if(stream==Stream.Null) return;
		await stream.WriteAsync(bytes,0,bytes.Length);
		await stream.FlushAsync();
	}

	public WebDocument Set(string s){
		Document=s;
		return this;
	}

	public WebDocument InsertRaw(string key,string val)=>SetRaw(key,val+"##"+key+"##");

	public WebDocument SetRaw(string key,string val){
		Document=Document.Replace("##"+key+"##",val);
		return this;
	}

	public WebDocument InsertFormatted(string key,string val)=>InsertRaw(key,Escape(val));
	public WebDocument SetFormatted(string key,string val)=>SetRaw(key,Escape(val));

	private static string Escape(string val)
		=>val.Replace('#','\x04')
		     .Replace("&","&amp;")
		     .Replace("<","&lt;")
		     .Replace(">","&gt;")
		     .Replace("'","&apos;")
		     .Replace("\"","&quot;");

	public WebDocument Replace(string key,string value){
		Document=Document.Replace(key,value);
		return this;
	}
}