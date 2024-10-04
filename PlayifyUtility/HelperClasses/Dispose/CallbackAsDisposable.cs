namespace PlayifyUtility.HelperClasses.Dispose;

public readonly struct CallbackAsDisposable:IDisposable{
	private readonly Action _dispose;

	public CallbackAsDisposable(Action dispose)=>_dispose=dispose;

	public void Dispose()=>_dispose();
}