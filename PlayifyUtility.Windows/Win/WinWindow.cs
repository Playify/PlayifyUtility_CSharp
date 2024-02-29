using System.ComponentModel;
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

	public readonly IntPtr Hwnd;

	public WinWindow(IntPtr hwnd)=>Hwnd=hwnd;
	public override string ToString()=>$"{nameof(WinWindow)}(0x{Hwnd.ToInt64():x})";

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

	public static WinWindow FindWindow(Func<WinWindow,bool> predicate){
		WinWindow result=default;
		EnumWindows((hwnd,_)=>{
			var window=new WinWindow(hwnd);
			if(!predicate(window)) return true;
			result=window;
			return false;
		},0);
		return result;
	}

	public static WinWindow GetWindowUnderCursor(bool detectInvisibleForeground=false){
		if(!WinCursor.TryGetCursorPos(out var cursor)) return Zero;
		
		if(!detectInvisibleForeground) return GetWindowAt(cursor.X,cursor.Y);
		
		var foreground=Foreground;
		var rect=foreground.WindowRect;
		if(rect.Left<=cursor.X&&cursor.Y<=rect.Right&&rect.Top<=cursor.Y&&cursor.Y<=rect.Bottom) return foreground;
		return GetWindowAt(cursor.X,cursor.Y);
	}

	public static WinWindow GetWindowAt(Point point)=>GetWindowAt(point.X,point.Y);

	public static WinWindow GetWindowAt(int x,int y)=>new WinControl(WindowFromPoint(new Point(x,y))).Window;

	public static WinWindow FindWindow(string? windowName)=>FindWindow(null,windowName);
	public static WinWindow FindAnyWindow(params string[] windowNames){
		foreach(var windowName in windowNames)
			if(FindWindow(windowName).NonZero(out var win))
				return win;
		return Zero;
	}

	public static WinWindow FindWindow(string? @class,string? windowName)=>new(FindWindow_Hwnd(@class,windowName));

	public static WinWindow DesktopWindow=>new(GetDesktopWindow());

	public static WinWindow Foreground{
		get=>new(GetForegroundWindow());
		set=>SetForegroundWindow(value.Hwnd);
	}
	public void SetForeground()=>Foreground=this;

	#region Rendering
	public ExStyle ExStyle{
		get=>(ExStyle)GetWindowLong(Hwnd,-20);
		set=>SetWindowLong(Hwnd,-20,(int)value);
	}
	public GwlStyle GwlStyle{
		get=>(GwlStyle)GetWindowLong(Hwnd,-16);
		set=>SetWindowLong(Hwnd,-16,(int)value);
	}
	public bool IsVisible{
		get=>IsWindowVisible(Hwnd);
		set=>ShowWindowCommand=value?ShowWindowCommands.Show:ShowWindowCommands.Hide;
	}
	public bool Exists=>IsWindow(Hwnd);

	public bool Enabled{
		get=>IsWindowEnabled(Hwnd);
		set=>EnableWindow(Hwnd,value);
	}
	public NativeRect WindowRect{
		get{
			GetWindowRect(Hwnd,out var rect);
			return rect;
		}
	}


	public bool ClickThrough{
		get=>(ExStyle&ExStyle.Transparent)!=0;
		set{
			var l=ExStyle;
			l|=ExStyle.Layered;
			if(value) l|=ExStyle.Transparent;
			else l&=~ExStyle.Transparent;
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
				GwlStyle|=GwlStyle.Caption|GwlStyle.Thickframe;

				SetWindowPos(Hwnd,0,rect.Left,rect.Top,rect.Right-rect.Left,rect.Bottom-rect.Top,0x4);
				GetClientRect(Hwnd,out rect);
				PostMessage(WindowMessage.WM_SIZE,0,((rect.Bottom-rect.Top)<<16)|((rect.Right-rect.Left)&0xffff));
				FullScreened.Remove(Hwnd);
			} else if(value){
				//Enable fullscreen
				GetWindowRect(Hwnd,out rect);
				var screen=Screen.FromRectangle(rect);
				var bnd=screen.Bounds;

				GwlStyle&=~(GwlStyle.Caption|GwlStyle.Thickframe);

				FullScreened.Add(Hwnd,rect);
				SetWindowPos(0,bnd,0);
				GetClientRect(Hwnd,out rect);
				PostMessage(WindowMessage.WM_SIZE,0,(rect.Height<<16)|(rect.Width&0xffff));
			}
		}
	}

	public bool Borderless{
		get=>(GwlStyle&(GwlStyle.Caption|GwlStyle.Thickframe))==0;
		set{
			const GwlStyle style=GwlStyle.Caption|GwlStyle.Thickframe;
			
			var l=GwlStyle;
			l=value?l&~style:l|style;
			GwlStyle=l;
			SetWindowPos(Hwnd,0,0,0,0,0,0x27);
			GetClientRect(Hwnd,out var rect);
			PostMessage(WindowMessage.WM_SIZE,0,(rect.Height<<16)|(rect.Width&0xffff));
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


	private static Stack<WinWindow>? _hiddenWindows;

	public void HidePush(){
		if(_hiddenWindows==null){
			_hiddenWindows=new Stack<WinWindow>();
			AppDomain.CurrentDomain.ProcessExit+=(_,_)=>RestoreAll();
		}
		_hiddenWindows.Push(this);
		ShowWindowCommand=ShowWindowCommands.Hide;
	}

	public static void RestoreLast(){
		if(_hiddenWindows?.TryPop(out var restore)??false)
			restore.ShowWindowCommand=ShowWindowCommands.Show;
	}

	public static void RestoreAll(){
		while(_hiddenWindows?.TryPop(out var restore)??false)
			restore.ShowWindowCommand=ShowWindowCommands.Show;
	}

	public bool Hidden{
		get=>!IsVisible;
		set=>IsVisible=!value;
	}


	public bool AlwaysOnTop{
		get=>(ExStyle&ExStyle.TopMost)!=0;
		set=>SetWindowPos(Hwnd,value?-1:-2,0,0,0,0,3);
	}

	public byte Alpha{
		get{
			GetLayeredWindowAttributes(Hwnd,out _,out var alpha,out var dw);
			return (dw&2)!=0?alpha:(byte)255;
		}
		set{
			var l=ExStyle;
			if((l&ExStyle.Layered)==0) ExStyle=l|ExStyle.Layered;

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
			if((l&ExStyle.Layered)==0) ExStyle=l|ExStyle.Layered;

			GetLayeredWindowAttributes(Hwnd,out var c,out var alpha,out var dw);
			SetLayeredWindowAttributes(Hwnd,value??c,alpha,value.HasValue?dw|1:dw&2);
		}
	}

	public void SetDarkMode(bool? dark){
		var val=dark??WinSystem.DarkMode?1:0;
		//https://gist.github.com/valinet/6afb524426635df9dbe2a9035701fcf4
		if(DwmSetWindowAttribute(Hwnd,19,ref val,4)!=0)//check if 19 works
			DwmSetWindowAttribute(Hwnd,20,ref val,4);//if not, then use 20
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
	
	public struct PropMap{
		private readonly WinWindow _win;

		internal PropMap(WinWindow win)=>_win=win;

		public int this[string s]{
			get=>Get(s);
			set{
				if(!Set(s,value)) throw new Win32Exception();
			}
		}
		public int Get(string s)=>GetProp(_win.Hwnd,s);
		public bool Set(string s,int value)=>SetProp(_win.Hwnd,s,value);
		public int Remove(string s)=>RemoveProp(_win.Hwnd,s);

		public bool TryOverride(string s,int value)=>GetProp(_win.Hwnd,s)!=value&&SetProp(_win.Hwnd,s,value);
	}
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
	
	public void SendSysCommand(SysCommand cmd,int lParam=0)=>SendMessage(WindowMessage.WM_SYSCOMMAND,(int)cmd,lParam);
	public void PostSysCommand(SysCommand cmd,int lParam=0)=>PostMessage(WindowMessage.WM_SYSCOMMAND,(int)cmd,lParam);

	public bool MoveWindow(int x,int y,int width,int height,bool redraw)=>MoveWindow(Hwnd,x,y,width,height,redraw);
	public bool MoveWindow(NativeRect rect,bool redraw)=>MoveWindow(Hwnd,rect.Left,rect.Top,rect.Width,rect.Height,redraw);
	public bool SetWindowPos(int hwndInsertAfter,int x,int y,int cx,int cy,uint uFlags)=>SetWindowPos(Hwnd,hwndInsertAfter,x,y,cx,cy,uFlags);
	public bool SetWindowPos(int hwndInsertAfter,NativeRect rect,uint uFlags)=>SetWindowPos(Hwnd,hwndInsertAfter,rect.X,rect.Y,rect.Width,rect.Height,uFlags);
	public bool DestroyWindow()=>DestroyWindow(Hwnd);
	#endregion
	
	#region Message

	[Obsolete("Use the overload with WindowMessage enum instead")]
	public int SendMessage(int msg,int wParam,int lParam)=>SendMessage((WindowMessage)msg,wParam,lParam);

	[Obsolete("Use the overload with WindowMessage enum instead")]
	public int SendMessage(int msg,int wParam,IntPtr lParam)=>SendMessage((WindowMessage)msg,wParam,lParam);

	[Obsolete("Use the overload with WindowMessage enum instead")]
	public int SendMessage<T>(int msg,int wParam,ref T lParam) where T:struct=>SendMessage((WindowMessage)msg,wParam,ref lParam);
	[Obsolete("Use the overload with WindowMessage enum instead")]
	public bool PostMessage(int msg,int wParam,int lParam)=>PostMessage((WindowMessage)msg,wParam,lParam);

	[Obsolete("Use the overload with WindowMessage enum instead")]
	public bool PostMessage(int msg,int wParam,IntPtr lParam)=>PostMessage((WindowMessage)msg,wParam,lParam);

	[Obsolete("Use the overload with WindowMessage enum instead")]
	public bool PostMessage<T>(int msg,int wParam,ref T lParam) where T:struct=>PostMessage((WindowMessage)msg,wParam,ref lParam);

	
	public void SendNull()=>SendMessage(WindowMessage.WM_NULL,0,0);
	public void SendNull(int count)=>SendNull(count,TimeSpan.FromMilliseconds(100));
	public void SendNull(int count,TimeSpan delay){
		while(count-->0){
			SendNull();
			Thread.Sleep(delay);
		}
	}
	
	
	public int SendMessage(WindowMessage msg,int wParam,int lParam)=>SendMessage(Hwnd,msg,wParam,lParam);
	public int SendMessage(WindowMessage msg,int wParam,IntPtr lParam)=>SendMessage(Hwnd,msg,wParam,lParam);
	public int SendMessage(WindowMessage msg,int wParam,StringBuilder lParam)=>SendMessage(Hwnd,msg,wParam,lParam);
	public int SendMessage(WindowMessage msg,int wParam,string lParam)=>SendMessage(Hwnd,msg,wParam,lParam);
	
	public int SendMessage<T>(WindowMessage msg,int wParam,ref T lParam) where T:struct{
		var ptr=Marshal.AllocHGlobal(Marshal.SizeOf<T>());
		try{
			Marshal.StructureToPtr(lParam,ptr,false);
			var result=SendMessage(msg,wParam,ptr);
			lParam=Marshal.PtrToStructure<T>(ptr);
			return result;
		} finally{
			Marshal.FreeHGlobal(ptr);
		}
	}
	public bool PostMessage(WindowMessage msg,int wParam,int lParam)=>PostMessage(Hwnd,msg,wParam,lParam);
	public bool PostMessage(WindowMessage msg,int wParam,IntPtr lParam)=>PostMessage(Hwnd,msg,wParam,lParam);
	
	public bool PostMessage<T>(WindowMessage msg,int wParam,ref T lParam) where T:struct{
		var ptr=Marshal.AllocHGlobal(Marshal.SizeOf<T>());
		try{
			Marshal.StructureToPtr(lParam,ptr,false);
			var result=PostMessage(msg,wParam,ptr);
			lParam=Marshal.PtrToStructure<T>(ptr);
			return result;
		} finally{
			Marshal.FreeHGlobal(ptr);
		}
	}
	#endregion

	#region Operators
	public WinControl AsControl=>new(Hwnd);
	
	public override bool Equals(object? obj)=>obj is WinWindow other&&this==other;
	public override int GetHashCode()=>Hwnd.GetHashCode();
	public static bool operator!=(WinWindow left,WinWindow right)=>!(left==right);
	public static bool operator==(WinWindow left,WinWindow right)=>left.Hwnd==right.Hwnd;
	public static implicit operator bool(WinWindow win)=>win.Hwnd!=IntPtr.Zero;
	public static implicit operator IntPtr(WinWindow win)=>win.Hwnd;
	
	
	public static readonly WinWindow Zero;
	public bool NonZero(out WinWindow win)=>win=this;
	#endregion
}