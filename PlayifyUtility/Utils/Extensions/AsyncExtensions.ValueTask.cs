namespace PlayifyUtility.Utils.Extensions;

public static partial class AsyncExtensions{

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


	[Obsolete("Use .Background() instead, or .Catch()")]
	public static ValueTask TryCatch(this ValueTask task)=>TryCatch(task,Console.Error.WriteLine);

	[Obsolete("Use .Background() instead, or .Catch()")]
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
}