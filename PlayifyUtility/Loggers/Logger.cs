using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using PlayifyUtility.Utils.Extensions;

namespace PlayifyUtility.Loggers;

[PublicAPI]
public class Logger:TextWriter{
	private readonly TextWriter _writer;
	private static ThreadLocal<Logger?> _find=new();

	public Logger():this(Console.Out){}
	public Logger(Logger writer)=>_writer=writer;

	public Logger(TextWriter writer){
		_writer=writer;
		if(_writer is Logger) return;
		_find.Value=null;
		_writer.Flush();//if logger is wrapped somehow, find it, remove all wrapping, and use the underlying logger
		_writer=_find.Value??writer;

		typeof(AnsiColor).RunClassConstructor();
		if(this is not AnsiLessLogger&&!AnsiColor.IsSupported)//Ensure the writer is wrapped in ansiless, if it is not supported
			_writer=new AnsiLessLogger(_writer);
	}

	#region TextWriter
	public override Encoding Encoding=>_writer.Encoding;
	public override void Close()=>_writer.Close();
	protected override void Dispose(bool disposing)=>_writer.Dispose();
#if !NETFRAMEWORK
	public override ValueTask DisposeAsync()=>_writer.DisposeAsync();
#endif
	public override void Flush(){
		_find.Value??=this;
		_writer.Flush();
	}

	public override Task FlushAsync()=>_writer.FlushAsync();
	public override void Write(char value)=>_writer.Write(value);
	public override void Write(char[] buffer,int index,int count)=>_writer.Write(buffer,index,count);
	public override void Write(string? value)=>_writer.Write(value);
	public override Task WriteAsync(char value)=>_writer.WriteAsync(value);
	public override Task WriteAsync(char[] buffer,int index,int count)=>_writer.WriteAsync(buffer,index,count);
	public override Task WriteAsync(string? value)=>_writer.WriteAsync(value);
	#endregion

	#region Logging
	public class LogLevel(string? tag,string bracketColor,string tagColor,string? msgColor=null){
		public static readonly LogLevel Log=new(null,ConsoleColor.White);
		public static readonly LogLevel Special=new(null,ConsoleColor.Magenta,ConsoleColor.Magenta.Ansi());
		public static readonly LogLevel Debug=new("Debug",ConsoleColor.Cyan);
		public static readonly LogLevel Info=new("Info",ConsoleColor.Green);
		public static readonly LogLevel Warning=new("Warning",ConsoleColor.Yellow);
		public static readonly LogLevel Error=new("Error",ConsoleColor.Red);
		public static readonly LogLevel Critical=new("Critical",ConsoleColor.Red,ConsoleColor.Red.Ansi());

		public readonly string? Tag=tag;
		public readonly string BracketColor=bracketColor;
		public readonly string TagColor=tagColor;
		public readonly string? MsgColor=msgColor;

		public LogLevel(string? tag,ConsoleColor color,string? msgColor=null):this(
			tag,
			color.Dark().Ansi(AnsiStyle.Bold),
			AnsiColor.Reset+color.Bright().Ansi(),
			msgColor){
		}

	}

	public void Log(LogLevel level,string msg){
		var str=new StringBuilder();

		using (var enumerator=Tags(level).GetEnumerator()){
			if(enumerator.MoveNext()){
				str.Append(level.BracketColor).Append('[');
				var first=true;

				do{
					if(first) first=false;
					else str.Append('|');
					str.Append(level.TagColor).Append(enumerator.Current).Append(level.BracketColor);
				} while(enumerator.MoveNext());

				str.Append(']').Append(AnsiColor.Reset).Append(": ");
			}
		}

		if(level.MsgColor!=null) str.Append(level.MsgColor).Append(msg).Append(AnsiColor.Reset);
		else str.Append(msg);
		str.Append(NewLine);

		Write(str.ToString());
	}

	public virtual IEnumerable<string> Tags(LogLevel level){
		if(_writer is Logger logger)
			foreach(var tag in logger.Tags(level))
				yield return tag;
		else if(level.Tag!=null)
			yield return level.Tag;
	}

	public void Log(string msg)=>Log(LogLevel.Log,msg);
	public void Special(string msg)=>Log(LogLevel.Special,msg);
	public void Debug(string msg)=>Log(LogLevel.Debug,msg);
	public void Info(string msg)=>Log(LogLevel.Info,msg);
	public void Warning(string msg)=>Log(LogLevel.Warning,msg);
	public void Error(string msg)=>Log(LogLevel.Error,msg);
	public void Critical(string msg)=>Log(LogLevel.Critical,msg);
	public void Error(Exception msg)=>Log(LogLevel.Error,msg.ToString());
	public void Critical(Exception msg)=>Log(LogLevel.Critical,msg.ToString());

	/// Only prints if Debugger.IsAttached
	public void Debugging(string msg){
		if(Debugger.IsAttached) Debug(msg);
	}
	#endregion

	#region Factory
	public Logger AlsoLogTo(params Logger[] loggers)=>loggers.Length==0?this:new MultiLogger(loggers.Prepend(this));
	public Logger AlsoLogToFile(string path,bool ansi=true)=>AlsoLogTo(new FileLogger(path).SetAnsi(ansi));

	public Logger AlsoLogToFile(string appName,string logName,bool includeDate,bool ansi=true)=>AlsoLogTo((includeDate
		                                                                                                       ?FileLogger.CreateWithDate(appName,logName)
		                                                                                                       :FileLogger.Create(appName,logName)).SetAnsi(ansi));

	public Logger WithoutColor()=>this is AnsiLessLogger?this:new AnsiLessLogger(this);
	public Logger WithDate(string format="yyyy-MM-dd HH:mm:ss",IFormatProvider? provider=null)=>new DateLogger(this,format,provider);
	public Logger WithName(params string[] names)=>names.Length==0?this:new NamedLogger(this,names);
	public Logger WithName<T>()=>new NamedLogger(this,typeof(T).Name);

	public Logger UseAsConsoleOut(){
		Console.SetOut(this);
		return this;
	}

	private Logger SetAnsi(bool b)=>b?this:WithoutColor();
	#endregion

}