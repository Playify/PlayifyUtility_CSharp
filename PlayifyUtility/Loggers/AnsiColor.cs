using System.Runtime.InteropServices;
using System.Text;
using PlayifyUtility.Utils;

namespace PlayifyUtility.Loggers;

[Flags]
public enum AnsiStyle:byte{
	Background=1,
	Bold=4,
}

public static class AnsiColor{
	public const string Reset="\u001b[0;0m";

	[DllImport("kernel32.dll")]
	private static extern bool GetConsoleMode(IntPtr hConsoleHandle,out uint lpMode);

	[DllImport("kernel32.dll")]
	private static extern bool SetConsoleMode(IntPtr hConsoleHandle,uint dwMode);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetStdHandle(int nStdHandle);

	public static bool IsSupported=>!PlatformUtils.IsWindows()||PlatformUtils.GetWindowsVersion().Major>=10;//Either Linux or Win10 upwards. Win7 is not supported

	static AnsiColor(){//Enable Ansi output in console, if needed
		if(!PlatformUtils.IsWindows()) return;
		if(!IsSupported) return;

		EnableAnsi(GetStdHandle(-11));//STD_OUTPUT_HANDLE
		EnableAnsi(GetStdHandle(-12));//STD_ERROR_HANDLE
	}

	//Same as in WinConsole:
	private static bool EnableAnsi(IntPtr stream)=>
		GetConsoleMode(stream,out var mode)&&
		SetConsoleMode(stream,mode|1|4);//ENABLE_PROCESSED_OUTPUT|ENABLE_VIRTUAL_TERMINAL_PROCESSING


	public static string Ansi(this ConsoleColor color,AnsiStyle style=default){
		// https://gist.github.com/fnky/458719343aabd01cfb17a3a4f7296797

		var str=new StringBuilder("\u001b[");
		if((style&AnsiStyle.Bold)!=0) str.Append("1;");
		str.Append(AnsiNumber(color)+((style&AnsiStyle.Background)!=0?10:0));
		str.Append('m');
		return str.ToString();
	}


	public static ConsoleColor Bright(this ConsoleColor color)=>color.SetBright(true);
	public static ConsoleColor Dark(this ConsoleColor color)=>color.SetBright(false);

	private static ConsoleColor SetBright(this ConsoleColor color,bool bright)=>color switch{
		ConsoleColor.DarkGray=>bright?ConsoleColor.DarkGray:ConsoleColor.Black,
		ConsoleColor.Blue=>bright?ConsoleColor.Blue:ConsoleColor.DarkBlue,
		ConsoleColor.Green=>bright?ConsoleColor.Green:ConsoleColor.DarkGreen,
		ConsoleColor.Cyan=>bright?ConsoleColor.Cyan:ConsoleColor.DarkCyan,
		ConsoleColor.Red=>bright?ConsoleColor.Red:ConsoleColor.DarkRed,
		ConsoleColor.Magenta=>bright?ConsoleColor.Magenta:ConsoleColor.DarkMagenta,
		ConsoleColor.Yellow=>bright?ConsoleColor.Yellow:ConsoleColor.DarkYellow,
		ConsoleColor.White=>bright?ConsoleColor.White:ConsoleColor.Gray,

		ConsoleColor.Black=>bright?ConsoleColor.DarkGray:ConsoleColor.Black,
		ConsoleColor.DarkBlue=>bright?ConsoleColor.Blue:ConsoleColor.DarkBlue,
		ConsoleColor.DarkGreen=>bright?ConsoleColor.Green:ConsoleColor.DarkGreen,
		ConsoleColor.DarkCyan=>bright?ConsoleColor.Cyan:ConsoleColor.DarkCyan,
		ConsoleColor.DarkRed=>bright?ConsoleColor.Red:ConsoleColor.DarkRed,
		ConsoleColor.DarkMagenta=>bright?ConsoleColor.Magenta:ConsoleColor.DarkMagenta,
		ConsoleColor.DarkYellow=>bright?ConsoleColor.Yellow:ConsoleColor.DarkYellow,
		ConsoleColor.Gray=>bright?ConsoleColor.White:ConsoleColor.Gray,
		_=>throw new ArgumentOutOfRangeException(nameof(color),color,null),
	};

	private static int AnsiNumber(this ConsoleColor color)=>color switch{
		ConsoleColor.Black=>30,
		ConsoleColor.DarkBlue=>34,
		ConsoleColor.DarkGreen=>32,
		ConsoleColor.DarkCyan=>36,
		ConsoleColor.DarkRed=>31,
		ConsoleColor.DarkMagenta=>35,
		ConsoleColor.DarkYellow=>33,
		ConsoleColor.Gray=>37,

		ConsoleColor.DarkGray=>90,
		ConsoleColor.Blue=>94,
		ConsoleColor.Green=>92,
		ConsoleColor.Cyan=>96,
		ConsoleColor.Red=>91,
		ConsoleColor.Magenta=>95,
		ConsoleColor.Yellow=>93,
		ConsoleColor.White=>97,
		_=>throw new ArgumentOutOfRangeException(nameof(color),color,null),
	};
}