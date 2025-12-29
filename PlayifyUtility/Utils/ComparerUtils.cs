using JetBrains.Annotations;
using PlayifyUtility.HelperClasses;

namespace PlayifyUtility.Utils;

[PublicAPI]
public static class ComparerUtils{
	#region Basics
	[MustUseReturnValue]
	public static IComparer<TSource> Default<TSource>()=>Comparer<TSource>.Default;
	[MustUseReturnValue]
	public static IComparer<TSource> Func<TSource>(Comparison<TSource> func)=>Comparer<TSource>.Create(func);
	[MustUseReturnValue]
	public static IComparer<TSource> Invert<TSource>(this IComparer<TSource> comparer)=>Func<TSource>((a,b)=>comparer.Compare(b,a));
	#endregion
	
	#region OrderBy
	[MustUseReturnValue]
	public static IComparer<TSource> OrderBy<TSource,TKey>(Func<TSource,TKey> func,bool cache=false)=>OrderBy(func,Default<TKey>(),cache);
	[MustUseReturnValue]
	public static IComparer<TSource> OrderBy<TSource,TKey>(Func<TSource,TKey> func,IComparer<TKey> comparer,bool cache=false){
		if(!cache) return Func<TSource>((a,b)=>comparer.Compare(func(a),func(b)));

#pragma warning disable CS8714
		var computed=new Dictionary<TSource,TKey>();
#pragma warning restore CS8714
		ReferenceTo<TKey>? computedNull=null;//Needs to be wrapped in ReferenceTo, as TKey can be null as well
		
		return Func<TSource>((a,b)=>{
			TKey? aa,bb;
			if(a==null) aa=(computedNull??=new ReferenceTo<TKey>(func(a))).Value;
			else if(!computed.TryGetValue(a,out aa)) computed[a]=aa=func(a);
			
			if(b==null) bb=(computedNull??=new ReferenceTo<TKey>(func(b))).Value;
			else if(!computed.TryGetValue(b,out bb)) computed[b]=bb=func(b);
			return comparer.Compare(aa,bb);
		});
	}

	[MustUseReturnValue]
	public static IComparer<TSource> OrderByDesc<TSource,TKey>(Func<TSource,TKey> func,bool cache=false)=>OrderByDesc(func,Default<TKey>(),cache);
	[MustUseReturnValue]
	public static IComparer<TSource> OrderByDesc<TSource,TKey>(Func<TSource,TKey> func,IComparer<TKey> comparer,bool cache=false)=>Invert(OrderBy(func,comparer,cache));
	#endregion

	#region ThenBy
	[MustUseReturnValue]
	public static IComparer<TSource> Then<TSource>(this IComparer<TSource> primary,IComparer<TSource> secondary)=>Func<TSource>((a,b)=>{
		var result=primary.Compare(a,b);
		return result!=0?result:secondary.Compare(a,b);
	});
	[MustUseReturnValue]
	public static IComparer<TSource> ThenDesc<TSource>(this IComparer<TSource> thiz,IComparer<TSource> then)=>Then(thiz,Invert(then));
	
	[MustUseReturnValue]
	public static IComparer<TSource> ThenBy<TSource,TKey>(this IComparer<TSource> thiz,Func<TSource,TKey> func,IComparer<TKey> comparer,bool cache=false)
		=>Then(thiz,OrderBy(func,comparer,cache));
	[MustUseReturnValue]
	public static IComparer<TSource> ThenBy<TSource,TKey>(this IComparer<TSource> thiz,Func<TSource,TKey> func,bool cache=false)
		=>Then(thiz,OrderBy(func,cache));
	
	[MustUseReturnValue]
	public static IComparer<TSource> ThenByDesc<TSource,TKey>(this IComparer<TSource> thiz,Func<TSource,TKey> func,IComparer<TKey> comparer,bool cache=false)
		=>ThenDesc(thiz,OrderByDesc(func,comparer,cache));
	[MustUseReturnValue]
	public static IComparer<TSource> ThenByDesc<TSource,TKey>(this IComparer<TSource> thiz,Func<TSource,TKey> func,bool cache=false)
		=>ThenDesc(thiz,OrderByDesc(func,cache));
	#endregion

	#region Nulls

	[MustUseReturnValue]
	public static IComparer<T> NullsFirst<T>(this IComparer<T> comparer)=>Func<T>((a,b)=>a is null?-1:b is null?1:comparer.Compare(a,b));
	[MustUseReturnValue]
	public static IComparer<T> NullsLast<T>(this IComparer<T> comparer)=>Func<T>((a,b)=>a is null?1:b is null?-1:comparer.Compare(a,b));
	#endregion
}