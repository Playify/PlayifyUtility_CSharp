using System.Runtime.InteropServices;
using PlayifyUtility.Windows.Win.Native;

namespace PlayifyUtility.Windows.Features.Hooks;

internal class ClipboardHookControl:Control{
	private static UiThread? _thread;
	private static ClipboardHookControl? _initialized;
	private IntPtr _nextClipboardViewer;

	internal static UiThread UiThread=>_thread??=UiThread.Create(nameof(GlobalClipboardHook));
	public static void Init()=>_initialized??=UiThread.Invoke(()=>new ClipboardHookControl());

	private ClipboardHookControl(){
		if(Thread.CurrentThread.GetApartmentState()!=ApartmentState.STA)
			throw new ThreadStateException("Not running on STA Thread");
		Visible=false;
		_nextClipboardViewer=SetClipboardViewer(Handle);
	}

	protected override void Dispose(bool disposing)=>ChangeClipboardChain(Handle,_nextClipboardViewer);

	protected override void WndProc(ref Message m){
		try{
			switch((WindowMessage)m.Msg){
				case WindowMessage.WM_CHANGECBCHAIN:
					if(m.WParam==_nextClipboardViewer) _nextClipboardViewer=m.LParam;
					else SendMessage(_nextClipboardViewer,m.Msg,m.WParam,m.LParam);
					break;
				case WindowMessage.WM_DRAWCLIPBOARD:
					GlobalClipboardHook.TriggerChange();
					SendMessage(_nextClipboardViewer,m.Msg,m.WParam,m.LParam);
					break;
			}
			base.WndProc(ref m);
		} catch(Exception e){
			Console.WriteLine(e);
		}
	}

	#region DLL imports
	[DllImport("user32.dll")]
	private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

	[DllImport("user32.dll")]
	private static extern bool ChangeClipboardChain(IntPtr hWndRemove,IntPtr hWndNewNext);

	[DllImport("user32.dll")]
	private static extern IntPtr SendMessage(IntPtr hWnd,int msg,IntPtr wParam,IntPtr lParam);
	#endregion
}