using JetBrains.Annotations;

namespace PlayifyUtility.Utils;

[PublicAPI]
public class FileUtils{

	[Pure]
	public static string MakeSafeFileName(string s,char replacement='#')
		=>Path.GetInvalidFileNameChars()
		      .Aggregate(s,(curr,c)=>curr.Replace(c,replacement));


	[MustUseReturnValue]
	public static long DirectorySize(string dir)=>DirectorySize(new DirectoryInfo(dir));
	[MustUseReturnValue]
	public static long DirectorySize(DirectoryInfo dir)=>dir.GetFiles().Sum(f=>f.Length)+dir.GetDirectories().Sum(DirectorySize);


	public static void MoveDirectory(string src,string dest,bool overwrite,bool skipIfSimilar=false){
		if(File.Exists(src)){
			if(skipIfSimilar&&File.Exists(dest)
			                &&File.GetLastWriteTime(src)==File.GetLastWriteTime(dest)
			                &&new FileInfo(src).Length==new FileInfo(dest).Length
			  ){
				File.Delete(src);
				return;
			}
#if NETFRAMEWORK
			if(overwrite&&File.Exists(dest)) File.Delete(dest);
			File.Move(src,dest);
#else
			File.Move(src,dest,overwrite);
#endif
			return;
		}
		if(!Directory.Exists(dest)){
			if(!overwrite&&Path.GetPathRoot(src)==Path.GetPathRoot(dest)){
				Directory.Move(src,dest);
				return;
			}
			Directory.CreateDirectory(dest);
		}
		foreach(var child in Directory.EnumerateFileSystemEntries(src))
			MoveDirectory(child,Path.Combine(dest,Path.GetFileName(child)),overwrite,skipIfSimilar);
		Directory.Delete(src);
	}


	[MustUseReturnValue]
	public static string TempPath(string extension)=>Path.Combine(Path.GetTempPath(),Guid.NewGuid()+extension);
}