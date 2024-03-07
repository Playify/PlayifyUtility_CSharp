using System.Runtime.InteropServices;

namespace PlayifyUtility.Windows.Win;

public static partial class WinConsole{
	private const uint GenericWrite=0x40000000;
	private const uint GenericRead=0x80000000;
	private const uint FileShareWrite=0x2;
	private const uint FileShareRead=0x1;
	private const uint OpenExisting=0x3;


	[DllImport("kernel32")]
	private static extern bool AllocConsole();

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetConsoleWindow();


	[DllImport("kernel32.dll",SetLastError=true,CharSet=CharSet.Unicode)]
	private static extern IntPtr CreateFile(string fileName,uint access,uint shareMode,uint securityAttribs,uint creationDisposition,uint flagsAndAttributes,uint template);


	[DllImport("Kernel32.dll",SetLastError=true,CharSet=CharSet.Auto)]
	private static extern bool SetStdHandle(int nStdHandle,IntPtr hHandle);

	[DllImport("kernel32.dll",SetLastError=true)]
	private static extern IntPtr GetStdHandle(int nStdHandle);


	[DllImport("kernel32.dll")]
	private static extern bool GetConsoleMode(IntPtr hConsoleHandle,out uint lpMode);

	[DllImport("kernel32.dll")]
	private static extern bool SetConsoleMode(IntPtr hConsoleHandle,uint dwMode);
}