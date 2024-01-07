using System.Runtime.InteropServices;

namespace PlayifyUtility.Windows.Features.Hooks;

internal class ClipboardHookControl:Control{
	private static ClipboardHookControl? _initialized;
	private IntPtr _nextClipboardViewer;

	public static void Init()=>_initialized??=MainThread.Invoke(()=>new ClipboardHookControl());

	public ClipboardHookControl(){
		if(Thread.CurrentThread.GetApartmentState()!=ApartmentState.STA){
			throw new ThreadStateException($"{nameof(GlobalClipboardHook)} only works when the MainThread is initialized and an STA-Thread.\n"+
			                               $"* Add the [STAThread] Attribute to the Main method.\n"+
			                               $"* Make sure it's return type is not 'async Task'.\n"+
			                               $"* Call {nameof(MainThread)}.{nameof(MainThread.Init)} inside the Main method.");


		}
		Visible=false;
		_nextClipboardViewer=SetClipboardViewer(Handle);
	}

	protected override void Dispose(bool disposing)=>ChangeClipboardChain(Handle,_nextClipboardViewer);

	protected override void WndProc(ref Message m){
		try{
			switch(m.Msg){
				case 0x030D:// WM_CHANGECBCHAIN
					if(m.WParam==_nextClipboardViewer) _nextClipboardViewer=m.LParam;
					else SendMessage(_nextClipboardViewer,m.Msg,m.WParam,m.LParam);
					break;
				case 0x0308:// WM_DRAWCLIPBOARD
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