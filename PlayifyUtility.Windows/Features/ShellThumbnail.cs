/*
 * --- Shell Thumbs ---
 *
 * https://github.com/rlv-dan/ShellThumbs
 *
 * Use the Windows Shell API to extract thumbnails and icons for files
 *
 * Original from http://stackoverflow.com/questions/21751747/extract-thumbnail-for-any-file-in-windows
 * I took above code and packaged it together with some tweaks and enhancements, plus comments and other findings along the way.
 *
 * Usage:
 *   Bitmap thumbnail_or_icon = ShellThumbs.WindowsThumbnailProvider.GetThumbnail( @"c:\temp\video.avi", 64, 64, ThumbnailOptions.None );
 *   Bitmap thumbnail_or_null = ShellThumbs.WindowsThumbnailProvider.GetThumbnail( @"c:\temp\video.avi", 64, 64, ThumbnailOptions.ThumbnailOnly );
 *   Bitmap icon = ShellThumbs.WindowsThumbnailProvider.GetThumbnail( @"c:\temp\video.avi", 64, 64, ThumbnailOptions.IconOnly );
 *
 * Notes:
 *   Normally, GetThumbnail returns the thumbnail if available, else the file icon.
 *   If using the ThumbnailOnly flag, GetThumbnail will return null for files that does not have a thumbnail handler.
 *
 */

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Features;

[SuppressMessage("ReSharper","IdentifierTypo")]
[SuppressMessage("ReSharper","CommentTypo")]
[PublicAPI]
public static partial class ShellThumbnail{


	public static Bitmap? GetThumbnail(string fileName,out string displayName)=>GetThumbnail(fileName,160,160,ThumbnailOptions.None,out displayName);

	public static Bitmap? GetThumbnail(string fileName,int width,int height,ThumbnailOptions options,out string displayName){
		if(!File.Exists(fileName)&&!Directory.Exists(fileName)) throw new FileNotFoundException();

		Bitmap? clonedBitmap;
		var hBitmap=IntPtr.Zero;

		try{
			hBitmap=GetHBitmap(Path.GetFullPath(fileName),width,height,options,out displayName);

			// Original code returned the bitmap directly:
			//   return GetBitmapFromHBitmap( hBitmap );
			// I'm making a clone first, so I can dispose of the original bitmap.
			// The returned clone should be managed and not need disposing of. (I think...)
			var thumbnail=GetBitmapFromHBitmap(hBitmap);
			//clonedBitmap=thumbnail.Clone() as Bitmap;
			//thumbnail.Dispose();
			clonedBitmap=thumbnail;
		} catch(COMException ex) when(ex.ErrorCode==-2147175936&&options.HasFlag(ThumbnailOptions.ThumbnailOnly)){
			clonedBitmap=null;
			displayName=Path.GetFileName(fileName);
		} finally{
			// delete HBitmap to avoid memory leaks
			DeleteObject(hBitmap);
		}

		return clonedBitmap;
	}


	public static Icon? GetOwnExeIcon(bool large){
		try{
			if(Assembly.GetEntryAssembly()?.Location is{Length: >0} file) return GetExeIcon(file,large);
		} catch(NotSupportedException){
		}
		return null;
	}

	public static Icon? GetExeIcon(string file,bool large){
		var hDummy=new[]{IntPtr.Zero};
		var hIconEx=new[]{IntPtr.Zero};

		try{
			var readIconCount=large?ExtractIconEx(file,0,hIconEx,hDummy,1):ExtractIconEx(file,0,hDummy,hIconEx,1);

			if(readIconCount>0&&hIconEx[0]!=IntPtr.Zero){
				// GET FIRST EXTRACTED ICON
				var extractedIcon=(Icon)Icon.FromHandle(hIconEx[0]).Clone();

				return extractedIcon;
			} else// NO ICONS READ
				return null;
		} catch(Exception ex){
			/* EXTRACT ICON ERROR */

			// BUBBLE UP
			throw new ApplicationException("Could not extract icon",ex);
		} finally{
			// RELEASE RESOURCES
			foreach(var ptr in hIconEx)
				if(ptr!=IntPtr.Zero)
					DestroyIcon(ptr);

			foreach(var ptr in hDummy)
				if(ptr!=IntPtr.Zero)
					DestroyIcon(ptr);
		}
	}
}