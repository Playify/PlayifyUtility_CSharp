using JetBrains.Annotations;
using PlayifyUtility.Windows.Features.Interact;
using PlayifyUtility.Windows.Utils;

namespace PlayifyUtility.Windows.Features.Hooks;

[PublicAPI]
// ReSharper disable CommentTypo
public static class GlobalClipboardHook{
	private static Action? _onChange;
	internal static void TriggerChange()=>_onChange?.Invoke();

	public static event Action OnClipboardChange{
		add{
			ClipboardHookControl.Init();//TODO unhook afterwards
			_onChange+=value;
		}
		remove=>_onChange-=value;
	}


	public static Task<Image> GetNextImage(TimeSpan? timeout=null,CancellationToken cancel=default)=>GetNext(Clipboard.GetImage,timeout,cancel);
	public static Task<string> GetNextString(TimeSpan? timeout=null,CancellationToken cancel=default)=>GetNext(Clipboard.GetText,timeout,cancel);

	public static async Task<T> GetNext<T>(Func<T> getter,TimeSpan? timeout=null,CancellationToken cancel=default){
		if(timeout.TryGet(out var timeSpan)){
			using var timer=new CancellationTokenSource(timeSpan);
			using var cts=CancellationTokenSource.CreateLinkedTokenSource(timer.Token,cancel);
			return await GetNext(getter,null,cts.Token);
		}

		var tcs=new TaskCompletionSource<T>();

		Action action=null!;
		action=()=>{
			tcs.TrySetResult(getter());
			OnClipboardChange-=action;
		};

		OnClipboardChange+=action;

		// ReSharper disable once UseAwaitUsing
		using var _=cancel.Register(()=>{
			OnClipboardChange-=action;
			tcs.TrySetCanceled(cancel);
		});
		return await tcs.Task;
	}

	public static Task<Image?> CopyImage(CancellationToken cancel=default)=>CopyImage(true,cancel);
	public static Task<Image?> CopyImage(bool revert,CancellationToken cancel=default)=>Copy(Clipboard.GetImage,revert,cancel);
	public static Task<string?> CopyString(CancellationToken cancel=default)=>CopyString(true,cancel);
	public static Task<string?> CopyString(bool revert,CancellationToken cancel=default)=>Copy(()=>Clipboard.ContainsText()?Clipboard.GetText():null,revert,cancel);

	public static async Task<T> Copy<T>(Func<T> getter,bool revert=true,CancellationToken cancel=default){
		await ClipboardHookControl.UiThread.JumpAsync();//Clipboard can only be edited on MainThread

		var text=Clipboard.ContainsText()?Clipboard.GetText():null;
		var image=text!=null&&Clipboard.ContainsImage()?Clipboard.GetImage():null;//otherwise would copy image of excel sheet
		var fileDropList=Clipboard.ContainsFileDropList()?Clipboard.GetFileDropList():null;

		var task=GetNext(getter,TimeSpan.FromSeconds(10),cancel);

		new Send().Hide().Combo(ModifierKeys.Control,Keys.C).SendNow();//needs to be hidden to not activate other hotkeys

		var s=await task;

		await ClipboardHookControl.UiThread.JumpAsync();
		if(!revert) return s;
		
		//Can only restore if successfully copied, therefore no try finally block
		if(text!=null) Clipboard.SetText(text);
		else if(image!=null) Clipboard.SetImage(image);
		else if(fileDropList!=null) Clipboard.SetFileDropList(fileDropList);
		else Clipboard.Clear();
		return s;
	}
}