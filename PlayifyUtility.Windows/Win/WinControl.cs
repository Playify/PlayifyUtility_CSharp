using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;
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

	public static WinControl GetFocused(){
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

	#region Info
	public string Text{
		get{
			var titleSize=SendMessage(Hwnd,0xE,0,0).ToInt32();//WM_GETTEXTLENGTH
			if(titleSize==0) return "";

			var title=new StringBuilder(titleSize+1);
			SendMessage(Hwnd,0xD,title.Capacity,title);//WM_GETTEXT
			return title.ToString();
		}
		set=>SendMessage(Hwnd,0xC,0,value.ReplaceLineEndings());//WM_SETTEXT
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

	public IntPtr Parent=>GetParent(Hwnd);

	public WinWindow Window{
		get{
			for(var hwnd=Hwnd;;){
				var parent=GetParent(hwnd);
				if(parent==IntPtr.Zero) return new WinWindow(hwnd);
				hwnd=parent;
			}
		}
	}
	#endregion

	#region Actions
	public void SetComboBox(string text,int tryCount=5)=>SetComboBox(text,text[0]);

	public void SetComboBox(string text,char firstChar,int tryCount=5){
		while(tryCount-->0){
			SendChar(firstChar);
			if(Text.Equals(text,StringComparison.OrdinalIgnoreCase)) return;
		}
		throw new Exception("Error setting combobox to "+text);
	}

	public void SendChar(char c)=>SendMessage(Hwnd,0x102,c,0);//0x102=WM_CHAR

	public void Click(bool opensWindow=false){
		SendMessage(Hwnd,0x201,0,0);//WM_LBUTTONDOWN
		if(opensWindow) PostMessage(Hwnd,0x202,0,0);//WM_LBUTTONUP
		else SendMessage(Hwnd,0x202,0,0);//WM_LBUTTONUP
	}

	public void DoubleClick()=>SendMessage(Hwnd,0x203,0,0);//WM_LBUTTONDBLCLK

	public void SendKey(Keys keys){
		PostMessage(Hwnd,0x100,(int)keys,0);//WM_KEYDOWN
		PostMessage(Hwnd,0x101,(int)keys,0);//WM_KEYUP
	}
	#endregion

	public override bool Equals(object? obj)=>obj is WinControl other&&this==other;
	public override int GetHashCode()=>Hwnd.GetHashCode();
	public static bool operator!=(WinControl left,WinControl right)=>!(left==right);
	public static bool operator==(WinControl left,WinControl right)=>left.Hwnd==right.Hwnd;
}