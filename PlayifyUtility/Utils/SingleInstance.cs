using System.Diagnostics;
using System.IO.Pipes;
using System.Reflection;
using JetBrains.Annotations;
using PlayifyUtility.Loggers;

namespace PlayifyUtility.Utils;

/// <summary>
/// Provides functionality to ensure that only a single instance of the application is running at any given time.
/// </summary>
[PublicAPI]
public static class SingleInstance{
	/// <summary>
	/// Initializes a single instance using the executable path as a key.
	/// </summary>
	/// <param name="logger">Optional logger instance for logging messages.</param>
	public static void ByExe(Logger? logger=null)=>ByName((Assembly.GetEntryAssembly()??Assembly.GetCallingAssembly()).Location,logger);

	/// <summary>
	/// Initializes a single instance using the calling method as a key.
	/// </summary>
	/// <param name="logger">Optional logger instance for logging messages.</param>
	public static void ByCaller(Logger? logger=null){
		var caller=new StackFrame(1,false).GetMethod()??MethodBase.GetCurrentMethod();
		var name=caller?.DeclaringType?.FullName+"@"+caller;

		ByName(name,logger);
	}

	/// <summary>
	/// Initializes a single instance using the provided name as a key.
	/// </summary>
	/// <param name="name">The name used as a key for the single instance.</param>
	/// <param name="logger">Optional logger instance for logging messages.</param>
	public static void ByName(string name,Logger? logger=null){
		try{
			using var pipeClient=new NamedPipeClientStream(".",name+"|SingleInstance|Pipe",PipeDirection.Out);
			pipeClient.Connect(0);
			logger?.Info("Killing previous instance");
		} catch(Exception){
			//ignored
		}

		new Thread(()=>{
			while(true){
				try{
					using var server=new NamedPipeServerStream(name+"|SingleInstance|Pipe",PipeDirection.In,NamedPipeServerStream.MaxAllowedServerInstances);
					server.WaitForConnection();
					logger?.Info("A new instance has started, the current instance will exit now");
					Environment.Exit(0);
					return;
				} catch(Exception){
					Thread.Sleep(100);
				}
			}
		}){
			Name=nameof(SingleInstance),
			IsBackground=true,
		}.Start();
	}
}