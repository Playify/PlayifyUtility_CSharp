namespace PlayifyUtility.HelperClasses.Dispose;

public class SemaphoreSlimReleaser:IDisposable{
	private SemaphoreSlim? _semaphore;

	public SemaphoreSlimReleaser(SemaphoreSlim semaphore)=>_semaphore=semaphore;

	public void Dispose(){
		_semaphore?.Release();
		_semaphore=null;
	}
}