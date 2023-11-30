using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using PlayifyUtility.Streams;

namespace PlayifyUtility.Web.Multipart;

[PublicAPI]
public class Multipart:MultipartRequest<Multipart>{
	private static readonly Regex NameFinder=new("(?<=(?<!file)name=\")(.*?)(?=\")");
	private static readonly Regex FilenameFinder=new("(?<=filename=\")(.*?)(?=\")");
	private readonly byte[] _boundary;
	private readonly byte[] _boundaryEnd;
	private bool _end;
	private bool _finished;

	public Multipart(WebSession session,IWebStream webStream,string boundary):base(session.WebBase,webStream){
		Session=session;
		_boundary=Encoding.UTF8.GetBytes($"--{boundary}");
		_boundaryEnd=Encoding.UTF8.GetBytes($"--{boundary}--");
	}


	protected override WebSession Session{get;}

	protected override bool End=>_end;
	protected override bool Finished=>_finished;
	public string? Name{get;private set;}
	public string? Filename{get;private set;}

	protected override Task ReadHeaders(){
		_finished=false;
		return base.ReadHeaders().ContinueWith(_=>{
			var disposition=Headers.GetValues("Content-Disposition")?.LastOrDefault();
			if(disposition!=null){
				var match=NameFinder.Match(disposition);
				Name=match.Success?match.Value:null;
				match=FilenameFinder.Match(disposition);
				Filename=match.Success?match.Value:null;
			} else{
				Name=null;
				Filename=null;
			}
		});
	}

	protected override Task Cleanup()=>Finished?Task.CompletedTask:IgnoreAsync();

	protected override async Task SkipEnd(){
		while(!End){
			//Skip headers
			while(!string.IsNullOrEmpty(await WebStream.ReadLineAsync())){
			}
			await IgnoreAsync();
		}
	}

	public override async Task IgnoreAsync(){
		if(Finished) return;
		await using var enumerator=ReadAll().GetAsyncEnumerator();
		while(await enumerator.MoveNextAsync()){
		}
	}

	public override async Task<byte[]> ReadByteArrayAsync(){
		if(Finished) throw new EndOfStreamException("Already finished this Multipart");
		var list=new List<byte[]>();
		await foreach(var bytes in ReadAll()) list.Add(bytes);
		var ret=new byte[list.Sum(b=>b.Length)];
		var pos=0;
		foreach(var bytes in list){
			Array.Copy(bytes,0,ret,pos,bytes.Length);
			pos+=bytes.Length;
		}
		return ret;
	}

	private static readonly byte[] NewLine=Encoding.ASCII.GetBytes("\r\n");

	private async IAsyncEnumerable<byte[]> ReadAll(){
		if(Finished) yield break;
		var first=true;
		while(true){
			var bytes=await WebStream.ReadUntilAsync(NewLine);
			if(bytes.SequenceEqual(_boundary)) break;
			if(bytes.SequenceEqual(_boundaryEnd)){
				_end=true;
				break;
			}
			//This is nececary, because otherwise every file would end in \r\n, even binary files
			if(first) first=false;
			else yield return NewLine;

			//Return current line
			yield return bytes;
		}
		_finished=true;
	}

	public override async Task<bool> ReadToFileAsync(string path){
		if(Finished) return false;
#if !NETFRAMEWORK
		await
#endif
		using var stream=new FileStream(path,FileMode.OpenOrCreate,FileAccess.Write,FileShare.Read,4096,FileOptions.Asynchronous);
		await foreach(var bytes in ReadAll()) await stream.WriteAsync(bytes,0,bytes.Length);
		return true;
	}

	protected override void MarkFinished()=>_finished=true;

	protected override async Task<bool> Begin(){
		await foreach(var unused in ReadAll()){
			//Console.WriteLine("Error reading Multipart, expected bound at beginning");
			return false;
		}
		return true;
	}
}