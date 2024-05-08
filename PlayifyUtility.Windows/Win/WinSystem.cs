using System.ComponentModel;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace PlayifyUtility.Windows.Win;

[PublicAPI]
public static partial class WinSystem{
	public static int KeyboardDelay{//Range from 0-3 (0=250ms, 3=1sec)
		get=>SystemParametersInfo(0x0016,0,out var delay,0)
			     ?delay
			     :throw new Exception("Failed to retrieve keyboard repeat delay.");
		set{
			if(!SystemParametersInfo(0x0017,value,0,0)) throw new Exception("Failed to set keyboard repeat delay.");
		}
	}

	public static bool IsSystemShuttingDown=>GetSystemMetrics(0x2000)!=0;

	public static bool DarkMode{
		get{
			try{
				using var key=Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize");
				return ((int?)key?.GetValue("AppsUseLightTheme",1)??1)==0;
			} catch(Exception){
				return false;
			}
		}
	}

	#region Input Idle
	public static DateTime InputIdleSince=>DateTime.Now-InputIdleDuration;
	public static TimeSpan InputIdleDuration{
		get{
			var lastInput=new LastInputInfo();
			lastInput.CbSize=(uint)Marshal.SizeOf(lastInput);
			if(!GetLastInputInfo(ref lastInput)) throw new Win32Exception();
			return TimeSpan.FromMilliseconds((uint)Environment.TickCount-lastInput.DwTime);
		}
	}
	#endregion

	#region Open Files & Folder
	public static void OpenFile(string path){
		var ret=ShellExecute(IntPtr.Zero,"open",path,null,null,1);
		if(ret<32) throw new Win32Exception();
	}

	public static void OpenExplorerAndSelect(string file)=>OpenFolderAndSelect(Path.GetDirectoryName(file)??file,file);
	public static void OpenFolderAndSelect(string folder,params string[] select)=>OpenFolderAndSelect(folder,(IEnumerable<string>)select);

	public static void OpenFolderAndSelect(string folder,IEnumerable<string> select){
		SHParseDisplayName(folder,IntPtr.Zero,out var nativeFolder,0,out _);
		if(nativeFolder==IntPtr.Zero) throw new DirectoryNotFoundException("Error opening Folder \""+folder+"\"");

		var files=select
		          .Select(file=>{
			          SHParseDisplayName(Path.Combine(folder,file),IntPtr.Zero,out var nativeFile,0,out _);
			          return nativeFile;
		          })
		          .Where(file=>file!=IntPtr.Zero)
		          .DefaultIfEmpty(IntPtr.Zero)
		          .ToArray();

		var ret=SHOpenFolderAndSelectItems(nativeFolder,(uint)files.Length,files,0);

		Marshal.FreeCoTaskMem(nativeFolder);
		foreach(var ptr in files)
			if(ptr!=IntPtr.Zero)
				Marshal.FreeCoTaskMem(ptr);

		if(ret!=0) throw new Win32Exception(ret);
	}
	#endregion

}