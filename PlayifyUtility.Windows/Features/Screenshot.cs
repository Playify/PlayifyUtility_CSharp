using System.Drawing.Imaging;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Win;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Features;

[PublicAPI]
public static partial class Screenshot{
	public static Bitmap CopyFromScreen()=>CopyFromScreen(Screen.AllScreens.Select(s=>s.Bounds).Aggregate(Rectangle.Union));
	public static Bitmap CopyFromScreen(Screen screen)=>CopyFromScreen(screen.Bounds);
	public static Bitmap CopyFromScreen(WinControl control)=>CopyFromScreen(control.Rect);
	public static Bitmap CopyFromScreen(WinWindow window)=>CopyFromScreen(window.WindowRect);

	public static Bitmap CopyFromScreen(NativeRect rect){
		var image=new Bitmap(rect.Width,rect.Height);
		using var g=Graphics.FromImage(image);
		g.CopyFromScreen(
			rect.Left,rect.Top,
			0,0,
			image.Size);
		return image;
	}

	public static Image CaptureDesktop()=>CaptureWindow(WinWindow.DesktopWindow);

	public static Image CaptureWindow(WinControl control)=>CaptureWindow(control.AsWindow);

	public static Image CaptureWindow(WinWindow window){
		var hdcSrc=GetWindowDC(window.Hwnd);

		var rect=window.WindowRect;
		var width=rect.Width;
		var height=rect.Height;

		var hdcDest=CreateCompatibleDC(hdcSrc);
		var hBitmap=CreateCompatibleBitmap(hdcSrc,width,height);

		var hOld=SelectObject(hdcDest,hBitmap);
		BitBlt(hdcDest,0,0,width,height,hdcSrc,0,0,13369376);//SRCCOPY
		SelectObject(hdcDest,hOld);
		DeleteDC(hdcDest);
		ReleaseDC(window.Hwnd,hdcSrc);

		Image image=Image.FromHbitmap(hBitmap);
		DeleteObject(hBitmap);

		return image;
	}

	/*
	public static Image CaptureHiddenWindow(WinWindow window){
		var hdcSrc = GetWindowDC(window.Hwnd);

		var rect=window.WindowRect;
		var bmp = new Bitmap(rect.Width,rect.Height);

		using (var g = Graphics.FromImage(bmp))
		{
			var hdcDest = g.GetHdc();
			PrintWindow(window.Hwnd, hdcDest, 0);
			g.ReleaseHdc(hdcDest);
		}
		ReleaseDC(window.Hwnd, hdcSrc);

		return bmp;
	}*/


	#region Color
	private static readonly Bitmap ScreenPixel=new(1,1,PixelFormat.Format32bppArgb);

	public static Color? GetColorUnderCursor()=>WinSystem.TryGetCursorPos(out var pos)?GetColorAt(pos):null;
	public static Color GetColorAt(Point location)=>GetColorAt(location.X,location.Y);
	public static Color GetColorAt(NativePoint location)=>GetColorAt(location.X,location.Y);

	public static Color GetColorAt(int x,int y){
		using (var gDest=Graphics.FromImage(ScreenPixel))
		using (var gSrc=Graphics.FromHwnd(IntPtr.Zero)){
			var hSrcDc=gSrc.GetHdc();
			var hDc=gDest.GetHdc();
			BitBlt(hDc,0,0,1,1,hSrcDc,x,y,(int)CopyPixelOperation.SourceCopy);
			gDest.ReleaseHdc();
			gSrc.ReleaseHdc();
		}

		return ScreenPixel.GetPixel(0,0);
	}
	#endregion

}