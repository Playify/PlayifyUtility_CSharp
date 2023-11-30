using System.Runtime.InteropServices;
using JetBrains.Annotations;
using PlayifyUtility.Windows.Features.Interact;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
// ReSharper disable CommentTypo
public static class GlobalClipboardHook{//TODO make it so it always uses main thread
	
	public static void Hook()=>_instance??=MainThread.Invoke(()=>new HookForm());

	public static Task<string> GetNextClipboard(){
		var cts=new CancellationTokenSource(TimeSpan.FromSeconds(10));
		var tcs=new TaskCompletionSource<string>();

		Hook();
		Action<string> action=s=>{
			tcs.TrySetResult(s);
			cts.Dispose();
		};
		Once.Add(action);
		cts.Token.Register(()=>{
			tcs.TrySetCanceled(cts.Token);
			Once.Remove(action);
		});

		return tcs.Task;
	}

	public static async Task<string> CopyText(){
		var image=Clipboard.ContainsImage()?Clipboard.GetImage():null;
		var text=Clipboard.ContainsText()?Clipboard.GetText():null;
		var fileDropList=Clipboard.ContainsFileDropList()?Clipboard.GetFileDropList():null;

		var task=GetNextClipboard();
		
		new Send().Hide().Mod(ModifierKeys.Control).Key(Keys.C).SendNow();//needs to be hidden to not activate other hotkeys

		var s=await task;
		
		//Can only restore if successfully copied, therefore no try finally block
		if(image!=null) Clipboard.SetImage(image);
		else if(text!=null) Clipboard.SetText(text);
		else if(fileDropList!=null) Clipboard.SetFileDropList(fileDropList);
		else Clipboard.Clear();

		return s;
	}
	private class HookForm:Form{
		private IntPtr _hWndNextWindow;

		public HookForm(){
			Visible=false;
			WindowState=FormWindowState.Minimized;
			Hide();
			_=RegisterWindowMessage("SHELLHOOK");
			RegisterShellHookWindow(Handle);
		}

		protected override void WndProc(ref Message m){//https://github.com/magicmanam/windows-clipboard-viewer/blob/master/magicmanam.Windows.ClipboardViewer/ClipboardViewer.cs
			try{
				switch(m.Msg){
					case 0x001://WM_CREATE
						_hWndNextWindow=SetClipboardViewer(Handle);
						break;
					case 0x0002://WM_DESTROY
						ChangeClipboardChain(Handle,_hWndNextWindow);
						break;
					case 0x030D:// WM_CHANGECBCHAIN
						if(m.WParam==_hWndNextWindow) _hWndNextWindow=m.LParam;
						else if(_hWndNextWindow!=IntPtr.Zero) SendMessage(_hWndNextWindow,m.Msg,m.WParam,m.LParam);
						break;
					case 0x0308:// WM_DRAWCLIPBOARD
						var actions=Once.Concat(Always).ToArray();
						Once.Clear();
						if(actions.Length!=0){
							if(Clipboard.ContainsText()){
								var s=Clipboard.GetText();
								foreach(var action in actions) action(s);
							} else Console.WriteLine("Error getting selected Text");
						}
						SendMessage(_hWndNextWindow,m.Msg,m.WParam,m.LParam);
						break;
				}
				base.WndProc(ref m);
			} catch(Exception e){
				Console.WriteLine(e);
			}
		}

		#region DLL imports
		[DllImport("user32.dll",EntryPoint="RegisterWindowMessageA",CharSet=CharSet.Unicode)]
		private static extern int RegisterWindowMessage(string lpString);

		[DllImport("user32.dll")]
		private static extern int RegisterShellHookWindow(IntPtr hWnd);


		[DllImport("user32.dll")]
		private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

		[DllImport("user32.dll")]
		[return:MarshalAs(UnmanagedType.Bool)]
		private static extern bool ChangeClipboardChain(IntPtr hWndRemove,IntPtr hWndNewNext);

		[DllImport("user32.dll",SetLastError=true)]
		private static extern int SendMessage(IntPtr hWnd,int msg,IntPtr wParam,IntPtr lParam);
		#endregion
	}

	private static HookForm? _instance;
	private static readonly HashSet<Action<string>> Once=new();
	private static readonly HashSet<Action<string>> Always=new();
}