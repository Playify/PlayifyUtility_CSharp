using System.Diagnostics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Utils;
using PlayifyUtility.Windows.Win;
using static System.Windows.Forms.Keys;

namespace PlayifyUtility.Windows.Features.Interact;

[PublicAPI]
public class Send{

	#region Constants
	public static readonly IntPtr ProcessHandle=Process.GetCurrentProcess().Handle;
	private static readonly Keys[] Extended=[
		RMenu,RControlKey,Insert,Delete,Home,End,PageDown,PageUp,
		Up,Down,Left,Right,NumLock,Pause,PrintScreen,Divide,Enter,
		Apps,LWin,RWin,
		BrowserBack,BrowserFavorites,BrowserForward,BrowserForward,BrowserHome,BrowserRefresh,BrowserSearch,BrowserStop,
		VolumeDown,VolumeMute,VolumeUp,
		MediaStop,MediaNextTrack,MediaPlayPause,MediaPreviousTrack,
		LaunchApplication1,LaunchApplication2,LaunchMail,
	];

	private static readonly Keys[] MouseButtons=[
		LButton,RButton,MButton,XButton1,XButton2,
	];
	#endregion

	#region Hidden
	private bool _hidden;

	public Send Hide(bool hide=true){
		_hidden=hide;
		return this;
	}
	#endregion

	#region Send
	public bool IsEmpty=>_list.Count==0;

	private readonly List<Input> _list=[];

	private Send Add(Input input){
		_list.Add(input);
		return this;
	}

	public void SendOn(UiThread thread){
		if(!IsEmpty) thread.Invoke(SendNow);
	}

	public void SendOn(SynchronizationContext ctx){
		if(!IsEmpty) ctx.Invoke(SendNow);
	}

	public void SendNow(){
		if(_startingMods.TryGet(out var startingMods)) Mods=startingMods;

		while(_list.Count!=0){
			if(_list[0].Type==-1){//Sleep node
				var time=_list[0].InputUnion.ki.Time;
				_list.RemoveAt(0);
				if(time!=0) Thread.Sleep(time);
				else Application.DoEvents();
			} else{
				var arr=_list.TakeWhile(i=>i.Type!=-1).ToArray();
				_list.RemoveRange(0,arr.Length);
				SendInput(arr.Length,arr,Marshal.SizeOf<Input>());
			}
		}
	}
	#endregion

	#region Modifiers
	[Flags]
	public enum ModsEnum:byte{
		LShift=1,
		RShift=2,
		LCtrl=4,
		RCtrl=8,
		LAlt=16,
		RAlt=32,
		LWin=64,
		RWin=128,
	}

	private ModsEnum? _startingMods;
	private ModsEnum? _currentMods;

	public ModsEnum Mods{
		get{
			var mods=_currentMods??=((GetKeyState(LShiftKey)&128)!=0?ModsEnum.LShift:0)|
			                        ((GetKeyState(RShiftKey)&128)!=0?ModsEnum.RShift:0)|
			                        ((GetKeyState(LControlKey)&128)!=0?ModsEnum.LCtrl:0)|
			                        ((GetKeyState(RControlKey)&128)!=0?ModsEnum.RCtrl:0)|
			                        ((GetKeyState(LMenu)&128)!=0?ModsEnum.LAlt:0)|
			                        ((GetKeyState(RMenu)&128)!=0?ModsEnum.RAlt:0)|
			                        ((GetKeyState(LWin)&128)!=0?ModsEnum.LWin:0)|
			                        ((GetKeyState(RWin)&128)!=0?ModsEnum.RWin:0);
			_startingMods??=mods;
			return mods;
		}
		set{
			var changes=(_currentMods??=Mods)^value;
			if((changes&ModsEnum.LShift)!=0) Key(LShiftKey,(value&ModsEnum.LShift)!=0);
			if((changes&ModsEnum.RShift)!=0) Key(RShiftKey,(value&ModsEnum.RShift)!=0);
			if((changes&ModsEnum.LCtrl)!=0) Key(LControlKey,(value&ModsEnum.LCtrl)!=0);
			if((changes&ModsEnum.RCtrl)!=0) Key(RControlKey,(value&ModsEnum.RCtrl)!=0);
			if((changes&ModsEnum.LAlt)!=0) Key(LMenu,(value&ModsEnum.LAlt)!=0);
			if((changes&ModsEnum.RAlt)!=0) Key(RMenu,(value&ModsEnum.RAlt)!=0);
			if((changes&ModsEnum.LWin)!=0) Key(LWin,(value&ModsEnum.LWin)!=0);
			if((changes&ModsEnum.RWin)!=0) Key(RWin,(value&ModsEnum.RWin)!=0);
			_currentMods=value;
		}
	}

	public Send GetMods(out ModsEnum mods){
		mods=Mods;
		return this;
	}

	public Send SetMods(ModsEnum mods){
		Mods=mods;
		return this;
	}

	public Send Mod(ModifierKeys mod){//Sets the active modifiers
		var mods=Mods;
		var start=_startingMods??=mods;


		if((mod&ModifierKeys.Shift)==0) mods&=~(ModsEnum.LShift|ModsEnum.RShift);
		else if((mods&(ModsEnum.LShift|ModsEnum.RShift))==0)
			mods|=(start&ModsEnum.RShift)!=0?ModsEnum.RShift:ModsEnum.LShift;

		if((mod&ModifierKeys.Control)==0) mods&=~(ModsEnum.LCtrl|ModsEnum.RCtrl);
		else if((mods&(ModsEnum.LCtrl|ModsEnum.RCtrl))==0)
			mods|=(start&ModsEnum.RCtrl)!=0?ModsEnum.RCtrl:ModsEnum.LCtrl;

		if((mod&ModifierKeys.Alt)==0) mods&=~(ModsEnum.LAlt|ModsEnum.RAlt);
		else if((mods&(ModsEnum.LAlt|ModsEnum.RAlt))==0)
			mods|=(start&ModsEnum.LAlt)!=0||(mod&ModifierKeys.AltGr)!=ModifierKeys.AltGr?ModsEnum.LAlt:ModsEnum.RAlt;

		if((mod&ModifierKeys.Windows)==0) mods&=~(ModsEnum.LWin|ModsEnum.RWin);
		else if((mods&(ModsEnum.LWin|ModsEnum.RWin))==0)
			mods|=(start&ModsEnum.RWin)!=0?ModsEnum.RWin:ModsEnum.LWin;

		Mods=mods;
		return this;
	}

	public Send Mod(ModifierKeys mod,bool down){
		var mods=Mods;
		var start=_startingMods??=mods;

		if((mod&ModifierKeys.Shift)!=0)
			if(!down) mods&=~(ModsEnum.LShift|ModsEnum.RShift);
			else if((mods&(ModsEnum.LShift|ModsEnum.RShift))==0)
				mods|=(start&ModsEnum.RShift)!=0?ModsEnum.RShift:ModsEnum.LShift;
		if((mod&ModifierKeys.Control)!=0)
			if(!down) mods&=~(ModsEnum.LCtrl|ModsEnum.RCtrl);
			else if((mods&(ModsEnum.LCtrl|ModsEnum.RCtrl))==0)
				mods|=(start&ModsEnum.RCtrl)!=0?ModsEnum.RCtrl:ModsEnum.LCtrl;
		if((mod&ModifierKeys.Alt)!=0)
			if(!down) mods&=~(ModsEnum.LAlt|ModsEnum.RAlt);
			else if((mods&(ModsEnum.LAlt|ModsEnum.RAlt))==0)
				mods|=(start&ModsEnum.LAlt)!=0||(mod&ModifierKeys.AltGr)!=ModifierKeys.AltGr?ModsEnum.LAlt:ModsEnum.RAlt;
		if((mod&ModifierKeys.Windows)!=0)
			if(!down) mods&=~(ModsEnum.LWin|ModsEnum.RWin);
			else if((mods&(ModsEnum.LWin|ModsEnum.RWin))==0)
				mods|=(start&ModsEnum.RWin)!=0?ModsEnum.RWin:ModsEnum.LWin;

		Mods=mods;
		return this;
	}
	#endregion

	#region Keyboard
	public Send Key(Keys key,bool? down=null)
		=>!down.TryGet(out var d)
			  ?Key(key,true).Key(key,false)
			  :MouseButtons.Contains(key)
				  ?_MouseButton(key,d)
				  :Add(new Input{
					  Type=1,
					  InputUnion=new InputUnion{
						  ki=new KeyBdInput{
							  WVk=(short)key,
							  WScan=MapVirtualKey((short)key,0),
							  Time=0,
							  DwFlags=(d?0:2)|(Extended.Contains(key)?1:0),
							  DwExtraInfo=_hidden?ProcessHandle:IntPtr.Zero,
						  },
					  },
				  });

	public Send Key(Keys key,int cnt,int delayMillis=-1){
		while(cnt-->0) Key(key).Wait(delayMillis);
		return this;
	}

	public Send Combo(ModifierKeys mod,Keys key,int repeat=1){
		var mods=Mods;
		Mod(mod).Key(key,repeat);
		Mods=mods;
		return this;
	}

	/**Warning: Does not revert changes to modifiers*/
	private Send Char(char c,bool? down=null){
		if(c=='\n') c='\r';

		int num1=VkKeyScan(c);
		if(num1==-1) return _Unicode(c,down);
		Mod(ModifierKeys.Shift,(num1&256)!=0);
		Mod(ModifierKeys.Control,(num1&512)!=0);
		Mod(ModifierKeys.Alt,(num1&1024)!=0);
		Mod(ModifierKeys.Windows,false);
		return Key((Keys)(num1&255),down);
	}

	public Send Text(string s,int wait=-1){
		s=s.Replace("\r\n","\n");
		if(s=="") return this;

		var mods=Mods;

		foreach(var c in s){
			Char(c);
			Wait(wait);
		}

		Mods=mods;

		return this;
	}


	private Send _Unicode(char c,bool? down)
		=>!down.TryGet(out var d)
			  ?_Unicode(c,true)._Unicode(c,false)
			  :Add(new Input{
				  Type=1,
				  InputUnion=new InputUnion{
					  ki=new KeyBdInput{
						  WVk=0,
						  WScan=(short)c,
						  Time=0,
						  DwFlags=d?4:6,
						  DwExtraInfo=_hidden?ProcessHandle:IntPtr.Zero,
					  },
				  },
			  });
	#endregion

	#region Mouse
	private Send _MouseButton(Keys key,bool down)
		=>Add(new Input{
			Type=0,
			InputUnion=new InputUnion{
				mi=new MouseInput{
					Dx=0,
					Dy=0,
					MouseData=key switch{
						XButton1=>1,
						XButton2=>2,
						_=>0,
					},
					Time=0,
					DwFlags=key switch{
						LButton=>down?0x2:0x4,
						RButton=>down?0x8:0x10,
						MButton=>down?0x20:0x40,
						XButton1=>down?0x80:0x100,
						XButton2=>down?0x80:0x100,
						_=>throw new ArgumentOutOfRangeException(nameof(key),key,null),
					},
					DwExtraInfo=_hidden?ProcessHandle:IntPtr.Zero,
				},
			},
		});


	public Send MouseMoveWindow(WinWindow window,(int X,int Y) delta)=>MouseMoveWindow(window,delta.X,delta.Y);
	public Send MouseMoveWindow(WinWindow window,Point delta)=>MouseMoveWindow(window,delta.X,delta.Y);

	public Send MouseMoveWindow(WinWindow window,int dx,int dy){
		var rect=window.WindowRect;
		if(dx>rect.Right-rect.Left) throw new OverflowException("dx coordinate is bigger than the window width");
		if(dy>rect.Bottom-rect.Top) throw new OverflowException("dy coordinate is bigger than the window height");
		dx=dx<0?rect.Right+dx:rect.Left+dx;
		dy=dy<0?rect.Bottom+dy:rect.Top+dy;

		return MouseMove(dx,dy);
	}


	public Send MouseMoveForeground(WinWindow window,(int X,int Y) delta)=>MouseMoveForeground(window,delta.X,delta.Y);
	public Send MouseMoveForeground(WinWindow window,Point delta)=>MouseMoveForeground(window,delta.X,delta.Y);

	public Send MouseMoveForeground(WinWindow window,int dx,int dy)
		=>WinWindow.Foreground!=window
			  ?throw new OverflowException("Window is not focused")
			  :MouseMoveWindow(window,dx,dy);

	public Send MouseMoveRelative(int dx,int dy)
		=>Add(new Input{
			Type=0,
			InputUnion=new InputUnion{
				mi=new MouseInput{
					Dx=dx,
					Dy=dy,
					MouseData=0,
					Time=0,
					DwFlags=1,
					DwExtraInfo=_hidden?ProcessHandle:IntPtr.Zero,
				},
			},
		});

	public Send MouseMove(int x,int y)
		=>Add(new Input{
			Type=0,
			InputUnion=new InputUnion{
				mi=new MouseInput{
					Dx=x*65536/GetSystemMetrics(SmCxScreen),
					Dy=y*65536/GetSystemMetrics(SmCyScreen),
					MouseData=0,
					Time=0,
					DwFlags=0x8001,
					DwExtraInfo=_hidden?ProcessHandle:IntPtr.Zero,
				},
			},
		});

	public Send MouseScroll(int downwards)
		=>Add(new Input{
			Type=0,
			InputUnion=new InputUnion{
				mi=new MouseInput{
					Dx=0,
					Dy=0,
					MouseData=downwards,
					Time=0,
					DwFlags=0x800,
					DwExtraInfo=_hidden?ProcessHandle:IntPtr.Zero
				},
			},
		});

	public Send Click(Keys mouseButton=LButton)=>Key(mouseButton);
	public Send Click(int x,int y,Keys mouseButton=LButton)=>MouseMove(x,y).Key(mouseButton);
	public Send ClickRelative(int x,int y,Keys mouseButton=LButton)=>MouseMoveRelative(x,y).Key(mouseButton);
	#endregion

	#region Extras
	public Send Wait(int delayMillis=0)=>delayMillis<0?this:Add(new Input{Type=-1,InputUnion={ki=new KeyBdInput{Time=delayMillis}}});
	#endregion

	#region Pinvoke
	[DllImport("user32.dll")]
	private static extern uint SendInput(int cInputs,Input[] pInputs,int cbSize);

	[DllImport("user32.dll",CharSet=CharSet.Auto)]
	private static extern short VkKeyScan(char key);

	[DllImport("user32.dll",CharSet=CharSet.Auto)]
	private static extern short MapVirtualKey(short uCode,short uMapType);

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	private struct MouseInput{
		public int Dx;
		public int Dy;
		public int MouseData;
		public int DwFlags;
		public int Time;
		public IntPtr DwExtraInfo;
	}

	[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
	private struct KeyBdInput{
		public short WVk;
		public short WScan;
		public int DwFlags;
		public int Time;
		public IntPtr DwExtraInfo;
	}

#pragma warning disable CS0649
	private struct HardwareInput{
		public int UMsg;
		public short WParamL;
		public short WParamH;
	}
#pragma warning restore CS0649

	private struct Input{
		public int Type;
		public InputUnion InputUnion;
	}

	[StructLayout(LayoutKind.Explicit)]
	private struct InputUnion{
		[FieldOffset(0)]public MouseInput mi;
		[FieldOffset(0)]public KeyBdInput ki;
		[FieldOffset(0)]public HardwareInput hi;
	}

	[DllImport("USER32.dll")]
	private static extern short GetKeyState(Keys key);


	[DllImport("user32.dll")]
	private static extern int GetSystemMetrics(int nIndex);

	private const int SmCxScreen=0;
	private const int SmCyScreen=1;
	#endregion

}