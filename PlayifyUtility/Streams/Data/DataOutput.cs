using System.Text;
using JetBrains.Annotations;

namespace PlayifyUtility.Streams.Data;

[PublicAPI]
public class DataOutput{
	private readonly byte[] _writeBuffer=new byte[8];
	protected readonly Stream OutputStream;


	public DataOutput(Stream outputStream)=>OutputStream=outputStream;

	public void Write(byte[] b)=>Write(b,0,b.Length);

	public virtual void Write(byte[] b,int off,int len)=>OutputStream.Write(b,off,len);

	public void Write(DataInputBuff data,bool useUpInputBuffer=true){
		var (b,off,len)=data.GetBufferOffsetAndLength();
		Write(b,off,len);
		if(useUpInputBuffer) data.Skip(len);
	}

	public virtual void Flush()=>OutputStream.Flush();

	public virtual Task FlushAsync()=>OutputStream.FlushAsync();

	public virtual void Close()=>OutputStream.Close();

	public virtual void Write(int b)=>OutputStream.WriteByte((byte) b);

	public void WriteBoolean(bool v)=>Write(v?1:0);

	public void WriteByte(byte v)=>Write(v);

	public void WriteShort(short v){
		Write(v>> 8);
		Write(v);
	}

	public void WriteShort(ushort v){
		Write(v>> 8);
		Write(v);
	}

	public void WriteChar(char v){
		Write(v>> 8);
		Write(v);
	}

	public void WriteInt(int v){
		Write((int) ((uint) v>> 24));
		Write((int) ((uint) v>> 16));
		Write((int) ((uint) v>> 8));
		Write((int) (uint) v);
	}

	public void WriteUInt(uint v){
		Write((int) (v>> 24));
		Write((int) (v>> 16));
		Write((int) (v>> 8));
		Write((int) v);
	}

	public void WriteLong(long v){
		_writeBuffer[0]=(byte) (v>> 56);
		_writeBuffer[1]=(byte) (v>> 48);
		_writeBuffer[2]=(byte) (v>> 40);
		_writeBuffer[3]=(byte) (v>> 32);
		_writeBuffer[4]=(byte) (v>> 24);
		_writeBuffer[5]=(byte) (v>> 16);
		_writeBuffer[6]=(byte) (v>> 8);
		_writeBuffer[7]=(byte) v;
		Write(_writeBuffer,0,8);
	}

	public void WriteLong(ulong v){
		_writeBuffer[0]=(byte) (v>> 56);
		_writeBuffer[1]=(byte) (v>> 48);
		_writeBuffer[2]=(byte) (v>> 40);
		_writeBuffer[3]=(byte) (v>> 32);
		_writeBuffer[4]=(byte) (v>> 24);
		_writeBuffer[5]=(byte) (v>> 16);
		_writeBuffer[6]=(byte) (v>> 8);
		_writeBuffer[7]=(byte) v;
		Write(_writeBuffer,0,8);
	}

	public void WriteFloat(float v){
		var bytes=BitConverter.GetBytes(v);
		if(BitConverter.IsLittleEndian) Array.Reverse(bytes);
		Write(bytes);
	}

	public void WriteDouble(double v)=>WriteLong(BitConverter.DoubleToInt64Bits(v));

	public void WriteString(string? str){
		if(str==null){
			WriteLength(-1);
			return;
		}
		var bytes=Encoding.UTF8.GetBytes(str);
		WriteLength(bytes.Length);
		Write(bytes);
	}

	/*public void WriteBooleans(params bool[] b){
		if(b.Length==0){
			return;
		}
		var byt=b[0]?1:0;
		for(var i=1;i<b.Length;i++){
			if(b[i]) byt|=1<<(i&7);
			if((i&7)==0){
				Write(byt);
				byt=0;
			}
		}
		if((b.Length&7)!=0){
			Write(byt);
		}
	}*/

	//7 bit encoding (special case for negatives)
	public void WriteLength(long i){
		var u=(ulong) (i<0?~i:i);
		while(u>=0x80){
			Write((int) (u|0x80));
			u>>= 7;
		}
		if(i<0){
			Write((int) (u|0x80));
			Write(0);
		} else Write((int) u);
	}

	public void WriteGuid(Guid guid)=>Write(guid.ToByteArray());

	public void WriteByteArray(byte[]? bytes){
		if(bytes==null) WriteLength(-1);
		else{
			WriteLength(bytes.Length);
			Write(bytes);
		}
	}

	public void WriteArray<T>(ICollection<T>? array,Action<T> write){
		if(array==null) WriteLength(-1);
		else{
			WriteLength(array.Count);
			foreach(var v in array) write(v);
		}
	}

	public void WriteArray<T,TArg>(ICollection<T>? array,Action<T,TArg> write,TArg arg){
		if(array==null) WriteLength(-1);
		else{
			WriteLength(array.Count);
			foreach(var v in array) write(v,arg);
		}
	}

	public void WriteDataInput(DataInputBuff? data,bool useUpInputBuffer=true){
		if(data==null) WriteLength(-1);
		else{
			WriteLength(data.Available());
			Write(data,useUpInputBuffer);
		}
	}
}