using System.Text;
using JetBrains.Annotations;

namespace PlayifyUtility.Streams.Data;

[PublicAPI]
public class DataInput:IDisposable{
	private readonly Stream _in;
	private readonly byte[] _readBuffer=new byte[16];

	public DataInput(Stream @in)=>_in=@in;

	protected DataInput()=>_in=Stream.Null;

	public void Dispose()=>_in.Dispose();

	public virtual int Read(byte[] b)=>_in.Read(b,0,b.Length);

	public virtual int Read(byte[] b,int off,int len)=>_in.Read(b,off,len);

	public virtual long Skip(long n)=>_in.Seek(n,SeekOrigin.Current);

	public virtual void Close()=>_in.Close();

	public virtual int Read()=>_in.ReadByte();

	public void ReadFully(byte[] b)=>ReadFully(b,0,b.Length);

	public byte[] ReadFully(int len){
		var bytes=new byte[len];
		ReadFully(bytes);
		return bytes;
	}

	public void ReadFully(byte[] b,int off,int len){
		if(len<0) throw new IndexOutOfRangeException();
		var n=0;
		while(n<len){
			var count=Read(b,off+n,len-n);
			if(count<=0) throw new EndOfStreamException();
			n+=count;
		}
	}

	public bool ReadBoolean(){
		var ch=Read();
		if(ch<0) throw new EndOfStreamException();
		return ch!=0;
	}

	public byte ReadByte(){
		var ch=Read();
		if(ch<0) throw new EndOfStreamException();
		return (byte)ch;
	}

	public short ReadShort(){
		var ch1=Read();
		var ch2=Read();
		if((ch1|ch2)<0) throw new EndOfStreamException();
		return (short)((ch1<<8)+ch2);
	}

	public ushort ReadUShort()=>(ushort)ReadShort();

	public char ReadChar()=>(char)ReadShort();

	public int ReadInt(){
		var ch1=Read();
		var ch2=Read();
		var ch3=Read();
		var ch4=Read();
		if((ch1|ch2|ch3|ch4)<0) throw new EndOfStreamException();
		return (ch1<<24)+(ch2<<16)+(ch3<<8)+ch4;
	}

	public uint ReadUInt()=>(uint)ReadInt();

	public long ReadLong(){
		ReadFully(_readBuffer,0,8);
		return ((long)_readBuffer[0]<<56)+((long)(_readBuffer[1]&255)<<48)+((long)(_readBuffer[2]&255)<<40)+((long)(_readBuffer[3]&255)<<32)+((long)(_readBuffer[4]&255)<<24)+((_readBuffer[5]&255)<<16)+((_readBuffer[6]&255)<<8)+
		       (_readBuffer[7]&255);
	}

	public ulong ReadULong()=>(ulong)ReadLong();

	public float ReadFloat(){
		ReadFully(_readBuffer,0,4);
		if(BitConverter.IsLittleEndian) Array.Reverse(_readBuffer,0,4);
		return BitConverter.ToSingle(_readBuffer,0);
	}

	public double ReadDouble()=>BitConverter.Int64BitsToDouble(ReadLong());

	public string? ReadString(){
		var length=ReadLength();
		if(length<0) return null;
		var bytes=new byte[length];
		ReadFully(bytes);
		return Encoding.UTF8.GetString(bytes);
	}

	/*public bool[] ReadBooleans(int cnt){
		var booleans=new bool[cnt];
		byte byt=0;
		for(var i=0;i<cnt;i++){
			if((i&7)==0) byt=ReadByte();
			if((byt&(1<<(i&7)))!=0) booleans[i]=true;
		}
		return booleans;
	}*/

	//7 bit encoding (special case for negative numbers)
	public int ReadLength(){
		var result=0;
		var push=0;
		while(true){
			var read=Read();
			if(read<0) throw new EndOfStreamException();

			if(read==0) return push==0?0:~result;
			if((read&0x80)==0){
				result|=read<<push;
				return result;
			}
			result|=(read&0x7f)<<push;
			push+=7;
		}
	}

	public Guid ReadGuid(){
		ReadFully(_readBuffer,0,16);
		return new Guid(_readBuffer);
	}

	public byte[]? ReadByteArray(){
		var i=ReadLength();
		if(i==-1) return null;
		var bytes=new byte[i];
		ReadFully(bytes);
		return bytes;
	}

	public T[]? ReadArray<T>(Func<T> read){
		var i=ReadLength();
		if(i==-1) return null;
		var array=new T[i];
		for(var j=0;j<i;j++) array[j]=read();
		return array;
	}

	public T[]? ReadArray<T,TArg>(Func<TArg,T> read,TArg arg){
		var i=ReadLength();
		if(i==-1) return null;
		var array=new T[i];
		for(var j=0;j<i;j++) array[j]=read(arg);
		return array;
	}

	public DataInputBuff? ReadDataInput(){
		var length=ReadLength();
		if(length==-1) return null;
		if(this is DataInputBuff buff){
			var (b,off,len)=buff.GetBufferOffsetAndLength();
			if(len<length) throw new EndOfStreamException();
			Skip(length);
			return new DataInputBuff(b,off,length);
		}
		return new DataInputBuff(ReadFully(length));
	}
}