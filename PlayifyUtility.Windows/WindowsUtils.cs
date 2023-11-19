using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using Point=System.Drawing.Point;

namespace PlayifyUtility.Windows;

[PublicAPI]
[SuppressMessage("Interoperability","CA1401:P/Invokes dürfen nicht sichtbar sein")]
[SuppressMessage("ReSharper","CommentTypo")]
public static class WindowsUtils{
	static WindowsUtils()=>AppDomain.CurrentDomain.ProcessExit+=(_,_)=>RestoreAllWindows();

	#region Finding Windows

	public static IntPtr GetWindowUnderCursor()=>NativeMethods.GetCursorPos(out var ptCursor)?GetWindowAt(ptCursor.x,ptCursor.y):IntPtr.Zero;

	public static IntPtr GetWindowAt(Point pos)=>GetWindowAt(pos.X,pos.Y);
	public static IntPtr GetWindowAt(int x,int y){
		var hwnd=NativeMethods.WindowFromPoint(new Point(x,y));
		while(true){
			var parent=GetParent(hwnd);
			if(parent==IntPtr.Zero) return hwnd;
			hwnd=parent;
		}
	}
	public static IntPtr GetInvisibleWindowUnderCursor(){
		var hWnd=GetForegroundWindow();

		NativeMethods.GetWindowRect(hWnd,out var rect);
		NativeMethods.GetCursorPos(out var ptCursor);

		if(rect.Left<=ptCursor.x&&ptCursor.y<=rect.Right&&rect.Top<=ptCursor.y&&ptCursor.y<=rect.Bottom) return hWnd;
		return GetWindowAt(ptCursor);
	}
	
	[DllImport("kernel32.dll")]
	public static extern IntPtr GetConsoleWindow();
	[DllImport("user32.dll")]
	public static extern IntPtr GetParent(IntPtr hWnd);
	[DllImport("user32.dll")]
	public static extern IntPtr GetForegroundWindow();
	[DllImport("user32.dll")]
	public static extern bool SetForegroundWindow(IntPtr hWnd);
	#endregion

	#region Click Through
	public static bool SetClickThrough(IntPtr hwnd,bool? b){
		var l=NativeMethods.GetWindowLong(hwnd,-20);
		l|=0x80000;
		if(b.HasValue)
			if(b.Value) l|=0x20;
			else l&=~0x20;
		else l^=0x20;
		NativeMethods.SetWindowLong(hwnd,-20,l);
		return (l&0x20)!=0;
	}
	#endregion

	#region Fullscreen
	private static readonly Dictionary<IntPtr,NativeMethods.Rect> FullScreened=new();

	public static bool IsFullscreen(IntPtr hwnd){
		var l=NativeMethods.GetWindowLong(hwnd,-16);//-16=GWL_STYLE
		return (l&0xc40000)==0;//0xC40000=WS_CAPTION|WS_THICKFRAME
	}

	public static void SetFullscreen(IntPtr hwnd,bool? b){
		if(b!=null&&b.Value==FullScreened.ContainsKey(hwnd)) return;
		if(FullScreened.TryGetValue(hwnd,out var rect)){
			var l=NativeMethods.GetWindowLong(hwnd,-16);//-16=GWL_STYLE
			l|=0xc40000;//0xC40000=WS_CAPTION|WS_THICKFRAME
			NativeMethods.SetWindowLong(hwnd,-16,l);
			NativeMethods.SetWindowPos(hwnd,IntPtr.Zero,rect.Left,rect.Top,rect.Right-rect.Left,rect.Bottom-rect.Top,0x4);
			NativeMethods.GetClientRect(hwnd,out rect);
			NativeMethods.PostMessage(hwnd,5,0,((rect.Bottom-rect.Top)<<16)|((rect.Right-rect.Left)&0xffff));//5=WM_SIZE
			FullScreened.Remove(hwnd);
		} else{
			NativeMethods.GetWindowRect(hwnd,out rect);
			var screen=Screen.FromRectangle(new Rectangle(rect.Left,rect.Top,rect.Right-rect.Left,rect.Bottom-rect.Top));
			var bnd=screen.Bounds;

			var l=NativeMethods.GetWindowLong(hwnd,-16);//-16=GWL_STYLE
			l&=~0xc40000;//0xC40000=WS_CAPTION|WS_THICKFRAME
			NativeMethods.SetWindowLong(hwnd,-16,l);

			FullScreened.Add(hwnd,rect);
			NativeMethods.SetWindowPos(hwnd,IntPtr.Zero,bnd.X,bnd.Y,bnd.Width,bnd.Height,0);
			NativeMethods.GetClientRect(hwnd,out rect);
			NativeMethods.PostMessage(hwnd,5,0,((rect.Bottom-rect.Top)<<16)|((rect.Right-rect.Left)&0xffff));//5=WM_SIZE
		}
	}

	public static bool SetBorderless(IntPtr hwnd,bool? b){
		var l=NativeMethods.GetWindowLong(hwnd,-16);//-16=GWL_STYLE
		var ret=!(b??(l&0xC40000)!=0);
		l=ret?l|0xc40000:l&~0xc40000;//0xC40000=WS_CAPTION|WS_THICKFRAME
		NativeMethods.SetWindowLong(hwnd,-16,l);
		NativeMethods.SetWindowPos(hwnd,IntPtr.Zero,0,0,0,0,0x27);
		NativeMethods.GetClientRect(hwnd,out var rect);
		NativeMethods.PostMessage(hwnd,5,0,((rect.Bottom-rect.Top)<<16)|((rect.Right-rect.Left)&0xffff));
		return ret;
	}
	#endregion

	#region Maximize
	public static bool IsMaximized(IntPtr hwnd){
		var placement=new NativeMethods.WindowPlacement{length=Marshal.SizeOf<NativeMethods.WindowPlacement>()};

		NativeMethods.GetWindowPlacement(hwnd,ref placement);

		return placement.showCmd==NativeMethods.ShowWindowCommands.Maximized;
	}

	public static void SetMaximized(IntPtr hwnd,bool? b)=>NativeMethods.ShowWindow(hwnd,b??!IsMaximized(hwnd)?NativeMethods.ShowWindowCommands.Maximized:NativeMethods.ShowWindowCommands.Normal);
	#endregion

	#region Always on top
	public static bool IsAlwaysOnTop(IntPtr hwnd)=>(NativeMethods.GetWindowLong(hwnd,-20)&8)==8;

	public static void SetAlwaysOnTop(IntPtr hwnd,bool b)=>NativeMethods.SetWindowPos(hwnd,b?-1:-2,0,0,0,0,3);
	#endregion

	#region Hide Windows
	private static readonly Stack<IntPtr> Hidden=new();

	public static void HideWindow(IntPtr hwnd,bool restoreAfterwards=true){
		if(restoreAfterwards) Hidden.Push(hwnd);
		NativeMethods.ShowWindow(hwnd,NativeMethods.ShowWindowCommands.Hide);
	}

	public static void RestoreWindow(){
		if(Hidden.TryPop(out var hwnd)) NativeMethods.ShowWindow(hwnd,NativeMethods.ShowWindowCommands.Show);
	}

	public static void RestoreAllWindows(){
		while(Hidden.TryPop(out var hwnd)) NativeMethods.ShowWindow(hwnd,NativeMethods.ShowWindowCommands.Show);
	}
	#endregion
	
	#region Transparency
	public static byte GetAlpha(IntPtr hwnd){
		NativeMethods.GetLayeredWindowAttributes(hwnd,out _,out var alpha,out var dw);
		return (dw&2)!=0?alpha:(byte) 255;
	}

	public static byte SetAlpha(IntPtr hwnd,int val,bool delta){
		var l=NativeMethods.GetWindowLong(hwnd,-20);
		if((l&0x80000)==0) NativeMethods.SetWindowLong(hwnd,-20,l|0x80000);

		NativeMethods.GetLayeredWindowAttributes(hwnd,out var color,out var alpha,out var dw);

		if((dw&2)==0) alpha=255;

		if(delta) val+=alpha;

		NativeMethods.SetLayeredWindowAttributes(hwnd,color,alpha=(byte) (val<0?0:val>255?255:val),dw|2);

		return alpha;
	}

	public static NativeMethods.ColorRef? GetTransparentColor(IntPtr hwnd){
		NativeMethods.GetLayeredWindowAttributes(hwnd,out var clr,out _,out var dw);
		return (dw&1)!=0?clr:null;
	}

	public static void SetTransparentColor(IntPtr hwnd,NativeMethods.ColorRef? color){
		var l=NativeMethods.GetWindowLong(hwnd,-20);
		if((l&0x80000)==0) NativeMethods.SetWindowLong(hwnd,-20,l|0x80000);

		NativeMethods.GetLayeredWindowAttributes(hwnd,out var c,out var alpha,out var dw);

		NativeMethods.SetLayeredWindowAttributes(hwnd,color??c,alpha,color.HasValue?dw|1:dw&2);
	}
	#endregion
	
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
			NativeMethods.BitBlt(hDc,0,0,1,1,hSrcDc,x,y,(int) CopyPixelOperation.SourceCopy);
			gDest.ReleaseHdc();
			gSrc.ReleaseHdc();
		}

		return ScreenPixel.GetPixel(0,0);
	}
	#endregion

	#region Window info
	public static Process? GetProcess(IntPtr hwnd){
		try{
			NativeMethods.GetWindowThreadProcessId(hwnd,out var pid);
			return Process.GetProcessById((int) pid);
		} catch(Exception){
			return null;
		}
	}

	public static string GetExe(IntPtr hwnd)=>GetExe(GetProcess(hwnd));

	private static readonly Dictionary<int,string> Exe=new();

	public static string GetExe(Process? process){
		try{
			var id=process?.Id??0;
			if(id==0) return "";
			if(Exe.TryGetValue(id,out var exe)) return exe;
			return Exe[id]=process?.MainModule?.FileName??"";
		} catch(Exception){
			return "";
		}
	}

	public static string? GetText(IntPtr hwnd){
		try{
			var length=NativeMethods.GetWindowTextLength(hwnd)+1;
			var title=new StringBuilder(length);
			NativeMethods.GetWindowText(hwnd,title,length);
			return title.ToString();
		} catch(AccessViolationException e){
			Console.WriteLine(e);
			return null;
		}
	}

	public static string? GetTitle(IntPtr hwnd){
		try{
			return !NativeMethods.IsWindow(hwnd)?null:GetText(hwnd);
		} catch(AccessViolationException e){
			Console.WriteLine(e);
			return null;
		}
	}

	public static string? GetClass(IntPtr hwnd){
		try{
			var title=new StringBuilder(256);
			if(NativeMethods.GetClassName(hwnd,title,title.Capacity)==0){
				Console.WriteLine("Error reading class from "+hwnd);
				return null;
			}
			return title.ToString();
		} catch(AccessViolationException e){
			Console.WriteLine(e);
			return null;
		}
	}
	#endregion
	
	#region SendKey
	public static void SendKey(IntPtr hwnd,Keys keys){
		NativeMethods.PostMessage(hwnd,0x100,(int) keys,0);//WM_KEYDOWN
		NativeMethods.PostMessage(hwnd,0x101,(int) keys,0);//WM_KEYUP
	}
	public static bool GetCursorPos(out Point point){
		var b=NativeMethods.GetCursorPos(out var ptCursor);
		point=ptCursor;
		return b;
	}
	public static bool SetCursorPos(Point point)=>NativeMethods.SetCursorPos(point.X,point.Y);
	public static bool SetCursorPos(int x,int y)=>NativeMethods.SetCursorPos(x,y);
	#endregion
}