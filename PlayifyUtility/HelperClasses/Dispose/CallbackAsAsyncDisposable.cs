namespace PlayifyUtility.HelperClasses.Dispose;

public readonly struct CallbackAsAsyncDisposable:IAsyncDisposable{
	private readonly Func<ValueTask> _dispose;

	public CallbackAsAsyncDisposable(Func<ValueTask> dispose)=>_dispose=dispose;
	public CallbackAsAsyncDisposable(Func<Task> dispose)=>_dispose=()=>new ValueTask(dispose());

	public ValueTask DisposeAsync()=>_dispose();
}