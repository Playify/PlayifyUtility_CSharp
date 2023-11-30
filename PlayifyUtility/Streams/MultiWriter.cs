using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;
using PlayifyUtility.Utils.Extensions;

namespace PlayifyUtility.Streams;

[PublicAPI]
public class MultiWriter:TextWriter{
	private readonly List<TextWriter> _children=new();
	public event Action? OnDispose;
	

	public MultiWriter(params TextWriter[] writers)=>_children.AddRange(writers);
	public MultiWriter(IEnumerable<TextWriter> writers)=>_children.AddRange(writers);
	public MultiWriter(TextWriter writer)=>_children.Add(writer);

	public override Encoding Encoding=>_children[0].Encoding;
	[AllowNull]
	public override string NewLine{
		get=>_children[0].NewLine;
		set=>_children.ForEach((c,v)=>c.NewLine=v,value);
	}

	public override void Flush()=>_children.ForEach(c=>c.Flush());
	public override Task FlushAsync()=>_children.Select(c=>c.FlushAsync()).WhenAll();

	public override void Close()=>_children.ForEach(c=>c.Close());

	protected override void Dispose(bool disposing){
		OnDispose?.Invoke();
		_children.ForEach(c=>c.Dispose());
	}
#if !NETFRAMEWORK
	public override ValueTask DisposeAsync(){
		OnDispose?.Invoke();
		return _children.Select(c=>c.DisposeAsync()).WhenAll();
	}
#endif

	public override void Write(char value)=>_children.ForEach((c,v)=>c.Write(v),value);
	public override void Write(string? value)=>_children.ForEach((c,v)=>c.Write(v),value);
	public override void WriteLine(string? value)=>_children.ForEach((c,v)=>c.WriteLine(v),value);

	public override Task WriteAsync(char value)=>_children.Select(c=>c.WriteAsync(value)).WhenAll();
	public override Task WriteAsync(string? value)=>_children.Select(c=>c.WriteAsync(value)).WhenAll();
	public override Task WriteLineAsync(string? value)=>_children.Select(c=>c.WriteLineAsync(value)).WhenAll();
}