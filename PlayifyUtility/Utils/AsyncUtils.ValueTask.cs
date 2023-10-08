namespace PlayifyUtility.Utils;

public static partial class AsyncUtils{
	public static async ValueTask ToNonGeneric<T>(this ValueTask<T> t)=>await t;

	public static async ValueTask<T> WithResult<T>(this ValueTask t,T result){
		await t;
		return result;
	}

	public static async Task<T> WithResult<T>(this Task t,T result){
		await t;
		return result;
	}


	public static async ValueTask<T[]> WhenAll<T>(params ValueTask<T>[] tasks){
		var results=new T[tasks.Length];

		List<Exception>? exceptions=null;
		for(var i=0;i<tasks.Length;i++)
			try{
				results[i]=await tasks[i].ConfigureAwait(false);
			} catch(Exception ex){
				exceptions??=new List<Exception>(tasks.Length);
				exceptions.Add(ex);
			}
		return exceptions==null?results:throw new AggregateException(exceptions);
	}

	public static async ValueTask WhenAll(params ValueTask[] tasks){
		List<Exception>? exceptions=null;
		for(var i=0;i<tasks.Length;i++)
			try{
				await tasks[i].ConfigureAwait(false);
			} catch(Exception ex){
				exceptions??=new List<Exception>(tasks.Length);
				exceptions.Add(ex);
			}
		if(exceptions!=null) throw new AggregateException(exceptions);
	}

	public static async ValueTask WhenAll(IEnumerable<ValueTask> tasks){
		List<Exception>? exceptions=null;
		foreach(var task in tasks)
			try{
				await task.ConfigureAwait(false);
			} catch(Exception ex){
				exceptions??=new List<Exception>();
				exceptions.Add(ex);
			}
		if(exceptions!=null) throw new AggregateException(exceptions);
	}
}