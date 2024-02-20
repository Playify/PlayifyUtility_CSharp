using PlayifyUtility.Windows.Utils;

namespace PlayifyUtility.Windows;

public class UiThread{
	public static UiThread Create(string name){
		TaskCompletionSource<WindowsFormsSynchronizationContext> ctx=new();

		var stack=Environment.StackTrace;

		var thread=new Thread(()=>{
			_=stack;
			try{
				Application.EnableVisualStyles();

				var context=new WindowsFormsSynchronizationContext();
				SynchronizationContext.SetSynchronizationContext(context);
				ctx.TrySetResult(context);

				//Console.WriteLine("Starting UiThread "+name);
				Application.Run();
			} catch(Exception e){
				ctx.TrySetException(e);
				Console.WriteLine("Error on UiThread("+name+"): "+e);
			} finally{
				Application.ExitThread();
				//Console.WriteLine("Exiting UiThread "+name);
			}
		}){Name=name};
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();

		return new UiThread(ctx.Task.Result);
	}

	public static UiThread? Current=>From(SynchronizationContext.Current);
	public static UiThread? From(SynchronizationContext? sc)=>sc is WindowsFormsSynchronizationContext ctx?new UiThread(ctx):null;


	private readonly WindowsFormsSynchronizationContext _ctx;
	private UiThread(WindowsFormsSynchronizationContext ctx)=>_ctx=ctx;

	public void BeginInvoke(Action a)=>_ctx.BeginInvoke(a);

	public void Invoke(Action a)=>_ctx.Invoke(a);

	public T Invoke<T>(Func<T> a)=>_ctx.Invoke(a);

	public ContextJumper JumpAsync()=>_ctx.JumpAsync();


	public bool IsCurrent=>SynchronizationContext.Current==_ctx;


	public void Exit(Action? a=null){
		var thread=Invoke(()=>Thread.CurrentThread);
		BeginInvoke(()=>{
			a?.Invoke();
			Application.ExitThread();
		});
		thread.Join();
	}
	public void BeginExit(Action? a=null){
		BeginInvoke(()=>{
			a?.Invoke();
			Application.ExitThread();
		});
	}
}