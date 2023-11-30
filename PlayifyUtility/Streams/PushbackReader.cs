
using JetBrains.Annotations;
#if NETFRAMEWORK
using PlayifyUtility.Utils.Extensions;
#endif

namespace PlayifyUtility.Streams;

[PublicAPI]
public class PushbackReader:TextReader{
	private readonly Stack<int> _pushback=new();
	private readonly TextReader _reader;

	public PushbackReader(TextReader reader)=>_reader=reader;

	public override int Peek()=>_pushback.TryPeek(out var r)?r:_reader.Peek();

	public override int Read()=>_pushback.TryPop(out var r)?r:_reader.Read();

	public void Unread(int c)=>_pushback.Push(c);

	public void Unread(char[] chars,int offset,int length){
		for(var i=offset+length-1;i>=offset;i--) _pushback.Push(chars[i]);
	}


	public override void Close()=>_reader.Close();
	protected override void Dispose(bool disposing)=>_reader.Dispose();
}