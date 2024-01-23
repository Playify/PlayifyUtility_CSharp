using System.Runtime.CompilerServices;

namespace PlayifyUtility.Windows;

public static class MainThread{
	private static SynchronizationContext? _ctx;
	public static SynchronizationContext? SynchronizationContext=>_ctx??=SynchronizationContext.Current;

	public static void Init(bool winForms=true){
		Application.EnableVisualStyles();
		//Initializes Singleton
		if(SynchronizationContext==null)
			SynchronizationContext.SetSynchronizationContext(_ctx??=winForms
				                                                        ?new WindowsFormsSynchronizationContext()
				                                                        :new SynchronizationContext());
	}


	public static bool IsMainThread()=>SynchronizationContext is {} notnull&&SynchronizationContext.Current==notnull;

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

	public static void Invoke(Action func){
		var ctx=SynchronizationContext;
		if(ctx==null) func();
		else ctx.Send(_=>func(),null);
	}

	public static JumpToMainThreadTask JumpAsync()=>new();


	public class JumpToMainThreadTask:INotifyCompletion{
		public bool IsCompleted=>IsMainThread();

		public JumpToMainThreadTask GetAwaiter()=>this;

		public void GetResult(){}
		public void OnCompleted(Action continuation)=>BeginInvoke(continuation);
	}
}