namespace PlayifyUtility.Utils.Extensions;

public static partial class AsyncExtensions{

	#region Then
	public static async Task Then(this Task task,Action a){
		await task;
		a();
	}

	public static async Task<T> Then<T>(this Task task,Func<T> f){
		await task;
		return f();
	}

	public static async Task Then<T>(this Task<T> task,Action<T> a)=>a(await task);

	public static async Task<TResult> Then<TArg,TResult>(this Task<TArg> task,Func<TArg,TResult> f)=>f(await task);
	#endregion

	#region ThenAsync
	public static async Task ThenAsync(this Task task,Func<Task> a){
		await task;
		await a();
	}

	public static async Task<T> ThenAsync<T>(this Task task,Func<Task<T>> f){
		await task;
		return await f();
	}

	public static async Task ThenAsync<T>(this Task<T> task,Func<T,Task> a)=>await a(await task);

	public static async Task<TResult> ThenAsync<TArg,TResult>(this Task<TArg> task,Func<TArg,Task<TResult>> f)=>await f(await task);
	#endregion

	#region Catch
	public static Task Catch(this Task task,Action<Exception> onError)=>Catch<Exception>(task,onError);

	public static Task<T> Catch<T>(this Task<T> task,Func<Exception,T> onError)=>Catch<T,Exception>(task,onError);

	public static async Task Catch<TException>(this Task task,Action<TException> onError) where TException : Exception{
		try{
			await task;
		} catch(TException e){
			onError(e);
		}
	}

	public static async Task<T> Catch<T,TException>(this Task<T> task,Func<TException,T> onError) where TException : Exception{
		try{
			return await task;
		} catch(TException e){
			return onError(e);
		}
	}
	#endregion

	#region CatchRethrow
	public static Task CatchRethrow(this Task task,Action<Exception> onError)=>CatchRethrow<Exception>(task,onError);
	public static Task<T> CatchRethrow<T>(this Task<T> task,Action<Exception> onError)=>CatchRethrow<T,Exception>(task,onError);

	public static async Task CatchRethrow<TException>(this Task task,Action<TException> onError) where TException : Exception{
		try{
			await task;
		} catch(TException e) when(FunctionUtils.RunThenReturn(()=>onError(e),false)){
			throw;
		}
	}

	public static async Task<T> CatchRethrow<T,TException>(this Task<T> task,Action<TException> onError) where TException : Exception{
		try{
			return await task;
		} catch(TException e) when(FunctionUtils.RunThenReturn(()=>onError(e),false)){
			throw;
		}
	}
	#endregion

	#region CatchAsync
	public static Task CatchAsync(this Task task,Func<Exception,Task> onError)=>CatchAsync<Exception>(task,onError);

	public static Task<T> CatchAsync<T>(this Task<T> task,Func<Exception,Task<T>> onError)=>CatchAsync<T,Exception>(task,onError);

	public static async Task CatchAsync<TException>(this Task task,Func<TException,Task> onError) where TException : Exception{
		try{
			await task;
		} catch(TException e){
			await onError(e);
		}
	}

	public static async Task<T> CatchAsync<T,TException>(this Task<T> task,Func<TException,Task<T>> onError) where TException : Exception{
		try{
			return await task;
		} catch(TException e){
			return await onError(e);
		}
	}
	#endregion

	#region Finally
	public static async Task Finally(this Task task,Action onFinally){
		try{
			await task;
		} finally{
			onFinally();
		}
	}

	public static async Task<T> Finally<T>(this Task<T> task,Action onFinally){
		try{
			return await task;
		} finally{
			onFinally();
		}
	}
	#endregion

	#region FinallyAsync
	public static async Task FinallyAsync(this Task task,Func<Task> onFinally){
		try{
			await task;
		} finally{
			await onFinally();
		}
	}

	public static async Task<T> FinallyAsync<T>(this Task<T> task,Func<Task> onFinally){
		try{
			return await task;
		} finally{
			await onFinally();
		}
	}
	#endregion

	#region Background
	public static async void Background(this Task task,Action<Exception>? @catch=null){
		try{
			await task.ConfigureAwait(false);
		} catch(Exception e){
			if(@catch!=null) @catch(e);
			else Console.Error.WriteLine(e);
		}
	}

	public static void Background(this ValueTask task,Action<Exception>? @catch=null){
		if(task.IsCompletedSuccessfully) return;
		Background(task.AsTask(),@catch);
	}

	public static void Background<T>(this ValueTask<T> task,Action<Exception>? @catch=null){
		if(task.IsCompletedSuccessfully) return;
		Background(task.AsTask(),@catch);
	}
	#endregion

	#region ValueTask
	public static async ValueTask ToNonGeneric<T>(this ValueTask<T> t)=>await t;

	public static async ValueTask<T> WithResult<T>(this ValueTask t,T result){
		await t;
		return result;
	}

	public static async Task<T> WithResult<T>(this Task t,T result){
		await t;
		return result;
	}
	#endregion

}