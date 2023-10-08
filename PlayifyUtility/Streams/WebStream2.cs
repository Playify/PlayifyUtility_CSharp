using System.Text;
using PlayifyUtility.Utils;

namespace PlayifyUtility.Streams;

public class WebStream2:IWebStream{
	private class ByteBuffer{
		public byte[] Bytes;
		public int Start;
		public int End;

		public ByteBuffer(byte[] bytes,int start,int end){
			Bytes=bytes;
			Start=start;
			End=end;
		}
	}

	private readonly Stream _stream;
	private readonly LinkedList<ByteBuffer> _list=new();

	public WebStream2(Stream stream){
		_stream=stream;
	}

	public async Task SkipAsync(int count){
		while(_list.First.NotNull(out var first)){
			var link=first.ValueRef;
			if(link.End-link.Start>=count){
				count-=link.End-link.Start;
				_list.RemoveFirst();
			} else{
				link.Start+=count;
				return;
			}
		}
		var buffer=new byte[Math.Min(1024,count)];
		while(count>0){
			var bytesRead=await _stream.ReadAsync(buffer,0,Math.Min(count,buffer.Length));
			if(bytesRead==0) throw new EndOfStreamException();// End of stream reached

			count-=bytesRead;
		}
	}

	private static readonly byte[] NewLine=Encoding.ASCII.GetBytes("\r\n");
	public async Task<string> ReadLineAsync()=>Encoding.UTF8.GetString(await ReadUntilAsync(NewLine));
	public Task ReadFullyAsync(byte[] bytes)=>ReadFullyAsync(bytes,0,bytes.Length);

	public async Task ReadFullyAsync(byte[] bytes,int offset,int length){
		var n=0;
		while(n<length){
			var count=await ReadAsync(bytes,offset+n,length-n);
			if(count==0) throw new EndOfStreamException();
			n+=count;
		}
	}
	public async Task<int> ReadAsync(byte[] buffer,int offset,int length){
		var result=0;
		while(length!=0)
			if(_list.First.NotNull(out var first)){
				var link=first.Value;
				var available=link.End-link.Start;
				if(available>=length){
					Array.Copy(link.Bytes,link.Start,buffer,offset,available);
					_list.RemoveFirst();
					offset+=available;
					length-=available;
					result+=available;
				} else{
					Array.Copy(link.Bytes,link.Start,buffer,offset,length);
					link.Start+=length;
					result+=length;
					return result;
				}
			} else if(result!=0) return result;
			else return await _stream.ReadAsync(buffer,offset,length);
		return result;
	}
	public async Task<byte[]> ReadUntilAsync(byte[] delimiter,bool withDelimiter=false){
		var candidates=new List<int>();
		var node=_list.First;
		while(true){
			if(node==null){//Read new bytes as needed
				var buff=new byte[1024];
				var len=await _stream.ReadAsync(buff,0,buff.Length);
				if(len==0) throw new EndOfStreamException();
				_list.AddLast(new ByteBuffer(buff,0,len));
				node=_list.Last!;
			}
			var buffer=node.Value;
			
			for(var i=buffer.Start;i<buffer.End;i++){
				var b=buffer.Bytes[i];
				if(b==delimiter[0])candidates.Add(0);//new candidate found
				
				for(var j=0;j<candidates.Count;j++){
					var candidate=candidates[j];
					if(b!=delimiter[candidate]){//not matched fully
						candidates.Remove(j);
						j--;
						continue;
					}
					candidate++;
					if(candidate!=delimiter.Length){//store incremented index
						candidates[j]=candidate;
						continue;
					}
					//Successfully found
					if(withDelimiter){
						var result=new byte[i+1];
						await ReadFullyAsync(result);
						return result;
					} else{
						var result=new byte[i+1-delimiter.Length];
						await ReadFullyAsync(result);
						await SkipAsync(delimiter.Length);
						return result;
					}
				}
			}

			node=node.Next;
		}
	}
}