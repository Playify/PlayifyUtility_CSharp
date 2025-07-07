using JetBrains.Annotations;

namespace PlayifyUtility.HelperClasses.Dispose;

[PublicAPI]
[MustDisposeResource]
public readonly struct CallbackAsDisposable(Action dispose):IDisposable{
	public void Dispose()=>dispose();

	[MustDisposeResource]
	public static CallbackAsDisposable RunLater(Action dispose)=>new(dispose);

	[MustDisposeResource]
	public static CallbackAsDisposable RunLater<T>(Action<T> dispose,T arg)=>new(()=>dispose(arg));

}