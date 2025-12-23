using JetBrains.Annotations;

namespace PlayifyUtility.Utils.Extensions;

[PublicAPI]
public static partial class AsyncExtensions{
	[Pure]
	public static async IAsyncEnumerable<TSource> Where<TSource>(this IAsyncEnumerable<TSource> source,Func<TSource,bool> predicate){
		await foreach(var v in source)
			if(predicate(v))
				yield return v;
	}

	[Pure]
	public static async IAsyncEnumerable<TSource> Where<TSource>(this IAsyncEnumerable<TSource> source,Func<TSource,int,bool> predicate){
		var i=0;
		await foreach(var v in source)
			if(predicate(v,i++))
				yield return v;
	}

	[Pure]
	public static async IAsyncEnumerable<TResult> Select<TSource,TResult>(this IAsyncEnumerable<TSource> source,Func<TSource,TResult> selector){
		await foreach(var v in source) yield return selector(v);
	}

	[Pure]
	public static async IAsyncEnumerable<TResult> Select<TSource,TResult>(this IAsyncEnumerable<TSource> source,Func<TSource,int,TResult> selector){
		var i=0;
		await foreach(var v in source) yield return selector(v,i++);
	}

	[Pure]
	public static async IAsyncEnumerable<TResult> SelectAsync<TSource,TResult>(this IAsyncEnumerable<TSource> source,Func<TSource,Task<TResult>> selector){
		await foreach(var v in source) yield return await selector(v);
	}

	[Pure]
	public static async IAsyncEnumerable<TResult> SelectAsync<TSource,TResult>(this IAsyncEnumerable<TSource> source,Func<TSource,int,Task<TResult>> selector){
		var i=0;
		await foreach(var v in source) yield return await selector(v,i++);
	}

	[Pure]
	public static async IAsyncEnumerable<TResult> SelectAsync<TResult>(this IAsyncEnumerable<Task<TResult>> source){
		await foreach(var v in source) yield return await v;
	}


	[Pure]
	public static async IAsyncEnumerable<TResult> SelectMany<TSource,TResult>(this IAsyncEnumerable<TSource> source,Func<TSource,IEnumerable<TResult>> selector){
		await foreach(var v in source)
		foreach(var v2 in selector(v))
			yield return v2;
	}

	[Pure]
	public static async IAsyncEnumerable<TResult> SelectMany<TSource,TResult>(this IAsyncEnumerable<TSource> source,Func<TSource,IAsyncEnumerable<TResult>> selector){
		await foreach(var v in source)
		await foreach(var v2 in selector(v))
			yield return v2;
	}


	[Pure]
	public static async IAsyncEnumerable<TSource> NonNull<TSource>(this IAsyncEnumerable<TSource> source){
		await foreach(var v in source)
			if(v!=null)
				yield return v;
	}

	[Pure]
	public static async IAsyncEnumerable<TResult> NonNull<TSource,TResult>(this IAsyncEnumerable<TSource> source,Func<TSource,TResult> selector){
		await foreach(var v in source){
			var v2=selector(v);
			if(v2!=null) yield return v2;
		}
	}

	[Pure]
	public static async Task<List<TResult>> ToList<TResult>(this IAsyncEnumerable<TResult> source){
		var q=new List<TResult>();
		await foreach(var result in source) q.Add(result);
		return q;
	}

	[Pure]
	public static async Task<TResult[]> ToArray<TResult>(this IAsyncEnumerable<TResult> source){
		return (await source.ToList()).ToArray();
	}
}