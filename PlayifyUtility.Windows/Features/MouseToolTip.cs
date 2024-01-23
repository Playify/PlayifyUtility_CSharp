using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Features.Hooks;
using PlayifyUtility.Windows.Win;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Features;

[SuppressMessage("ReSharper","CommentTypo")]
[PublicAPI]
public static partial class MouseToolTip{
	public static void HideToolTip(){
		_cancel.Cancel();
		if(_toolInfo.lpszText==null) return;
		_toolInfo.lpszText=null;
		SendMessage(_currentToolTip.Hwnd,0x411,0,ref _toolInfo);//TTM_TRACKACTIVATE
	}

	public static void ShowToolTip(string s)=>ShowToolTip(s,TimeSpan.FromSeconds(1));

	public static void ShowToolTip(string s,TimeSpan timeout){
		if(_currentToolTip==default){
			
			var hwnd=CreateWindowEx(0x28,//WS_EX_TRANSPARENT|WS_EX_TOPMOST
			                        "tooltips_class32",//TOOLTIPS_CLASS
			                        null,
			                        0x33,//TTS_NOANIMATE|TTS_NOFADE|TTS_NOPREFIX|TTS_ALWAYSTIP
			                        int.MinValue,
			                        int.MinValue,
			                        int.MinValue,
			                        int.MinValue,//CW_USEDEFAULT
			                        IntPtr.Zero,
			                        IntPtr.Zero,
			                        IntPtr.Zero,
			                        IntPtr.Zero);
			_currentToolTip=new WinWindow(hwnd);

			AppDomain.CurrentDomain.ProcessExit+=(_,_)=>_currentToolTip.DestroyWindow();


			_toolInfo=new ToolInfo();
			_toolInfo.cbSize=(uint) Marshal.SizeOf(_toolInfo);
			_toolInfo.uFlags=0x120;//TTF_TRACK
			_toolInfo.hwnd=IntPtr.Zero;
			_toolInfo.hInst=IntPtr.Zero;
			_toolInfo.uId=(UIntPtr) 0;
			_toolInfo.lpszText=s;
			_toolInfo.rect=new NativeRect();

			SendMessage(_currentToolTip.Hwnd,0x432,0,ref _toolInfo);//TTM_ADDTOOLW

			_currentToolTip.SendMessage(0x418,0,0);//TTM_SETMAXTIPWIDTH
		}
		if(_toolInfo.lpszText!=s){
			_toolInfo.lpszText=s;
			SendMessage(_currentToolTip.Hwnd,0x439,0,ref _toolInfo);//TTM_UPDATETIPTEXTW
		}

		if(!_hooked){
			_hooked=true;
			GlobalMouseHook.MouseMove+=_hookFunc;
		}
		CorrectToolTip(null);

		SendMessage(_currentToolTip.Hwnd,0x411,1,ref _toolInfo);//TTM_TRACKACTIVATE
		_currentToolTip.SetWindowPos(-1,0,0,0,0,0x13);

		CancellationToken token;
		lock(typeof(MouseToolTip)){
			_cancel.Cancel();

			_cancel.Dispose();
			_cancel=new CancellationTokenSource();
			token=_cancel.Token;
		}

		Task.Delay(timeout,token).ContinueWith(_=>HideToolTip(),
		                                               TaskContinuationOptions.OnlyOnRanToCompletion);
	}
}