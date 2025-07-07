using JetBrains.Annotations;
#if NETFRAMEWORK
using PlayifyUtility.Utils.Extensions;
#endif

namespace PlayifyUtility.Streams;

[PublicAPI]
public class PushbackReader(TextReader reader):TextReader{
	private readonly Stack<int> _pushback=new();

	public override int Peek()=>_pushback.TryPeek(out var r)?r:reader.Peek();

	public override int Read()=>_pushback.TryPop(out var r)?r:reader.Read();

	public void Unread(int c)=>_pushback.Push(c);

	public void Unread(char[] chars,int offset,int length){
		for(var i=offset+length-1;i>=offset;i--) _pushback.Push(chars[i]);
	}

	//unreads, such that when reading again, the start of the string gets returned first
	public void Unread(string s)=>Unread(s.ToCharArray(),0,s.Length);


	public override void Close()=>reader.Close();
	protected override void Dispose(bool disposing)=>reader.Dispose();
}