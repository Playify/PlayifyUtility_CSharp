using System.ComponentModel;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Features;

[PublicAPI]
public static partial class CaseSensitiveDirectory{
	// Read access is NOT required
	public static bool IsDirectoryCaseSensitive(string directory){
		var hFile=CreateFile(directory,0,FileShare.ReadWrite,
		                     IntPtr.Zero,FileMode.Open,
		                     (FileAttributes) 0x02000000,IntPtr.Zero);//0x02000000 = FILE_FLAG_BACKUP_SEMANTIC

		if(hFile==new IntPtr(-1)) throw new Win32Exception();

		try{
			var ioSb=new IoStatusBlock();
			var caseSensitive=new FileCaseSensitiveInformation();
			var status=NtQueryInformationFile(hFile,ref ioSb,ref caseSensitive,
			                                  Marshal.SizeOf<FileCaseSensitiveInformation>(),
			                                  71);//71 = FileCaseSensitiveInformation
			switch(status){
				case NtStatus.Success:return (caseSensitive.Flags&1)!=0;//1 = FILE_CS_FLAG_CASE_SENSITIVE_DIR
				case NtStatus.NotImplemented:
				case NtStatus.NotSupported:
				case NtStatus.InvalidInfoClass:
				case NtStatus.InvalidParameter:
					// Not supported, must be older version of windows.
					// Directory case sensitivity is impossible.
					return false;
				default:throw new Exception($"Unknown NTSTATUS: {(uint) status:X8}!");
			}
		} finally{
			CloseHandle(hFile);
		}
	}


	// Requires elevated privileges
	// FILE_WRITE_ATTRIBUTES access is the only requirement
	public static void SetDirectoryCaseSensitive(string directory,bool enable){
		var hFile=CreateFile(directory,(FileAccess) 0x00000100,FileShare.ReadWrite,//0x100 = FILE_WRITE_ATTRIBUTES
		                     IntPtr.Zero,FileMode.Open,
		                     (FileAttributes) 0x02000000,IntPtr.Zero);//0x02000000 = FILE_FLAG_BACKUP_SEMANTICS

		if(hFile==new IntPtr(-1)) throw new Win32Exception();

		try{
			var ioSb=new IoStatusBlock();
			var caseSensitive=new FileCaseSensitiveInformation();
			if(enable) caseSensitive.Flags|=1;//1 = FILE_CS_FLAG_CASE_SENSITIVE_DIR
			var status=NtSetInformationFile(hFile,ref ioSb,ref caseSensitive,
			                                Marshal.SizeOf<FileCaseSensitiveInformation>(),
			                                71);//71 = FileCaseSensitiveInformation


			switch(status){
				case NtStatus.Success:return;
				case NtStatus.DirectoryNotEmpty:
					throw new IOException($"Directory \"{directory}\" contains matching "+
					                      $"case-insensitive files!");

				case NtStatus.NotImplemented:
				case NtStatus.NotSupported:
				case NtStatus.InvalidInfoClass:
				case NtStatus.InvalidParameter:
					// Not supported, must be older version of windows.
					// Directory case sensitivity is impossible.
					throw new NotSupportedException("This version of Windows does not support directory case sensitivity!");
				default:throw new Exception($"Unknown NTSTATUS: {(uint) status:X8}!");
			}
		} finally{
			CloseHandle(hFile);
		}
	}

	public static bool IsDirectoryCaseSensitivitySupported(){
		if(_isSupported.HasValue) return _isSupported.Value;

		// Make sure the directory exists
		Directory.CreateDirectory(TempDirectory);

		var hFile=CreateFile(TempDirectory,0,FileShare.ReadWrite,
		                     IntPtr.Zero,FileMode.Open,
		                     (FileAttributes) 0x02000000,IntPtr.Zero);//0x02000000 = FILE_FLAG_BACKUP_SEMANTICS

		if(hFile==new IntPtr(-1)) throw new Exception("Failed to open file while checking case sensitivity support!");

		try{
			var ioSb=new IoStatusBlock();
			var caseSensitive=new FileCaseSensitiveInformation();
			// Strangely enough, this doesn't fail on files
			var result=NtQueryInformationFile(hFile,ref ioSb,ref caseSensitive,
			                                  Marshal.SizeOf<FileCaseSensitiveInformation>(),
			                                  71);//71 = FileCaseSensitiveInformation
			switch(result){
				case NtStatus.Success:return (_isSupported=true).Value;
				case NtStatus.NotImplemented:
				case NtStatus.InvalidInfoClass:
				case NtStatus.InvalidParameter:
				case NtStatus.NotSupported:
					// Not supported, must be older version of windows.
					// Directory case sensitivity is impossible.
					return (_isSupported=false).Value;
				default:throw new Exception($"Unknown NTSTATUS {(uint) result:X8} while checking case sensitivity support!");
			}
		} finally{
			CloseHandle(hFile);
			try{
				Directory.Delete(TempDirectory);
			} catch{
				//ignore
			}
		}
	}
}