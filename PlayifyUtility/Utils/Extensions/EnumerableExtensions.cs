using System.Collections;
using JetBrains.Annotations;

namespace PlayifyUtility.Utils.Extensions;

[PublicAPI]
public static class EnumerableExtensions{
	public static IEnumerator<T> GetEnumerator<T>(this IEnumerator<T> e)=>e;
	public static IEnumerator GetEnumerator(this IEnumerator e)=>e;
	
	public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> source)=>source.SelectMany(enumerable=>enumerable);

	public static IEnumerable<T> NonNull<T>(this IEnumerable<T?> source) where T:class=>source.Where(t=>t!=null)!;

	public static IEnumerable<T> NonNull<T>(this IEnumerable<T?> source) where T:struct{
		foreach(var nullable in source)
			if(nullable.TryGet(out var t))
				yield return t;
	}

	public static IEnumerable<(T value,int index)> WithIndex<T>(this IEnumerable<T> source)=>source.Select((value,i)=>(value,i));

	public static IEnumerable<(TFirst a,TSecond b,int index)> ZipWithIndex<TFirst,TSecond>(this IEnumerable<TFirst> first,
	                                                                                       IEnumerable<TSecond> second){
		return first.Zip(second,(f,s,i)=>(f,s,i));
	}

	public static IEnumerable<(TFirst a,TSecond b)> Zip<TFirst,TSecond>(this IEnumerable<TFirst> first,
	                                                                    IEnumerable<TSecond> second)
		=>first.Zip(second,(f,s)=>(f,s));

	public static IEnumerable<TResult> Zip<TFirst,TSecond,TResult>(this IEnumerable<TFirst> first,
	                                                               IEnumerable<TSecond> second,
	                                                               Func<TFirst,TSecond,int,TResult> resultSelector){
		return first.Zip(second).Select((tuple,i)=>resultSelector(tuple.a,tuple.b,i));
	}

	public static void ForEach<T>(this IEnumerable<T> source,Action<T> act){
		foreach(var e in source) act(e);
	}
	public static void ForEach<T,T2>(this IEnumerable<T> source,Action<T,T2> act,T2 arg2){
		foreach(var e in source) act(e,arg2);
	}

	public static void ForEach<T>(this IEnumerable<T> source,Action<T,int> act){
		var i=0;
		foreach(var e in source) act(e,i++);
	}

	public static IEnumerable<T> RunAll<T>(this IEnumerable<T> source,Action<T> act){
		foreach(var e in source){
			act(e);
			yield return e;
		}
	}

	public static IEnumerable<T> RunAll<T>(this IEnumerable<T> source,Action<T,int> act){
		var i=0;
		foreach(var e in source){
			act(e,i++);
			yield return e;
		}
	}

	public static IEnumerable<TResult> TryParseAll<TSource,TResult>(
		this IEnumerable<TSource> source,
		TryParseFunction<TSource,TResult> tryParse){
		foreach(var e in source)
			if(tryParse(e,out var result))
				yield return result;
	}
	
	public static string Join<T>(this IEnumerable<T> source,string sep)=>string.Join(sep,source);
	
	public static Task<Task<T>> WhenAny<T>(this IEnumerable<Task<T>> source)=>Task.WhenAny(source);
	public static Task<Task> WhenAny(this IEnumerable<Task> source)=>Task.WhenAny(source);
	
	public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> source)=>Task.WhenAll(source);
	public static Task WhenAll(this IEnumerable<Task> source)=>Task.WhenAll(source);
	
	public static ValueTask<T[]> WhenAll<T>(this IEnumerable<ValueTask<T>> source)=>TaskUtils.WhenAll(source);
	public static ValueTask WhenAll(this IEnumerable<ValueTask> source)=>TaskUtils.WhenAll(source);
	
}