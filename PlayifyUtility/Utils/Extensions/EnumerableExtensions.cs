using System.Collections;
using JetBrains.Annotations;

namespace PlayifyUtility.Utils.Extensions;

[PublicAPI]
public static class EnumerableExtensions{

	#region Enumerator as Enumerable
	public static IEnumerator<T> GetEnumerator<T>(this IEnumerator<T> e)=>e;
	public static IEnumerator GetEnumerator(this IEnumerator e)=>e;
	#endregion

	#region Null
	public static T? FirstOrNull<T>(this IEnumerable<T> source) where T : struct{
		if(source is IList<T>{Count: >0} list) return list[0];
		using var enumerator=source.GetEnumerator();
		return enumerator.MoveNext()?enumerator.Current:null;
	}

	public static T? FirstOrNull<T>(this IEnumerable<T> source,Func<T,bool> predicate) where T : struct{
		foreach(var t in source)
			if(predicate(t))
				return t;
		return null;
	}

	public static T? LastOrNull<T>(this IEnumerable<T> source) where T : struct=>source.AsNullable().LastOrDefault();
	public static T? LastOrNull<T>(this IEnumerable<T> source,Func<T,bool> predicate) where T : struct=>source.Where(predicate).AsNullable().LastOrDefault();
	public static IEnumerable<T?> AsNullable<T>(this IEnumerable<T> source) where T : struct=>source.Select(s=>(T?)s);

	public static IEnumerable<T> NonNull<T>(this IEnumerable<T?> source) where T : class=>source.Where(t=>t!=null)!;

	public static IEnumerable<T> NonNull<T>(this IEnumerable<T?> source) where T : struct{
		foreach(var nullable in source)
			if(nullable is{} t)
				yield return t;
	}
	#endregion

	#region Select and Zip
	public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> source)=>source.SelectMany(enumerable=>enumerable);

	public static IEnumerable<TTo> SelectMany<TFrom,TTo>(this IEnumerable<TFrom> source,Func<TFrom,(TTo,TTo)> selector){
		foreach(var from in source.Select(selector)){
			yield return from.Item1;
			yield return from.Item2;
		}
	}

	public static IEnumerable<TTo> SelectMany<TFrom,TTo>(this IEnumerable<TFrom> source,Func<TFrom,(TTo,TTo,TTo)> selector){
		foreach(var from in source.Select(selector)){
			yield return from.Item1;
			yield return from.Item2;
			yield return from.Item3;
		}
	}

	public static IEnumerable<TTo> SelectMany<TFrom,TTo>(this IEnumerable<TFrom> source,Func<TFrom,(TTo,TTo,TTo,TTo)> selector){
		foreach(var from in source.Select(selector)){
			yield return from.Item1;
			yield return from.Item2;
			yield return from.Item3;
			yield return from.Item4;
		}
	}

	public static IEnumerable<TTo> SelectMany<TFrom,TTo>(this IEnumerable<TFrom> source,Func<TFrom,(TTo,TTo,TTo,TTo,TTo)> selector){
		foreach(var from in source.Select(selector)){
			yield return from.Item1;
			yield return from.Item2;
			yield return from.Item3;
			yield return from.Item4;
			yield return from.Item5;
		}
	}

	public static IEnumerable<TTo> SelectMany<TFrom,TTo>(this IEnumerable<TFrom> source,Func<TFrom,(TTo,TTo,TTo,TTo,TTo,TTo)> selector){
		foreach(var from in source.Select(selector)){
			yield return from.Item1;
			yield return from.Item2;
			yield return from.Item3;
			yield return from.Item4;
			yield return from.Item5;
			yield return from.Item6;
		}
	}

	public static IEnumerable<TTo> SelectMany<TFrom,TTo>(this IEnumerable<TFrom> source,Func<TFrom,(TTo,TTo,TTo,TTo,TTo,TTo,TTo)> selector){
		foreach(var from in source.Select(selector)){
			yield return from.Item1;
			yield return from.Item2;
			yield return from.Item3;
			yield return from.Item4;
			yield return from.Item5;
			yield return from.Item6;
			yield return from.Item7;
		}
	}

	public static IEnumerable<TTo> SelectMany<TFrom,TTo>(this IEnumerable<TFrom> source,Func<TFrom,(TTo,TTo,TTo,TTo,TTo,TTo,TTo,TTo)> selector){
		foreach(var from in source.Select(selector)){
			yield return from.Item1;
			yield return from.Item2;
			yield return from.Item3;
			yield return from.Item4;
			yield return from.Item5;
			yield return from.Item6;
			yield return from.Item7;
			yield return from.Item8;
		}
	}


	public static IOrderedEnumerable<T> Ordered<T>(this IEnumerable<T> source)=>source.OrderBy(t=>t);
	public static IOrderedEnumerable<T> OrderedDescending<T>(this IEnumerable<T> source)=>source.OrderByDescending(t=>t);

	public static IEnumerable<(T value,int index)> WithIndex<T>(this IEnumerable<T> source)=>source.Select((value,i)=>(value,i));

	public static IEnumerable<(TFirst a,TSecond b,int index)> ZipWithIndex<TFirst,TSecond>(this IEnumerable<TFirst> first,
		IEnumerable<TSecond> second){
		return first.Zip(second,(f,s,i)=>(f,s,i));
	}

#if NETFRAMEWORK
	public static IEnumerable<(TFirst First,TSecond Second)> Zip<TFirst,TSecond>(this IEnumerable<TFirst> first,
		IEnumerable<TSecond> second)
		=>first.Zip(second,(f,s)=>(f,s));
#endif

	public static void Enumerate<T>(this IEnumerable<T> e){
		foreach(var _ in e){
			//do nothing
		}
	}

	[MustUseReturnValue]
	public static bool IfEmpty<T>(this IEnumerable<T> e,out IEnumerable<T> all){
		all=IsEmpty(e,out var b);
		return b;
	}
	[MustUseReturnValue]
	public static bool IfEmpty<T>(this IEnumerable<T> e){
		if(e is ICollection collection) return collection.Count==0;
		using var enumerator=e.GetEnumerator();
		return !enumerator.MoveNext();
	}

	[MustUseReturnValue]
	public static IEnumerable<T> IsEmpty<T>(this IEnumerable<T> e,out bool b){
		if(e is ICollection collection){
			b=collection.Count==0;
			return e;
		}
		var enumerator=e.GetEnumerator();
		if(!enumerator.MoveNext()){
			b=true;
			return[];
		}
		b=false;
		return YieldAll();

		IEnumerable<T> YieldAll(){
			do yield return enumerator.Current;
			while(enumerator.MoveNext());
			enumerator.Dispose();
		}
	}

	[MustUseReturnValue]
	public static bool IfSingle<T>(this IEnumerable<T> e,out IEnumerable<T> all,out T? firstElement){
		all=IsSingle(e,out var b,out firstElement);
		return b;
	}

	[MustUseReturnValue]
	public static bool IfSingle<T>(this IEnumerable<T> e,out T? firstElement){
		if(e is IList<T> collection){
			var b=collection.Count==1;
			firstElement=b?collection[0]:default;
			return b;
		}
		using var enumerator=e.GetEnumerator();
		if(!enumerator.MoveNext()){
			firstElement=default;
			return false;
		}
		firstElement=enumerator.Current;
		return !enumerator.MoveNext();
	}

	[MustUseReturnValue]
	public static IEnumerable<T> IsSingle<T>(this IEnumerable<T> e,out bool b,out T? firstElement){
		if(e is IList<T> collection){
			b=collection.Count==1;
			firstElement=b?collection[0]:default;
			return e;
		}
		var enumerator=e.GetEnumerator();
		if(!enumerator.MoveNext()){
			b=false;
			firstElement=default;
			return[];
		}
		firstElement=enumerator.Current;
		if(!enumerator.MoveNext()){
			b=true;
			return[firstElement];
		}

		b=false;
		return YieldAll().Prepend(firstElement);

		IEnumerable<T> YieldAll(){
			do yield return enumerator.Current;
			while(enumerator.MoveNext());
			enumerator.Dispose();
		}
	}


	public static IEnumerable<TResult> Zip<TFirst,TSecond,TResult>(this IEnumerable<TFirst> first,
		IEnumerable<TSecond> second,
		Func<TFirst,TSecond,int,TResult> resultSelector){
		return first.Zip(second).Select((tuple,i)=>resultSelector(tuple.First,tuple.Second,i));
	}

	public static IEnumerable<TResult> TryGetAll<TSource,TResult>(this IEnumerable<TSource> source,
		TryParseFunction<TSource,TResult> tryGet){
		foreach(var e in source)
			if(tryGet(e,out var result))
				yield return result;
	}

	public static IEnumerable<TResult> TryGetAll<TSource,TResult>(this IEnumerable<TSource> source,
		IReadOnlyDictionary<TSource,TResult> dictionary){
		foreach(var e in source)
			if(dictionary.TryGetValue(e,out var result))
				yield return result;
	}
	#endregion

	#region Apply actions
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
	#endregion

	#region Type specific
	public static string Join<T>(this IEnumerable<T> source,string sep)=>string.Join(sep,source);
	public static string Join<T>(this IEnumerable<T> source,char sep)
#if NETFRAMEWORK
		=>string.Join(char.ToString(sep),source);
#else
		=>string.Join(sep,source);
#endif
	public static string ConcatString<T>(this IEnumerable<T> source)=>string.Concat(source);

	public static Dictionary<TKey,TValue> ToDictionary<TKey,TValue>(this IEnumerable<(TKey key,TValue value)> source)
		where TKey : notnull
		=>source.ToDictionary(t=>t.key,t=>t.value);

	public static Dictionary<TKey,TValue> ToDictionary<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> source)
		where TKey : notnull
		=>source.ToDictionary(t=>t.Key,t=>t.Value);

	public static IEnumerable<(TKey key,TValue value)> ToTuples<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> source)
		where TKey : notnull
		=>source.Select(pair=>(pair.Key,pair.Value));

	public static IEnumerable<TResult> Select<TKey,TValue,TResult>(this IEnumerable<KeyValuePair<TKey,TValue>> source,Func<TKey,TValue,TResult> selector)=>
		source.Select(pair=>selector(pair.Key,pair.Value));

	public static IEnumerable<TKey> SelectKey<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> source)=>
		source.Select(pair=>pair.Key);

	public static IEnumerable<TValue> SelectValue<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> source)=>source.Select(pair=>pair.Value);

	public static IEnumerable<KeyValuePair<TKey,TValue>> Where<TKey,TValue>(this IEnumerable<KeyValuePair<TKey,TValue>> source,Func<TKey,TValue,bool> selector)=>
		source.Where(pair=>selector(pair.Key,pair.Value));


	public static Task<Task<T>> WhenAny<T>(this IEnumerable<Task<T>> source)=>Task.WhenAny(source);
	public static Task<Task> WhenAny(this IEnumerable<Task> source)=>Task.WhenAny(source);

	public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> source)=>Task.WhenAll(source);
	public static Task WhenAll(this IEnumerable<Task> source)=>Task.WhenAll(source);

	public static ValueTask<T[]> WhenAll<T>(this IEnumerable<ValueTask<T>> source)=>TaskUtils.WhenAll(source);
	public static ValueTask WhenAll(this IEnumerable<ValueTask> source)=>TaskUtils.WhenAll(source);
	#endregion

}