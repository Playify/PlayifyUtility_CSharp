using JetBrains.Annotations;

namespace PlayifyUtility.HelperClasses.Dispose;

[PublicAPI]
[MustDisposeResource]
public readonly struct CallbackAsAsyncDisposable(Func<ValueTask> dispose):IAsyncDisposable{
	[MustDisposeResource]
	public static CallbackAsAsyncDisposable FromTask(Func<Task> dispose)=>new(()=>new ValueTask(dispose()));

	public ValueTask DisposeAsync()=>dispose();

}