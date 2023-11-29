using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using static PlayifyUtility.Windows.NativeMethods;
using Point=System.Drawing.Point;

namespace PlayifyUtility.Windows;

[SuppressMessage("Interoperability","CA1401:P/Invokes dürfen nicht sichtbar sein")]
[SuppressMessage("ReSharper","CommentTypo")]
public static class WindowsUtils{
	
	#region Get Color
	public static Color GetPixelColorUnderMouse(){
		NativeMethods.GetCursorPos(out var p);
		return GetColorAt(p);
	}

	private static readonly Bitmap ScreenPixel=new(1,1,PixelFormat.Format32bppArgb);

	public static Color GetColorAt(Point location)=>GetColorAt(location.X,location.Y);
	public static Color GetColorAt(NativeMethods.Point location)=>GetColorAt(location.x,location.y);
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
	
	#region SendKey & CursorPos
	public static bool GetCursorPos(out Point point){
		var b=NativeMethods.GetCursorPos(out var ptCursor);
		point=ptCursor;
		return b;
	}
	public static bool SetCursorPos(Point point)=>NativeMethods.SetCursorPos(point.X,point.Y);
	public static bool SetCursorPos(int x,int y)=>NativeMethods.SetCursorPos(x,y);
	#endregion

}