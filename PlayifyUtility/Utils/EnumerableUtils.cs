using JetBrains.Annotations;

namespace PlayifyUtility.Utils;

[PublicAPI]
public static class EnumerableUtils{
	public static T[] Combine<T>(T a,T[] b){
		var arr=new T[b.Length+1];
		arr[0]=a;
		Array.Copy(b,0,arr,1,b.Length);
		return arr;
	}

	public static T[] Combine<T>(T[] a,T b){
		var arr=new T[a.Length+1];
		Array.Copy(a,arr,a.Length);
		arr[a.Length]=b;
		return arr;
	}

	public static T[] Combine<T>(T[] a,T[] b){
		var arr=new T[a.Length+b.Length];
		Array.Copy(a,arr,a.Length);
		Array.Copy(b,0,arr,a.Length,b.Length);
		return arr;
	}

	public static IEnumerable<T> Infinite<T>(T t){
		while(true)
			yield return t;
		// ReSharper disable once IteratorNeverReturns
	}

	public static IEnumerable<T> RepeatSelect<T>(int count,Func<T> selector){
		while(count-->0)
			yield return selector();
	}

	public static IEnumerable<T> RepeatSelect<T,TArg>(int count,Func<TArg,T> selector,TArg arg){
		while(count-->0)
			yield return selector(arg);
	}


	public static IEnumerable<T> SelectMany<T>(this IEnumerable<IEnumerable<T>> source)=>source.SelectMany(enumerable=>enumerable);

	public static IEnumerable<T> NonNull<T>(this IEnumerable<T?> source) where T:class=>source.Where(t=>t!=null)!;

	public static IEnumerable<T> NonNull<T>(this IEnumerable<T?> source) where T:struct{
		foreach(var nullable in source)
			if(nullable.TryGet(out var t))
				yield return t;
	}

	public static IEnumerable<(T value,int index)> WithIndex<T>(this IEnumerable<T> source,int offset=0)=>source.Select(value=>(value,offset++));

	public static IEnumerable<(TFirst a,TSecond b,int index)> ZipWithIndex<TFirst,TSecond>(this IEnumerable<TFirst> first,
	                                                                                       IEnumerable<TSecond> second){
		var i=0;
		return first.Zip(second,(f,s)=>(f,s,i++));
	}

	public static IEnumerable<(TFirst a,TSecond b)> Zip<TFirst,TSecond>(this IEnumerable<TFirst> first,
	                                                                    IEnumerable<TSecond> second)
		=>first.Zip(second,(f,s)=>(f,s));

	public static IEnumerable<TResult> Zip<TFirst,TSecond,TResult>(this IEnumerable<TFirst> first,
	                                                               IEnumerable<TSecond> second,
	                                                               Func<TFirst,TSecond,int,TResult> resultSelector){
		var i=0;
		return first.Zip(second,(f,s)=>resultSelector(f,s,i++));
	}

	public static string Join<T>(this IEnumerable<T> source,string sep)=>string.Join(sep,source);

	public static void ForEach<T>(this IEnumerable<T> source,Action<T> act){
		foreach(var e in source) act(e);
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
}