using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PlayifyUtility.Windows.Helpers;

[PublicAPI]
public static partial class ToolTip{
	public static void HideToolTip(){
		_cancel.Cancel();
		if(_toolInfo.lpszText==null) return;
		_toolInfo.lpszText=null;
		SendMessage(_currentToolTip,0x411,0,ref _toolInfo);//TTM_TRACKACTIVATE
	}

	public static void ShowToolTip(string s)=>ShowToolTip(s,TimeSpan.FromSeconds(1));
	public static void ShowToolTip(string s,TimeSpan timeout){
		if(_currentToolTip==IntPtr.Zero){
			_currentToolTip=CreateWindowEx(0x28,//WS_EX_TRANSPARENT|WS_EX_TOPMOST
			                               "tooltips_class32",//TOOLTIPS_CLASS
			                               null,0x33,//TTS_NOANIMATE|TTS_NOFADE|TTS_NOPREFIX|TTS_ALWAYSTIP
			                               int.MinValue,int.MinValue,int.MinValue,int.MinValue,//CW_USEDEFAULT
			                               IntPtr.Zero,IntPtr.Zero,IntPtr.Zero,IntPtr.Zero);

			AppDomain.CurrentDomain.ProcessExit+=(_,_)=>NativeMethods.DestroyWindow(_currentToolTip);


			_toolInfo=new ToolInfo();
			_toolInfo.cbSize=(uint) Marshal.SizeOf(_toolInfo);
			_toolInfo.uFlags=0x120;//TTF_TRACK
			_toolInfo.hwnd=IntPtr.Zero;
			_toolInfo.hInst=IntPtr.Zero;
			_toolInfo.uId=(UIntPtr) 0;
			_toolInfo.lpszText=s;
			_toolInfo.rect=new NativeMethods.Rect();

			SendMessage(_currentToolTip,0x432,0,ref _toolInfo);//TTM_ADDTOOLW

			NativeMethods.SendMessage(_currentToolTip,0x418,0,0);//TTM_SETMAXTIPWIDTH
		}
		if(_toolInfo.lpszText!=s){
			_toolInfo.lpszText=s;
			SendMessage(_currentToolTip,0x439,0,ref _toolInfo);//TTM_UPDATETIPTEXTW
		}

		CorrectToolTip(null);

		SendMessage(_currentToolTip,0x411,1,ref _toolInfo);//TTM_TRACKACTIVATE
		NativeMethods.SetWindowPos(_currentToolTip,new IntPtr(-1),0,0,0,0,0x13);

		_cancel.Cancel();
		if(!_cancel.TryReset()) _cancel=new CancellationTokenSource();
		_cancel.CancelAfter(timeout);
		Task.Delay(timeout,_cancel.Token).ContinueWith(t=>{
			if(t.IsCompletedSuccessfully) HideToolTip();
		});
	}
}