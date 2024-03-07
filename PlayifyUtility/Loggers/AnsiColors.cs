using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using PlayifyUtility.Utils;

namespace PlayifyUtility.Loggers;

[PublicAPI]
public enum AnsiColor:byte{
	Black=0,
	Red=1,
	Green=2,
	Yellow=3,
	Blue=4,
	Magenta=5,
	Cyan=6,
	White=7,
	Default=9,
}

[Flags]
public enum AnsiStyle:byte{
	Background=1,
	Bright=2,
	Bold=4,
	Reset=128,
}

public static class AnsiColors{
	[DllImport("kernel32.dll")]
	private static extern bool GetConsoleMode(IntPtr hConsoleHandle,out uint lpMode);

	[DllImport("kernel32.dll")]
	private static extern bool SetConsoleMode(IntPtr hConsoleHandle,uint dwMode);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetStdHandle(int nStdHandle);

	static AnsiColors(){//Enable Ansi output in console
		if(!PlatformUtils.IsWindows()) return;
		var iStdOut=GetStdHandle(-11);//STD_OUTPUT_HANDLE
		if(!GetConsoleMode(iStdOut,out var outConsoleMode)) return;
		outConsoleMode|=0x0004|0x0008;//ENABLE_VIRTUAL_TERMINAL_PROCESSING|DISABLE_NEWLINE_AUTO_RETURN
		SetConsoleMode(iStdOut,outConsoleMode);
	}

	public static string Get(this AnsiColor color,AnsiStyle style=default){
		// https://gist.github.com/fnky/458719343aabd01cfb17a3a4f7296797

		var str=new StringBuilder("\u001b[");

		if((style&AnsiStyle.Reset)!=0) str.Append("0m\u001b[");//Insert reset at the beginning

		if((style&AnsiStyle.Bold)!=0) str.Append("1;");
		str.Append(((style&AnsiStyle.Bright)!=0?9:3)+((style&AnsiStyle.Background)!=0?1:0));
		str.Append((int)color);
		str.Append('m');
		return str.ToString();
	}

	public const string Reset="\u001b[0;0m";
}