using System.Text;

namespace PlayifyUtility.Streams;

public class WebStream3(Stream stream):IWebStream{
	private readonly byte[] _buffered=new byte[8192];
	private int _start;
	private int _end;

	public async Task SkipAsync(int count){
		while(count>0){
			if(_start==_end){
				var bytesRead=await stream.ReadAsync(_buffered,0,Math.Min(_buffered.Length,count));
				if(bytesRead==0) throw new EndOfStreamException();// End of stream reached
				count-=bytesRead;
			} else{
				var available=_end-_start;
				if(available<=count){
					_start=_end;
					count-=available;
				} else{
					_start+=count;
					return;
				}
			}
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

	public Task<int> ReadAsync(byte[] buffer,int offset,int length){
		var result=0;
		while(length!=0)
			if(_start!=_end){
				var available=_end-_start;
				if(available<=length){
					Array.Copy(_buffered,_start,buffer,offset,available);
					_start=_end;
					offset+=available;
					length-=available;
					result+=available;
				} else{
					Array.Copy(_buffered,_start,buffer,offset,length);
					_start+=length;
					result+=length;
					return Task.FromResult(result);
				}
			} else if(result!=0) return Task.FromResult(result);
			else return stream.ReadAsync(buffer,offset,length);
		return Task.FromResult(result);
	}


	public async Task<byte[]> ReadUntilAsync(byte[] delimiter,bool withDelimiter=false){
		// KMP failure table
		var failure=new int[delimiter.Length];
		for(int i=1,j=0;i<delimiter.Length;i++){
			while(j>0&&delimiter[i]!=delimiter[j]) j=failure[j-1];
			if(delimiter[i]==delimiter[j]) j++;
			failure[i]=j;
		}
		var matchIndex=0;
		var result=new List<byte>();

		while(true){
			if(_start==_end){
				_start=0;
				_end=await stream.ReadAsync(_buffered,0,_buffered.Length);
				if(_end==0) throw new EndOfStreamException();
			}
			var b=_buffered[_start++];

			result.Add(b);

			// KMP matching
			while(matchIndex>0&&b!=delimiter[matchIndex]) matchIndex=failure[matchIndex-1];
			if(b==delimiter[matchIndex]) matchIndex++;

			if(matchIndex!=delimiter.Length) continue;

			if(!withDelimiter) result.RemoveRange(result.Count-delimiter.Length,delimiter.Length);
			return result.ToArray();
		}
	}
}