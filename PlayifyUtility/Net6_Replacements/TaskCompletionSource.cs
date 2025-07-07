#if NETFRAMEWORK
using JetBrains.Annotations;
using PlayifyUtility.HelperClasses;

// ReSharper disable once CheckNamespace
namespace System.Threading.Tasks;

[PublicAPI]
public class TaskCompletionSource{
	private readonly TaskCompletionSource<VoidType> _tcs;

	public TaskCompletionSource()=>_tcs=new TaskCompletionSource<VoidType>();
	public TaskCompletionSource(TaskCreationOptions creationOptions)=>_tcs=new TaskCompletionSource<VoidType>(creationOptions);
	public TaskCompletionSource(object? state)=>_tcs=new TaskCompletionSource<VoidType>(state);
	public TaskCompletionSource(object? state,TaskCreationOptions creationOptions)=>_tcs=new TaskCompletionSource<VoidType>(state,creationOptions);

	public Task Task=>_tcs.Task;

	public void SetException(Exception exception)=>_tcs.SetException(exception);
	public void SetException(IEnumerable<Exception> exceptions)=>_tcs.SetException(exceptions);
	public bool TrySetException(Exception exception)=>_tcs.TrySetException(exception);
	public bool TrySetException(IEnumerable<Exception> exceptions)=>_tcs.TrySetException(exceptions);

	public void SetResult()=>_tcs.SetResult(default);
	public bool TrySetResult()=>_tcs.TrySetResult(default);
	
	public void SetCanceled()=>_tcs.SetCanceled();
	public void SetCanceled(CancellationToken cancellationToken)=>_tcs.TrySetCanceled(cancellationToken);
	public bool TrySetCanceled()=>_tcs.TrySetCanceled();
	public bool TrySetCanceled(CancellationToken cancellationToken)=>_tcs.TrySetCanceled(cancellationToken);
}
#endif