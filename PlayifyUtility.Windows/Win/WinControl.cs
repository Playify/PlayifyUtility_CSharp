using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Win.Controls;
using PlayifyUtility.Windows.Win.Native;
#if NETFRAMEWORK
using PlayifyUtility.Windows.Utils;
#endif

namespace PlayifyUtility.Windows.Win;

[SuppressMessage("ReSharper","CommentTypo")]
[PublicAPI]
public readonly partial struct WinControl{
	public readonly IntPtr Hwnd;

	public WinControl(IntPtr hwnd)=>Hwnd=hwnd;
	public override string ToString()=>$"{nameof(WinControl)}(0x{Hwnd:x})";

	public static WinControl Focused{
		get{
			var result=GetFocus();
			if(result!=IntPtr.Zero) return new WinControl(result);

			var window=GetForegroundWindow();
			if(window==IntPtr.Zero) return new WinControl(IntPtr.Zero);

			var tid=GetWindowThreadProcessId(window,out _);
			var currentThreadId=GetCurrentThreadId();
			if(!AttachThreadInput(currentThreadId,tid,true)) return new WinControl(IntPtr.Zero);

			result=GetFocus();
			AttachThreadInput(currentThreadId,tid,false);
			return new WinControl(result);
		}
		set=>value.Focus();
	}

	public static Dictionary<string,WinControl> GetControls(IntPtr window){
		var controls=new Dictionary<string,WinControl>();
		var counter=new Dictionary<string,int>();
		EnumChildWindows(window,
			(child,_)=>{
				var control=new WinControl(child);
				var controlClass=control.Class??"";

				var count=counter.TryGetValue(controlClass,out var already)?already+1:1;
				counter[controlClass]=count;


				controls[controlClass+count]=control;
				return true;
			},
			0);
		return controls;
	}

	public static WinControl? GetControl(IntPtr window,string classNn){
		var counter=new Dictionary<string,int>();
		WinControl? con=null;
		EnumChildWindows(window,
			(child,_)=>{
				var control=new WinControl(child);
				var controlClass=control.Class??"";

				if(!classNn.StartsWith(controlClass)) return true;

				var count=counter.TryGetValue(controlClass,out var already)?already+1:1;

				if(classNn==controlClass+count){
					con=control;
					return false;
				}
				counter[controlClass]=count;
				return true;
			},
			0);
		return con;
	}

	public bool Enabled{
		get=>AsWindow.Enabled;
		set{
			var window=AsWindow;
			window.Enabled=value;
		}
	}

	public void Focus(){
		var foregroundThreadId=GetWindowThreadProcessId(Window.Hwnd,out _);
		var currentThreadId=GetCurrentThreadId();
		AttachThreadInput(currentThreadId,foregroundThreadId,true);
		SetFocus(Hwnd);
		AttachThreadInput(currentThreadId,foregroundThreadId,false);
	}

	#region Info
	public string Text{
		get{
			var titleSize=SendMessage(WindowMessage.WM_GETTEXTLENGTH,0,0);
			if(titleSize==0) return "";

			var title=new StringBuilder(titleSize+1);
			SendMessage(WindowMessage.WM_GETTEXT,title.Capacity,title);
			return title.ToString();
		}
		set=>SendMessage(WindowMessage.WM_SETTEXT,0,value.ReplaceLineEndings());
	}
	public string? Class{
		get{
			try{
				var title=new StringBuilder(256);
				if(GetClassName(Hwnd,title,title.Capacity)==0) return null;
				return title.ToString();
			} catch(AccessViolationException){
				return null;
			}
		}
	}

	public IntPtr ParentHwnd=>GetParent(Hwnd);
	public WinWindow ParentWindow=>new(GetParent(Hwnd));
	public WinControl ParentControl=>new(GetParent(Hwnd));

	public WinWindow Window{
		get{
			for(var hwnd=Hwnd;;){
				var parent=GetParent(hwnd);
				if(parent==IntPtr.Zero) return new WinWindow(hwnd);
				hwnd=parent;
			}
		}
	}

	public NativeRect Rect=>AsWindow.WindowRect;

	public NativeRect ClientRect{
		get{
			GetClientRect(Hwnd,out var rect);
			MapWindowPoints(Hwnd,ParentHwnd,ref rect,2);
			return rect;
		}
	}
	#endregion

	#region Interact
	[Obsolete("Use AsComboBox.SelectUsingKeyboard")]
	public void SetComboBox(string text,int tryCount=5)=>SetComboBox(text,text[0]);

	[Obsolete("Use AsComboBox.SelectUsingKeyboard")]
	public void SetComboBox(string text,char firstChar,int tryCount=5){
		while(tryCount-->0){
			SendChar(firstChar);
			if(Text.Equals(text,StringComparison.OrdinalIgnoreCase)) return;
		}
		throw new Exception("Error setting combobox to "+text);
	}

	public void SendChar(char c,bool sync=true){//async doesn't make much sense here
		if(sync) SendMessage(WindowMessage.WM_CHAR,c,0);
		else PostMessage(WindowMessage.WM_CHAR,c,0);
	}

	public void SendKey(Keys keys,bool sync=false){
		if(sync){
			SendMessage(WindowMessage.WM_KEYDOWN,(int)keys,0);
			SendMessage(WindowMessage.WM_KEYUP,(int)keys,0);
		} else{
			PostMessage(WindowMessage.WM_KEYDOWN,(int)keys,0);
			PostMessage(WindowMessage.WM_KEYUP,(int)keys,0);
		}
	}

	public void Click(bool sync=false){
		if(sync) SendMessage(WindowMessage.BM_CLICK,0,0);
		else PostMessage(WindowMessage.BM_CLICK,0,0);
	}

	public void Click(MouseButtons buttons,bool sync=false)=>Click(0,0,buttons,sync);
	public void Click(Point point,MouseButtons buttons=MouseButtons.Left,bool sync=false)=>Click(point.X,point.Y,buttons,sync);

	public void Click(int x,int y,MouseButtons buttons=MouseButtons.Left,bool sync=false){
		var lParam=(y<<16)|x;

		var wParam=buttons switch{
			MouseButtons.XButton1=>1<<16,
			MouseButtons.XButton2=>2<<16,
			_=>0,
		};

		var down=buttons switch{
			MouseButtons.None=>throw new Exception("Can't click None button"),
			MouseButtons.Left=>WindowMessage.WM_LBUTTONDOWN,
			MouseButtons.Right=>WindowMessage.WM_RBUTTONDOWN,
			MouseButtons.Middle=>WindowMessage.WM_MBUTTONDOWN,
			MouseButtons.XButton1=>WindowMessage.WM_XBUTTONDOWN,
			MouseButtons.XButton2=>WindowMessage.WM_XBUTTONDOWN,
			_=>throw new ArgumentOutOfRangeException(nameof(buttons),buttons,null),
		};
		var up=buttons switch{
			MouseButtons.None=>throw new Exception("Can't click None button"),
			MouseButtons.Left=>WindowMessage.WM_LBUTTONUP,
			MouseButtons.Right=>WindowMessage.WM_RBUTTONUP,
			MouseButtons.Middle=>WindowMessage.WM_MBUTTONUP,
			MouseButtons.XButton1=>WindowMessage.WM_XBUTTONUP,
			MouseButtons.XButton2=>WindowMessage.WM_XBUTTONUP,
			_=>throw new ArgumentOutOfRangeException(nameof(buttons),buttons,null),
		};

		if(sync){
			SendMessage(down,wParam,lParam);
			SendMessage(up,wParam,lParam);
		} else{
			SendMessage(WindowMessage.WM_NULL,0,0);//At least try to synchronize a bit
			PostMessage(down,wParam,lParam);
			PostMessage(up,wParam,lParam);
		}
	}

	public void DoubleClick(bool sync=false)=>DoubleClick(0,0,sync);
	public void DoubleClick(Point point,bool sync=false)=>DoubleClick(point.X,point.Y,sync);

	public void DoubleClick(int x,int y,bool sync=false){
		var lParam=(y<<16)|x;
		SendMessage(WindowMessage.WM_NULL,0,0);
		if(sync) SendMessage(WindowMessage.WM_LBUTTONDBLCLK,0,lParam);
		else PostMessage(WindowMessage.WM_LBUTTONDBLCLK,0,lParam);
	}

	public WinComboBox AsComboBox=>new(this);
	#endregion

	#region Message
	[Obsolete("Use the overload with WindowMessage enum instead")]
	public int SendMessage(int msg,int wParam,int lParam)=>SendMessage((WindowMessage)msg,wParam,lParam);

	[Obsolete("Use the overload with WindowMessage enum instead")]
	public int SendMessage(int msg,int wParam,IntPtr lParam)=>SendMessage((WindowMessage)msg,wParam,lParam);

	[Obsolete("Use the overload with WindowMessage enum instead")]
	public int SendMessage<T>(int msg,int wParam,ref T lParam) where T : struct=>SendMessage((WindowMessage)msg,wParam,ref lParam);

	[Obsolete("Use the overload with WindowMessage enum instead")]
	public bool PostMessage(int msg,int wParam,int lParam)=>PostMessage((WindowMessage)msg,wParam,lParam);

	[Obsolete("Use the overload with WindowMessage enum instead")]
	public bool PostMessage(int msg,int wParam,IntPtr lParam)=>PostMessage((WindowMessage)msg,wParam,lParam);

	[Obsolete("Use the overload with WindowMessage enum instead")]
	public bool PostMessage<T>(int msg,int wParam,ref T lParam) where T : struct=>PostMessage((WindowMessage)msg,wParam,ref lParam);


	public void SendNull()=>AsWindow.SendNull();
	public void SendNull(int count)=>AsWindow.SendNull(count);
	public void SendNull(int count,TimeSpan delay)=>AsWindow.SendNull(count,delay);


	public int SendMessage(WindowMessage msg,int wParam,int lParam)=>AsWindow.SendMessage(msg,wParam,lParam);

	public int SendMessage(WindowMessage msg,int wParam,IntPtr lParam)=>AsWindow.SendMessage(msg,wParam,lParam);
	public int SendMessage(WindowMessage msg,int wParam,StringBuilder lParam)=>AsWindow.SendMessage(msg,wParam,lParam);
	public int SendMessage(WindowMessage msg,int wParam,string lParam)=>AsWindow.SendMessage(msg,wParam,lParam);

	public int SendMessage<T>(WindowMessage msg,int wParam,ref T lParam) where T : struct=>AsWindow.SendMessage(msg,wParam,ref lParam);
	public bool PostMessage(WindowMessage msg,int wParam,int lParam)=>AsWindow.PostMessage(msg,wParam,lParam);

	public bool PostMessage(WindowMessage msg,int wParam,IntPtr lParam)=>AsWindow.PostMessage(msg,wParam,lParam);

	public bool PostMessage<T>(WindowMessage msg,int wParam,ref T lParam) where T : struct=>AsWindow.PostMessage(msg,wParam,ref lParam);
	#endregion

	#region Operators
	public WinWindow AsWindow=>new(Hwnd);

	public override bool Equals(object? obj)=>obj is WinControl other&&this==other;
	public override int GetHashCode()=>Hwnd.GetHashCode();
	public static bool operator !=(WinControl left,WinControl right)=>!(left==right);
	public static bool operator ==(WinControl left,WinControl right)=>left.Hwnd==right.Hwnd;
	public static implicit operator IntPtr(WinControl win)=>win.Hwnd;


	public static readonly WinControl Zero;
	public bool NonZero(out WinControl win)=>(win=this)!=Zero;
	#endregion

}