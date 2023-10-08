namespace PlayifyUtility.HelperClasses;

public sealed class CallbackAsDisposable:IDisposable{
	private readonly Action _dispose;

	public CallbackAsDisposable(Action dispose)=>_dispose=dispose;

	public void Dispose()=>_dispose();
}