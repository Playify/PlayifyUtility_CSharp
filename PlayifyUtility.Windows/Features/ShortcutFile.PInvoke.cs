using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace PlayifyUtility.Windows.Features;

public partial class ShortcutFile{
	// ReSharper disable once SuspiciousTypeConversion.Global
	private IShellLinkW _link=(IShellLinkW)new CShellLink();

	// ReSharper disable once SuspiciousTypeConversion.Global
	private IPersistFile Persist=>(IPersistFile)_link;


	public void Dispose(){
		if(_link==null!) return;
		Marshal.ReleaseComObject(_link);
		_link=null!;
	}

	private static void RunChecked(int err){
		if(err!=0) throw new Win32Exception(err);
	}

	[ComImport]
	[Guid("00021401-0000-0000-C000-000000000046")]
	private class CShellLink{
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("000214F9-0000-0000-C000-000000000046")]
	private interface IShellLinkW{
		int GetPath([Out,MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszFile,int cchMaxPath,IntPtr pfd,uint fFlags);
		int GetIDList(out IntPtr ppidl);
		int SetIDList(IntPtr pidl);
		int GetDescription([Out,MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszName,int cchMaxName);
		int SetDescription([MarshalAs(UnmanagedType.LPWStr)]string pszName);
		int GetWorkingDirectory([Out,MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszDir,int cchMaxPath);
		int SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)]string pszDir);
		int GetArguments([Out,MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszArgs,int cchMaxPath);
		int SetArguments([MarshalAs(UnmanagedType.LPWStr)]string pszArgs);
		int GetHotkey(out short pwHotkey);
		int SetHotkey(short wHotkey);
		int GetShowCmd(out int piShowCmd);
		int SetShowCmd(int iShowCmd);
		int GetIconLocation([Out,MarshalAs(UnmanagedType.LPWStr)]StringBuilder pszIconPath,int cchIconPath,out int piIcon);
		int SetIconLocation([MarshalAs(UnmanagedType.LPWStr)]string pszIconPath,int iIcon);
		int SetRelativePath([MarshalAs(UnmanagedType.LPWStr)]string pszPathRel,uint dwReserved);
		int Resolve(IntPtr hwnd,uint fFlags);
		int SetPath([MarshalAs(UnmanagedType.LPWStr)]string pszFile);
	}

	[ComImport]
	[Guid("0000010b-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IPersistFile{
		int GetClassID(out Guid pClassId);
		int IsDirty();
		int Load([MarshalAs(UnmanagedType.LPWStr)]string pszFileName,uint dwMode);
		int Save([MarshalAs(UnmanagedType.LPWStr)]string pszFileName,[MarshalAs(UnmanagedType.Bool)]bool fRemember);
		int SaveCompleted([MarshalAs(UnmanagedType.LPWStr)]string pszFileName);
		int GetCurFile([MarshalAs(UnmanagedType.LPWStr)]out string ppszFileName);
	}
}