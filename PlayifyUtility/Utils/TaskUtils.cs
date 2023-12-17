using JetBrains.Annotations;

namespace PlayifyUtility.Utils;

[PublicAPI]
public static class TaskUtils{
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

	public static ValueTask<T[]> WhenAll<T>(IEnumerable<ValueTask<T>> tasks)=>WhenAll(tasks.ToArray());

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

	public static ValueTask WhenAll(IEnumerable<ValueTask> tasks)=>WhenAll(tasks.ToArray());
}