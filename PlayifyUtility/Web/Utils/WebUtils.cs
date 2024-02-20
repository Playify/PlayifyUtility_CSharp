using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;
using JetBrains.Annotations;

namespace PlayifyUtility.Web.Utils;

[PublicAPI]
public static class WebUtils{
	#region Extension Methods
	public static async IAsyncEnumerator<T> ToAsyncEnumerable<T>(this IReceivableSourceBlock<T> source,CancellationToken cancelToken){
		while(await source.OutputAvailableAsync(cancelToken).ConfigureAwait(false))
		while(source.TryReceive(out var item))
			yield return item;
		await source.Completion.ConfigureAwait(false);// Propagate possible exception
	}
	#endregion

	public static async Task<string> Read(string path){
		using var v=new StreamReader(new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite));
		return await v.ReadToEndAsync();
	}

	public static Task<string> Read(FileInfo path)=>Read(path.FullName);

	public static NameValueCollection ParseQueryString(string value){
		if(value.Length>0&&value[0]=='?') value=value.Substring(1);
		var collection=new NameValueCollection();

		var num1=value.Length;
		for(var index=0;index<num1;++index){
			var startIndex=index;
			var num2=-1;
			for(;index<num1;++index)
				switch(value[index]){
					case '&':goto label_7;
					case '=':
						if(num2<0){
							num2=index;
						}
						break;
				}
			label_7:
			string? str1;
			string str2;
			if(num2>=0){
				str1=value.Substring(startIndex,num2-startIndex);
				str2=value.Substring(num2+1,index-num2-1);
			} else{
				str1=null;
				str2=value.Substring(startIndex,index-startIndex);
			}
			collection.Add(WebUtility.UrlDecode(str1),WebUtility.UrlDecode(str2));
			if(index==num1-1&&value[index]=='&') collection.Add(null,string.Empty);
		}

		return collection;
	}

	#region Web
	public static string MimeType(string ext){
		ext=ext.Trim(' ','.');
		var mime=ext switch{
			"properties"=>"text/plain; charset=UTF-8",
			"txt"=>"text/plain; charset=UTF-8",
			"html"=>"text/html; charset=UTF-8",
			"ico"=>"image/x-icon",
			"png"=>"image/png",
			"jpg" or "jpeg"=>"image/jpeg",
			"json"=>"application/json; charset=UTF-8",
			"css"=>"text/css; charset=UTF-8",
			"js"=>"text/javascript; charset=UTF-8",
			"ts"=>"application/x-typescript; charset=UTF-8",
			"sass"=>"text/x-sass; charset=UTF-8",
			"scss"=>"text/x-scss; charset=UTF-8",
			"map"=>"application/json",
			"webmanifest"=>"application/manifest+json; charset=UTF-8",
			"svg"=>"image/svg+xml",
			"lnk"=>"application/x-ms-shortcut",
			"jar"=>"application/java-archive",
			"mp4"=>"video/mp4",
			"mp3"=>"audio/mpeg",
			"zip"=>"application/zip",
			"m3u"=>"audio/x-mpegurl",
			"m3u8"=>"audio/x-mpegurl",
			"wpl"=>"application/vnd.ms-wpl",
			"url"=>"application/internet-shortcut",
			"appcache"=>"text/cache-manifest",
			"otf"=>"font/otf",
			"fbx"=>"application/octet-stream",
			""=>"application/octet-stream",
			_=>null,
		};
		if(mime!=null) return mime;
		
		

		Console.WriteLine("Unknown Mime-Type for "+ext);
		return "application/octet-stream";
	}

	public static string GetHttpCodeName(int code){
		switch(code){
			case 101:return "Switching Protocols";
			case 102:return "Processing";
			case 200:return "OK";
			case 206:return "Partial Content";
			case 300:return "Multiple Choices";
			case 301:return "Moved Permanently";
			case 302:return "Found (Moved Temporarily)";
			case 303:return "See Other";
			case 304:return "Not Modified";
			case 307:return "Temporary Redirect";
			case 308:return "Permanent Redirect";
			case 400:return "Bad Request";
			case 401:return "Unauthorized";
			case 403:return "Forbidden";
			case 404:return "Not Found";
			case 416:return "Requested Range Not Satisfiable";
			case 418:return "I'm a teapod";
			case 500:return "Internal Server Error";
			case 501:return "Not Implemented";
			default:
				Console.WriteLine("Unknown HTTP Status Code: "+code);
				return (code/100) switch{
					2=>"OK",
					3=>"Moved",
					4=>"Bad Request",
					5=>"Internal Server Error",
					_=>"Unknown Error",
				};
		}
	}
	#endregion

	#region Hash
	public static Task<string> GetHashAsync(FileInfo fileInfo){
		var read=new FileStream(fileInfo.FullName,FileMode.Open,FileAccess.Read,FileShare.ReadWrite);
		return GetHashAsync(read).ContinueWith(task=>{
			read.Dispose();
			return task.Result;
		});
	}

	public static async Task<string> GetHashAsync(Stream stream){
		var algo=SHA1.Create();
		var buffer=new byte[8192];
		int bytesRead;

		while((bytesRead=await stream.ReadAsync(buffer,0,buffer.Length))!=0)
			algo.TransformBlock(buffer,0,bytesRead,buffer,0);
		algo.TransformFinalBlock(buffer,0,bytesRead);

		return ToString(algo.Hash??throw new NullReferenceException());
	}

	public static byte[] Sha1(byte[] bytes){
		using var sha1=SHA1.Create();
		return sha1.ComputeHash(bytes);
	}

	public static string GetHash(byte[] bytes)=>ToString(Sha1(bytes));

	public static string ToString(byte[] array)=>BitConverter.ToString(array).Replace("-","").ToLowerInvariant();
	#endregion
}