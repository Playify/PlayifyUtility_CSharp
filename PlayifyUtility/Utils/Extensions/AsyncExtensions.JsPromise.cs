namespace PlayifyUtility.Utils.Extensions;

public static partial class AsyncExtensions{
	public static async Task Then(this Task task,Action a){
		await task;
		a();
	}

	public static async Task ThenAsync(this Task task,Func<Task> a){
		await task;
		await a();
	}

	public static async Task ThenAsync(this Task task,Func<ValueTask> a){
		await task;
		await a();
	}

	public static async Task<T> Then<T>(this Task task,Func<T> f){
		await task;
		return f();
	}

	public static async Task<T> ThenAsync<T>(this Task task,Func<Task<T>> f){
		await task;
		return await f();
	}

	public static async Task<T> ThenAsync<T>(this Task task,Func<ValueTask<T>> f){
		await task;
		return await f();
	}

	public static async Task Then<T>(this Task<T> task,Action<T> a){
		var arg=await task;
		a(arg);
	}

	public static async Task ThenAsync<T>(this Task<T> task,Func<T,Task> a){
		var arg=await task;
		await a(arg);
	}

	public static async Task ThenAsync<T>(this Task<T> task,Func<T,ValueTask> a){
		var arg=await task;
		await a(arg);
	}

	public static async Task<TResult> Then<TArg,TResult>(this Task<TArg> task,Func<TArg,TResult> f){
		var arg=await task;
		return f(arg);
	}

	public static async Task<TResult> ThenAsync<TArg,TResult>(this Task<TArg> task,Func<TArg,Task<TResult>> f){
		var arg=await task;
		return await f(arg);
	}

	public static async Task<TResult> ThenAsync<TArg,TResult>(this Task<TArg> task,Func<TArg,ValueTask<TResult>> f){
		var arg=await task;
		return await f(arg);
	}

	public static async Task Catch(this Task task,Action<Exception> onError){
		try{
			await task;
		} catch(Exception e){
			onError(e);
		}
	}

	public static async Task CatchAsync(this Task task,Func<Exception,Task> onError){
		try{
			await task;
		} catch(Exception e){
			await onError(e);
		}
	}

	public static async Task CatchAsync(this Task task,Func<Exception,ValueTask> onError){
		try{
			await task;
		} catch(Exception e){
			await onError(e);
		}
	}

	public static async Task<T> Catch<T>(this Task<T> task,Func<Exception,T> onError){
		try{
			return await task;
		} catch(Exception e){
			return onError(e);
		}
	}

	public static async Task<T> CatchAsync<T>(this Task<T> task,Func<Exception,Task<T>> onError){
		try{
			return await task;
		} catch(Exception e){
			return await onError(e);
		}
	}

	public static async Task<T> CatchAsync<T>(this Task<T> task,Func<Exception,ValueTask<T>> onError){
		try{
			return await task;
		} catch(Exception e){
			return await onError(e);
		}
	}

	public static async Task Finally(this Task task,Action onFinally){
		try{
			await task;
		} finally{
			onFinally();
		}
	}

	public static async Task FinallyAsync(this Task task,Func<Task> onFinally){
		try{
			await task;
		} finally{
			await onFinally();
		}
	}

	public static async Task FinallyAsync(this Task task,Func<ValueTask> onFinally){
		try{
			await task;
		} finally{
			await onFinally();
		}
	}

	[Obsolete("Use .Background() instead, or .Catch()")]
	public static Task TryCatch(this Task task)=>TryCatch(task,Console.Error.WriteLine);

	[Obsolete("Use .Background() instead, or .Catch()")]
	public static async Task TryCatch(this Task task,Action<Exception> @catch){
		try{
			await task;
		} catch(Exception e){
			@catch(e);
		}
	}

	[Obsolete("Use .Background() instead, or .Catch()")]
	public static Task<T> TryCatch<T>(this Task<T> task,T def)=>TryCatch(task,Console.Error.WriteLine,def);

	[Obsolete("Use .Background() instead, or .Catch()")]
	public static async Task<T> TryCatch<T>(this Task<T> task,Action<Exception> @catch,T def){
		try{
			return await task;
		} catch(Exception e){
			@catch(e);
			return def;
		}
	}


	public static async void Background(this Task task,Action<Exception>? @catch=null){
		try{
			await task.ConfigureAwait(false);
		} catch(Exception e){
			if(@catch.NotNull(out var c)) c(e);
			else Console.Error.WriteLine(e);
		}
	}
}