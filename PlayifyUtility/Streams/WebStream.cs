using System.Text;

namespace PlayifyUtility.Streams;

public class WebStream:IWebStream{
	public static readonly byte[] NewLine=Encoding.ASCII.GetBytes("\r\n");
	private readonly byte[] _buffer=new byte[1024];
	private readonly LocalStorage _local=new();
	private readonly Stream _stream;

	public WebStream(Stream stream)=>_stream=stream;

	public async Task<string> ReadLineAsync(){
		var bytes=await ReadUntilAsync(NewLine);
		return Encoding.UTF8.GetString(bytes);
	}

	public async Task ReadFullyAsync(byte[] key,int offset,int length){
		while(length!=0)
			if(_local.List.Count>0){
				var first=_local.List.First.Value;
				if(first.Length<=length){
					Array.Copy(first.Bytes,0,key,offset,first.Length);
					length-=first.Length;
					offset+=first.Length;
					_local.List.RemoveFirst();
				} else{
					Array.Copy(first.Bytes,0,key,offset,length);
					_local.Remove(length);
					return;
				}
			} else{
				var read=await _stream.ReadAsync(key,offset,length);
				if(read==0) throw new EndOfStreamException();
				offset+=read;
				length-=read;
			}
	}

	public Task ReadFullyAsync(byte[] bytes)=>ReadFullyAsync(bytes,0,bytes.Length);

	public async Task<int> ReadAsync(byte[] key,int offset,int length){
		var originalLength=length;
		while(length!=0)
			if(_local.List.Count>0){
				var first=_local.List.First.Value;
				if(first.Length<=length){
					Array.Copy(first.Bytes,0,key,offset,first.Length);
					offset+=first.Length;
					length-=first.Length;
					_local.List.RemoveFirst();
				} else{
					Array.Copy(first.Bytes,0,key,offset,length);
					_local.Remove(length);
					return originalLength-length;
				}
			} else if(originalLength==length) return await _stream.ReadAsync(key,offset,length);
			else return originalLength-length;
		return 0;
	}

	public async Task<byte[]> ReadUntilAsync(byte[] delimiter,bool withDelimiter=false){
		var ret=_local.Check(delimiter,withDelimiter);
		if(ret!=null) return ret;
		int i;
		while((i=await _stream.ReadAsync(_buffer,0,_buffer.Length))!=0){
			ret=_local.Append(_buffer,i,delimiter,withDelimiter);
			if(ret!=null) return ret;
		}
		_stream.Close();
		throw new EndOfStreamException();
	}

	public Task SkipAsync(int length){
		Skip(length);
		return Task.CompletedTask;
	}

	public string ReadLine(){
		var bytes=ReadUntil(NewLine);
		return Encoding.UTF8.GetString(bytes);
	}

	public void ReadFully(byte[] b)=>ReadFully(b,0,b.Length);

	public byte Read(){
		if(_local.List.Count>0){
			var first=_local.List.First.Value;
			var b=first.Bytes[0];
			first.Length--;
			_local.Remove(1);
			if(first.Length==0) _local.List.RemoveFirst();
			return b;
		}
		var read=_stream.ReadByte();
		if(read==-1) throw new EndOfStreamException();
		return (byte)read;
	}

	public void ReadFully(byte[] key,int offset,int length){
		while(length!=0)
			if(_local.List.Count>0){
				var first=_local.List.First.Value;
				if(first.Length<=length){
					Array.Copy(first.Bytes,0,key,offset,first.Length);
					length-=first.Length;
					offset+=first.Length;
					_local.List.RemoveFirst();
				} else{
					Array.Copy(first.Bytes,0,key,offset,length);
					_local.Remove(length);
					return;
				}
			} else{
				var read=_stream.Read(key,offset,length);
				if(read==0) throw new EndOfStreamException();
				offset+=read;
				length-=read;
			}
	}

	public int Read(byte[] key,int offset,int length){
		var originalLength=length;
		while(length!=0)
			if(_local.List.Count>0){
				var first=_local.List.First.Value;
				if(first.Length<=length){
					Array.Copy(first.Bytes,0,key,offset,first.Length);
					offset+=first.Length;
					length-=first.Length;
					_local.List.RemoveFirst();
				} else{
					Array.Copy(first.Bytes,0,key,offset,length);
					_local.Remove(length);
					return originalLength-length;
				}
			} else if(originalLength==length) return _stream.Read(key,offset,length);
			else return originalLength-length;
		return 0;
	}

	public byte[] ReadUntil(string until,bool withDelimiter=false)=>ReadUntil(Encoding.ASCII.GetBytes(until),withDelimiter);

	public byte[] ReadUntil(byte[] bytes,bool withDelimiter=false){
		var ret=_local.Check(bytes,withDelimiter);
		if(ret!=null) return ret;
		int i;
		while((i=_stream.Read(_buffer,0,_buffer.Length))!=0){
			ret=_local.Append(_buffer,i,bytes,withDelimiter);
			if(ret!=null) return ret;
		}
		_stream.Close();
		throw new EndOfStreamException();
	}

	public Task<byte[]> ReadUntilAsync(string until,bool withDelimiter=false)=>ReadUntilAsync(Encoding.ASCII.GetBytes(until),withDelimiter);

	public void Skip(int limit)=>_local.Remove(limit);


	private sealed class ByteStorage{
		public readonly byte[] Bytes=new byte[1024];
		public int Length;

		public override string ToString()=>Encoding.UTF8.GetString(Bytes,0,Length);

		internal int? Calculate(byte[] check,List<int> values){
			for(var i=0;i<Length;i++){
				var b=Bytes[i];
				if(b==check[0]) values.Add(0);

				for(var j=0;j<values.Count;j++){
					var value=values[j];
					if(b==check[value]){
						value++;
						if(value==check.Length) return i-value+1;
						values[j]=value;
					} else{
						values.Remove(j);
						j--;
					}
				}
			}
			return null;
		}
	}

	private sealed class LocalStorage{
		private readonly List<int> _value=new();
		public readonly LinkedList<ByteStorage> List=new();


		public byte[]? Check(byte[] bytes,bool withDelimiter){
			_value.Clear();
			foreach(var storage in List){
				var i=storage.Calculate(bytes,_value);
				if(i!=null) return Until2(bytes,storage,i.Value,withDelimiter);
			}
			return null;
		}

		private byte[] Until2(byte[] bytes,ByteStorage storage,int i,bool withDelimiter){
			if(withDelimiter){
				var u=Until(storage,i,bytes.Length);
				Array.Copy(bytes,0,u,u.Length-bytes.Length,bytes.Length);
				Remove(bytes.Length);
				return u;
			} else{
				var u=Until(storage,i,0);
				Remove(bytes.Length);
				return u;
			}
		}

		public byte[]? Append(byte[] buffer,int length,byte[] bytes,bool withDelimiter){
			if(length==0) return null;
			var s=new ByteStorage();
			Array.Copy(buffer,0,s.Bytes,0,length);
			s.Length=length;
			var calculate=s.Calculate(bytes,_value);
			List.AddLast(s);
			if(calculate!=null) return Until2(bytes,s,calculate.Value,withDelimiter);
			return null;
		}

		private byte[] Until(ByteStorage s,int posInStorage,int additionalSize){
			while(posInStorage<0){
				s=List.Find(s).Previous.Value;
				posInStorage+=s.Length;
			}

			var length=posInStorage;
			foreach(var storage in List){
				if(storage==s) break;
				length+=storage.Length;
			}
			var bytes=new byte[length+additionalSize];

			length=0;
			foreach(var storage in List){
				if(storage==s) break;
				Array.Copy(storage.Bytes,0,bytes,length,storage.Length);

				length+=storage.Length;
			}
			Array.Copy(s.Bytes,0,bytes,length,posInStorage);
			Array.Copy(s.Bytes,posInStorage,s.Bytes,0,s.Length-=posInStorage);
			while(List.First.Value!=s) List.RemoveFirst();
			if(s.Length==0) List.RemoveFirst();
			return bytes;
		}

		internal void Remove(int count){
			while(count>List.First.Value.Length){
				count-=List.First.Value.Length;
				List.RemoveFirst();
			}
			var s=List.First.Value;
			Array.Copy(s.Bytes,count,s.Bytes,0,s.Length-=count);
			if(s.Length==0) List.RemoveFirst();
		}

		public override string ToString(){
			var builder=new StringBuilder();
			foreach(var storage in List) builder.Append(storage);
			return builder.ToString();
		}
	}
}