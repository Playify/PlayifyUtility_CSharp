namespace PlayifyUtility.Windows;

public static class MainThread{
	private static SynchronizationContext? _ctx;
	public static SynchronizationContext? SynchronizationContext=>_ctx??=SynchronizationContext.Current;
	
	public static void Init(){
		if(SynchronizationContext==null) throw new Exception("Error getting "+nameof(SynchronizationContext));
	}
	

	public static bool IsMainThread()=>SynchronizationContext.Current==SynchronizationContext;

	public static void BeginInvoke(Action a){
		var ctx=SynchronizationContext;
		if(ctx==null) a();
		else ctx.Post(_=>a(),null);
	}

	public static T Invoke<T>(Func<T> func){
		var ctx=SynchronizationContext;
		if(ctx==null) return func();
		
		T result=default!;
		ctx.Send(_=>result=func(),null);
		return result;
	}
}