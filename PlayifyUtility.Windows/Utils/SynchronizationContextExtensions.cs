using System.Runtime.CompilerServices;

namespace PlayifyUtility.Windows.Utils;

public static class SynchronizationContextExtensions{
	public static void BeginInvoke(this SynchronizationContext ctx,Action a)=>ctx.Post(_=>a(),null);
	public static void Invoke(this SynchronizationContext ctx,Action a)=>ctx.Send(_=>a(),null);

	public static T Invoke<T>(this SynchronizationContext ctx,Func<T> a){
		T result=default!;
		ctx.Send(_=>result=a(),null);
		return result;
	}

	public static async Task InvokeAsync(this SynchronizationContext ctx,Action a)=>await InvokeAsync(ctx,()=>{
		a();
		return false;//Return value doesn't matter at all, gets converted to void using await
	});

	public static Task<T> InvokeAsync<T>(this SynchronizationContext ctx,Func<T> a){
		var tcs=new TaskCompletionSource<T>();
		ctx.Post(_=>{
			try{
				tcs.TrySetResult(a());
			} catch(Exception e){
				tcs.TrySetException(e);
			}
		},null);
		return tcs.Task;
	}

	public static ContextJumper JumpAsync(this SynchronizationContext ctx)=>new(ctx);
}

public class ContextJumper:INotifyCompletion{
	private readonly SynchronizationContext _ctx;
	public ContextJumper(SynchronizationContext ctx)=>_ctx=ctx;

	public bool IsCompleted=>_ctx==SynchronizationContext.Current;
	public ContextJumper GetAwaiter()=>this;
	public void GetResult(){}
	public void OnCompleted(Action a)=>_ctx.BeginInvoke(a);
}