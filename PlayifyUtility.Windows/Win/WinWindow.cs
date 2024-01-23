using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Win.Native;
#if NETFRAMEWORK
using PlayifyUtility.Windows.Utils;
#endif

namespace PlayifyUtility.Windows.Win;

[PublicAPI]
public partial struct WinWindow{
	static WinWindow()=>AppDomain.CurrentDomain.ProcessExit+=(_,_)=>RestoreAll();

	public readonly IntPtr Hwnd;

	public WinWindow(IntPtr hwnd)=>Hwnd=hwnd;
	public override string ToString()=>$"{nameof(WinWindow)}(0x{Hwnd:x})";

	public static List<WinWindow> GetOpenWindows()=>GetOpenWindows(w=>w.IsVisible);

	public static List<WinWindow> GetOpenWindows(Func<WinWindow,bool> predicate){
		var list=new List<WinWindow>();
		EnumWindows((hwnd,_)=>{
			var window=new WinWindow(hwnd);
			if(predicate(window)) list.Add(window);
			return true;
		},0);
		return list;
	}

	public static WinWindow? FindWindow(Func<WinWindow,bool> predicate){
		WinWindow? result=null;
		EnumWindows((hwnd,_)=>{
			var window=new WinWindow(hwnd);
			if(!predicate(window)) return true;
			result=window;
			return false;
		},0);
		return result;
	}

	public static WinWindow GetWindowUnderCursor(bool detectInvisibleForeground=false){
		var cursor=WinCursor.CursorPos;
		if(detectInvisibleForeground){
			var foreground=Foreground;
			var rect=foreground.WindowRect;
			if(rect.Left<=cursor.X&&cursor.Y<=rect.Right&&rect.Top<=cursor.Y&&cursor.Y<=rect.Bottom) return foreground;
		}
		return GetWindowAt(cursor.X,cursor.Y);
	}

	public static WinWindow GetWindowAt(Point point)=>GetWindowAt(point.X,point.Y);

	public static WinWindow GetWindowAt(int x,int y)=>new WinControl(WindowFromPoint(new Point(x,y))).Window;

	public static WinWindow FindWindow(string? @class,string? windowName)=>new(FindWindow_Hwnd(@class,windowName));

	public static WinWindow DesktopWindow=>new(GetDesktopWindow());

	public static WinWindow Foreground{
		get=>new(GetForegroundWindow());
		set=>SetForegroundWindow(value.Hwnd);
	}
	public void SetForeground()=>Foreground=this;

	#region Rendering
	public int ExStyle{
		get=>GetWindowLong(Hwnd,-20);
		set=>SetWindowLong(Hwnd,-20,value);
	}
	public int GwlStyle{
		get=>GetWindowLong(Hwnd,-16);
		set=>SetWindowLong(Hwnd,-16,value);
	}
	public bool IsVisible{
		get=>IsWindowVisible(Hwnd);
		set=>ShowWindowCommand=value?ShowWindowCommands.Show:ShowWindowCommands.Hide;
	}
	public bool Exists=>IsWindow(Hwnd);
	public NativeRect WindowRect{
		get{
			GetWindowRect(Hwnd,out var rect);
			return rect;
		}
	}


	public bool ClickThrough{
		get=>(ExStyle&0x20)!=0;
		set{
			var l=ExStyle;
			l|=0x80000;
			if(value) l|=0x20;
			else l&=~0x20;
			ExStyle=l;
		}
	}

	private static readonly Dictionary<IntPtr,NativeRect> FullScreened=new();
	public bool Fullscreen{
		get=>Borderless&&FullScreened.ContainsKey(Hwnd);
		set{
			if(FullScreened.TryGetValue(Hwnd,out var rect)){
				if(value) return;
				//Disable fullscreen
				GwlStyle|=0xc40000;//0xC40000=WS_CAPTION|WS_THICKFRAME

				SetWindowPos(Hwnd,0,rect.Left,rect.Top,rect.Right-rect.Left,rect.Bottom-rect.Top,0x4);
				GetClientRect(Hwnd,out rect);
				PostMessage(Hwnd,5,0,((rect.Bottom-rect.Top)<<16)|((rect.Right-rect.Left)&0xffff));//5=WM_SIZE
				FullScreened.Remove(Hwnd);
			} else if(!value){
				//Enable fullscreen
				GetWindowRect(Hwnd,out rect);
				var screen=Screen.FromRectangle(new Rectangle(rect.Left,rect.Top,rect.Right-rect.Left,rect.Bottom-rect.Top));
				var bnd=screen.Bounds;

				GwlStyle&=~0xc40000;//0xC40000=WS_CAPTION|WS_THICKFRAME

				FullScreened.Add(Hwnd,rect);
				SetWindowPos(Hwnd,0,bnd.X,bnd.Y,bnd.Width,bnd.Height,0);
				GetClientRect(Hwnd,out rect);
				PostMessage(Hwnd,5,0,((rect.Bottom-rect.Top)<<16)|((rect.Right-rect.Left)&0xffff));//5=WM_SIZE
			}
		}
	}

	public bool Borderless{
		get=>(GwlStyle&0xC40000)==0;
		set{
			var l=GetWindowLong(Hwnd,-16);//-16=GWL_STYLE
			l=value?l&~0xc40000:l|0xc40000;//0xC40000=WS_CAPTION|WS_THICKFRAME
			SetWindowLong(Hwnd,-16,l);
			SetWindowPos(Hwnd,0,0,0,0,0,0x27);
			GetClientRect(Hwnd,out var rect);
			PostMessage(Hwnd,5,0,((rect.Bottom-rect.Top)<<16)|((rect.Right-rect.Left)&0xffff));
		}
	}


	public bool Maximized{
		get=>ShowWindowCommand==ShowWindowCommands.Maximized;
		set=>ShowWindowCommand=value?ShowWindowCommands.Maximized:ShowWindowCommands.Normal;
	}

	public ShowWindowCommands ShowWindowCommand{
		get{
			var placement=new WindowPlacement{length=Marshal.SizeOf<WindowPlacement>()};
			GetWindowPlacement(Hwnd,ref placement);
			return placement.showCmd;
		}
		set=>ShowWindow(Hwnd,value);
	}


	private static readonly Stack<WinWindow> _hidden=new();


	public void HidePush(){
		_hidden.Push(this);
		ShowWindowCommand=ShowWindowCommands.Hide;
	}

	public static void RestoreLast(){
		if(_hidden.TryPop(out var restore))
			restore.ShowWindowCommand=ShowWindowCommands.Show;
	}

	public static void RestoreAll(){
		while(_hidden.TryPop(out var restore))
			restore.ShowWindowCommand=ShowWindowCommands.Show;
	}

	public bool Hidden{
		get=>!IsVisible;
		set=>IsVisible=!value;
	}


	public bool AlwaysOnTop{
		get=>(ExStyle&8)!=0;
		set=>SetWindowPos(Hwnd,value?-1:-2,0,0,0,0,3);
	}

	public byte Alpha{
		get{
			GetLayeredWindowAttributes(Hwnd,out _,out var alpha,out var dw);
			return (dw&2)!=0?alpha:(byte)255;
		}
		set{
			var l=ExStyle;
			if((l&0x80000)==0) ExStyle=l|0x80000;

			GetLayeredWindowAttributes(Hwnd,out var color,out _,out var dw);
			SetLayeredWindowAttributes(Hwnd,color,value,dw|2);
		}
	}

	public NativeColor? TransparentColor{
		get{
			GetLayeredWindowAttributes(Hwnd,out var clr,out _,out var dw);
			return (dw&1)!=0?clr:null;
		}
		set{
			var l=ExStyle;
			if((l&0x80000)==0) ExStyle=l|0x80000;

			GetLayeredWindowAttributes(Hwnd,out var c,out var alpha,out var dw);
			SetLayeredWindowAttributes(Hwnd,value??c,alpha,value.HasValue?dw|1:dw&2);
		}
	}
	#endregion


	#region Info
	public Process? Process{
		get{
			try{
				GetWindowThreadProcessId(Hwnd,out var pid);
				return Process.GetProcessById(pid);
			} catch{
				return null;
			}
		}
	}

	private static readonly Dictionary<int,string?> ProcessExeCache=new();
	public string? ProcessExe{
		get{
			try{
				GetWindowThreadProcessId(Hwnd,out var pid);
				if(pid==0) return null;
				if(ProcessExeCache.TryGetValue(pid,out var exe)) return exe;
				return ProcessExeCache[pid]=Process.GetProcessById(pid).MainModule?.FileName;
			} catch{
				return null;
			}
		}
	}

	public string? Title{
		get{
			try{
				var length=GetWindowTextLength(Hwnd)+1;
				var title=new StringBuilder(length);
				GetWindowText(Hwnd,title,length);
				return title.ToString();
			} catch(AccessViolationException){
				return null;
			}
		}
	}
	public string? Class=>new WinControl(Hwnd).Class;


	public PropMap Props=>new(this);
	#endregion


	#region Commands
	public Dictionary<string,WinControl> GetControls()=>WinControl.GetControls(Hwnd);


	public enum SysCommand{
		Close=0xF060,//Closes the window.
		Maximize=0xF030,//Maximizes the window.
		Minimize=0xF020,//Minimizes the window.
		Restore=0xF120,//Restores the window to its normal position and size.

		NextWindow=0xF040,//Moves to the next window. (Mostly useless)
		PrevWindow=0xF050,//Moves to the previous window. (Mostly useless)

		AltMenu=0xF100,//KeyMenu  //Retrieves the window menu as a result of a keystroke. (Similar to pressing Alt to get the menu)
		WindowsMenu=0xF130,//TaskList  //Activates the Start menu. (Similar to pressing Win to get the Windows Menu)
	}

	public void SendSysCommand(SysCommand cmd,int lParam=0)=>SendMessage(0x112,(int)cmd,lParam);
	public void PostSysCommand(SysCommand cmd,int lParam=0)=>PostMessage(0x112,(int)cmd,lParam);

	public bool MoveWindow(int x,int y,int width,int height,bool redraw)=>MoveWindow(Hwnd,x,y,width,height,redraw);
	public bool MoveWindow(NativeRect rect,bool redraw)=>MoveWindow(Hwnd,rect.Left,rect.Top,rect.Width,rect.Height,redraw);
	public bool SetWindowPos(int hwndInsertAfter,int x,int y,int cx,int cy,uint uFlags)=>SetWindowPos(Hwnd,hwndInsertAfter,x,y,cx,cy,uFlags);
	public bool DestroyWindow()=>DestroyWindow(Hwnd);
	public int SendMessage(int msg,int wParam,int lParam)=>SendMessage(Hwnd,msg,wParam,lParam);
	public int SendMessage(int msg,int wParam,IntPtr lParam)=>SendMessage(Hwnd,msg,wParam,lParam);
	
	public int SendMessage<T>(int msg,int wParam,ref T lParam){
		var handle=GCHandle.Alloc(lParam,GCHandleType.Pinned);
		var result=SendMessage(msg,wParam,handle.AddrOfPinnedObject());
		lParam=(T)handle.Target!;
		handle.Free();
		return result;
	}
	public bool PostMessage(int msg,int wParam,int lParam)=>PostMessage(Hwnd,msg,wParam,lParam);
	public bool PostMessage(int msg,int wParam,IntPtr lParam)=>PostMessage(Hwnd,msg,wParam,lParam);
	
	public bool PostMessage<T>(int msg,int wParam,ref T lParam){
		var handle=GCHandle.Alloc(lParam,GCHandleType.Pinned);
		var result=PostMessage(msg,wParam,handle.AddrOfPinnedObject());
		lParam=(T)handle.Target!;
		handle.Free();
		return result;
	}
	#endregion

	#region Operators
	public override bool Equals(object? obj)=>obj is WinWindow other&&this==other;
	public override int GetHashCode()=>Hwnd.GetHashCode();
	public static bool operator!=(WinWindow left,WinWindow right)=>!(left==right);
	public static bool operator==(WinWindow left,WinWindow right)=>left.Hwnd==right.Hwnd;
	public static implicit operator bool(WinWindow win)=>win.Hwnd!=IntPtr.Zero;
	
	
	public static readonly WinWindow Zero;
	public bool NonZero(out WinWindow win)=>win=this;
	#endregion
}