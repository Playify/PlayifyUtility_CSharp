using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Win;

[PublicAPI]
public static partial class WinCursor{
	#region Cursor
	
	public static Point CursorPos{
		get{
			if(GetCursorPos(out var point)) return point;
			throw new Exception("Error getting cursor pos");
		}
		set=>SetCursorPos(value.X,value.Y);
	}
	public static bool TryGetCursorPos(out Point point){
		var b=GetCursorPos(out var nativePoint);
		point=nativePoint;
		return b;
	}
	
	[DllImport("user32.dll",EntryPoint = "SetCursorPos")]
	public static extern bool SetCursorPos(int x,int y);
	#endregion

	#region Color
	private static readonly Bitmap ScreenPixel=new(1,1,PixelFormat.Format32bppArgb);

	public static Color GetColorUnderCursor()=>GetColorAt(CursorPos);
	public static Color GetColorAt(Point location)=>GetColorAt(location.X,location.Y);
	public static Color GetColorAt(NativePoint location)=>GetColorAt(location.X,location.Y);
	public static Color GetColorAt(int x,int y){
		using(var gDest=Graphics.FromImage(ScreenPixel))
		using(var gSrc=Graphics.FromHwnd(IntPtr.Zero)){
			var hSrcDc=gSrc.GetHdc();
			var hDc=gDest.GetHdc();
			BitBlt(hDc,0,0,1,1,hSrcDc,x,y,(int) CopyPixelOperation.SourceCopy);
			gDest.ReleaseHdc();
			gSrc.ReleaseHdc();
		}

		return ScreenPixel.GetPixel(0,0);
	}
	#endregion
}
