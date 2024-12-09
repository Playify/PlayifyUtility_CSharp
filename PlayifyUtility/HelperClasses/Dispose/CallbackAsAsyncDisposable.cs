using JetBrains.Annotations;

namespace PlayifyUtility.HelperClasses.Dispose;

[PublicAPI]
[MustDisposeResource]
public readonly struct CallbackAsAsyncDisposable(Func<ValueTask> dispose):IAsyncDisposable{
	public CallbackAsAsyncDisposable(Func<Task> dispose):this(()=>new ValueTask(dispose())){
	}

	public ValueTask DisposeAsync()=>dispose();
}