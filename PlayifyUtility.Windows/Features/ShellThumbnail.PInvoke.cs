using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PlayifyUtility.Windows.Features;

public static partial class ShellThumbnail{

	#region Thumbnail
	[Flags]
	public enum ThumbnailOptions// IShellItemImageFactory Flags: https://msdn.microsoft.com/en-us/library/windows/desktop/bb761082%28v=vs.85%29.aspx
	{
		None=0x00,// Shrink the bitmap as necessary to fit, preserving its aspect ratio. Returns thumbnail if available, else icon.
		BiggerSizeOk=
			0x01,// Passed by callers if they want to stretch the returned image themselves. For example, if the caller passes an icon size of 80x80, a 96x96 thumbnail could be returned. This action can be used as a performance optimization if the caller expects that they will need to stretch the image. Note that the Shell implementation of IShellItemImageFactory performs a GDI stretch blit. If the caller wants a higher quality image stretch than provided through that mechanism, they should pass this flag and perform the stretch themselves.
		InMemoryOnly=
			0x02,// Return the item only if it is already in memory. Do not access the disk even if the item is cached. Note that this only returns an already-cached icon and can fall back to a per-class icon if an item has a per-instance icon that has not been cached. Retrieving a thumbnail, even if it is cached, always requires the disk to be accessed, so GetImage should not be called from the UI thread without passing SIIGBF_MEMORYONLY.
		IconOnly=0x04,// Return only the icon, never the thumbnail.
		ThumbnailOnly=0x08,// Return only the thumbnail, never the icon. Note that not all items have thumbnails, so SIIGBF_THUMBNAILONLY will cause the method to fail in these cases.
		InCacheOnly=
			0x10,// Allows access to the disk, but only to retrieve a cached item. This returns a cached thumbnail if it is available. If no cached thumbnail is available, it returns a cached per-instance icon but does not extract a thumbnail or icon.
		Win8CropToSquare=0x20,// Introduced in Windows 8. If necessary, crop the bitmap to a square.
		Win8WideThumbnails=0x40,// Introduced in Windows 8. Stretch and crop the bitmap to a 0.7 aspect ratio.
		Win8IconBackground=0x80,// Introduced in Windows 8. If returning an icon, paint a background using the associated app's registered background color.
		Win8ScaleUp=0x100,// Introduced in Windows 8. If necessary, stretch the bitmap so that the height and width fit the given size.
	}

	private const string ShellItem2Guid="7E9FB0D3-919F-4307-AB2E-9B1860310C93";

	[DllImport("shell32.dll",CharSet=CharSet.Unicode,SetLastError=true)]
	private static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)]string path,
		// The following parameter is not used - binding context.
		IntPtr pbc,
		ref Guid guid,
		[MarshalAs(UnmanagedType.Interface)]out IShellItem shellItem);

	[DllImport("gdi32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DeleteObject(IntPtr hObject);

	[DllImport("msvcrt.dll",CallingConvention=CallingConvention.Cdecl)]
	private static extern IntPtr memcpy(IntPtr dest,IntPtr src,int count);

	private static Bitmap GetBitmapFromHBitmap(IntPtr nativeHBitmap){
		var bmp=Image.FromHbitmap(nativeHBitmap);

		if(Image.GetPixelFormatSize(bmp.PixelFormat)<32)
			return bmp;

		return CreateAlphaBitmap(bmp,PixelFormat.Format32bppArgb);
	}

	private static Bitmap CreateAlphaBitmap(Bitmap srcBitmap,PixelFormat targetPixelFormat){
		var result=new Bitmap(srcBitmap.Width,srcBitmap.Height,targetPixelFormat);

		var bmpBounds=new Rectangle(0,0,srcBitmap.Width,srcBitmap.Height);
		var srcData=srcBitmap.LockBits(bmpBounds,ImageLockMode.ReadOnly,srcBitmap.PixelFormat);
		var destData=result.LockBits(bmpBounds,ImageLockMode.ReadOnly,targetPixelFormat);

		var srcDataPtr=srcData.Scan0;
		var destDataPtr=destData.Scan0;

		try{
			for(var y=0;y<=srcData.Height-1;y++){
				for(var x=0;x<=srcData.Width-1;x++){
					//this is really important because one stride may be positive and the other negative
					var position=srcData.Stride*y+4*x;
					var position2=destData.Stride*y+4*x;

					memcpy(destDataPtr+position2,srcDataPtr+position,4);
				}
			}
		} finally{
			srcBitmap.UnlockBits(srcData);
			result.UnlockBits(destData);
		}

		using (srcBitmap)
			return result;
	}

	private static IntPtr GetHBitmap(string fileName,int width,int height,ThumbnailOptions options,out string displayName){
		var shellItem2Guid=new Guid(ShellItem2Guid);
		var retCode=SHCreateItemFromParsingName(fileName,IntPtr.Zero,ref shellItem2Guid,out var nativeShellItem);

		if(retCode!=0) throw Marshal.GetExceptionForHR(retCode)!;

		nativeShellItem.GetDisplayName(0,out displayName);

		var nativeSize=new NativeSize{
			Width=width,
			Height=height,
		};

		// ReSharper disable once SuspiciousTypeConversion.Global
		var hr=((IShellItemImageFactory)nativeShellItem).GetImage(nativeSize,options,out var hBitmap);

		Marshal.ReleaseComObject(nativeShellItem);

		if(hr==0) return hBitmap;
		throw Marshal.GetExceptionForHR(hr)!;
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
	internal interface IShellItem{
		void BindToHandler(IntPtr pbc,
			[MarshalAs(UnmanagedType.LPStruct)]Guid bhid,
			[MarshalAs(UnmanagedType.LPStruct)]Guid riid,
			out IntPtr ppv);

		void GetParent(out IShellItem ppsi);
		void GetDisplayName(uint sigdnName,[MarshalAs(UnmanagedType.LPWStr)]out string ppszName);
		void GetAttributes(uint sfgaoMask,out uint psfgaoAttribs);
		void Compare(IShellItem psi,uint hint,out int piOrder);
	}

	[ComImport]
	[Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface IShellItemImageFactory{
		[PreserveSig]
		int GetImage([In,MarshalAs(UnmanagedType.Struct)]NativeSize size,
			[In]ThumbnailOptions flags,
			[Out]out IntPtr phbm);
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct NativeSize{
		public int Width;
		public int Height;
	}
	#endregion

	#region Exe Icon
	[DllImport("Shell32",CharSet=CharSet.Auto)]
	private static extern int ExtractIconEx(
		string lpszFile,
		int nIconIndex,
		IntPtr[] phIconLarge,
		IntPtr[] phIconSmall,
		int nIcons);

	[DllImport("user32.dll",EntryPoint="DestroyIcon",SetLastError=true)]
	private static extern IntPtr DestroyIcon(IntPtr hIcon);
	#endregion

}