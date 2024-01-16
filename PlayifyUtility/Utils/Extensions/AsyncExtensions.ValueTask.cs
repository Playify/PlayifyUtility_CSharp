namespace PlayifyUtility.Utils.Extensions;

public static partial class AsyncExtensions{
	public static async ValueTask ToNonGeneric<T>(this ValueTask<T> t)=>await t;

	public static async ValueTask<T> WithResult<T>(this ValueTask t,T result){
		await t;
		return result;
	}

	public static async Task<T> WithResult<T>(this Task t,T result){
		await t;
		return result;
	}


	public static ValueTask TryCatch(this ValueTask task)=>TryCatch(task,Console.Error.WriteLine);

	public static async ValueTask TryCatch(this ValueTask task,Action<Exception> @catch){
		try{
			await task;
		} catch(Exception e){
			@catch(e);
		}
	}

	[Obsolete("Use .Background() instead, or .Catch()")]
	public static ValueTask TryCatch<T>(this ValueTask<T> task)=>TryCatch(task,Console.Error.WriteLine);

	[Obsolete("Use .Background() instead, or .Catch()")]
	public static async ValueTask TryCatch<T>(this ValueTask<T> task,Action<Exception> @catch){
		try{
			await task;
		} catch(Exception e){
			@catch(e);
		}
	}

	[Obsolete("Use .Background() instead, or .Catch()")]
	public static ValueTask<T> TryCatch<T>(this ValueTask<T> task,T def)=>TryCatch(task,Console.Error.WriteLine,def);

	[Obsolete("Use .Background() instead, or .Catch()")]
	public static async ValueTask<T> TryCatch<T>(this ValueTask<T> task,Action<Exception> @catch,T def){
		try{
			return await task;
		} catch(Exception e){
			@catch(e);
			return def;
		}
	}

	public static void Background(this ValueTask task)=>Background(task.IsCompleted?Task.CompletedTask:task.AsTask());
	public static void Background<T>(this ValueTask<T> task)=>Background(task.IsCompleted?Task.CompletedTask:task.AsTask());
}