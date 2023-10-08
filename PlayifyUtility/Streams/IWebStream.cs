namespace PlayifyUtility.Streams;

public interface IWebStream{
	Task SkipAsync(int count);
	Task<string> ReadLineAsync();
	Task ReadFullyAsync(byte[] bytes,int offset,int length);
	Task ReadFullyAsync(byte[] bytes);
	Task<byte[]> ReadUntilAsync(byte[] delimiter,bool withDelimiter=false);
	Task<int> ReadAsync(byte[] buffer,int offset,int length);
}