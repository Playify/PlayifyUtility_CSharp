using JetBrains.Annotations;

namespace PlayifyUtility.Loggers;

[PublicAPI]
public class FileLogger:Logger{
	public readonly string LogPath;
	public FileLogger(string path):base(new StreamWriter(path){AutoFlush=true})=>LogPath=path;

	public static FileLogger Create(string appName,string logName){
		var logDir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),appName,"Logs");
		Directory.CreateDirectory(logDir);
		var logFile=Path.Combine(logDir,logName+".log");
		return new FileLogger(logFile);
	}

	public static FileLogger CreateWithDate(string appName,string logName)=>Create(appName,$"{DateTime.Now:yyyyMMdd_HHmmss}_{logName}");
}