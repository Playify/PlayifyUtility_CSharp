using JetBrains.Annotations;

namespace PlayifyUtility.Streams.Data;

[PublicAPI]
public class DataInputBuff:DataInput,ICloneable{
	private static DataInputBuff? _empty;

	//Buffer
	private readonly byte[] _buf;
	private readonly int _count;
	private int _pos;

	public DataInputBuff(byte[] buf):this(buf,0,buf.Length){
	}

	public DataInputBuff(byte[] buf,int offset,int length){
		_buf=buf;
		_pos=offset;
		_count=offset+length;
	}

	public DataInputBuff(DataOutputBuff buff,int offset,int length){
		var (b,_)=buff.GetBufferAndLength();
		_buf=b;
		_pos=offset;
		_count=offset+length;
	}

	public DataInputBuff(DataOutputBuff buff,int from=0){
		var (b,len)=buff.GetBufferAndLength();
		_buf=b;
		_pos=from;
		_count=len;
	}

	public static DataInputBuff Empty=>_empty??=new DataInputBuff(Array.Empty<byte>());

	object ICloneable.Clone()=>Clone();

	public override int Read(byte[] b)=>Read(b,0,b.Length);

	public override int Read(byte[] b,int off,int len){
		if(b==null) throw new NullReferenceException();
		if(off<0||len<0||len>b.Length-off) throw new IndexOutOfRangeException();

		if(_pos>=_count) return 0;

		var avail=_count-_pos;
		if(len>avail) len=avail;
		if(len<=0) return 0;
		Array.Copy(_buf,_pos,b,off,len);
		_pos+=len;
		return len;
	}

	public override long Skip(long n){
		long k=_count-_pos;
		if(n<k) k=n<0?0:n;

		_pos+=(int) k;
		return k;
	}

	public int Available()=>_count-_pos;

	public override void Close(){
	}

	public override int Read()=>_pos<_count?_buf[_pos++]&0xff:-1;

	public byte[] ReadAll(){
		var bytes=new byte[Available()];
		ReadFully(bytes);
		return bytes;
	}

	public (byte[] b,int off,int len) GetBufferOffsetAndLength()=>(_buf,_pos,_count-_pos);

	public DataInputBuff Clone(){
		var (b,off,len)=GetBufferOffsetAndLength();
		return new DataInputBuff(b,off,len);
	}
}