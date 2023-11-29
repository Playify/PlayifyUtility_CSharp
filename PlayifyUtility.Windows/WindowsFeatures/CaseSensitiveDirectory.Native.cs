using System.Runtime.InteropServices;

namespace PlayifyUtility.Windows.WindowsFeatures;

public static partial class CaseSensitiveDirectory{
	[StructLayout(LayoutKind.Sequential)]
	private struct IoStatusBlock{
		[MarshalAs(UnmanagedType.U4)]
		public uint Status;
		public ulong Information;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct FileCaseSensitiveInformation{
		public uint Flags;
	}

	public enum NtStatus:uint{
		Success=0x00000000,
		NotImplemented=0xC0000002,
		InvalidInfoClass=0xC0000003,
		InvalidParameter=0xC000000D,
		NotSupported=0xC00000BB,
		DirectoryNotEmpty=0xC0000101,
	}

	[DllImport("ntdll.dll")]
	private static extern NtStatus NtQueryInformationFile(IntPtr fileHandle,
	                                                      ref IoStatusBlock ioStatusBlock,
	                                                      ref FileCaseSensitiveInformation fileInformation,
	                                                      int length,
	                                                      int fileInformationClass);

	[DllImport("kernel32.dll",SetLastError=true,CharSet=CharSet.Auto)]
	private static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPTStr)]string filename,
	                                        FileAccess access,
	                                        FileShare share,
	                                        IntPtr securityAttributes,
	                                        FileMode creationDisposition,
	                                        FileAttributes flagsAndAttributes,
	                                        IntPtr templateFile);

	[DllImport("kernel32.dll",SetLastError=true)]
	[return:MarshalAs(UnmanagedType.Bool)]
	private static extern bool CloseHandle(IntPtr hObject);

	[DllImport("ntdll.dll")]
	[return:MarshalAs(UnmanagedType.U4)]
	private static extern NtStatus NtSetInformationFile(IntPtr fileHandle,
	                                                    ref IoStatusBlock ioStatusBlock,
	                                                    ref FileCaseSensitiveInformation fileInformation,
	                                                    int length,
	                                                    int fileInformationClass);


	// Use the same directory so it does not need to be recreated when restarting the program
	private static readonly string TempDirectory=
	Path.Combine(Path.GetTempPath(),"88DEB13C-E516-46C3-97CA-46A8D0DDD8B2");

	private static bool? _isSupported;
}