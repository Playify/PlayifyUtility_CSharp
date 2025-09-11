using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Utils;

[PublicAPI]
public class FileComparerWindowsExplorer:IComparer<string>{

	private static FileComparerWindowsExplorer? _instance;
	public static FileComparerWindowsExplorer Instance=>_instance??=new FileComparerWindowsExplorer();
	private FileComparerWindowsExplorer(){}


	public int Compare(string? x,string? y){
		if(ReferenceEquals(x,y)) return 0;
		if(x is null) return -1;
		if(y is null) return 1;

		return StrCmpLogicalW(x,y);
	}


	[DllImport("shlwapi.dll",CharSet=CharSet.Unicode)]
	private static extern int StrCmpLogicalW(string psz1,string psz2);
}