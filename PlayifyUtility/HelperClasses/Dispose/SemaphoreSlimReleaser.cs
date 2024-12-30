using JetBrains.Annotations;

namespace PlayifyUtility.HelperClasses.Dispose;

[PublicAPI]
[MustDisposeResource]
public sealed class SemaphoreSlimReleaser(SemaphoreSlim semaphore):IDisposable{
	private SemaphoreSlim? _semaphore=semaphore;

	public void Dispose(){
		_semaphore?.Release();
		_semaphore=null;
	}
}