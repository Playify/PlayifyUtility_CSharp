namespace PlayifyUtility.Streams.Data;

public class DataOutputBuff:DataOutput{
	//DataOutput

	private const int MaxArraySize=int.MaxValue-8;

	//Buffer
	private byte[] _buf=new byte[32];

	public DataOutputBuff(Stream? output=null):base(output??Stream.Null){
	}

	private static int HugeCapacity(int minCapacity){
		if(minCapacity<0) throw new OutOfMemoryException();
		return minCapacity>MaxArraySize?int.MaxValue:MaxArraySize;
	}

	private void EnsureCapacity(int minCapacity){
		// overflow-conscious code
		if(minCapacity-_buf.Length>0) Grow(minCapacity);
	}

	private void Grow(int minCapacity){
		// overflow-conscious code
		var oldCapacity=_buf.Length;
		var newCapacity=oldCapacity<<1;
		if(newCapacity-minCapacity<0) newCapacity=minCapacity;
		if(newCapacity-MaxArraySize>0) newCapacity=HugeCapacity(minCapacity);
		Array.Resize(ref _buf,newCapacity);
	}

	public byte[] ToByteArray(){
		var arr=new byte[Length];
		Array.Copy(_buf,arr,Length);
		return arr;
	}

	public byte[] ToByteArray(int from){
		var arr=new byte[Length-from];
		if(from<0) Array.Copy(_buf,0,arr,-from,Length);
		else Array.Copy(_buf,from,arr,0,Length-from);
		return arr;
	}

	public override void Write(int b){
		EnsureCapacity(Length+1);
		_buf[Length]=(byte) b;
		Length+=1;
	}

	public override void Write(byte[] b,int off,int len){
		if(off<0||off>b.Length||len<0||off+len-b.Length>0) throw new IndexOutOfRangeException();
		EnsureCapacity(Length+len);
		Array.Copy(b,off,_buf,Length,len);
		Length+=len;
	}

	public override void Flush(){
		if(OutputStream==Stream.Null) return;
		OutputStream.Write(_buf,0,Length);
		OutputStream.Flush();
		Length=0;
	}

	public override async Task FlushAsync(){
		if(OutputStream==Stream.Null) return;
		await OutputStream.WriteAsync(_buf,0,Length);
		await OutputStream.FlushAsync();
		Length=0;
	}

	public override void Close()=>OutputStream.Close();

	public int Length{get;private set;}

	public (byte[] b,int len) GetBufferAndLength()=>(_buf,Length);
}