using JetBrains.Annotations;
using PlayifyUtility.Windows.Features.Interact;

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

	public static async Task<T> GetNext<T>(Func<T?> getter,TimeSpan? timeout=null,CancellationToken cancel=default) where T : notnull{
		if(timeout is{} timeSpan){
			using var timer=new CancellationTokenSource(timeSpan);
			using var cts=CancellationTokenSource.CreateLinkedTokenSource(timer.Token,cancel);
			return await GetNext(getter,null,cts.Token);
		}

		var tcs=new TaskCompletionSource<T>();

		Action action=null!;
		action=()=>{
			var result=getter();
			if(result==null) return;
			tcs.TrySetResult(result);
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

	public static Task<Image> CopyImage(CancellationToken cancel=default)=>CopyImage(true,cancel);
	public static Task<Image> CopyImage(bool revert,CancellationToken cancel=default)=>Copy(Clipboard.GetImage,revert,cancel);
	public static Task<string> CopyString(CancellationToken cancel=default)=>CopyString(true,cancel);

	public static Task<string> CopyString(bool revert,CancellationToken cancel=default)
		=>Copy(()=>Clipboard.ContainsText()?Clipboard.GetText():null,revert,cancel);

	public static async Task<T> Copy<T>(Func<T?> getter,bool revert=true,CancellationToken cancel=default) where T : notnull{
		if(revert){
			var (
				text,
				image,
				fileDropList
				)=await ClipboardHookControl.UiThread.InvokeAsync(
					  ()=>(
						      Clipboard.ContainsText()?Clipboard.GetText():null,
						      !Clipboard.ContainsText()&&Clipboard.ContainsImage()?Clipboard.GetImage():null,//otherwise would copy image of excel shee
						      Clipboard.ContainsFileDropList()?Clipboard.GetFileDropList():null
					      ));
			var result=await Copy(getter,false,cancel);

			await ClipboardHookControl.UiThread.InvokeAsync(()=>{
				//Can only restore if successfully copied, therefore no try finally block
				if(text!=null) Clipboard.SetText(text);
				else if(image!=null) Clipboard.SetImage(image);
				else if(fileDropList!=null) Clipboard.SetFileDropList(fileDropList);
				else Clipboard.Clear();
			});

			return result;
		}

		var task=GetNext(getter,TimeSpan.FromSeconds(10),cancel);
		new Send().Hide().Combo(ModifierKeys.Control,Keys.C).SendNow();//needs to be hidden to not activate other hotkeys
		var s=await task;

		return s;
	}
}