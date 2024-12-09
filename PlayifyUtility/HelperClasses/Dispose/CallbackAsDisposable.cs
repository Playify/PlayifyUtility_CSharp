using JetBrains.Annotations;

namespace PlayifyUtility.HelperClasses.Dispose;

[PublicAPI]
[MustDisposeResource]
public readonly struct CallbackAsDisposable(Action dispose):IDisposable{
	public void Dispose()=>dispose();
}