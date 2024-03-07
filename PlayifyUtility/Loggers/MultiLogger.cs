using System.Text;
using JetBrains.Annotations;
using PlayifyUtility.Utils.Extensions;
#if !NETFRAMEWORK
using PlayifyUtility.Utils;
#endif

namespace PlayifyUtility.Loggers;

[PublicAPI]
public class MultiLogger:Logger{
	private readonly Logger[] _loggers;
	public MultiLogger(IEnumerable<Logger> loggers):base(loggers.ToArray().Push(out var arr)[0])=>_loggers=arr;
	public MultiLogger(params Logger[] loggers):base(loggers.ToArray().Push(out var arr)[0])=>_loggers=arr;

	public override Encoding Encoding=>_loggers.First().Encoding;
	public override void Close()=>_loggers.ForEach(writer=>writer.Close());
	protected override void Dispose(bool disposing)=>_loggers.ForEach(writer=>writer.Dispose());
#if !NETFRAMEWORK
	public override ValueTask DisposeAsync()=>TaskUtils.WhenAll(_loggers.Select(writer=>writer.DisposeAsync()));
#endif
	public override void Flush()=>_loggers.ForEach(writer=>writer.Flush());
	public override Task FlushAsync()=>Task.WhenAll(_loggers.Select(writer=>writer.FlushAsync()));
	public override void Write(char value)=>_loggers.ForEach(writer=>writer.Write(value));
	public override void Write(char[] buffer,int index,int count)=>_loggers.ForEach(writer=>writer.Write(buffer,index,count));
	public override void Write(string? value)=>_loggers.ForEach(writer=>writer.Write(value));
	public override Task WriteAsync(char value)=>Task.WhenAll(_loggers.Select(writer=>writer.WriteAsync(value)));
	public override Task WriteAsync(char[] buffer,int index,int count)=>Task.WhenAll(_loggers.Select(writer=>writer.WriteAsync(buffer,index,count)));
	public override Task WriteAsync(string? value)=>Task.WhenAll(_loggers.Select(writer=>writer.WriteAsync(value)));
}